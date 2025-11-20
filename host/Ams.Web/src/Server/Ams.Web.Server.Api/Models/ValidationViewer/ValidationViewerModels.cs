using System.Text.Json.Serialization;
using Ams.Core.Artifacts.Hydrate;

namespace Ams.Web.Server.Api.Models.ValidationViewer;

public sealed class ValidationViewerOptions
{
    /// <summary>Root directory containing chapter folders and book-index.json.</summary>
    public string BookRoot { get; set; } = string.Empty;

    /// <summary>Optional path to a CRX Excel template copied when the first CRX file is created.</summary>
    public string? CrxTemplatePath { get; set; }
        = null;

    /// <summary>Name of the CRX output directory relative to BookRoot.</summary>
    public string CrxDirectoryName { get; set; } = "CRX";

    /// <summary>Default error type used when callers do not provide one.</summary>
    public string DefaultErrorType { get; set; } = "MR";

    /// <summary>Optional override for where reviewed-status.json is stored.</summary>
    public string? ReviewedStatusPath { get; set; }
        = null;
}

public sealed record ValidationChapterMetrics(
    int SentenceCount,
    int SentenceFlagged,
    string SentenceAvgWer,
    int ParagraphCount,
    int ParagraphFlagged,
    string ParagraphAvgWer);

public sealed record ValidationChapterSummary(
    string Name,
    string Path,
    ValidationChapterMetrics Metrics);

public sealed record ValidationOverviewResponse(
    string BookName,
    int ChapterCount,
    int TotalSentences,
    int TotalFlaggedSentences,
    string AvgSentenceWer,
    int TotalParagraphs,
    int TotalFlaggedParagraphs,
    string AvgParagraphWer,
    IReadOnlyList<ValidationChapterSummary> Chapters);

public sealed record ValidationReportStats(
    string SentenceCount,
    string AvgWer,
    string MaxWer,
    string FlaggedCount,
    string ParagraphCount,
    string ParagraphAvgWer,
    string AvgCoverage);

public sealed record ValidationSentenceResponse(
    int Id,
    string Wer,
    string Cer,
    string Status,
    string BookRange,
    string ScriptRange,
    string Timing,
    string BookText,
    string ScriptText,
    string Excerpt,
    HydratedDiff? Diff,
    double? StartTime,
    double? EndTime,
    int? BookRangeStart,
    int? BookRangeEnd,
    int? ParagraphId,
    IReadOnlyList<Dictionary<string, string?>>? WordOps);

public sealed record ValidationParagraphResponse(
    int Id,
    string Wer,
    string Coverage,
    string Status,
    string BookRange,
    string? Timing,
    IReadOnlyList<int> SentenceIds,
    IReadOnlyList<int> FlaggedSentenceIds,
    string BookText,
    double? StartTime,
    double? EndTime);

public sealed record ValidationReportResponse(
    string ChapterName,
    string AudioPath,
    string ScriptPath,
    string BookIndex,
    string Created,
    ValidationReportStats Stats,
    IReadOnlyList<ValidationSentenceResponse> Sentences,
    IReadOnlyList<ValidationParagraphResponse> Paragraphs);

public sealed record ReviewedStatusDto(
    [property: JsonPropertyName("reviewed")] bool Reviewed,
    [property: JsonPropertyName("timestamp")] string Timestamp);

