using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ams.Cli.Repl;

internal static class ReplContext
{
    private static readonly AsyncLocal<ReplState?> _state = new();

    public static ReplState? Current
    {
        get => _state.Value;
        internal set => _state.Value = value;
    }
}

internal sealed class ReplState
{
    private FileInfo? _chapterOverride;
    private readonly string _stateFilePath;
    private bool _suppressPersist;
    private string? _pendingChapterName;
    private bool _pendingRunAll;
    private string? _lastSelectedChapterName;

    public ReplState()
    {
        _stateFilePath = ResolveStateFilePath();
        WorkingDirectory = Directory.GetCurrentDirectory();

        _suppressPersist = true;
        LoadPersistedState();

        RefreshChapters();

        if (_pendingRunAll && Chapters.Count > 0)
        {
            RunAllChapters = true;
            SelectedChapterIndex = null;
        }
        else if (!string.IsNullOrWhiteSpace(_pendingChapterName))
        {
            if (!SelectChapterByNameInternal(_pendingChapterName!, updateLastSelected: true))
            {
                InitializeFallbackSelection();
            }
        }
        else
        {
            InitializeFallbackSelection();
        }

        _suppressPersist = false;
        PersistState();
    }

    public string WorkingDirectory { get; private set; }

    public List<FileInfo> Chapters { get; private set; } = new();

    public bool RunAllChapters { get; private set; }

    private int? SelectedChapterIndex { get; set; }

    public FileInfo? SelectedChapter =>
        SelectedChapterIndex is int idx && idx >= 0 && idx < Chapters.Count
            ? Chapters[idx]
            : null;

    public FileInfo? ActiveChapter => _chapterOverride ?? SelectedChapter;

    public string? ActiveChapterStem =>
        ActiveChapter is null
            ? null
            : Path.GetFileNameWithoutExtension(ActiveChapter.Name);

    public string WorkingDirectoryLabel
    {
        get
        {
            var dir = WorkingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var leaf = Path.GetFileName(dir);
            return string.IsNullOrEmpty(leaf) ? dir : leaf;
        }
    }

    public string ScopeLabel
    {
        get
        {
            if (RunAllChapters && Chapters.Count > 0)
            {
                return "ALL";
            }

            if (ActiveChapter is not null)
            {
                return ActiveChapter.Name;
            }

            return "NONE";
        }
    }

