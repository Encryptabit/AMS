using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ams.Core;

namespace Ams.Core.Pipeline;

public sealed record RefinedTokenDetail(
    [property: JsonPropertyName("startTime")] double StartTime,
    [property: JsonPropertyName("duration")] double Duration,
    [property: JsonPropertyName("word")] string Word,
    [property: JsonPropertyName("originalStartTime")] double OriginalStartTime,
    [property: JsonPropertyName("originalDuration")] double OriginalDuration,
    [property: JsonPropertyName("confidence")] double Confidence);

public sealed record RefinementDetails(
    [property: JsonPropertyName("modelVersion")] string ModelVersion,
    [property: JsonPropertyName("refinedTokens")] IReadOnlyList<RefinedTokenDetail> RefinedTokens,
    [property: JsonPropertyName("detectedSilences")] IReadOnlyList<SilenceInfo> DetectedSilences,
    [property: JsonPropertyName("totalDurationSeconds")] double TotalDurationSeconds);
