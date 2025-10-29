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
            bool isUnreliable = string.Equals(sentence.Status, "unreliable", StringComparison.OrdinalIgnoreCase);

            var baseTiming = fragments != null && fragments.TryGetValue(sentence.Id, out var fragment)
                ? new SentenceTiming(fragment, fragmentBacked: true)
                : new SentenceTiming(sentence.Timing);

            var windowTiming = isUnreliable
                ? new SentenceTiming(TimingRange.Empty)
                : ComputeWindowFromScript(sentence, asr);

            if (!isUnreliable && windowTiming.Duration > 0)
            {
                const double ReliableWindowPad = 0.08; // add small buffer around scripted window
                double start = Math.Min(windowTiming.StartSec, baseTiming.StartSec) - ReliableWindowPad;
                double end = Math.Max(windowTiming.EndSec, baseTiming.EndSec) + ReliableWindowPad;
                windowTiming = new SentenceTiming(Math.Max(0.0, start), end);
            }

            double effectiveRms = isUnreliable ? rmsThresholdDb + 2.0 : rmsThresholdDb;
            double effectiveSearch = isUnreliable ? Math.Max(searchWindowSec, 0.8) : searchWindowSec;

            var energyTiming = CalculateEnergyTiming(
                analyzer,
                windowTiming,
                baseTiming,
                effectiveRms,
                effectiveSearch,
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

        const double MinUnreliableDuration = 0.45;
        for (int i = 0; i < entries.Count; i++)
        {
            var sentence = sentences[i];
            if (!string.Equals(sentence.Status, "unreliable", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var entry = entries[i];
            double duration = entry.Timing.Duration;
            if (duration >= MinUnreliableDuration)
            {
                continue;
            }

            double prevEnd = i > 0 ? entries[i - 1].Timing.EndSec : Math.Max(0.0, entry.Timing.StartSec - 0.2);
            double nextStart = i + 1 < entries.Count ? entries[i + 1].Timing.StartSec : entry.Timing.EndSec + 0.2;

            double availableLeft = Math.Max(0.0, entry.Timing.StartSec - prevEnd);
            double availableRight = Math.Max(0.0, nextStart - entry.Timing.EndSec);

            double needed = MinUnreliableDuration - duration;
            if (needed <= 0)
            {
                continue;
            }

            double extendLeft = Math.Min(availableLeft, needed / 2.0);
            double extendRight = Math.Min(availableRight, needed - extendLeft);

            if (extendLeft <= 0 && extendRight <= 0)
            {
                continue;
            }

            var adjustedTiming = new SentenceTiming(
                entry.Timing.StartSec - extendLeft,
                entry.Timing.EndSec + extendRight,
                entry.Timing.FragmentBacked,
                entry.Timing.Confidence);

            entries[i] = entry with { Timing = adjustedTiming };
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
            double exitThresholdDb = rmsThresholdDb - 8.0;
            
            var snapped = analyzer.SnapToEnergy(
                seed,
                enterThresholdDb: rmsThresholdDb,
                exitThresholdDb: exitThresholdDb,
                searchWindowSec: searchWindowSec,
                stepMs: stepMs,
                preRollMs: 20.0,
                postRollMs: 20.0,
                hangoverMs: 35.0);

          //var snapped = analyzer.SnapToEnergyAuto(seed: seed, AutoTuneStyle.Tight);
            if (windowTiming.Duration > 0)
            {
                snapped = ClampToWindow(snapped, windowTiming);
            }

            double start = snapped.StartSec;
            double end = snapped.EndSec;
            if (end < start)
            {
                end = start;
            }



            if (end - start <= 0)

            {

                start = seed.StartSec;

                end = Math.Max(start, seed.EndSec);

            }



            return new SentenceTiming(start, end, baseTiming.FragmentBacked, baseTiming.Confidence);

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
