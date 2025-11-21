namespace Ams.Web.Api.Payloads;

public sealed record ChapterSummaryResponse(
    string Id,
    string Path,
    bool HasHydrate,
    int SentenceCount,
    int ParagraphCount);
