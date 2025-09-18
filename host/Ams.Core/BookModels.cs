using System.Text.Json.Serialization;

namespace Ams.Core;

/// <summary>
/// Aggregate counts for the canonical book index. Matches CORRECT_RESULTS layout.
/// </summary>
public record BookTotals(
    [property: JsonPropertyName("words")] int Words,
    [property: JsonPropertyName("sentences")] int Sentences,
    [property: JsonPropertyName("paragraphs")] int Paragraphs,
    [property: JsonPropertyName("estimatedDurationSec")] double EstimatedDurationSec
);

/// <summary>
/// Canonical word token from the source text. Timing metadata is omitted until
/// refined artifacts hydrate it back in.
/// </summary>
public record BookWord(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("wordIndex")] int WordIndex,
    [property: JsonPropertyName("sentenceIndex")] int SentenceIndex,
    [property: JsonPropertyName("paragraphIndex")] int ParagraphIndex,
    [property: JsonPropertyName("sectionIndex")] int SectionIndex = -1
);

/// <summary>
/// Sentence slice expressed in word offsets.
/// </summary>
public record BookSentence(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End
);

/// <summary>
/// Paragraph slice expressed in word offsets, tracked with style metadata for downstream heuristics.
/// </summary>
public record BookParagraph(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("style")] string Style
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
/// Canonical book index schema identical to CORRECT_RESULTS artifacts.
/// </summary>
public record BookIndex(
    [property: JsonPropertyName("sourceFile")] string SourceFile,
    [property: JsonPropertyName("sourceFileHash")] string SourceFileHash,
    [property: JsonPropertyName("indexedAt")] DateTime IndexedAt,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("totals")] BookTotals Totals,
    [property: JsonPropertyName("words")] BookWord[] Words,
    [property: JsonPropertyName("sentences")] BookSentence[] Sentences,
    [property: JsonPropertyName("paragraphs")] BookParagraph[] Paragraphs,
    [property: JsonPropertyName("sections")] SectionRange[]? Sections,
    [property: JsonPropertyName("buildWarnings")] string[]? BuildWarnings = null
);

/// <summary>
/// Options relevant to canonical indexing.
/// </summary>
public record BookIndexOptions
{
    /// <summary>Average words per minute for duration estimation (default: 200)</summary>
    public double AverageWpm { get; init; } = 200.0;
}
