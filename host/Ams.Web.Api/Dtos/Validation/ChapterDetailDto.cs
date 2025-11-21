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
    double? StartSec,
    double? EndSec,
    string BookText,
    string ScriptText,
    double Wer,
    double Cer);

public sealed record ParagraphDto(
    int Id,
    string Status,
    IReadOnlyList<int> SentenceIds,
    double Wer);

public sealed record AudioAvailabilityDto(
    bool Raw,
    bool Treated,
    bool Filtered);

public sealed record AudioExportResponse(
    int ErrorNumber,
    string FileName,
    string RelativePath);

