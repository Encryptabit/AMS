using System;
using System.Collections.Generic;
using System.IO;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterContextHandle : IDisposable
{
    private sealed record ManagerKey(string BookRoot);

    private static readonly Dictionary<ManagerKey, BookManager> Managers = new();
    private static readonly object Sync = new();

    private readonly BookContext _bookContext;
    private readonly ChapterContext _chapterContext;
    private bool _disposed;

    internal ChapterContextHandle(BookContext bookContext, ChapterContext chapterContext)
    {
        _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
        _chapterContext = chapterContext ?? throw new ArgumentNullException(nameof(chapterContext));
    }

    public BookContext Book => _bookContext;
    public ChapterContext Chapter => _chapterContext;

    public static ChapterContextHandle Create(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null)
    {
        ArgumentNullException.ThrowIfNull(bookIndexFile);
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var bookRoot = bookIndexFile.Directory?.FullName ?? Directory.GetCurrentDirectory();
        var bookDescriptor = new BookDescriptor(
            bookId: Path.GetFileName(bookRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            rootPath: bookRoot,
            chapters: Array.Empty<ChapterDescriptor>());

        var book = GetOrCreateManager(bookDescriptor).Current;
        return book.Chapters.CreateContext(
            bookIndexFile,
            asrFile,
            transcriptFile,
            hydrateFile,
            audioFile,
            chapterDirectory,
            chapterId);
    }

    public void Save()
    {
        _chapterContext.Save();
        _bookContext.Save();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var chapterId = _chapterContext.Descriptor.ChapterId;
        _bookContext.Chapters.Deallocate(chapterId);
        _disposed = true;
    }

    private static BookManager GetOrCreateManager(BookDescriptor descriptor)
    {
        var key = new ManagerKey(descriptor.RootPath);
        lock (Sync)
        {
            if (!Managers.TryGetValue(key, out var manager))
            {
                manager = new BookManager(new[] { descriptor });
                Managers[key] = manager;
            }

            return manager;
        }
    }
}
