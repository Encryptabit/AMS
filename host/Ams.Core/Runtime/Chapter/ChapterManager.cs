using System.IO;
using System.Text;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Interfaces;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterManager : IChapterManager
{
    private const int DefaultMaxCachedContexts = 30;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

    public ChapterContextHandle CreateContext(
        FileInfo bookIndexFile,
        FileInfo? asrFile = null,
        FileInfo? transcriptFile = null,
        FileInfo? hydrateFile = null,
        FileInfo? audioFile = null,
        DirectoryInfo? chapterDirectory = null,
        string? chapterId = null,
        bool reloadBookIndex = false)
    {
        ArgumentNullException.ThrowIfNull(bookIndexFile);
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException("Book index not found", bookIndexFile.FullName);
        }

        var chapterStem = DetermineChapterStem(chapterId, audioFile, asrFile);
        var chapterRoot = ResolveChapterRoot(chapterDirectory, audioFile, asrFile, bookIndexFile.Directory, chapterStem);
        var audioPath = audioFile?.FullName ?? Path.Combine(chapterRoot, $"{chapterStem}.wav");
        var bookIndex = LoadJson<BookIndex>(bookIndexFile.FullName);

        var aliases = BuildAliasSet(chapterStem, chapterRoot, bookIndex, out var matchedSection);

        var bufferList = new List<AudioBufferDescriptor>
        {
            new AudioBufferDescriptor("raw", audioPath)
        };

        var treatedPath = Path.Combine(chapterRoot, $"{chapterStem}.treated.wav");
        bufferList.Add(new AudioBufferDescriptor("treated", treatedPath));

        var filteredPath = Path.Combine(chapterRoot, $"{chapterStem}.filtered.wav");
        bufferList.Add(new AudioBufferDescriptor("filtered", filteredPath));

        var initialDescriptor = new ChapterDescriptor(
            chapterId: chapterStem,
            rootPath: chapterRoot,
            audioBuffers: bufferList,
            aliases: aliases,
            bookStartWord: matchedSection?.StartWord,
            bookEndWord: matchedSection?.EndWord);

        var descriptor = EnsureChapterDescriptor(initialDescriptor);
        var chapter = Load(descriptor.ChapterId);

        var currentBookIndex = _bookContext.Documents.BookIndex;
        if (currentBookIndex is null || reloadBookIndex)
        {
            _bookContext.Documents.SetLoadedBookIndex(bookIndex);
        }

        if (asrFile?.Exists == true)
        {
            var asrDocument = LoadJson<AsrResponse>(asrFile.FullName);
            if (asrDocument is not null)
            {
                chapter.Documents.Asr = asrDocument;
                var currentCorpus = chapter.Documents.AsrTranscriptText;
                if (string.IsNullOrWhiteSpace(currentCorpus))
                {
                    chapter.Documents.AsrTranscriptText = AsrTranscriptBuilder.BuildCorpusText(asrDocument);
                }
            }
        }

        if (transcriptFile?.Exists == true)
        {
            chapter.Documents.Transcript = LoadJson<TranscriptIndex>(transcriptFile.FullName);
        }

        if (hydrateFile?.Exists == true)
        {
            chapter.Documents.HydratedTranscript = LoadJson<HydratedTranscript>(hydrateFile.FullName);
        }

        return new ChapterContextHandle(_bookContext, chapter);
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

    private ChapterDescriptor EnsureChapterDescriptor(ChapterDescriptor template)
    {
        if (Contains(template.ChapterId))
        {
            return UpsertDescriptor(template);
        }

        var normalizedRequested = NormalizeChapterId(template.ChapterId);
        var aliasMatch = FindByAlias(_descriptors, normalizedRequested);
        if (aliasMatch is not null)
        {
            var merged = CloneWithAliases(aliasMatch, template);
            return UpsertDescriptor(merged);
        }

        var rootMatch = TryMatchByRootPath(_descriptors, template.RootPath);
        if (rootMatch is not null)
        {
            var merged = CloneWithAliases(rootMatch, template);
            return UpsertDescriptor(merged);
        }

        var slugMatch = TryMatchByRootSlug(_descriptors, normalizedRequested);
        if (slugMatch is not null)
        {
            var merged = CloneWithAliases(slugMatch, template);
            return UpsertDescriptor(merged);
        }

        return UpsertDescriptor(template);
    }

    private static ChapterDescriptor? FindByAlias(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedAlias)
    {
        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return null;
        }

        foreach (var descriptor in descriptors)
        {
            if (string.Equals(NormalizeChapterId(descriptor.ChapterId), normalizedAlias, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }

            foreach (var alias in descriptor.Aliases)
            {
                if (string.Equals(NormalizeChapterId(alias), normalizedAlias, StringComparison.OrdinalIgnoreCase))
                {
                    return descriptor;
                }
            }
        }

        return null;
    }

    private static ChapterDescriptor? TryMatchByRootPath(IReadOnlyList<ChapterDescriptor> descriptors, string chapterRoot)
    {
        if (string.IsNullOrWhiteSpace(chapterRoot))
        {
            return null;
        }

        var normalizedRoot = NormalizePath(chapterRoot);
        foreach (var descriptor in descriptors)
        {
            if (string.IsNullOrWhiteSpace(descriptor.RootPath))
            {
                continue;
            }

            if (string.Equals(NormalizePath(descriptor.RootPath), normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }
        }

        return null;
    }

    private static ChapterDescriptor? TryMatchByRootSlug(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedRequested)
    {
        foreach (var descriptor in descriptors)
        {
            if (string.IsNullOrWhiteSpace(descriptor.RootPath))
            {
                continue;
            }

            var slug = Path.GetFileName(descriptor.RootPath);
            if (string.IsNullOrEmpty(slug))
            {
                continue;
            }

            if (string.Equals(NormalizeChapterId(slug), normalizedRequested, StringComparison.OrdinalIgnoreCase))
            {
                return descriptor;
            }
        }

        return null;
    }

    private static ChapterDescriptor CloneWithAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
    {
        var aliases = MergeAliases(existing, incoming);
        var rootPath = string.IsNullOrWhiteSpace(incoming.RootPath) ? existing.RootPath : incoming.RootPath;
        var audioBuffers = existing.AudioBuffers.Count > 0 ? existing.AudioBuffers : incoming.AudioBuffers;
        var documents = existing.Documents ?? incoming.Documents;
        var startWord = incoming.BookStartWord ?? existing.BookStartWord;
        var endWord = incoming.BookEndWord ?? existing.BookEndWord;

        return new ChapterDescriptor(existing.ChapterId, rootPath, audioBuffers, documents, aliases, startWord, endWord);
    }

    private static IReadOnlyCollection<string> MergeAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAlias(aliases, existing.ChapterId);
        AddAlias(aliases, incoming.ChapterId);

        foreach (var alias in existing.Aliases)
        {
            AddAlias(aliases, alias);
        }

        foreach (var alias in incoming.Aliases)
        {
            AddAlias(aliases, alias);
        }

        return aliases.ToArray();
    }

    private static IReadOnlyCollection<string> BuildAliasSet(
        string chapterId,
        string chapterRoot,
        BookIndex bookIndex,
        out SectionRange? matchedSection)
    {
        matchedSection = null;
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAlias(aliases, chapterId);

        if (!string.IsNullOrWhiteSpace(chapterRoot))
        {
            var rootName = Path.GetFileName(chapterRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            AddAlias(aliases, rootName);
        }

        matchedSection = TryResolveSection(bookIndex, chapterId)
            ?? TryResolveSection(bookIndex, Path.GetFileName(chapterRoot?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty))
            ?? TryResolveSectionFromAliases(bookIndex, aliases);

        if (matchedSection is not null && !string.IsNullOrWhiteSpace(matchedSection.Title))
        {
            AddAlias(aliases, matchedSection.Title);
        }

        return aliases.ToArray();
    }

    private static SectionRange? TryResolveSection(BookIndex bookIndex, string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        return SectionLocator.ResolveSectionByTitle(bookIndex, label);
    }

    private static SectionRange? TryResolveSectionFromAliases(BookIndex bookIndex, IEnumerable<string> aliases)
    {
        foreach (var alias in aliases)
        {
            var section = TryResolveSection(bookIndex, alias);
            if (section is not null)
            {
                return section;
            }
        }

        return null;
    }

    private static void AddAlias(ISet<string> aliases, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (aliases.Add(value))
        {
            var normalized = NormalizeChapterId(value);
            if (!string.IsNullOrEmpty(normalized))
            {
                aliases.Add(normalized);
            }
        }
    }

    private static string NormalizePath(string path)
    {
        var normalized = Path.GetFullPath(path);
        return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string NormalizeChapterId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }

    private static string DetermineChapterStem(string? supplied, FileInfo? audioFile, FileInfo? asrFile)
    {
        if (!string.IsNullOrWhiteSpace(supplied))
        {
            return supplied;
        }

        var candidate = audioFile ?? asrFile;
        if (candidate is not null)
        {
            var chopped = candidate.Name.Split('.');
            return Path.GetFileNameWithoutExtension(chopped[0]);
        }

        throw new ArgumentException("Chapter identifier must be provided.");
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
