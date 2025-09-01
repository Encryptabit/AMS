using System.Text.Json.Serialization;

namespace Ams.Core;

/// <summary>
/// Canonical word token from the source, with exact text preserved.
/// Only positional indexes are stored. No timing/confidence/normalization.
/// </summary>
public record BookWord(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("wordIndex")] int WordIndex,
    [property: JsonPropertyName("sentenceIndex")] int SentenceIndex,
    [property: JsonPropertyName("paragraphIndex")] int ParagraphIndex
);

/// <summary>
/// Sentence range by word indices (inclusive).
/// </summary>
public record SentenceRange(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End
);

/// <summary>
/// Paragraph range by word indices (inclusive), with optional kind/style.
/// </summary>
public record ParagraphRange(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("style")] string Style
);

/// <summary>
/// Totals computed from arrays; estimatedDurationSec is WPM-based.
/// </summary>
public record BookTotals(
    [property: JsonPropertyName("words")] int Words,
    [property: JsonPropertyName("sentences")] int Sentences,
    [property: JsonPropertyName("paragraphs")] int Paragraphs,
    [property: JsonPropertyName("estimatedDurationSec")] double EstimatedDurationSec
);

/// <summary>
/// Canonical, slim BookIndex: exact text, structure ranges, and minimal metadata.
/// Deterministic ordering and no normalization at rest.
/// </summary>
public record BookIndex(
    [property: JsonPropertyName("sourceFile")] string SourceFile,
    [property: JsonPropertyName("sourceFileHash")] string SourceFileHash,
    [property: JsonPropertyName("indexedAt")] DateTime IndexedAt,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("totals")] BookTotals Totals,
    [property: JsonPropertyName("words")] BookWord[] Words,
    [property: JsonPropertyName("sentences")] SentenceRange[] Sentences,
    [property: JsonPropertyName("paragraphs")] ParagraphRange[] Paragraphs,
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
