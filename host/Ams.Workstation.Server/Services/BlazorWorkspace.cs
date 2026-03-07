using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Blazor workspace implementation following the CliWorkspace pattern.
/// Singleton - single-user workstation with shared state across all requests.
///
/// Data access follows explicit drill-down pattern:
///   workspace.Book.Chapters.CreateContext(...).Context.Audio.Current.Buffer
/// No shortcut aliases - verbosity ensures clarity about data sources.
/// </summary>
public sealed class BlazorWorkspace : IWorkspace, IDisposable
{
    private static readonly string StateFilePath = AmsAppDataPaths.Resolve("workstation-state.json");
    private static readonly StringComparer PersistedPathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private const int BackgroundPeakPxPerSec = 1200;
    private const int MaxBackgroundPeakBuckets = 500_000;

    private BookManager? _manager;
    private ChapterContextHandle? _currentChapterHandle;
    private string? _rootPath;
    private bool _disposed;
    private CancellationTokenSource? _backgroundPeakPrecomputeCts;
    private Task? _backgroundPeakPrecomputeTask;

    // Maps display title (e.g., "CHAPTER 3") to WAV stem (e.g., "03_CultistOfCerebon2_Ch3")
    private readonly Dictionary<string, string> _stemByTitle = new(StringComparer.OrdinalIgnoreCase);

    // Cache chapter handles to avoid re-creating contexts (LRU managed by ChapterManager)
    private readonly Dictionary<string, ChapterContextHandle> _chapterHandles = new(StringComparer.OrdinalIgnoreCase);

    public BlazorWorkspace()
    {
        LoadPersistedState();
    }

    #region IWorkspace Implementation

    /// <inheritdoc />
    public string RootPath => _rootPath ?? throw new InvalidOperationException("Working directory not set.");

    /// <inheritdoc />
    public BookContext Book => _manager?.Current ?? throw new InvalidOperationException("Workspace not initialized.");

    /// <inheritdoc />
    public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (_manager == null)
            throw new InvalidOperationException("Workspace not initialized. Set working directory first.");

        var bookIndex = options.BookIndexFile ?? ResolveDefaultBookIndex();
        var chapterDir = options.ChapterDirectory;

        if (chapterDir is null && options.ChapterId is { Length: > 0 })
        {
            chapterDir = new DirectoryInfo(Path.Combine(RootPath, options.ChapterId));
        }

