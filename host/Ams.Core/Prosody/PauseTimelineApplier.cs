using System.Collections.ObjectModel;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Core.Prosody;

internal static class PauseTimelineApplier
{
    private const double DurationEpsilon = 1e-9;
    internal const int TailSentinelSentenceId = int.MinValue;

    public sealed record PauseTimelineApplyResult(
        IReadOnlyDictionary<int, SentenceTiming> Timeline,
        IReadOnlyList<PauseIntraGap> IntraSentenceGaps);

    public static PauseTimelineApplyResult Apply(
        IReadOnlyDictionary<int, SentenceTiming> baseline,
        IReadOnlyList<PauseAdjust> adjustments)
    {
        if (baseline is null) throw new ArgumentNullException(nameof(baseline));
        if (adjustments is null) throw new ArgumentNullException(nameof(adjustments));

        if (baseline.Count == 0 || adjustments.Count == 0)
        {
            return new PauseTimelineApplyResult(
                new ReadOnlyDictionary<int, SentenceTiming>(CloneTimeline(baseline)),
                Array.Empty<PauseIntraGap>());
        }

        var result = CloneTimeline(baseline);
        var orderedSentenceIds = result
            .OrderBy(kvp => kvp.Value.StartSec)
            .Select(kvp => kvp.Key)
            .ToList();

        if (orderedSentenceIds.Count == 0)
        {
            return new PauseTimelineApplyResult(
                new ReadOnlyDictionary<int, SentenceTiming>(result),
                Array.Empty<PauseIntraGap>());
        }

        var indexBySentence = new Dictionary<int, int>(orderedSentenceIds.Count);
        for (int i = 0; i < orderedSentenceIds.Count; i++)
        {
            indexBySentence[orderedSentenceIds[i]] = i;
        }

        var orderedAdjustments = adjustments
            .Where(adj => adj is not null && double.IsFinite(adj.TargetDurationSec) && adj.TargetDurationSec >= 0)
            .OrderBy(adj => adj.StartSec)
            .ToList();

        var intraGaps = new List<PauseIntraGap>();
        var sentenceInteriorShift = new Dictionary<int, double>();

        foreach (var adjust in orderedAdjustments)
        {
            if (adjust.Class == PauseClass.ChapterHead)
            {
                ApplyChapterHeadAdjust(result, orderedSentenceIds, indexBySentence, adjust);
                continue;
            }

            if (adjust.Class == PauseClass.Tail)
            {
                ApplyTailAdjust(result, orderedSentenceIds, indexBySentence, adjust);
                continue;
            }

            double delta = adjust.TargetDurationSec - adjust.OriginalDurationSec;
            if (!adjust.IsIntraSentence && Math.Abs(delta) < DurationEpsilon)
            {
                continue;
            }

            if (Math.Abs(delta) < DurationEpsilon)
            {
                continue;
            }

            if (adjust.IsIntraSentence)
            {
                if (!indexBySentence.TryGetValue(adjust.LeftSentenceId, out var index)
                    || !result.TryGetValue(adjust.LeftSentenceId, out var timing))
                {
                    continue;
                }

                if (!baseline.TryGetValue(adjust.LeftSentenceId, out var baselineTiming))
                {
                    baselineTiming = timing;
                }

                sentenceInteriorShift.TryGetValue(adjust.LeftSentenceId, out var interiorShift);

                double targetDuration = Math.Max(0d, adjust.TargetDurationSec);
                double sentenceStartShift = timing.StartSec - baselineTiming.StartSec;
                double targetStart = adjust.StartSec + sentenceStartShift + interiorShift;
                double targetEnd = targetStart + targetDuration;
                if (targetEnd < targetStart) targetEnd = targetStart;

                intraGaps.Add(new PauseIntraGap(
                    adjust.LeftSentenceId,
                    adjust.StartSec,
                    adjust.EndSec,
                    targetStart,
                    targetEnd));

                double newEnd = Math.Max(timing.StartSec, timing.EndSec + delta);
                result[adjust.LeftSentenceId] = new SentenceTiming(
                    timing.StartSec,
                    newEnd,
                    timing.FragmentBacked,
                    timing.Confidence);

                ShiftFollowingSentences(result, orderedSentenceIds, indexBySentence, index, delta);
                sentenceInteriorShift[adjust.LeftSentenceId] = interiorShift + delta;
                continue;
            }

            if (!result.ContainsKey(adjust.LeftSentenceId) || !result.ContainsKey(adjust.RightSentenceId))
            {
                continue;
            }

            ApplyInterSentenceAdjust(result, orderedSentenceIds, indexBySentence, adjust.LeftSentenceId,
                adjust.RightSentenceId, adjust.TargetDurationSec);
        }

        return new PauseTimelineApplyResult(
            new ReadOnlyDictionary<int, SentenceTiming>(result),
            intraGaps);
    }

