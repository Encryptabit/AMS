using System.Text.Json.Serialization;

namespace Ams.Core;

/// <summary>
/// Canonical word token from the source text. Timing metadata stays optional
/// to support future hydrated indexes from aligned audio.
/// </summary>
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
/// Sentence or paragraph slice expressed in word offsets. Keeps text as it
/// appeared in the original document to remain human friendly.
/// </summary>
public record BookSegment(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("wordStartIndex")] int WordStartIndex,
    [property: JsonPropertyName("wordEndIndex")] int WordEndIndex,
    [property: JsonPropertyName("startTime")] double? StartTime = null,
    [property: JsonPropertyName("endTime")] double? EndTime = null,
    [property: JsonPropertyName("confidence")] double? Confidence = null
);

/// <summary>
/// Section/chapter range derived from paragraph heading styles (e.g., Heading 1).
/// Enables processing chapters individually without reparsing.
/// </summary>
public record SectionRange(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("startWord")] int StartWord,
    [property: JsonPropertyName("endWord")] int EndWord,
    [property: JsonPropertyName("startParagraph")] int StartParagraph,
    [property: JsonPropertyName("endParagraph")] int EndParagraph
);

/// <summary>
/// Legacy-friendly book index schema used by downstream tooling and tests.
/// Totals stay as top-level properties and segments collapse sentences and
/// paragraphs into a single collection for easy inspection.
/// </summary>
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
    [property: JsonPropertyName("segments")] BookSegment[] Segments,
    [property: JsonPropertyName("sections"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SectionRange[]? Sections = null
);

/// <summary>
/// Options relevant to canonical indexing.
/// </summary>
public record BookIndexOptions
{
    /// <summary>Average words per minute for duration estimation (default: 200)</summary>
    public double AverageWpm { get; init; } = 200.0;
}
