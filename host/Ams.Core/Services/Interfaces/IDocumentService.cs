using Ams.Core.Runtime.Book;

namespace Ams.Core.Services.Interfaces;

public enum BookIndexCacheMode
{
    PreferCache = 0,
    Rebuild = 1,
    DisableCache = 2
}

public enum BookIndexCacheDisposition
{
    CacheHit = 0,
    CacheMiss = 1,
    ForceRefresh = 2,
    CacheDisabled = 3
}

public sealed record DocumentBuildIndexRequest
{
    public DocumentBuildIndexRequest(
        string sourceFile,
        BookIndexOptions? options = null,
        BookIndexCacheMode cacheMode = BookIndexCacheMode.PreferCache)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);

        SourceFile = sourceFile;
        Options = options;
        CacheMode = cacheMode;
    }

    public string SourceFile { get; }

    public BookIndexOptions? Options { get; }

    public BookIndexCacheMode CacheMode { get; }
}

public sealed record DocumentBuildIndexResult(BookIndex Index, BookIndexCacheDisposition CacheDisposition,
    bool PhonemesBackfilled)
{
    public bool WasRebuilt => CacheDisposition != BookIndexCacheDisposition.CacheHit;
}

public interface IDocumentService
{
    Task<DocumentBuildIndexResult> BuildIndexAsync(
        DocumentBuildIndexRequest request,
        CancellationToken cancellationToken = default);

    Task<BookIndex> PopulateMissingPhonemesAsync(
        BookIndex index,
        CancellationToken cancellationToken = default);

    Task<BookIndex> ParseAndPopulatePhonemesAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default);
}