    private static void ApplyTailAdjust(
        IDictionary<int, SentenceTiming> timeline,
        IReadOnlyList<int> orderedSentenceIds,
        IReadOnlyDictionary<int, int> indexBySentence,
        PauseAdjust adjust)
    {
        if (!indexBySentence.TryGetValue(adjust.LeftSentenceId, out _))
        {
            return;
        }

        if (!timeline.TryGetValue(adjust.LeftSentenceId, out var leftTiming))
        {
            return;
        }

        double tail = Math.Max(0d, adjust.TargetDurationSec);
        if (tail < DurationEpsilon)
        {
            if (timeline.ContainsKey(TailSentinelSentenceId))
            {
                timeline.Remove(TailSentinelSentenceId);
            }

            return;
        }

        double start = leftTiming.EndSec;
        double end = start + tail;
        timeline[TailSentinelSentenceId] = new SentenceTiming(start, end, fragmentBacked: false, confidence: null);
    }

    private static void ApplyChapterHeadAdjust(
        IDictionary<int, SentenceTiming> timeline,
        IReadOnlyList<int> orderedSentenceIds,
        IReadOnlyDictionary<int, int> indexBySentence,
        PauseAdjust adjust)
    {
        if (!indexBySentence.TryGetValue(adjust.LeftSentenceId, out var index))
        {
            return;
        }

        if (!timeline.TryGetValue(adjust.LeftSentenceId, out var timing))
        {
            return;
        }

        double targetStart = Math.Max(0d, adjust.TargetDurationSec);
        double delta = targetStart - timing.StartSec;
        if (Math.Abs(delta) < DurationEpsilon)
        {
            return;
        }

        double newStart = timing.StartSec + delta;
        double newEnd = timing.EndSec + delta;
        timeline[adjust.LeftSentenceId] =
            new SentenceTiming(newStart, newEnd, timing.FragmentBacked, timing.Confidence);

        ShiftFollowingSentences(timeline, orderedSentenceIds, indexBySentence, index, delta);
    }

    private static void ApplyInterSentenceAdjust(
        IDictionary<int, SentenceTiming> timeline,
        IReadOnlyList<int> orderedSentenceIds,
        IReadOnlyDictionary<int, int> indexBySentence,
        int leftId,
        int rightId,
        double targetDuration)
    {
        if (!indexBySentence.TryGetValue(leftId, out var leftIndex) ||
            !indexBySentence.TryGetValue(rightId, out var rightIndex))
        {
            return;
        }

        if (rightIndex <= leftIndex)
        {
            return;
        }

        var left = timeline[leftId];
        var right = timeline[rightId];

        double desiredStart = left.EndSec + targetDuration;
        double shift = desiredStart - right.StartSec;
        if (Math.Abs(shift) < DurationEpsilon)
        {
            return;
        }

        double newStart = desiredStart;
        double newEnd = Math.Max(newStart, right.EndSec + shift);
        timeline[rightId] = new SentenceTiming(newStart, newEnd, right.FragmentBacked, right.Confidence);

        ShiftFollowingSentences(timeline, orderedSentenceIds, indexBySentence, rightIndex, shift);
    }

    private static void ShiftFollowingSentences(
        IDictionary<int, SentenceTiming> timeline,
        IReadOnlyList<int> orderedSentenceIds,
        IReadOnlyDictionary<int, int> indexBySentence,
        int pivotIndex,
        double delta)
    {
        if (Math.Abs(delta) < DurationEpsilon)
        {
            return;
        }

        for (int i = pivotIndex + 1; i < orderedSentenceIds.Count; i++)
        {
            int sentenceId = orderedSentenceIds[i];
            if (!timeline.TryGetValue(sentenceId, out var timing))
            {
                continue;
            }

            double newStart = timing.StartSec + delta;
            double newEnd = timing.EndSec + delta;
            timeline[sentenceId] = new SentenceTiming(newStart, newEnd, timing.FragmentBacked, timing.Confidence);
        }
    }

    private static Dictionary<int, SentenceTiming> CloneTimeline(IReadOnlyDictionary<int, SentenceTiming> baseline)
    {
        var clone = new Dictionary<int, SentenceTiming>(baseline.Count);
        foreach (var kvp in baseline)
        {
            var timing = kvp.Value;
            clone[kvp.Key] =
                new SentenceTiming(timing.StartSec, timing.EndSec, timing.FragmentBacked, timing.Confidence);
        }

        return clone;
    }
}