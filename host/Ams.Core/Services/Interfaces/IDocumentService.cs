using Ams.Core.Runtime.Book;

namespace Ams.Core.Services.Interfaces;

public interface IDocumentService
{
    Task<BookIndex> BuildIndexAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<BookIndex> PopulateMissingPhonemesAsync(
        BookIndex index,
        CancellationToken cancellationToken = default);

    Task<BookIndex> ParseAndPopulatePhonemesAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default);
}