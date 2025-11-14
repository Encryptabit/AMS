using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ams.Core.Artifacts;
using Ams.Core.Processors.Alignment.Tx;

namespace Ams.Core.Artifacts.Hydrate;

public sealed record HydratedTranscript(
    [property: JsonPropertyName("audioPath")] string AudioPath,
    [property: JsonPropertyName("scriptPath")] string ScriptPath,
    [property: JsonPropertyName("bookIndexPath")] string BookIndexPath,
    [property: JsonPropertyName("createdAtUtc")] DateTime CreatedAtUtc,
    [property: JsonPropertyName("normalizationVersion")] string? NormalizationVersion,
    [property: JsonPropertyName("words")] IReadOnlyList<HydratedWord> Words,
    [property: JsonPropertyName("sentences")] IReadOnlyList<HydratedSentence> Sentences,
    [property: JsonPropertyName("paragraphs")] IReadOnlyList<HydratedParagraph> Paragraphs);

public sealed record HydratedWord(
    [property: JsonPropertyName("bookIdx")] int? BookIdx,
    [property: JsonPropertyName("asrIdx")] int? AsrIdx,
    [property: JsonPropertyName("bookWord")] string? BookWord,
    [property: JsonPropertyName("asrWord")] string? AsrWord,
    [property: JsonPropertyName("op")] string Op,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("score")] double Score)
{
    [JsonPropertyName("startSec")]
    public double? StartSec { get; init; }

    [JsonPropertyName("endSec")]
    public double? EndSec { get; init; }

    [JsonPropertyName("durationSec")]
    public double? DurationSec { get; init; }
}

public sealed record HydratedSentence(
    int Id,
    HydratedRange BookRange,
    HydratedScriptRange? ScriptRange,
    string BookText,
    string ScriptText,
    SentenceMetrics Metrics,
    string Status,
    TimingRange? Timing,
    HydratedDiff? Diff)
{
    [JsonPropertyName("timing")]
    public TimingRange? Timing { get; set; } = Timing;

    [JsonPropertyName("id")] public int Id { get; init; } = Id;

    [JsonPropertyName("bookRange")]
    public HydratedRange BookRange { get; init; } = BookRange;

    [JsonPropertyName("scriptRange")]
    public HydratedScriptRange? ScriptRange { get; init; } = ScriptRange;

    [JsonPropertyName("bookText")]
    public string BookText { get; init; } = BookText;

    [JsonPropertyName("scriptText")]
    public string ScriptText { get; init; } = ScriptText;

    [JsonPropertyName("metrics")]
    public SentenceMetrics Metrics { get; init; } = Metrics;

    [JsonPropertyName("status")] public string Status { get; init; } = Status;
    [JsonPropertyName("diff")] public HydratedDiff? Diff { get; init; } = Diff;
}

public sealed record HydratedDiff(
    [property: JsonPropertyName("ops")] IReadOnlyList<HydratedDiffOp> Ops,
    [property: JsonPropertyName("stats")] HydratedDiffStats Stats);

public sealed record HydratedDiffOp(
    [property: JsonPropertyName("op")] string Operation,
    [property: JsonPropertyName("tokens")] IReadOnlyList<string> Tokens);

public sealed record HydratedDiffStats(
    [property: JsonPropertyName("referenceTokens")] int ReferenceTokens,
    [property: JsonPropertyName("hypothesisTokens")] int HypothesisTokens,
    [property: JsonPropertyName("matches")] int Matches,
    [property: JsonPropertyName("insertions")] int Insertions,
    [property: JsonPropertyName("deletions")] int Deletions);

public sealed record HydratedParagraph(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bookRange")] HydratedRange BookRange,
    [property: JsonPropertyName("sentenceIds")] IReadOnlyList<int> SentenceIds,
    [property: JsonPropertyName("bookText")] string BookText,
    [property: JsonPropertyName("metrics")] ParagraphMetrics Metrics,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("diff")] HydratedDiff? Diff);

public sealed record HydratedRange(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End);

public sealed record HydratedScriptRange(
    [property: JsonPropertyName("start")] int? Start,
    [property: JsonPropertyName("end")] int? End);
