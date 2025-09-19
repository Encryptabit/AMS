namespace Ams.Core.Book;

/// <summary>
/// Interface for parsing book content from various file formats.
/// Implementations should handle format-specific parsing while providing
/// a consistent text extraction API.
/// </summary>
public interface IBookParser
{
    /// <summary>
    /// Gets the file extensions supported by this parser (e.g., ".docx", ".txt").
    /// </summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }
    
    /// <summary>
    /// Determines if this parser can handle the specified file.
    /// </summary>
    /// <param name="filePath">Path to the file to check</param>
    /// <returns>True if this parser supports the file format</returns>
    bool CanParse(string filePath);
    
    /// <summary>
    /// Extracts raw text content from the specified file.
    /// This method performs format-specific parsing but does not
    /// perform any text processing or indexing.
    /// </summary>
    /// <param name="filePath">Path to the file to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content and optional metadata</returns>
    /// <exception cref="FileNotFoundException">File does not exist</exception>
    /// <exception cref="InvalidOperationException">File format not supported</exception>
    /// <exception cref="IOException">File could not be read</exception>
    Task<BookParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of book parsing operation containing extracted text and metadata.
/// </summary>
/// <param name="Text">The extracted text content</param>
/// <param name="Title">Document title if available</param>
/// <param name="Author">Document author if available</param>
/// <param name="Metadata">Additional document properties</param>
public record ParsedParagraph(string Text, string? Style = null, string? Kind = null);

public record BookParseResult(
    string Text,
    string? Title = null,
    string? Author = null,
    IReadOnlyDictionary<string, object>? Metadata = null,
    IReadOnlyList<ParsedParagraph>? Paragraphs = null
);

/// <summary>
/// Interface for processing parsed book text into indexed structures.
/// Handles text segmentation, word extraction, and timing estimation.
/// </summary>
public interface IBookIndexer
{
    /// <summary>
    /// Creates a complete book index from parsed text content.
    /// This includes word tokenization, sentence/paragraph segmentation,
    /// and timing estimation for audio alignment.
    /// </summary>
    /// <param name="parseResult">Result from book parsing operation</param>
    /// <param name="sourceFile">Path to the original source file</param>
    /// <param name="options">Indexing configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete book index with timing metadata</returns>
    /// <exception cref="ArgumentException">Invalid parse result or options</exception>
    Task<BookIndex> CreateIndexAsync(
        BookParseResult parseResult,
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for caching book indexes with file integrity validation.
/// Provides persistent storage for processed book data with automatic
/// invalidation when source files change.
/// </summary>
public interface IBookCache
{
    /// <summary>
    /// Retrieves a cached book index for the specified source file.
    /// Returns null if no valid cache exists or if the source file has changed.
    /// </summary>
    /// <param name="sourceFile">Path to the original source file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached book index or null if not found/invalid</returns>
    /// <exception cref="IOException">Cache file could not be read</exception>
    Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stores a book index in the cache with file integrity validation.
    /// The cache entry will be associated with the SHA256 hash of the source file.
    /// </summary>
    /// <param name="bookIndex">Book index to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successfully cached</returns>
    /// <exception cref="ArgumentException">Invalid book index</exception>
    /// <exception cref="IOException">Cache file could not be written</exception>
    Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a cached book index for the specified source file.
    /// </summary>
    /// <param name="sourceFile">Path to the original source file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cache entry was removed</returns>
    Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates that a cached book index is still current for its source file.
    /// Checks file modification time and content hash.
    /// </summary>
    /// <param name="bookIndex">Book index to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the cache entry is still valid</returns>
    Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clears all cached book indexes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when book parsing operations fail.
/// </summary>
public class BookParseException : Exception
{
    public BookParseException(string message) : base(message) { }
    public BookParseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when book indexing operations fail.
/// </summary>
public class BookIndexException : Exception
{
    public BookIndexException(string message) : base(message) { }
    public BookIndexException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when book cache operations fail.
/// </summary>
public class BookCacheException : Exception
{
    public BookCacheException(string message) : base(message) { }
    public BookCacheException(string message, Exception innerException) : base(message, innerException) { }
}

