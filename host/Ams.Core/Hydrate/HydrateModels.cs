using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ams.Core.Artifacts;
using Ams.Core.Alignment.Tx;

namespace Ams.Core.Hydrate;

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
    [property: JsonPropertyName("score")] double Score);

public sealed record HydratedSentence(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bookRange")] HydratedRange BookRange,
    [property: JsonPropertyName("scriptRange")] HydratedScriptRange? ScriptRange,
    [property: JsonPropertyName("bookText")] string BookText,
    [property: JsonPropertyName("scriptText")] string ScriptText,
    [property: JsonPropertyName("metrics")] SentenceMetrics Metrics,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timing")] TimingRange? Timing);

public sealed record HydratedParagraph(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bookRange")] HydratedRange BookRange,
    [property: JsonPropertyName("sentenceIds")] IReadOnlyList<int> SentenceIds,
    [property: JsonPropertyName("bookText")] string BookText,
    [property: JsonPropertyName("metrics")] ParagraphMetrics Metrics,
    [property: JsonPropertyName("status")] string Status);

public sealed record HydratedRange(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End);

public sealed record HydratedScriptRange(
    [property: JsonPropertyName("start")] int? Start,
    [property: JsonPropertyName("end")] int? End);
