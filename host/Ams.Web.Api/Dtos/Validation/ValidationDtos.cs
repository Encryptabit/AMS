namespace Ams.Web.Api.Dtos.Validation;

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

public sealed record ReviewedStatusDto(
    bool Reviewed,
    string? TimestampUtc);
