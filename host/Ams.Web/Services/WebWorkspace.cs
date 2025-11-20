using System.IO;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Web.Services;

public sealed class WebWorkspace : IWorkspace
{
    private readonly WorkspaceState _state;
    private readonly IArtifactResolver _resolver;
    private readonly object _sync = new();
    private BookManager _manager;
    private string _currentRoot;

    public WebWorkspace(WorkspaceState state, IArtifactResolver? resolver = null)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _resolver = resolver ?? FileArtifactResolver.Instance;
        _currentRoot = state.WorkspaceRoot;
        _manager = BuildManager(_currentRoot);
    }

    public string RootPath => _state.WorkspaceRoot;

    public BookContext Book
    {
        get
        {
            EnsureManager();
            return _manager.Current;
        }
    }

    public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        EnsureManager();

        var normalized = NormalizeOptions(options);
        var bookIndexFile = normalized.BookIndexFile
            ?? throw new InvalidOperationException("Book index file could not be resolved for this workspace.");

        return _manager.Current.Chapters.CreateContext(
            bookIndexFile,
            normalized.AsrFile,
            normalized.TranscriptFile,
            normalized.HydrateFile,
            normalized.AudioFile,
            normalized.ChapterDirectory,
            normalized.ChapterId,
            normalized.ReloadBookIndex);
    }

    private void EnsureManager()
    {
        var desiredRoot = _state.WorkspaceRoot;
        if (string.Equals(desiredRoot, _currentRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        lock (_sync)
        {
            if (string.Equals(desiredRoot, _currentRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _manager = BuildManager(desiredRoot);
            _currentRoot = desiredRoot;
        }
    }

    private BookManager BuildManager(string rootPath)
    {
        var descriptor = BuildDescriptor(rootPath);
        return new BookManager(new[] { descriptor }, _resolver);
    }

    private static BookDescriptor BuildDescriptor(string rootPath)
    {
        var trimmed = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var bookId = Path.GetFileName(trimmed);
        if (string.IsNullOrWhiteSpace(bookId))
        {
            bookId = "workspace";
        }

        return new BookDescriptor(bookId, trimmed, Array.Empty<ChapterDescriptor>());
    }

    private ChapterOpenOptions NormalizeOptions(ChapterOpenOptions options)
    {
        var bookIndex = options.BookIndexFile ?? new FileInfo(_state.BookIndexPath);
        return options with { BookIndexFile = bookIndex };
    }
}
