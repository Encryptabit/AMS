using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

/// <summary>
/// Provides BookManager/IWorkspace instances for book roots configured at runtime.
/// Currently supports a single book root via ValidationViewerWorkspaceState; structured for expansion.
/// </summary>
internal sealed class WorkspaceFactory
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly IArtifactResolver _resolver;

    public WorkspaceFactory(ValidationViewerWorkspaceState state, IArtifactResolver? resolver = null)
    {
        _state = state;
        _resolver = resolver ?? FileArtifactResolver.Instance;
    }

    public BookManager CreateBookManager(string bookId)
    {
        var root = _state.BookRoot;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("ValidationViewer: BookRoot is not configured.");
        }

        var descriptors = WorkspaceChapterDiscovery.Discover(root);

        var descriptor = new BookDescriptor(
            bookId: bookId,
            rootPath: root,
            chapters: descriptors);

        return new BookManager(new[] { descriptor }, _resolver);
    }

    public IWorkspace CreateWorkspace(string bookId)
    {
        // Minimal workspace wrapper using the BookManager
        var manager = CreateBookManager(bookId);
        var context = manager.Current;
        return new SimpleWorkspace(_state.BookRoot!, _state.BookIndexPath, context);
    }

    private sealed class SimpleWorkspace : IWorkspace
    {
        private readonly string? _bookIndexPath;

        public SimpleWorkspace(string root, string? bookIndexPath, BookContext book)
        {
            RootPath = root;
            _bookIndexPath = bookIndexPath;
            Book = book;
        }

        public string RootPath { get; }
        public BookContext Book { get; }

        public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
        {
            var bookIndex = options.BookIndexFile ?? new FileInfo(_bookIndexPath ?? Path.Combine(RootPath, "book-index.json"));
            return ChapterContextHandle.Create(
                bookIndex,
                options.AsrFile,
                options.TranscriptFile,
                options.HydrateFile,
                options.AudioFile,
                options.ChapterDirectory,
                options.ChapterId);
        }
    }
}
