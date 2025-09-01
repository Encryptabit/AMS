using System.Text.Json.Serialization;

namespace Ams.Core;

/// <summary>
/// Represents a single word in a book with its position and timing metadata.
/// </summary>
/// <param name="Text">The word text (normalized)</param>
/// <param name="WordIndex">Zero-based index of this word in the book</param>
/// <param name="SentenceIndex">Zero-based index of the sentence containing this word</param>
/// <param name="ParagraphIndex">Zero-based index of the paragraph containing this word</param>
/// <param name="StartTime">Estimated start time in seconds (for audio alignment)</param>
/// <param name="EndTime">Estimated end time in seconds (for audio alignment)</param>
/// <param name="Confidence">Optional confidence score for timing estimates (0.0-1.0)</param>
public record BookWord(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("wordIndex")] int WordIndex,
    [property: JsonPropertyName("sentenceIndex")] int SentenceIndex,
    [property: JsonPropertyName("paragraphIndex")] int ParagraphIndex,
    [property: JsonPropertyName("startTime")] double? StartTime = null,
    [property: JsonPropertyName("endTime")] double? EndTime = null,
    [property: JsonPropertyName("confidence")] double? Confidence = null
);

/// <summary>
/// Represents a segment (sentence or paragraph) in a book with timing metadata.
/// Granularity determines whether this represents a sentence or paragraph boundary.
/// </summary>
/// <param name="Text">The complete text of this segment</param>
/// <param name="Type">The type of segment (sentence or paragraph)</param>
/// <param name="Index">Zero-based index of this segment in the book</param>
/// <param name="WordStartIndex">Index of the first word in this segment</param>
/// <param name="WordEndIndex">Index of the last word in this segment (inclusive)</param>
/// <param name="StartTime">Estimated start time in seconds</param>
/// <param name="EndTime">Estimated end time in seconds</param>
/// <param name="Confidence">Optional confidence score for timing estimates (0.0-1.0)</param>
public record BookSegment(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("type")] BookSegmentType Type,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("wordStartIndex")] int WordStartIndex,
    [property: JsonPropertyName("wordEndIndex")] int WordEndIndex,
    [property: JsonPropertyName("startTime")] double? StartTime = null,
    [property: JsonPropertyName("endTime")] double? EndTime = null,
    [property: JsonPropertyName("confidence")] double? Confidence = null
);

/// <summary>
/// Defines the type of book segment for proper indexing and alignment.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookSegmentType
{
    /// <summary>A sentence boundary</summary>
    Sentence,
    
    /// <summary>A paragraph boundary</summary>
    Paragraph
}

/// <summary>
/// Complete indexed representation of a book with metadata and timing information.
/// This is the primary cache structure for book processing results.
/// </summary>
/// <param name="SourceFile">Original source file path</param>
/// <param name="SourceFileHash">SHA256 hash of the source file content</param>
/// <param name="IndexedAt">UTC timestamp when this index was created</param>
/// <param name="Title">Book title (extracted from document metadata if available)</param>
/// <param name="Author">Book author (extracted from document metadata if available)</param>
/// <param name="TotalWords">Total number of words in the book</param>
/// <param name="TotalSentences">Total number of sentences in the book</param>
/// <param name="TotalParagraphs">Total number of paragraphs in the book</param>
/// <param name="EstimatedDuration">Estimated reading/audio duration in seconds</param>
/// <param name="Words">Array of all words with their metadata</param>
/// <param name="Segments">Array of all segments (sentences and paragraphs) with metadata</param>
public record BookIndex(
    [property: JsonPropertyName("sourceFile")] string SourceFile,
    [property: JsonPropertyName("sourceFileHash")] string SourceFileHash,
    [property: JsonPropertyName("indexedAt")] DateTime IndexedAt,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("totalWords")] int TotalWords,
    [property: JsonPropertyName("totalSentences")] int TotalSentences,
    [property: JsonPropertyName("totalParagraphs")] int TotalParagraphs,
    [property: JsonPropertyName("estimatedDuration")] double EstimatedDuration,
    [property: JsonPropertyName("words")] BookWord[] Words,
    [property: JsonPropertyName("segments")] BookSegment[] Segments
);

/// <summary>
/// Configuration options for book parsing and indexing operations.
/// </summary>
public record BookIndexOptions
{
    /// <summary>Average words per minute for duration estimation (default: 200)</summary>
    public double AverageWpm { get; init; } = 200.0;
    
    /// <summary>Whether to extract and preserve document metadata (title, author)</summary>
    public bool ExtractMetadata { get; init; } = true;
    
    /// <summary>Whether to normalize text (remove extra whitespace, standardize punctuation)</summary>
    public bool NormalizeText { get; init; } = true;
    
    /// <summary>Whether to split on paragraph boundaries in addition to sentences</summary>
    public bool IncludeParagraphSegments { get; init; } = true;
    
    /// <summary>Minimum characters required for a valid sentence (default: 5)</summary>
    public int MinimumSentenceLength { get; init; } = 5;
    
    /// <summary>Minimum words required for a valid paragraph (default: 2)</summary>
    public int MinimumParagraphWords { get; init; } = 2;
}