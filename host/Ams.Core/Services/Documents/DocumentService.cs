using Ams.Core.Processors.DocumentProcessor;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly IPronunciationProvider? _pronunciationProvider;
    private readonly IBookCache? _cache;

    public DocumentService(
        IPronunciationProvider? pronunciationProvider = null,
        IBookCache? cache = null)
    {
        _pronunciationProvider = pronunciationProvider;
        _cache = cache;
    }

    public async Task<DocumentBuildIndexResult> BuildIndexAsync(
        DocumentBuildIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sourceFile = request.SourceFile;

        if (!DocumentProcessor.CanParseBook(sourceFile))
        {
            var supportedExts = string.Join(", ", DocumentProcessor.GetSupportedBookExtensions());
            throw new InvalidOperationException($"Unsupported file format. Supported formats: {supportedExts}");
        }

        if (request.CacheMode == BookIndexCacheMode.PreferCache && _cache is not null)
        {
            var cached = await _cache.GetAsync(sourceFile, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
            {
                var (cachedIndex, phonemesBackfilled) = await EnsurePhonemesAsync(cached, cancellationToken)
                    .ConfigureAwait(false);

                if (phonemesBackfilled)
                {
                    await _cache.SetAsync(cachedIndex, cancellationToken).ConfigureAwait(false);
                }

                return new DocumentBuildIndexResult(
                    cachedIndex,
                    BookIndexCacheDisposition.CacheHit,
                    phonemesBackfilled);
            }
        }

        var parseResult = await DocumentProcessor.ParseBookAsync(sourceFile, cancellationToken).ConfigureAwait(false);
        var index = await DocumentProcessor
            .BuildBookIndexAsync(
                parseResult,
                sourceFile,
                request.Options,
                _pronunciationProvider,
                cancellationToken)
            .ConfigureAwait(false);

        if (_cache is not null && request.CacheMode != BookIndexCacheMode.DisableCache)
        {
            await _cache.SetAsync(index, cancellationToken).ConfigureAwait(false);
        }

        return new DocumentBuildIndexResult(index, MapDisposition(request.CacheMode), PhonemesBackfilled: false);
    }

    public Task<BookIndex> PopulateMissingPhonemesAsync(
        BookIndex index,
        CancellationToken cancellationToken = default)
    {
        if (_pronunciationProvider == null)
        {
            throw new InvalidOperationException(
                "Pronunciation provider was not supplied when constructing DocumentService.");
        }

        return DocumentProcessor.PopulateMissingPhonemesAsync(index, _pronunciationProvider, cancellationToken);
    }

    public async Task<BookIndex> ParseAndPopulatePhonemesAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_pronunciationProvider == null)
        {
            throw new InvalidOperationException(
                "Pronunciation provider was not supplied when constructing DocumentService.");
        }

        var result = await BuildIndexAsync(
                new DocumentBuildIndexRequest(sourceFile, options, BookIndexCacheMode.Rebuild),
                cancellationToken)
            .ConfigureAwait(false);

        return result.Index;
    }

    private async Task<(BookIndex Index, bool PhonemesBackfilled)> EnsurePhonemesAsync(
        BookIndex index,
        CancellationToken cancellationToken)
    {
        if (_pronunciationProvider == null)
        {
            return (index, false);
        }

        var missingBefore = CountMissingPhonemes(index);
        if (missingBefore == 0)
        {
            return (index, false);
        }

        var enriched = await DocumentProcessor
            .PopulateMissingPhonemesAsync(index, _pronunciationProvider, cancellationToken)
            .ConfigureAwait(false);

        var missingAfter = CountMissingPhonemes(enriched);
        if (missingAfter < missingBefore)
        {
            return (enriched, true);
        }

        return (index, false);
    }

    private static int CountMissingPhonemes(BookIndex index)
    {
        return index.Words.Count(word =>
            word.Phonemes is not { Length: > 0 } &&
            !string.IsNullOrEmpty(PronunciationHelper.NormalizeForLookup(word.Text)));
    }

    private static BookIndexCacheDisposition MapDisposition(BookIndexCacheMode cacheMode)
    {
        return cacheMode switch
        {
            BookIndexCacheMode.Rebuild => BookIndexCacheDisposition.ForceRefresh,
            BookIndexCacheMode.DisableCache => BookIndexCacheDisposition.CacheDisabled,
            _ => BookIndexCacheDisposition.CacheMiss
        };
    }
}
