using Ams.Core.Runtime.Book;

namespace Ams.Core.Processors.DocumentProcessor;

public static partial class DocumentProcessor
{
    public static bool CanParseBook(string sourceFile)
    {
        var parser = new BookParser();
        return parser.CanParse(sourceFile);
    }

    public static IReadOnlyCollection<string> GetSupportedBookExtensions()
    {
        var parser = new BookParser();
        return parser.SupportedExtensions;
    }

    public static Task<BookParseResult> ParseBookAsync(
        string sourceFile,
        CancellationToken cancellationToken = default)
    {
        var parser = new BookParser();
        return parser.ParseAsync(sourceFile, cancellationToken);
    }

    public static Task<BookIndex> BuildBookIndexAsync(
        BookParseResult parseResult,
        string sourceFile,
        BookIndexOptions? options = null,
        IPronunciationProvider? pronunciationProvider = null,
        CancellationToken cancellationToken = default)
    {
        var indexer = new BookIndexer(pronunciationProvider);
        return indexer.CreateIndexAsync(parseResult, sourceFile, options, cancellationToken);
    }

    public static async Task<BookIndex> BuildBookIndexAsync(
        string sourceFile,
        BookIndexOptions? options = null,
        IPronunciationProvider? pronunciationProvider = null,
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseBookAsync(sourceFile, cancellationToken).ConfigureAwait(false);
        return await BuildBookIndexAsync(parseResult, sourceFile, options, pronunciationProvider, cancellationToken).ConfigureAwait(false);
    }
}
