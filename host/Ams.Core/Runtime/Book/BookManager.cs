using System;
using System.Collections.Generic;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Chapter;

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
        IReadOnlyDictionary<string, string>? documents = null)
    {
        ChapterId = chapterId ?? throw new ArgumentNullException(nameof(chapterId));
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        AudioBuffers = audioBuffers ?? Array.Empty<AudioBufferDescriptor>();
        Documents = documents;
    }

    public string ChapterId { get; }
    public string RootPath { get; }
    public IReadOnlyList<AudioBufferDescriptor> AudioBuffers { get; }
    public IReadOnlyDictionary<string, string>? Documents { get; }
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

public sealed class BookManager
{
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

    public BookContext Current => Load(_cursor);

    public BookContext Load(int index)
    {
        if ((uint)index >= (uint)_descriptors.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cursor = index;
        return GetOrCreate(_descriptors[index]);
    }

    public BookContext Load(string bookId)
    {
        ArgumentException.ThrowIfNullOrEmpty(bookId);
        for (int i = 0; i < _descriptors.Count; i++)
        {
            if (string.Equals(_descriptors[i].BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                _cursor = i;
                return GetOrCreate(_descriptors[i]);
            }
        }

        throw new KeyNotFoundException($"Book '{bookId}' was not found in this manager instance.");
    }

    public bool TryMoveNext(out BookContext context)
    {
        if (_cursor + 1 >= _descriptors.Count)
        {
            context = Current;
            return false;
        }

        context = Load(_cursor + 1);
        return true;
    }

    public bool TryMovePrevious(out BookContext context)
    {
        if (_cursor <= 0)
        {
            context = Current;
            return false;
        }

        context = Load(_cursor - 1);
        return true;
    }

    public void Reset() => _cursor = 0;

    public void Deallocate(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            return;
        }

        if (_cache.Remove(bookId, out var context))
        {
            context.Save();
            context.Chapters.DeallocateAll();
        }
    }

    public void DeallocateAll()
    {
        foreach (var context in _cache.Values)
        {
            context.Save();
            context.Chapters.DeallocateAll();
        }

        _cache.Clear();
    }

    private BookContext GetOrCreate(BookDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.BookId, out var context))
        {
            context = new BookContext(descriptor, _artifactResolver);
            _cache[descriptor.BookId] = context;
        }

        return context;
    }
}
