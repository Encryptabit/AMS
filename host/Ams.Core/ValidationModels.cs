using System.Text.Json.Serialization;

namespace Ams.Core.Validation;

public enum ValidationLevel
{
    Word,
    Segment,
    Document
}

public enum FindingType
{
    Missing,
    Extra,
    Substitution,
    Timing
}

public record ValidationFinding(
    FindingType Type,
    ValidationLevel Level,
    double? StartTime = null,
    double? EndTime = null,
    string? Expected = null,
    string? Actual = null,
    double Cost = 0.0,
    string? Context = null
);

public record ValidationReport(
    [property: JsonPropertyName("audioFile")] string AudioFile,
    [property: JsonPropertyName("scriptFile")] string ScriptFile,
    [property: JsonPropertyName("asrFile")] string AsrFile,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("wordErrorRate")] double WordErrorRate,
    [property: JsonPropertyName("characterErrorRate")] double CharacterErrorRate,
    [property: JsonPropertyName("totalWords")] int TotalWords,
    [property: JsonPropertyName("correctWords")] int CorrectWords,
    [property: JsonPropertyName("substitutions")] int Substitutions,
    [property: JsonPropertyName("insertions")] int Insertions,
    [property: JsonPropertyName("deletions")] int Deletions,
    [property: JsonPropertyName("findings")] ValidationFinding[] Findings,
    [property: JsonPropertyName("segmentStats")] SegmentStats[] SegmentStats
);

public record SegmentStats(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("startTime")] double StartTime,
    [property: JsonPropertyName("endTime")] double EndTime,
    [property: JsonPropertyName("expectedText")] string ExpectedText,
    [property: JsonPropertyName("actualText")] string ActualText,
    [property: JsonPropertyName("wordErrorRate")] double WordErrorRate,
    [property: JsonPropertyName("confidence")] double? Confidence = null
);