using System;
using System.Collections.Generic;
using Ams.Core.Asr;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
    public sealed record SentenceTimelineEntry(int SentenceId, SentenceTiming Timing, bool HasTiming, SentenceTiming Window);

    public static class SentenceTimelineBuilder
    {
        public static IReadOnlyList<SentenceTimelineEntry> Build(
            IReadOnlyList<SentenceAlign> sentences,
            AudioAnalysisService analyzer,
            AsrResponse asr,
            IReadOnlyDictionary<int, FragmentTiming>? fragments = null,
            double rmsThresholdDb = -35.0,
            double searchWindowSec = 0.5,
            double stepMs = 10.0)
        {
            if (sentences is null) throw new ArgumentNullException(nameof(sentences));
            if (analyzer is null) throw new ArgumentNullException(nameof(analyzer));
            if (asr is null) throw new ArgumentNullException(nameof(asr));

            var entries = new List<SentenceTimelineEntry>(sentences.Count);

            foreach (var sentence in sentences)
            {
                var windowTiming = ComputeWindowFromScript(sentence, asr);
                var baseTiming = fragments != null && fragments.TryGetValue(sentence.Id, out var fragment)
                    ? new SentenceTiming(fragment, fragmentBacked: true)
                    : new SentenceTiming(sentence.Timing);

                var energyTiming = CalculateEnergyTiming(
                    analyzer,
                    windowTiming,
                    baseTiming,
                    rmsThresholdDb,
                    searchWindowSec,
                    stepMs);

                bool hasTiming = energyTiming.Duration > 0;
                entries.Add(new SentenceTimelineEntry(sentence.Id, energyTiming, hasTiming, windowTiming));
            }

            for (int i = 1; i < entries.Count; i++)
            {
                var prev = entries[i - 1];
                var curr = entries[i];
                if (curr.Timing.StartSec < prev.Timing.EndSec)
                {
                    var adjusted = prev.Timing.WithEnd(curr.Timing.StartSec);
                    entries[i - 1] = prev with { Timing = adjusted };
                }
            }

            return entries;
        }

        private static SentenceTiming CalculateEnergyTiming(
            AudioAnalysisService analyzer,
            SentenceTiming windowTiming,
            SentenceTiming baseTiming,
            double rmsThresholdDb,
            double searchWindowSec,
            double stepMs)
        {
            var seed = windowTiming.Duration > 0 ? windowTiming : baseTiming;
            if (seed.Duration <= 0)
            {
                return baseTiming;
            }

            var snapped = analyzer.SnapToEnergy(seed, rmsThresholdDb, searchWindowSec, stepMs);
            //var snapped = analyzer.SnapToEnergyAuto(seed);
            if (windowTiming.Duration > 0)
            {
                snapped = ClampToWindow(snapped, windowTiming);
            }

            if (snapped.Duration <= 0)
            {
                snapped = new TimingRange(seed.StartSec, seed.EndSec);
            }

            return new SentenceTiming(snapped.StartSec, snapped.EndSec, baseTiming.FragmentBacked, baseTiming.Confidence);
        }

        private static TimingRange ClampToWindow(TimingRange range, SentenceTiming window)
        {
            if (window.Duration <= 0) return range;

            double start = Math.Clamp(range.StartSec, window.StartSec, window.EndSec);
            double end = Math.Clamp(range.EndSec, window.StartSec, window.EndSec);
            if (end < start)
            {
                end = start;
            }

            return new TimingRange(start, end);
        }

        private static SentenceTiming ComputeWindowFromScript(SentenceAlign sentence, AsrResponse asr)
        {
            if (sentence.ScriptRange is { Start: { } startIdx, End: { } endIdx } && asr.Tokens.Length > 0)
            {
                var start = Math.Clamp(startIdx, 0, asr.Tokens.Length - 1);
                var end = Math.Clamp(endIdx, start, asr.Tokens.Length - 1);
                var startToken = asr.Tokens[start];
                var endToken = asr.Tokens[end];
                var range = new TimingRange(startToken.StartTime, endToken.StartTime + endToken.Duration);
                if (range.Duration > 0)
                {
                    return new SentenceTiming(range);
                }
            }

            return new SentenceTiming(sentence.Timing);
        }
    }
}