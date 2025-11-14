using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Artifacts;

namespace Ams.Web.Dtos;

public sealed record ChapterListItemDto(
    string Id,
    string Name,
    int SentenceCount,
    double DurationSeconds,
    DateTime HydrateUpdatedUtc,
    bool Reviewed);

public sealed record ChapterDetailDto(
    string Id,
    string Name,
    double DurationSeconds,
    int SentenceCount,
    bool Reviewed,
    IReadOnlyList<SentenceDto> Sentences);

public sealed record SentenceDto(
    int Id,
    string BookText,
    string ScriptText,
    string Status,
    TimingDto? Timing,
    SentenceMetrics Metrics,
    HydratedDiff? Diff,
    HydratedRange BookRange,
    HydratedScriptRange? ScriptRange);

public sealed record TimingDto(double Start, double End, double Duration);

public sealed record SentenceExportResponse(
    string ChapterId,
    int SentenceId,
    string SegmentPath,
    string WorkbookPath,
    int RowNumber,
    int ErrorNumber,
    string ErrorType,
    double StartSeconds,
    double EndSeconds);
