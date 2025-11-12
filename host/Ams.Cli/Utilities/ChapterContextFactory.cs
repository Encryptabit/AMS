using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Cli.Utilities;

internal sealed class ChapterContextHandle : IDisposable
{
    private readonly BookManager _manager;
    private readonly BookContext _book;
    private readonly string _chapterId;
    private bool _disposed;

    internal ChapterContextHandle(BookManager manager, BookContext book, ChapterContext chapter)
    {
        _manager = manager;
        _book = book;
        Chapter = chapter;
        _chapterId = chapter.Descriptor.ChapterId;
    }

    public BookContext Book => _book;
    public ChapterContext Chapter { get; }

    public void Save()
    {
        Chapter.Save();
        _book.Save();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _manager.Deallocate(_chapterId);
        _disposed = true;
    }
}

internal static class ChapterContextFactory
{
    private sealed record ManagerKey(string BookRoot);

    private static readonly Dictionary<ManagerKey, BookManager> _managers = new();
    private static readonly object _sync = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ChapterContextHandle Create(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null)
    {
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var chapterStem = DetermineChapterStem(chapterId, audioFile, asrFile);
        var chapterRoot = ResolveChapterRoot(chapterDirectory, audioFile, asrFile, bookIndexFile.Directory, chapterStem);
        var audioPath = audioFile?.FullName ?? Path.Combine(chapterRoot, $"{chapterStem}.wav");

        var chapterDescriptor = new ChapterDescriptor(
            chapterId: chapterStem,
            rootPath: chapterRoot,
            audioBuffers: new[]
            {
                new AudioBufferDescriptor("raw", audioPath)
            });

        var bookRoot = bookIndexFile.Directory?.FullName ?? Directory.GetCurrentDirectory();
        var bookDescriptor = new BookDescriptor(
            bookId: Path.GetFileName(bookRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            rootPath: bookRoot,
            chapters: new[] { chapterDescriptor });

        var manager = GetOrCreateManager(bookDescriptor);
        var book = manager.Current;
        var chapter = book.Chapters.Load(chapterStem);

        book.Documents.BookIndex = LoadJson<BookIndex>(bookIndexFile.FullName);

        if (asrFile?.Exists == true)
        {
            chapter.Documents.Asr = LoadJson<AsrResponse>(asrFile.FullName);
        }

        if (transcriptFile?.Exists == true)
        {
            chapter.Documents.Transcript = LoadJson<TranscriptIndex>(transcriptFile.FullName);
        }

        if (hydrateFile?.Exists == true)
        {
            chapter.Documents.HydratedTranscript = LoadJson<HydratedTranscript>(hydrateFile.FullName);
        }

        return new ChapterContextHandle(manager, book, chapter);
    }

    private static BookManager GetOrCreateManager(BookDescriptor descriptor)
    {
        var key = new ManagerKey(descriptor.RootPath);
        lock (_sync)
        {
            if (!_managers.TryGetValue(key, out var manager))
            {
                manager = new BookManager(new[] { descriptor });
                _managers[key] = manager;
            }

            return manager;
        }
    }

    private static string DetermineChapterStem(string? supplied, FileInfo? audioFile, FileInfo? asrFile)
    {
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            return supplied!;
        }

        var candidate = audioFile ?? asrFile;
        if (candidate is not null)
        {
            return Path.GetFileNameWithoutExtension(candidate.Name);
        }

        return "chapter";
    }

    private static string ResolveChapterRoot(
        DirectoryInfo? chapterDirectory,
        FileInfo? audioFile,
        FileInfo? asrFile,
        DirectoryInfo? bookIndexDirectory,
        string chapterStem)
    {
        if (chapterDirectory is not null)
        {
            Directory.CreateDirectory(chapterDirectory.FullName);
            return chapterDirectory.FullName;
        }

        var candidate = audioFile?.Directory ?? asrFile?.Directory ?? bookIndexDirectory;
        if (candidate is null)
        {
            var fallback = Path.Combine(Directory.GetCurrentDirectory(), chapterStem);
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        Directory.CreateDirectory(candidate.FullName);
        return candidate.FullName;
    }

    private static T LoadJson<T>(string path)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from {path}");
}
