using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Runtime.Artifacts;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

/// <summary>
/// Minimal resolver that turns (bookId, workspaceRoot, bookIndexPath) into ChapterContextHandle/BookContext.
/// Intended as a bridge until a full BookManager/IWorkspace factory is plugged in.
/// </summary>
internal sealed class WorkspaceResolver
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly IArtifactResolver _artifactResolver;
    private readonly ILogger<WorkspaceResolver> _logger;

    public WorkspaceResolver(ValidationViewerWorkspaceState state, ILogger<WorkspaceResolver> logger)
    {
        _state = state;
        _artifactResolver = FileArtifactResolver.Instance;
        _logger = logger;
    }

    public ChapterContextHandle? OpenChapter(string bookId, string chapterId)
    {
        var bookIndex = ResolveBookIndex(bookId);
        if (bookIndex is null) return null;

        var bookRoot = ResolveBookRoot(bookId);
        var chapterDir = new DirectoryInfo(Path.Combine(bookRoot.FullName, chapterId));
        var audio = new FileInfo(Path.Combine(chapterDir.FullName, $"{chapterId}.wav"));
        if (!audio.Exists)
        {
            var rootAudio = new FileInfo(Path.Combine(bookRoot.FullName, $"{chapterId}.wav"));
            audio = rootAudio.Exists ? rootAudio : audio;
        }

        // hydrate/asr/transcript optional; Core will resolve via DocumentSlots if present
        return ChapterContextHandle.Create(
            bookIndex,
            asrFile: null,
            transcriptFile: null,
            hydrateFile: null,
            audioFile: audio.Exists ? audio : null,
            chapterDirectory: chapterDir.Exists ? chapterDir : null,
            chapterId: chapterId);
    }

    public DirectoryInfo ResolveBookRoot(string bookId)
    {
        var root = _state.BookRoot;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("ValidationViewer: BookRoot not configured");
        }

        var dir = new DirectoryInfo(root);
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(dir.FullName);
        }

        return dir;
    }

    public FileInfo? ResolveBookIndex(string bookId)
    {
        var path = _state.BookIndexPath ?? Path.Combine(ResolveBookRoot(bookId).FullName, "book-index.json");
        var file = new FileInfo(path);
        if (!file.Exists)
        {
            _logger.LogWarning("book-index.json not found at {Path}", path);
            return null;
        }

        return file;
    }

    public BookContext ResolveBook(string bookId)
    {
        var root = ResolveBookRoot(bookId);
        var descriptors = BuildChapterDescriptors(root);
        var manager = new BookManager(new[]
        {
            new BookDescriptor(bookId, root.FullName, descriptors)
        }, _artifactResolver);
        return manager.Current;
    }

    private static IReadOnlyList<ChapterDescriptor> BuildChapterDescriptors(DirectoryInfo bookRoot)
    {
        var list = new List<ChapterDescriptor>();
        foreach (var dir in bookRoot.EnumerateDirectories())
        {
            var hydrate = Path.Combine(dir.FullName, $"{dir.Name}.align.hydrate.json");
            if (!File.Exists(hydrate))
            {
                continue;
            }

            var audioBuffers = new List<AudioBufferDescriptor>
            {
                new AudioBufferDescriptor("raw", Path.Combine(dir.FullName, $"{dir.Name}.wav")),
                new AudioBufferDescriptor("treated", Path.Combine(dir.FullName, $"{dir.Name}.treated.wav")),
                new AudioBufferDescriptor("filtered", Path.Combine(dir.FullName, $"{dir.Name}.filtered.wav"))
            };

            list.Add(new ChapterDescriptor(dir.Name, dir.FullName, audioBuffers));
        }

        return list;
    }
}
