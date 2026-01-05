using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Processors.Alignment.Anchors;
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
    private static readonly string StateFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AMS",
        "workstation-state.json");

    private BookManager? _manager;
    private ChapterContextHandle? _currentChapterHandle;
    private string? _rootPath;
    private bool _disposed;

    // Maps display title (e.g., "CHAPTER 3") to WAV stem (e.g., "03_CultistOfCerebon2_Ch3")
    private readonly Dictionary<string, string> _stemByTitle = new(StringComparer.OrdinalIgnoreCase);

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
    /// The currently open chapter handle, or null if no chapter is selected.
    /// Access chapter data via: CurrentChapterHandle.Context.Audio.Current.Buffer
    /// </summary>
    public ChapterContextHandle? CurrentChapterHandle => _currentChapterHandle;

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

        var trimmed = path.Trim();
        if (!Directory.Exists(trimmed)) return false;

        // Dispose previous state
        _currentChapterHandle?.Dispose();
        _currentChapterHandle = null;
        CurrentChapterName = null;
        AvailableChapters.Clear();

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
        return true;
    }

    /// <summary>
    /// Selects a chapter by name, opening its context handle.
    /// The handle remains open until another chapter is selected or workspace is disposed.
    /// </summary>
    /// <param name="chapterName">The chapter name (display title) to select.</param>
    /// <returns>True if chapter was opened successfully.</returns>
    public bool SelectChapter(string chapterName)
    {
        if (string.IsNullOrEmpty(chapterName) || _manager == null || string.IsNullOrEmpty(_rootPath))
            return false;

        // Resolve display title to WAV stem
        // If not found in mapping, assume chapterName IS the stem (direct usage)
        var chapterStem = _stemByTitle.GetValueOrDefault(chapterName, chapterName);

        // Dispose previous handle
        _currentChapterHandle?.Dispose();
        _currentChapterHandle = null;

        try
        {
            // WAV files are in the root directory, not inside the chapter folder
            var audioPath = Path.Combine(_rootPath, $"{chapterStem}.wav");
            var chapterDir = Path.Combine(_rootPath, chapterStem);

            _currentChapterHandle = OpenChapter(new ChapterOpenOptions
            {
                ChapterId = chapterStem,
                AudioFile = File.Exists(audioPath) ? new FileInfo(audioPath) : null,
                ChapterDirectory = Directory.Exists(chapterDir) ? new DirectoryInfo(chapterDir) : null
            });
            CurrentChapterName = chapterName; // Store display name for UI
            SavePersistedState();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open chapter '{chapterName}' (stem: {chapterStem}): {ex.Message}");
            CurrentChapterName = null;
            return false;
        }
    }

    /// <summary>
    /// Clears all state, resetting to initial values.
    /// </summary>
    public void Clear()
    {
        _currentChapterHandle?.Dispose();
        _currentChapterHandle = null;
        _manager = null;
        _rootPath = null;
        CurrentChapterName = null;
        AvailableChapters.Clear();
        _stemByTitle.Clear();
        SavePersistedState();
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

        var indexPath = Path.Combine(_rootPath, "book-index.json");
        _stemByTitle.Clear();

        try
        {
            // Load book index for section matching
            var bookIndexJson = File.ReadAllText(indexPath);
            var bookIndex = JsonSerializer.Deserialize<BookIndex>(bookIndexJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (bookIndex?.Sections == null || bookIndex.Sections.Length == 0)
            {
                Console.WriteLine("No sections found in book-index.json");
                return;
            }

            // Scan for WAV files in root directory
            var wavFiles = Directory.GetFiles(_rootPath, "*.wav", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(stem => !string.IsNullOrEmpty(stem))
                .ToList();

            // Match each WAV file to a section using SectionLocator
            foreach (var wavStem in wavFiles)
            {
                var section = SectionLocator.ResolveSectionByTitle(bookIndex, wavStem);
                if (section?.Title != null)
                {
                    // Store mapping: title -> stem
                    if (!_stemByTitle.ContainsKey(section.Title))
                    {
                        _stemByTitle[section.Title] = wavStem!;
                        AvailableChapters.Add(section.Title);
                    }
                }
            }

            // Sort chapters by their order in the book index
            var sectionOrder = bookIndex.Sections
                .Select((s, i) => (s.Title, Index: i))
                .Where(x => x.Title != null)
                .ToDictionary(x => x.Title!, x => x.Index, StringComparer.OrdinalIgnoreCase);

            AvailableChapters.Sort((a, b) =>
            {
                var aIdx = sectionOrder.GetValueOrDefault(a, int.MaxValue);
                var bIdx = sectionOrder.GetValueOrDefault(b, int.MaxValue);
                return aIdx.CompareTo(bIdx);
            });
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

            if (root.TryGetProperty("workingDirectory", out var wdProp))
            {
                var wd = wdProp.GetString();
                if (!string.IsNullOrEmpty(wd) && Directory.Exists(wd))
                {
                    SetWorkingDirectory(wd);

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
                currentChapter = CurrentChapterName
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _currentChapterHandle?.Dispose();
    }
}
