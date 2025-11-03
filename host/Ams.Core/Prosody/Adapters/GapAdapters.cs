using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Audio;

namespace Ams.Core.Prosody.Adapters;

public static class GapAdapters
{
    public static PauseSpan ToPauseSpan(
        this RoomtonePlanGap gap,
        PauseClass pauseClass,
        bool crossesParagraph,
        bool crossesChapterHead,
        bool hasGapHint = false)
    {
        if (gap is null) throw new ArgumentNullException(nameof(gap));

        return new PauseSpan(
            gap.PreviousSentenceId ?? -1,
            gap.NextSentenceId ?? -1,
            gap.StartSec,
            gap.EndSec,
            gap.DurationSec,
            pauseClass,
            hasGapHint,
            crossesParagraph,
            crossesChapterHead,
            PauseProvenance.TimelineGap);
    }

    public static IReadOnlyList<PauseSpan> BuildFromTimeline(
        IReadOnlyList<SentenceTimelineEntry> timeline,
        Func<SentenceTimelineEntry, SentenceTimelineEntry, PauseClass> classifier,
        Func<int, int, bool> isParagraphBoundary,
        Func<int, int, bool> isChapterHeadBoundary,
        Func<int, int, bool>? hasGapHint = null)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (classifier is null) throw new ArgumentNullException(nameof(classifier));
        if (isParagraphBoundary is null) throw new ArgumentNullException(nameof(isParagraphBoundary));
        if (isChapterHeadBoundary is null) throw new ArgumentNullException(nameof(isChapterHeadBoundary));

        var spans = new List<PauseSpan>(Math.Max(0, timeline.Count - 1));
        if (timeline.Count < 2)
        {
            return spans;
        }

        var ordered = timeline.OrderBy(entry => entry.Timing.StartSec).ToList();
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            var left = ordered[i];
            var right = ordered[i + 1];

            double start = left.Timing.EndSec;
            double end = right.Timing.StartSec;
            if (end <= start)
            {
                continue;
            }

            var pauseClass = classifier(left, right);
            bool crossesParagraph = isParagraphBoundary(left.SentenceId, right.SentenceId);
            bool crossesChapter = isChapterHeadBoundary(left.SentenceId, right.SentenceId);
            bool hint = hasGapHint?.Invoke(left.SentenceId, right.SentenceId) ?? false;

            spans.Add(new PauseSpan(
                left.SentenceId,
                right.SentenceId,
                start,
                end,
                end - start,
                pauseClass,
                hint,
                crossesParagraph,
                crossesChapter,
                PauseProvenance.TimelineGap));
        }

        return spans;
    }
}
