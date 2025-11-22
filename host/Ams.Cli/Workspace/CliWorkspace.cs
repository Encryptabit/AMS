using System.IO;
using Ams.Cli.Repl;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Cli.Workspace;

internal sealed class CliWorkspace : IWorkspace
{
    private readonly ReplState? _state;
    private readonly BookManager _manager;
    private readonly string _rootPath;

    public CliWorkspace(ReplState state, IArtifactResolver? resolver = null)
        : this(state.WorkingDirectory, state, resolver)
    {
    }

    public CliWorkspace(string rootPath, ReplState? state = null, IArtifactResolver? resolver = null)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path must be provided.", nameof(rootPath));
        }

        _state = state;
        _rootPath = Path.GetFullPath(rootPath);
        var descriptor = BuildDescriptor(_rootPath);
        _manager = new BookManager(new[] { descriptor }, resolver ?? FileArtifactResolver.Instance);
    }

    public string RootPath => _rootPath;

    public BookContext Book => _manager.Current;

    public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var normalized = NormalizeOptions(options);
        var bookIndexFile = normalized.BookIndexFile
                            ?? throw new InvalidOperationException(
                                "Book index file could not be resolved for this workspace.");

        return Book.Chapters.CreateContext(
            bookIndexFile,
            normalized.AsrFile,
            normalized.TranscriptFile,
            normalized.HydrateFile,
            normalized.AudioFile,
            normalized.ChapterDirectory,
            normalized.ChapterId,
            normalized.ReloadBookIndex);
    }

    private ChapterOpenOptions NormalizeOptions(ChapterOpenOptions options)
    {
        var bookIndex = options.BookIndexFile ?? ResolveDefaultBookIndex();
        DirectoryInfo? chapterDirectory = options.ChapterDirectory;
        if (chapterDirectory is null && options.ChapterId is { Length: > 0 })
        {
            var baseDir = _state?.WorkingDirectory ?? _rootPath;
            var defaultChapterDir = Path.Combine(baseDir, options.ChapterId);
            chapterDirectory = new DirectoryInfo(defaultChapterDir);
        }

        return options with
        {
            BookIndexFile = bookIndex,
            ChapterDirectory = chapterDirectory
        };
    }

    private FileInfo ResolveDefaultBookIndex()
    {
        if (_state is not null)
        {
            return _state.ResolveBookIndex(mustExist: true);
        }

        var fallback = Path.Combine(_rootPath, "book-index.json");
        if (!File.Exists(fallback))
        {
            throw new FileNotFoundException("Book index not found. Provide --book-index.", fallback);
        }

        return new FileInfo(fallback);
    }

    private static BookDescriptor BuildDescriptor(string rootPath)
    {
        var trimmedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var bookId = Path.GetFileName(trimmedRoot);
        if (string.IsNullOrWhiteSpace(bookId))
        {
            bookId = "workspace";
        }

        return new BookDescriptor(bookId, trimmedRoot, Array.Empty<ChapterDescriptor>());
    }
}