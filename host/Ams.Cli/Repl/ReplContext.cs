using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public ReplState()
    {
        WorkingDirectory = Directory.GetCurrentDirectory();
        RefreshChapters();
        if (Chapters.Count > 0)
        {
            SelectedChapterIndex = 0;
        }
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
        SelectedChapterIndex = Chapters.Count > 0 ? 0 : null;
    }

    public void RefreshChapters()
    {
        try
        {
            Chapters = Directory.EnumerateFiles(WorkingDirectory, "*.wav", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
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
    }

    public bool UseChapterByIndex(int index)
    {
        if (index < 0 || index >= Chapters.Count)
        {
            return false;
        }

        RunAllChapters = false;
        SelectedChapterIndex = index;
        return true;
    }

    public bool UseChapterByName(string name)
    {
        for (int i = 0; i < Chapters.Count; i++)
        {
            if (Chapters[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileNameWithoutExtension(Chapters[i].Name).Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                RunAllChapters = false;
                SelectedChapterIndex = i;
                return true;
            }
        }

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
}
