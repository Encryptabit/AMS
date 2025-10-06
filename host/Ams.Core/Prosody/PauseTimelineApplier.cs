using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ams.Core.Artifacts;

namespace Ams.Core.Prosody;

internal static class PauseTimelineApplier
{
    private const double DurationEpsilon = 1e-9;

    public static IReadOnlyDictionary<int, SentenceTiming> Apply(
        IReadOnlyDictionary<int, SentenceTiming> baseline,
        IReadOnlyList<PauseAdjust> adjustments)
    {
        if (baseline is null) throw new ArgumentNullException(nameof(baseline));
        if (adjustments is null) throw new ArgumentNullException(nameof(adjustments));

        if (baseline.Count == 0 || adjustments.Count == 0)
        {
            return new ReadOnlyDictionary<int, SentenceTiming>(CloneTimeline(baseline));
        }

        var result = CloneTimeline(baseline);
        var orderedSentenceIds = result
            .OrderBy(kvp => kvp.Value.StartSec)
            .Select(kvp => kvp.Key)
            .ToList();

        if (orderedSentenceIds.Count == 0)
        {
            return new ReadOnlyDictionary<int, SentenceTiming>(result);
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

        foreach (var adjust in orderedAdjustments)
        {
            double delta = adjust.TargetDurationSec - adjust.OriginalDurationSec;
            if (Math.Abs(delta) < DurationEpsilon)
            {
                continue;
            }

            if (adjust.IsIntraSentence)
            {
                ApplyIntraSentenceAdjust(result, orderedSentenceIds, indexBySentence, adjust.LeftSentenceId, delta);
                continue;
            }

            if (!result.ContainsKey(adjust.LeftSentenceId) || !result.ContainsKey(adjust.RightSentenceId))
            {
                continue;
            }

            ApplyInterSentenceAdjust(result, orderedSentenceIds, indexBySentence, adjust.LeftSentenceId, adjust.RightSentenceId, adjust.TargetDurationSec);
        }

        return new ReadOnlyDictionary<int, SentenceTiming>(result);
    }

    private static void ApplyIntraSentenceAdjust(
        IDictionary<int, SentenceTiming> timeline,
        IReadOnlyList<int> orderedSentenceIds,
        IReadOnlyDictionary<int, int> indexBySentence,
        int sentenceId,
        double delta)
    {
        if (!timeline.TryGetValue(sentenceId, out var sentence) || !indexBySentence.TryGetValue(sentenceId, out var index))
        {
            return;
        }

        double newEnd = Math.Max(sentence.StartSec, sentence.EndSec + delta);
        timeline[sentenceId] = new SentenceTiming(sentence.StartSec, newEnd, sentence.FragmentBacked, sentence.Confidence);

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
        if (!indexBySentence.TryGetValue(leftId, out var leftIndex) || !indexBySentence.TryGetValue(rightId, out var rightIndex))
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
            clone[kvp.Key] = new SentenceTiming(timing.StartSec, timing.EndSec, timing.FragmentBacked, timing.Confidence);
        }
        return clone;
    }
}
