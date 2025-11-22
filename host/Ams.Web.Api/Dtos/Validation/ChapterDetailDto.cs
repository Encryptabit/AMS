namespace Ams.Web.Api.Dtos.Validation;

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

public sealed record DiffStatsDto(int ReferenceTokens, int HypothesisTokens, int Matches, int Insertions, int Deletions);
