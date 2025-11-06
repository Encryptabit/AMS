using System;
using System.Collections.Generic;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterDocumentManager
{
    private readonly Dictionary<string, string> _documents;

    public ChapterDocumentManager(IReadOnlyDictionary<string, string>? initial = null)
    {
        _documents = initial != null
            ? new Dictionary<string, string>(initial, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string> Documents => _documents;

    public void Register(string key, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(path);
        _documents[key] = path;
    }

    public bool TryGet(string key, out string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _documents.TryGetValue(key, out path!);
    }

    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _documents.Remove(key);
    }

    public void Clear() => _documents.Clear();
}

public sealed class ChapterManager
{
    private readonly BookContext _bookContext;
    private readonly IReadOnlyList<ChapterDescriptor> _descriptors;
    private readonly Dictionary<string, ChapterContext> _cache;
    private int _cursor;

    internal ChapterManager(BookContext bookContext)
    {
        _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
        _descriptors = bookContext.Descriptor.Chapters;
        _cache = new Dictionary<string, ChapterContext>(StringComparer.OrdinalIgnoreCase);
        _cursor = 0;
    }

    public int Count => _descriptors.Count;

    public ChapterContext Current
    {
        get
        {
            if (_descriptors.Count == 0)
            {
                throw new InvalidOperationException("This book does not define any chapters.");
            }

            return Load(_cursor);
        }
    }

    public ChapterContext Load(int index)
    {
        if ((uint)index >= (uint)_descriptors.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _cursor = index;
        return GetOrCreate(_descriptors[index]);
    }

    public ChapterContext Load(string chapterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(chapterId);
        for (int i = 0; i < _descriptors.Count; i++)
        {
            if (string.Equals(_descriptors[i].ChapterId, chapterId, StringComparison.OrdinalIgnoreCase))
            {
                _cursor = i;
                return GetOrCreate(_descriptors[i]);
            }
        }

        throw new KeyNotFoundException($"Chapter '{chapterId}' was not found in book '{_bookContext.Descriptor.BookId}'.");
    }

    public bool TryMoveNext(out ChapterContext context)
    {
        if (_cursor + 1 >= _descriptors.Count)
        {
            context = _descriptors.Count == 0 ? null! : Current;
            return false;
        }

        context = Load(_cursor + 1);
        return true;
    }

    public bool TryMovePrevious(out ChapterContext context)
    {
        if (_cursor <= 0 || _descriptors.Count == 0)
        {
            context = _descriptors.Count == 0 ? null! : Current;
            return false;
        }

        context = Load(_cursor - 1);
        return true;
    }

    public void Reset() => _cursor = 0;

    public void Deallocate(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return;
        }

        if (_cache.Remove(chapterId, out var context))
        {
            context.Audio.DeallocateAll();
        }
    }

    public void DeallocateAll()
    {
        foreach (var context in _cache.Values)
        {
            context.Audio.DeallocateAll();
        }

        _cache.Clear();
    }

    private ChapterContext GetOrCreate(ChapterDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.ChapterId, out var context))
        {
            context = new ChapterContext(_bookContext, descriptor);
            _cache[descriptor.ChapterId] = context;
        }

        return context;
    }
}
