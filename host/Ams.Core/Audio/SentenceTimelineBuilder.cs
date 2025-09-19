using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public sealed record SentenceTimelineEntry(int SentenceId, SentenceTiming Timing, bool HasTiming, SentenceTiming Window);

public static class SentenceTimelineBuilder
{
    public static IReadOnlyList<SentenceTimelineEntry> Build(
        IReadOnlyList<SentenceAlign> sentences,
        AudioAnalysisService analyzer,
        IReadOnlyDictionary<int, FragmentTiming>? fragments = null,
        double rmsThresholdDb = -35.0,
        double searchWindowSec = 0.5,
        double stepMs = 10.0)
    {
        if (sentences is null) throw new ArgumentNullException(nameof(sentences));
        if (analyzer is null) throw new ArgumentNullException(nameof(analyzer));

        var entries = new List<SentenceTimelineEntry>(sentences.Count);

        foreach (var sentence in sentences)
        {
            var baseTiming = new SentenceTiming(sentence.Timing);
            var timingSource = fragments != null && fragments.TryGetValue(sentence.Id, out var fragment)
                ? new SentenceTiming(fragment, fragmentBacked: true)
                : baseTiming;

            bool hasTiming = timingSource.Duration > 0;

            SentenceTiming energyTiming = timingSource;
            if (hasTiming)
            {
                var snapped = analyzer.SnapToEnergy(timingSource, rmsThresholdDb, searchWindowSec, stepMs);
                energyTiming = new SentenceTiming(snapped.StartSec, snapped.EndSec, timingSource.FragmentBacked, timingSource.Confidence);
            }

            entries.Add(new SentenceTimelineEntry(sentence.Id, energyTiming, hasTiming, baseTiming));
        }

        // Ensure monotonically increasing windows to avoid overlap downstream.
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
}

