using System.Collections.Generic;

namespace Ams.Web.Shared.Validation;

public sealed record ValidationChapterSummaryDto(
    string Id,
    string Path,
    bool HasHydrate,
    int SentenceCount,
    int ParagraphCount);

public sealed record ValidationOverviewDto(
    string BookId,
    int ChapterCount,
    int TotalSentences,
    int TotalParagraphs);

public sealed record ValidationReportDto(
    string ChapterId,
    string? HydratePath,
    int SentenceCount,
    int ParagraphCount);

public sealed record ChapterDetailDto(
    string ChapterId,
    bool HasHydrate,
    int SentenceCount,
    int ParagraphCount,
    IReadOnlyList<SentenceDto> Sentences,
    IReadOnlyList<ParagraphDto> Paragraphs,
    AudioAvailabilityDto Audio);

public sealed record SentenceDto(
    int Id,
    string Status,
    TimingDto? Timing,
    RangeDto BookRange,
    RangeDto? ScriptRange,
    string BookText,
    string ScriptText,
    MetricsDto Metrics,
    DiffDto? Diff);

public sealed record ParagraphDto(
    int Id,
    string Status,
    RangeDto BookRange,
    IReadOnlyList<int> SentenceIds,
    string BookText,
    ParagraphMetricsDto Metrics,
    DiffDto? Diff);

public sealed record AudioAvailabilityDto(
    bool Raw,
    bool Treated,
    bool Filtered);

public sealed record AudioExportRequest(double? Start, double? End, string? Variant);

public sealed record AudioExportResponse(
    int ErrorNumber,
    string FileName,
    string RelativePath);

public sealed record TimingDto(double StartSec, double EndSec, double Duration);

public sealed record RangeDto(int Start, int End);

public sealed record MetricsDto(double Wer, double Cer, double SpanWer, int MissingRuns, int ExtraRuns);

public sealed record ParagraphMetricsDto(double Wer, double Cer, double Coverage);

public sealed record DiffDto(IReadOnlyList<DiffOpDto> Ops, DiffStatsDto Stats);

public sealed record DiffOpDto(string Op, IReadOnlyList<string> Tokens);

public sealed record DiffStatsDto(
    int ReferenceTokens,
    int HypothesisTokens,
    int Matches,
    int Insertions,
    int Deletions);

public sealed record ReviewedStatusDto(bool Reviewed, string? TimestampUtc);

public sealed record ReviewedStatusResponse(Dictionary<string, ReviewedStatusDto> Chapters);