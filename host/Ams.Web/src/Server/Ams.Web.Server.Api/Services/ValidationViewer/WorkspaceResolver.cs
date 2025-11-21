using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

/// <summary>
/// Minimal resolver that turns (bookId, workspaceRoot, bookIndexPath) into ChapterContextHandle/BookContext.
/// Intended as a bridge until a full BookManager/IWorkspace factory is plugged in.
/// </summary>
internal sealed class WorkspaceResolver
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly WorkspaceFactory _factory;
    private readonly ILogger<WorkspaceResolver> _logger;

    public WorkspaceResolver(ValidationViewerWorkspaceState state, WorkspaceFactory factory, ILogger<WorkspaceResolver> logger)
    {
        _state = state;
        _factory = factory;
        _logger = logger;
    }

    public ChapterContextHandle? OpenChapter(string bookId, string chapterId)
    {
        try
        {
            var workspace = _factory.CreateWorkspace(bookId);
            var options = new ChapterOpenOptions
            {
                BookIndexFile = ResolveBookIndex(bookId),
                ChapterId = chapterId
            };

            return workspace.OpenChapter(options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ValidationViewer: failed to open chapter {ChapterId} in book {BookId}", chapterId, bookId);
            return null;
        }
    }

    public DirectoryInfo ResolveBookRoot(string bookId)
    {
        var root = _state.BookRoot;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("ValidationViewer: BookRoot not configured");
        }

        return new DirectoryInfo(root);
    }

    public FileInfo? ResolveBookIndex(string bookId)
    {
        var path = _state.BookIndexPath ?? Path.Combine(ResolveBookRoot(bookId).FullName, "book-index.json");
        return new FileInfo(path);
    }

    public BookContext ResolveBook(string bookId)
    {
        return _factory.CreateBookManager(bookId).Current;
    }
}
