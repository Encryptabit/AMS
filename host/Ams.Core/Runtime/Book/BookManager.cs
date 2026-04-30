using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Interfaces;

namespace Ams.Core.Runtime.Book;

public sealed record AudioBufferDescriptor
{
    public AudioBufferDescriptor(
        string bufferId,
        string path,
        int? sampleRate = null,
        int? channels = null,
        TimeSpan? start = null,
        TimeSpan? duration = null)
    {
        BufferId = bufferId ?? throw new ArgumentNullException(nameof(bufferId));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        SampleRate = sampleRate;
        Channels = channels;
        Start = start;
        Duration = duration;
    }

    public string BufferId { get; }
    public string Path { get; }
    public int? SampleRate { get; }
    public int? Channels { get; }
    public TimeSpan? Start { get; }
    public TimeSpan? Duration { get; }
}

public sealed record ChapterDescriptor
{
    public ChapterDescriptor(
        string chapterId,
        string rootPath,
        IReadOnlyList<AudioBufferDescriptor> audioBuffers,
        IReadOnlyDictionary<string, string>? documents = null,
        IReadOnlyCollection<string>? aliases = null,
        int? bookStartWord = null,
        int? bookEndWord = null)
    {
        ChapterId = chapterId ?? throw new ArgumentNullException(nameof(chapterId));
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        AudioBuffers = audioBuffers ?? Array.Empty<AudioBufferDescriptor>();
        Documents = documents;
        Aliases = aliases ?? Array.Empty<string>();
        BookStartWord = bookStartWord;
        BookEndWord = bookEndWord;
    }

    public string ChapterId { get; }
    public string RootPath { get; }
    public IReadOnlyList<AudioBufferDescriptor> AudioBuffers { get; }
    public IReadOnlyDictionary<string, string>? Documents { get; }
    public IReadOnlyCollection<string> Aliases { get; }
    public int? BookStartWord { get; }
    public int? BookEndWord { get; }
}

public sealed record BookDescriptor
{
    public BookDescriptor(
        string bookId,
        string rootPath,
        IReadOnlyList<ChapterDescriptor> chapters,
        IReadOnlyDictionary<string, string>? documents = null)
    {
        BookId = bookId ?? throw new ArgumentNullException(nameof(bookId));
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Chapters = chapters ?? Array.Empty<ChapterDescriptor>();
        Documents = documents;
    }

    public string BookId { get; }
    public string RootPath { get; }
    public IReadOnlyList<ChapterDescriptor> Chapters { get; }
    public IReadOnlyDictionary<string, string>? Documents { get; }
}

public sealed class BookManager : IBookManager
{
    private readonly object _sync = new();
    private readonly IReadOnlyList<BookDescriptor> _descriptors;
    private readonly Dictionary<string, BookContext> _cache;
    private readonly IArtifactResolver _artifactResolver;
    private int _cursor;

    public BookManager(IReadOnlyList<BookDescriptor> descriptors, IArtifactResolver? resolver = null)
    {
        _descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));
        if (descriptors.Count == 0)
        {
            throw new ArgumentException("At least one book descriptor must be provided.", nameof(descriptors));
        }

        _cache = new Dictionary<string, BookContext>(StringComparer.OrdinalIgnoreCase);
        _artifactResolver = resolver ?? FileArtifactResolver.Instance;
        _cursor = 0;
    }

    public int Count => _descriptors.Count;

    public BookContext Current
    {
        get
        {
            lock (_sync)
            {
                return LoadCore(_cursor);
            }
        }
    }

    public BookContext Load(int index)
    {
        lock (_sync)
        {
            if ((uint)index >= (uint)_descriptors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return LoadCore(index);
        }
    }

    public BookContext Load(string bookId)
    {
        ArgumentException.ThrowIfNullOrEmpty(bookId);
        lock (_sync)
        {
            for (int i = 0; i < _descriptors.Count; i++)
            {
                if (string.Equals(_descriptors[i].BookId, bookId, StringComparison.OrdinalIgnoreCase))
                {
                    return LoadCore(i);
                }
            }

            throw new KeyNotFoundException($"Book '{bookId}' was not found in this manager instance.");
        }
    }

    public bool TryMoveNext(out BookContext context)
    {
        lock (_sync)
        {
            if (_cursor + 1 >= _descriptors.Count)
            {
                context = LoadCore(_cursor);
                return false;
            }

            context = LoadCore(_cursor + 1);
            return true;
        }
    }

    public bool TryMovePrevious(out BookContext context)
    {
        lock (_sync)
        {
            if (_cursor <= 0)
            {
                context = LoadCore(_cursor);
                return false;
            }

            context = LoadCore(_cursor - 1);
            return true;
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _cursor = 0;
        }
    }

    public void Deallocate(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            return;
        }

        lock (_sync)
        {
            if (_cache.Remove(bookId, out var context))
            {
                context.Save();
                context.Chapters.DeallocateAll();
                context.Audio.UnloadAll();
                Log.Debug("BookManager deallocated context {BookId}", bookId);
            }
        }
    }

    public void DeallocateAll()
    {
        lock (_sync)
        {
            foreach (var context in _cache.Values)
            {
                context.Save();
                context.Chapters.DeallocateAll();
                context.Audio.UnloadAll();
                Log.Debug("BookManager flushed context {BookId}", context.Descriptor.BookId);
            }

            _cache.Clear();
        }
    }

    private BookContext LoadCore(int index)
    {
        _cursor = index;
        return GetOrCreate(_descriptors[index]);
    }

    private BookContext GetOrCreate(BookDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.BookId, out var context))
        {
            context = new BookContext(descriptor, _artifactResolver);
            _cache[descriptor.BookId] = context;
            Log.Debug("BookManager created context {BookId}", descriptor.BookId);
        }
        else
        {
            Log.Debug("BookManager reused context {BookId}", descriptor.BookId);
        }

        return context;
    }
}
