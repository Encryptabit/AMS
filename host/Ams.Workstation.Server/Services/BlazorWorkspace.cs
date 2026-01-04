using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Blazor workspace implementation following the CliWorkspace pattern.
/// Provides IWorkspace integration for the Blazor Server workstation.
/// </summary>
public sealed class BlazorWorkspace : IWorkspace, IDisposable
{
    private readonly BookManager _manager;
    private readonly string _rootPath;
    private bool _disposed;

    public BlazorWorkspace(string rootPath, IArtifactResolver? resolver = null)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Root path required.", nameof(rootPath));

        _rootPath = Path.GetFullPath(rootPath);
        var descriptor = BuildDescriptor(_rootPath);
        _manager = new BookManager(new[] { descriptor }, resolver ?? FileArtifactResolver.Instance);
    }

    /// <inheritdoc />
    public string RootPath => _rootPath;

    /// <inheritdoc />
    public BookContext Book => _manager.Current;

    /// <inheritdoc />
    public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var bookIndex = options.BookIndexFile ?? ResolveDefaultBookIndex();
        var chapterDir = options.ChapterDirectory;

        if (chapterDir is null && options.ChapterId is { Length: > 0 })
        {
            chapterDir = new DirectoryInfo(Path.Combine(_rootPath, options.ChapterId));
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

    private FileInfo ResolveDefaultBookIndex()
    {
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // BookManager doesn't implement IDisposable currently
    }
}
