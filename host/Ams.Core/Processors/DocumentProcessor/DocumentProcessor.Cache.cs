using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Runtime.Documents;

namespace Ams.Core.Processors.DocumentProcessor;

public static partial class DocumentProcessor
{
    public static IBookCache CreateBookCache(string? cacheDirectory = null)
        => new BookCache(cacheDirectory);

    public static Task<BookIndex?> TryLoadCachedIndexAsync(
        string sourceFile,
        string? cacheDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var cache = CreateBookCache(cacheDirectory);
        return cache.GetAsync(sourceFile, cancellationToken);
    }

    public static Task<bool> WriteCacheAsync(
        BookIndex bookIndex,
        string? cacheDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var cache = CreateBookCache(cacheDirectory);
        return cache.SetAsync(bookIndex, cancellationToken);
    }
}