    public void SetWorkingDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Path cannot be empty");
            return;
        }

        var full = Path.GetFullPath(path);
        if (!Directory.Exists(full))
        {
            Console.WriteLine($"Directory not found: {full}");
            return;
        }

        WorkingDirectory = full;
        RefreshChapters();
        RunAllChapters = false;
        if (Chapters.Count > 0)
        {
            SelectChapterByIndexInternal(0);
        }
        else
        {
            SelectedChapterIndex = null;
        }

        PersistState();
    }

    public void RefreshChapters()
    {
        var previousName = SelectedChapter?.Name ?? _lastSelectedChapterName;
        var previousRunAll = RunAllChapters;

        try
        {
            Chapters = Directory.EnumerateFiles(WorkingDirectory, "*.wav", SearchOption.TopDirectoryOnly)
                .Where(path => !string.Equals(Path.GetFileName(path), "roomtone.wav", StringComparison.OrdinalIgnoreCase))
                .Select(path => new FileInfo(path))
                .OrderBy(file => file, ChapterFileComparer.Instance)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to enumerate WAV files: {ex.Message}");
            Chapters = new List<FileInfo>();
        }

        if (Chapters.Count == 0)
        {
            RunAllChapters = false;
            SelectedChapterIndex = null;
        }
        else if (previousRunAll)
        {
            RunAllChapters = true;
            SelectedChapterIndex = null;
        }
        else if (!string.IsNullOrWhiteSpace(previousName) && SelectChapterByNameInternal(previousName, updateLastSelected: false))
        {
            // selection restored
        }
        else if (SelectedChapterIndex is null || SelectedChapterIndex < 0 || SelectedChapterIndex >= Chapters.Count)
        {
            SelectChapterByIndexInternal(0, updateLastSelected: true);
        }

        if (!RunAllChapters && SelectedChapter is not null)
        {
            _lastSelectedChapterName = SelectedChapter.Name;
        }

        PersistState();
    }

    public void ListChapters()
    {
        if (Chapters.Count == 0)
        {
            Console.WriteLine("No WAV files found in current directory.");
            return;
        }

        for (int i = 0; i < Chapters.Count; i++)
        {
            var marker = SelectedChapterIndex == i && !RunAllChapters ? "*" : " ";
            Console.WriteLine($"{marker}[{i}] {Chapters[i].Name}");
        }
        if (RunAllChapters)
        {
            Console.WriteLine("Mode: ALL chapters");
        }
    }

    public void PrintState()
    {
        Console.WriteLine($"Working directory : {WorkingDirectory}");
        Console.WriteLine($"Chapters          : {Chapters.Count}");
        if (Chapters.Count > 0)
        {
            Console.WriteLine($"Mode              : {ScopeLabel}");
        }
        else
        {
            Console.WriteLine("Mode              : NONE (no WAV files detected)");
        }
        Console.WriteLine($"ASR Service      : {AsrProcessSupervisor.StatusDescription}");
        if (!string.IsNullOrWhiteSpace(AsrProcessSupervisor.BaseUrl))
        {
            Console.WriteLine($"ASR Endpoint     : {AsrProcessSupervisor.BaseUrl}");
        }
    }

    public void UseAllChapters()
    {
        if (Chapters.Count == 0)
        {
            Console.WriteLine("No chapters available.");
            return;
        }

        RunAllChapters = true;
        SelectedChapterIndex = null;
        _chapterOverride = null;

        PersistState();
    }

    public bool UseChapterByIndex(int index)
    {
        if (index < 0 || index >= Chapters.Count)
        {
            return false;
        }

        RunAllChapters = false;
        SelectChapterByIndexInternal(index);
        return true;
    }

    public bool UseChapterByName(string name)
    {
        var originalRunAll = RunAllChapters;
        RunAllChapters = false;
        if (SelectChapterByNameInternal(name))
        {
            return true;
        }

        RunAllChapters = originalRunAll;
        return false;
    }

    public IDisposable BeginChapterScope(FileInfo chapter)
    {
        _chapterOverride = chapter;
        return new ChapterScope(this);
    }

    public void ClearChapterScope()
    {
        _chapterOverride = null;
    }

    public FileInfo ResolveChapterFile(string suffix, bool mustExist)
    {
        if (ActiveChapterStem is null)
        {
            throw new InvalidOperationException("No active chapter. Use 'use' command or provide explicit path.");
        }

        var stem = ActiveChapterStem;
        var rootCandidate = Path.Combine(WorkingDirectory, $"{stem}.{suffix}");
        var chapterDir = Path.Combine(WorkingDirectory, stem);
        var chapterCandidate = Path.Combine(chapterDir, $"{stem}.{suffix}");

        if (mustExist)
        {
            if (File.Exists(rootCandidate))
            {
                return new FileInfo(rootCandidate);
            }

            if (File.Exists(chapterCandidate))
            {
                return new FileInfo(chapterCandidate);
            }

            throw new FileNotFoundException($"Derived chapter file not found. Checked: {rootCandidate} and {chapterCandidate}");
        }

        if (Directory.Exists(chapterDir))
        {
            return new FileInfo(chapterCandidate);
        }

        return new FileInfo(rootCandidate);
    }

    private sealed class ChapterScope : IDisposable
    {
        private readonly ReplState _state;
        private readonly FileInfo? _previous;
        private bool _disposed;

        public ChapterScope(ReplState state)
        {
            _state = state;
            _previous = state._chapterOverride;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _state._chapterOverride = _previous;
            _disposed = true;
        }
    }

    private void InitializeFallbackSelection()
    {
        if (Chapters.Count > 0)
        {
            SelectChapterByIndexInternal(SelectedChapterIndex is int idx && idx >= 0 && idx < Chapters.Count ? idx : 0,
                updateLastSelected: true);
            RunAllChapters = false;
        }
        else
        {
            SelectedChapterIndex = null;
            RunAllChapters = false;
        }
    }

    private bool SelectChapterByNameInternal(string name, bool updateLastSelected = true)
    {
        for (int i = 0; i < Chapters.Count; i++)
        {
            var candidate = Chapters[i].Name;
            if (candidate.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileNameWithoutExtension(candidate).Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                SelectChapterByIndexInternal(i, updateLastSelected);
                return true;
            }
        }

        return false;
    }

    private void SelectChapterByIndexInternal(int index, bool updateLastSelected = true)
    {
        if (Chapters.Count == 0)
        {
            SelectedChapterIndex = null;
            return;
        }

        SelectedChapterIndex = Math.Clamp(index, 0, Chapters.Count - 1);
        _chapterOverride = null;
        if (updateLastSelected && SelectedChapter is not null)
        {
            _lastSelectedChapterName = SelectedChapter.Name;
        }

        PersistState();
    }

    private void LoadPersistedState()
    {
        try
        {
            if (!File.Exists(_stateFilePath))
            {
                return;
            }

            var json = File.ReadAllText(_stateFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var state = JsonSerializer.Deserialize<PersistedReplState>(json);
            if (state is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(state.WorkingDirectory) && Directory.Exists(state.WorkingDirectory))
            {
                WorkingDirectory = state.WorkingDirectory;
            }

            _pendingChapterName = state.SelectedChapterName;
            _pendingRunAll = state.RunAllChapters;
            _lastSelectedChapterName = state.SelectedChapterName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to load REPL state: {ex.Message}");
        }
    }

    private void PersistState()
    {
        if (_suppressPersist)
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(_stateFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var snapshot = new PersistedReplState(
                WorkingDirectory,
                _lastSelectedChapterName,
                RunAllChapters);

            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to persist REPL state: {ex.Message}");
        }
    }

    private static string ResolveStateFilePath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (string.IsNullOrEmpty(basePath))
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ams");
        }

        var directory = Path.Combine(basePath, "AMS");
        return Path.Combine(directory, "repl-state.json");
    }

    private sealed record PersistedReplState(string WorkingDirectory, string? SelectedChapterName, bool RunAllChapters);

    private sealed class ChapterFileComparer : IComparer<FileInfo>
    {
        public static readonly ChapterFileComparer Instance = new();

        private static readonly Regex NumberRegex = new("\\d+", RegexOptions.Compiled);

        public int Compare(FileInfo? x, FileInfo? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var keyX = GetSortKey(x);
            var keyY = GetSortKey(y);

            var category = keyX.Category.CompareTo(keyY.Category);
            if (category != 0) return category;

            var number = keyX.PrimaryNumber.CompareTo(keyY.PrimaryNumber);
            if (number != 0) return number;

            var name = string.Compare(keyX.NameLower, keyY.NameLower, StringComparison.Ordinal);
            if (name != 0) return name;

            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static SortKey GetSortKey(FileInfo file)
        {
            var stem = Path.GetFileNameWithoutExtension(file.Name);
            var match = NumberRegex.Match(stem);
            if (match.Success && int.TryParse(match.Value, out var primary))
            {
                return new SortKey(0, primary, stem.ToLowerInvariant());
            }

            return new SortKey(1, int.MaxValue, stem.ToLowerInvariant());
        }

        private readonly record struct SortKey(int Category, int PrimaryNumber, string NameLower);
    }
}
