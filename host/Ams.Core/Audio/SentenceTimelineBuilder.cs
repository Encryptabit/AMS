using System.Collections.Generic;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public sealed record SentenceTimelineEntry(int SentenceId, TimingRange Timing, bool HasTiming, bool FragmentBacked);

public static class SentenceTimelineBuilder
{
    public static IReadOnlyList<SentenceTimelineEntry> Build(
        IReadOnlyList<SentenceAlign> sentences,
        IReadOnlyDictionary<int, FragmentTiming>? fragments = null)
    {
        var results = new List<SentenceTimelineEntry>(sentences.Count);

        foreach (var sentence in sentences)
        {
            var timing = sentence.Timing;
            bool hasTiming = timing.Duration > 0;
            bool fragmentBacked = false;

            if (fragments != null && fragments.TryGetValue(sentence.Id, out var fragment))
            {
                timing = fragment;
                fragmentBacked = true;
                hasTiming = true;
            }

            results.Add(new SentenceTimelineEntry(sentence.Id, timing, hasTiming, fragmentBacked));
        }

        return results;
    }
}
