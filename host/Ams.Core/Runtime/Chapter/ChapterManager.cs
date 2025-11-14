using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterManager : IChapterManager
{
    private const int DefaultMaxCachedContexts = 30;

    private readonly BookContext _bookContext;
    private readonly List<ChapterDescriptor> _descriptors;
    private readonly Dictionary<string, ChapterContext> _cache;
    private readonly Dictionary<string, LinkedListNode<string>> _usageNodes;
    private readonly LinkedList<string> _usageOrder;
    private readonly int _maxCachedContexts;
    private int _cursor;

    internal ChapterManager(BookContext bookContext, int maxCachedContexts = DefaultMaxCachedContexts)
    {
        _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
        _descriptors = new List<ChapterDescriptor>(bookContext.Descriptor.Chapters);
        _cache = new Dictionary<string, ChapterContext>(StringComparer.OrdinalIgnoreCase);
        _usageNodes = new Dictionary<string, LinkedListNode<string>>(StringComparer.OrdinalIgnoreCase);
        _usageOrder = new LinkedList<string>();
        _maxCachedContexts = Math.Max(1, maxCachedContexts);
        _cursor = 0;
    }

    public int Count => _descriptors.Count;
    public IReadOnlyList<ChapterDescriptor> Descriptors => _descriptors;

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

    public bool Contains(string chapterId)
    {
        ArgumentException.ThrowIfNullOrEmpty(chapterId);
        for (int i = 0; i < _descriptors.Count; i++)
        {
            if (string.Equals(_descriptors[i].ChapterId, chapterId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public ChapterDescriptor UpsertDescriptor(ChapterDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        for (int i = 0; i < _descriptors.Count; i++)
        {
            if (string.Equals(_descriptors[i].ChapterId, descriptor.ChapterId, StringComparison.OrdinalIgnoreCase))
            {
                var merged = MergeDescriptors(_descriptors[i], descriptor);
                _descriptors[i] = merged;
                return merged;
            }
        }

        _descriptors.Add(descriptor);
        return descriptor;
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
            context.Save();
            context.Audio.DeallocateAll();
            RemoveUsageNode(chapterId);
            Log.Debug(
                "ChapterManager[{BookId}] deallocated context {ChapterId} (cache {CacheCount}/{Max})",
                _bookContext.Descriptor.BookId,
                chapterId,
                _cache.Count,
                _maxCachedContexts);
        }
    }

    public void DeallocateAll()
    {
        foreach (var context in _cache.Values)
        {
            context.Save();
            context.Audio.DeallocateAll();
        }

        _cache.Clear();
        _usageNodes.Clear();
        _usageOrder.Clear();
    }

    private ChapterContext GetOrCreate(ChapterDescriptor descriptor)
    {
        if (!_cache.TryGetValue(descriptor.ChapterId, out var context))
        {
            context = new ChapterContext(_bookContext, descriptor);
            _cache[descriptor.ChapterId] = context;
            TrackUsage(descriptor.ChapterId);
            Log.Debug(
                "ChapterManager[{BookId}] created context {ChapterId} (cache {CacheCount}/{Max})",
                _bookContext.Descriptor.BookId,
                descriptor.ChapterId,
                _cache.Count,
                _maxCachedContexts);
            EnsureCapacity();
        }
        else
        {
            TrackUsage(descriptor.ChapterId);
            Log.Debug(
                "ChapterManager[{BookId}] reused context {ChapterId} (cache {CacheCount}/{Max})",
                _bookContext.Descriptor.BookId,
                descriptor.ChapterId,
                _cache.Count,
                _maxCachedContexts);
        }

        return context;
    }

    private void TrackUsage(string chapterId)
    {
        if (_usageNodes.TryGetValue(chapterId, out var node))
        {
            _usageOrder.Remove(node);
            _usageOrder.AddLast(node);
        }
        else
        {
            var newNode = _usageOrder.AddLast(chapterId);
            _usageNodes[chapterId] = newNode;
        }
    }

    private void RemoveUsageNode(string chapterId)
    {
        if (_usageNodes.TryGetValue(chapterId, out var node))
        {
            _usageOrder.Remove(node);
            _usageNodes.Remove(chapterId);
        }
    }

    private void EnsureCapacity()
    {
        while (_cache.Count > _maxCachedContexts && _usageOrder.First is { } lru)
        {
            var lruId = lru.Value;
            _usageOrder.RemoveFirst();
            _usageNodes.Remove(lruId);

            if (_cache.Remove(lruId, out var context))
            {
                context.Save();
                context.Audio.DeallocateAll();
                Log.Debug(
                    "ChapterManager[{BookId}] evicted LRU context {ChapterId} (cache {CacheCount}/{Max})",
                    _bookContext.Descriptor.BookId,
                    lruId,
                    _cache.Count,
                    _maxCachedContexts);
            }
        }
    }

    private static ChapterDescriptor MergeDescriptors(ChapterDescriptor existing, ChapterDescriptor incoming)
    {
        var aliasSet = new HashSet<string>(existing.Aliases, StringComparer.OrdinalIgnoreCase);
        foreach (var alias in incoming.Aliases)
        {
            aliasSet.Add(alias);
        }

        var rootPath = string.IsNullOrWhiteSpace(incoming.RootPath) ? existing.RootPath : incoming.RootPath;
        var audioBuffers = incoming.AudioBuffers.Count > 0 ? incoming.AudioBuffers : existing.AudioBuffers;
        var documents = incoming.Documents ?? existing.Documents;
        var startWord = incoming.BookStartWord ?? existing.BookStartWord;
        var endWord = incoming.BookEndWord ?? existing.BookEndWord;

        return new ChapterDescriptor(existing.ChapterId, rootPath, audioBuffers, documents, aliasSet.ToArray(), startWord, endWord);
    }
}