        return Book.Chapters.CreateContext(
            bookIndex,
            options.AsrFile,
            options.TranscriptFile,
            options.HydrateFile,
            options.AudioFile,
            chapterDir,
            options.ChapterId,
            options.ReloadBookIndex);
    }

    #endregion

    #region Blazor-Specific State

    /// <summary>
    /// Whether a working directory has been set and workspace is initialized.
    /// </summary>
    public bool IsInitialized => _manager != null;

    /// <summary>
    /// The working directory path, or null if not set.
    /// </summary>
    public string? WorkingDirectory => _rootPath;

    /// <summary>
    /// List of available chapter names from the book-index.
    /// Populated when working directory is set.
    /// </summary>
    public List<string> AvailableChapters { get; } = new();

    /// <summary>
    /// Whether book-index.json exists in the working directory.
    /// </summary>
    public bool HasBookIndex => !string.IsNullOrEmpty(_rootPath)
        && File.Exists(Path.Combine(_rootPath, "book-index.json"));

    /// <summary>
    /// The name of the currently selected chapter, or null if none.
    /// </summary>
    public string? CurrentChapterName { get; private set; }

    /// <summary>
    /// Last pickup session path used in Polish, persisted with workspace state.
    /// </summary>
    public string? PickupSessionPath { get; private set; }

    /// <summary>
    /// Last roomtone file path used in Polish, persisted with workspace state.
    /// </summary>
    public string? RoomtoneFilePath { get; private set; }

    /// <summary>
    /// Whether waveform peaks should be computed for all chapters in the background after loading a workspace.
    /// </summary>
    public bool PrecomputePeaksInBackground { get; private set; }

    /// <summary>
    /// The currently open chapter handle, or null if no chapter is selected.
    /// Access chapter data via: CurrentChapterHandle.Context.Audio.Current.Buffer
    /// </summary>
    public ChapterContextHandle? CurrentChapterHandle => _currentChapterHandle;

    /// <summary>
    /// Cached book overview metrics. Computed on first access, invalidated when working directory changes.
    /// </summary>
    public BookOverview? CachedBookOverview { get; private set; }

    /// <summary>
    /// Try to load the hydrated transcript for a chapter without changing the current selection.
    /// </summary>
    public bool TryGetHydratedTranscript(string chapterName, out HydratedTranscript? hydrated)
    {
        hydrated = null;

        if (string.IsNullOrEmpty(chapterName) || _manager == null || string.IsNullOrEmpty(_rootPath))
            return false;

        if (!TryGetOrCreateChapterHandle(chapterName, out var handle))
            return false;

        hydrated = handle.Chapter.Documents.HydratedTranscript;
        return hydrated != null;
    }

    #endregion

    #region Workspace Lifecycle

    /// <summary>
    /// Sets the working directory and initializes the workspace.
    /// </summary>
    /// <param name="path">Path to the working directory containing book-index.json.</param>
    /// <returns>True if successful, false if path is invalid.</returns>
    public bool SetWorkingDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        var trimmed = NormalizeOptionalPath(path);
        if (string.IsNullOrWhiteSpace(trimmed)) return false;
        if (!Directory.Exists(trimmed)) return false;

        CancelBackgroundPeakPrecompute();

        // Dispose previous state
        foreach (var handle in _chapterHandles.Values)
        {
            handle.Dispose();
        }
        _chapterHandles.Clear();
        _currentChapterHandle = null;
        CurrentChapterName = null;
        AvailableChapters.Clear();
        CachedBookOverview = null; // Invalidate cached overview

        _rootPath = Path.GetFullPath(trimmed);

        // Initialize book manager if book-index exists
        if (HasBookIndex)
        {
            var descriptor = BuildDescriptor(_rootPath);
            _manager = new BookManager(new[] { descriptor }, FileArtifactResolver.Instance);
            LoadChaptersFromIndex();
        }
        else
        {
            _manager = null;
        }

        SavePersistedState();
        RestartBackgroundPeakPrecompute();
        return true;
    }

    /// <summary>
    /// Persists Polish path inputs (pickup + roomtone) alongside workspace state.
    /// </summary>
    public void SetPolishPaths(string? pickupSessionPath, string? roomtoneFilePath)
    {
        var normalizedPickup = NormalizeOptionalPath(pickupSessionPath);
        var normalizedRoomtone = NormalizeOptionalPath(roomtoneFilePath);

        var pickupChanged = !PersistedPathComparer.Equals(
            PickupSessionPath ?? string.Empty,
            normalizedPickup ?? string.Empty);
        var roomtoneChanged = !PersistedPathComparer.Equals(
            RoomtoneFilePath ?? string.Empty,
            normalizedRoomtone ?? string.Empty);

        if (!pickupChanged && !roomtoneChanged)
            return;

        PickupSessionPath = normalizedPickup;
        RoomtoneFilePath = normalizedRoomtone;
        SavePersistedState();
    }

    /// <summary>
    /// Enables or disables background waveform peak precomputation for all chapters in the active workspace.
    /// </summary>
    public void SetPrecomputePeaksInBackground(bool enabled)
    {
        if (PrecomputePeaksInBackground == enabled)
            return;

        PrecomputePeaksInBackground = enabled;
        SavePersistedState();
        RestartBackgroundPeakPrecompute();
    }

    /// <summary>
    /// Selects a chapter by name, opening its context handle.
    /// Chapters remain cached until workspace is disposed (LRU managed by ChapterManager).
    /// </summary>
    /// <param name="chapterName">The chapter name (display title) to select.</param>
    /// <returns>True if chapter was opened successfully.</returns>
    public bool SelectChapter(string chapterName)
    {
        if (string.IsNullOrEmpty(chapterName) || _manager == null || string.IsNullOrEmpty(_rootPath))
            return false;

        if (!TryGetOrCreateChapterHandle(chapterName, out var handle))
        {
            CurrentChapterName = null;
            return false;
        }

        _currentChapterHandle = handle;
        CurrentChapterName = chapterName; // Store display name for UI
        SavePersistedState();
        return true;
    }

    /// <summary>
    /// Clears all state, resetting to initial values.
    /// </summary>
    public void Clear()
    {
        CancelBackgroundPeakPrecompute();
        foreach (var handle in _chapterHandles.Values)
        {
            handle.Dispose();
        }
        _chapterHandles.Clear();
        _currentChapterHandle = null;
        _manager = null;
        _rootPath = null;
        CurrentChapterName = null;
        AvailableChapters.Clear();
        _stemByTitle.Clear();
        CachedBookOverview = null;
        SavePersistedState();
    }

    /// <summary>
    /// Sets the cached book overview. Called by ValidationMetricsService after computing.
    /// </summary>
    public void SetCachedBookOverview(BookOverview overview)
    {
        CachedBookOverview = overview;
    }

    /// <summary>
    /// Gets the WAV stem for a chapter display title.
    /// </summary>
    public string? GetStemForChapter(string displayTitle)
    {
        return _stemByTitle.GetValueOrDefault(displayTitle);
    }

    #endregion

    #region Private Helpers

    private FileInfo ResolveDefaultBookIndex()
    {
        if (string.IsNullOrEmpty(_rootPath))
            throw new InvalidOperationException("Working directory not set.");

        var path = Path.Combine(_rootPath, "book-index.json");
        return new FileInfo(path);
    }

    private static BookDescriptor BuildDescriptor(string rootPath)
    {
        var trimmed = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var bookId = Path.GetFileName(trimmed);
        if (string.IsNullOrWhiteSpace(bookId)) bookId = "workspace";
        return new BookDescriptor(bookId, trimmed, Array.Empty<ChapterDescriptor>());
    }

    private void LoadChaptersFromIndex()
    {
        if (string.IsNullOrEmpty(_rootPath)) return;

        _stemByTitle.Clear();

        try
        {
            // Use ChapterDiscoveryService to scan WAV files and match to book index sections
            var discoveredChapters = ChapterDiscoveryService.DiscoverChapters(_rootPath);

            if (discoveredChapters.Count == 0)
            {
                Console.WriteLine("No chapters discovered in working directory");
                return;
            }

            // Populate mappings from discovered chapters
            foreach (var chapter in discoveredChapters)
            {
                if (!_stemByTitle.ContainsKey(chapter.DisplayTitle))
                {
                    _stemByTitle[chapter.DisplayTitle] = chapter.Stem;
                    AvailableChapters.Add(chapter.DisplayTitle);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load chapters: {ex.Message}");
        }
    }

    #endregion

    #region Persistence

    private void LoadPersistedState()
    {
        try
        {
            if (!File.Exists(StateFilePath)) return;

            var json = File.ReadAllText(StateFilePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("pickupSessionPath", out var pickupProp))
            {
                PickupSessionPath = NormalizeOptionalPath(pickupProp.GetString());
            }

            if (root.TryGetProperty("roomtoneFilePath", out var roomtoneProp))
            {
                RoomtoneFilePath = NormalizeOptionalPath(roomtoneProp.GetString());
            }

            if (root.TryGetProperty("precomputePeaksInBackground", out var precomputeProp) &&
                (precomputeProp.ValueKind == JsonValueKind.True || precomputeProp.ValueKind == JsonValueKind.False))
            {
                PrecomputePeaksInBackground = precomputeProp.GetBoolean();
            }

            if (root.TryGetProperty("workingDirectory", out var wdProp))
            {
                var wd = NormalizeOptionalPath(wdProp.GetString());
                if (!string.IsNullOrEmpty(wd) && SetWorkingDirectory(wd))
                {
                    // Restore current chapter if still valid
                    if (root.TryGetProperty("currentChapter", out var chProp))
                    {
                        var ch = chProp.GetString();
                        if (!string.IsNullOrEmpty(ch) && AvailableChapters.Contains(ch))
                        {
                            SelectChapter(ch);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load persisted state: {ex.Message}");
        }
    }

    private void SavePersistedState()
    {
        try
        {
            var dir = Path.GetDirectoryName(StateFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var state = new
            {
                workingDirectory = _rootPath,
                currentChapter = CurrentChapterName,
                pickupSessionPath = PickupSessionPath,
                roomtoneFilePath = RoomtoneFilePath,
                precomputePeaksInBackground = PrecomputePeaksInBackground
            };

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StateFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save persisted state: {ex.Message}");
        }
    }

    #endregion

    private static string? NormalizeOptionalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var trimmed = path.Trim().Trim('"', '\'').Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch
        {
            return trimmed;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelBackgroundPeakPrecompute();
        foreach (var handle in _chapterHandles.Values)
        {
            handle.Dispose();
        }
        _chapterHandles.Clear();
        _currentChapterHandle = null;
    }

    private bool TryGetOrCreateChapterHandle(string chapterName, out ChapterContextHandle handle)
    {
        var chapterStem = _stemByTitle.GetValueOrDefault(chapterName, chapterName);

        if (_chapterHandles.TryGetValue(chapterStem, out handle!))
        {
            return true;
        }

        try
        {
            var audioPath = Path.Combine(_rootPath!, $"{chapterStem}.wav");
            var chapterDir = Path.Combine(_rootPath!, chapterStem);

            handle = OpenChapter(new ChapterOpenOptions
            {
                ChapterId = chapterStem,
                AudioFile = File.Exists(audioPath) ? new FileInfo(audioPath) : null,
                ChapterDirectory = Directory.Exists(chapterDir) ? new DirectoryInfo(chapterDir) : null
            });

            _chapterHandles[chapterStem] = handle;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open chapter '{chapterName}' (stem: {chapterStem}): {ex.Message}");
            handle = null!;
            return false;
        }
    }

    private void RestartBackgroundPeakPrecompute()
    {
        CancelBackgroundPeakPrecompute();

        if (!PrecomputePeaksInBackground || _manager == null || string.IsNullOrEmpty(_rootPath) || AvailableChapters.Count == 0)
            return;

        var chapters = AvailableChapters.ToList();
        var cts = new CancellationTokenSource();
        _backgroundPeakPrecomputeCts = cts;
        _backgroundPeakPrecomputeTask = Task.Run(() => PrecomputeWaveformPeaksAsync(chapters, cts.Token), cts.Token);
    }

    private void CancelBackgroundPeakPrecompute()
    {
        if (_backgroundPeakPrecomputeCts is null)
            return;

        try
        {
            _backgroundPeakPrecomputeCts.Cancel();
        }
        catch
        {
            // Ignore cancellation disposal races
        }
        finally
        {
            _backgroundPeakPrecomputeCts.Dispose();
            _backgroundPeakPrecomputeCts = null;
            _backgroundPeakPrecomputeTask = null;
        }
    }

    private async Task PrecomputeWaveformPeaksAsync(IReadOnlyList<string> chapters, CancellationToken cancellationToken)
    {
        foreach (var chapter in chapters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryGetOrCreateChapterHandle(chapter, out var handle))
                continue;

            try
            {
                var seenBufferIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var audioContexts = new[]
                {
                    handle.Chapter.Audio.Current,
                    handle.Chapter.Audio.Treated,
                    handle.Chapter.Audio.Corrected
                };

                foreach (var audioContext in audioContexts)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (audioContext is null || !seenBufferIds.Add(audioContext.Descriptor.BufferId))
                        continue;

                    var buffer = audioContext.Buffer;
                    if (buffer is null)
                        continue;

                    var durationSeconds = buffer.SampleRate > 0
                        ? buffer.Length / (double)buffer.SampleRate
                        : 0d;
                    var requestedBuckets = Math.Max(1, (int)Math.Ceiling(durationSeconds * BackgroundPeakPxPerSec));
                    var bucketCount = Math.Min(requestedBuckets, MaxBackgroundPeakBuckets);

                    audioContext.GetOrCreateWaveformPeaks(bucketCount);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to precompute waveform peaks for '{chapter}': {ex.Message}");
            }

            await Task.Yield();
        }
    }
}
