using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts;

// (1) Enums and POCOs for deterministic transcript indices

public enum AlignOp
{
    Match,
    Sub,
    Ins,
    Del
}

public sealed record WordAlign(int? BookIdx, int? AsrIdx, AlignOp Op, string Reason, double Score);

public sealed record SentenceMetrics(double Wer, double Cer, double SpanWer, int MissingRuns, int ExtraRuns);

// Use explicit range records instead of tuples for stable, readable JSON
public sealed record IntRange(int Start, int End);

public sealed record ScriptRange(int? Start, int? End);

public sealed record SentenceAlign(
    int Id,
    IntRange BookRange,
    ScriptRange? ScriptRange,
    TimingRange Timing,
    SentenceMetrics Metrics,
    string Status)
{
    [JsonConstructor]
    public SentenceAlign(int id, IntRange bookRange, ScriptRange? scriptRange, SentenceMetrics metrics, string status)
        : this(id, bookRange, scriptRange, TimingRange.Empty, metrics, status)
    {
    }
}

public sealed record ParagraphMetrics(double Wer, double Cer, double Coverage);

public sealed record ParagraphAlign(
    int Id,
    IntRange BookRange,
    IReadOnlyList<int> SentenceIds,
    ParagraphMetrics Metrics,
    string Status);

public sealed record TranscriptIndex(
    string AudioPath,
    string ScriptPath,
    string BookIndexPath,
    DateTime CreatedAtUtc,
    string NormalizationVersion,
    IReadOnlyList<WordAlign> Words,
    IReadOnlyList<SentenceAlign> Sentences,
    IReadOnlyList<ParagraphAlign> Paragraphs);