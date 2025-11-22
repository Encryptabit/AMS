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

    public async Task<BookIndex> BuildIndexAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);

        if (!forceRefresh && _cache != null)
        {
            var cached = await _cache.GetAsync(sourceFile, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }
        }

        var parseResult = await DocumentProcessor.ParseBookAsync(sourceFile, cancellationToken).ConfigureAwait(false);
        var index = await DocumentProcessor
            .BuildBookIndexAsync(parseResult, sourceFile, options, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (_pronunciationProvider != null)
        {
            index = await DocumentProcessor
                .PopulateMissingPhonemesAsync(index, _pronunciationProvider, cancellationToken).ConfigureAwait(false);
        }

        if (_cache != null)
        {
            await _cache.SetAsync(index, cancellationToken).ConfigureAwait(false);
        }

        return index;
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

    public Task<BookIndex> ParseAndPopulatePhonemesAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_pronunciationProvider == null)
        {
            throw new InvalidOperationException(
                "Pronunciation provider was not supplied when constructing DocumentService.");
        }

        return BuildIndexAsync(sourceFile, options, forceRefresh: true, cancellationToken);
    }
}