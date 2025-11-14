using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Web.Dtos;
using Ams.Web.Services;

namespace Ams.Web.Mappers;

public static class ChapterMapper
{
    public static ChapterListItemDto ToListItem(ChapterSummary summary, bool reviewed)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return new ChapterListItemDto(
            summary.Id,
            summary.DisplayName,
            summary.SentenceCount,
            summary.DurationSeconds,
            summary.HydrateUpdatedUtc,
            reviewed);
    }

    public static ChapterDetailDto ToDetail(ChapterSummary summary, bool reviewed, IReadOnlyList<SentenceDto> sentences)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(sentences);

        return new ChapterDetailDto(
            summary.Id,
            summary.DisplayName,
            summary.DurationSeconds,
            summary.SentenceCount,
            reviewed,
            sentences);
    }

    public static SentenceDto ToSentenceDto(HydratedSentence sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        return new SentenceDto(
            sentence.Id,
            sentence.BookText,
            sentence.ScriptText,
            sentence.Status,
            sentence.Timing is null ? null : new TimingDto(sentence.Timing.StartSec, sentence.Timing.EndSec, sentence.Timing.Duration),
            sentence.Metrics,
            sentence.Diff,
            sentence.BookRange,
            sentence.ScriptRange);
    }
}
