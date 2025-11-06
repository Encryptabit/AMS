using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Alignment.Tx;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Common;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Core.Prosody;

internal static class PauseAudioApplier
{
    private const double DurationEpsilon = 1e-6;
    private const double SegmentFadeMs = 60.0;
    private static readonly IReadOnlyList<PauseIntraGap> EmptyIntraGaps = Array.Empty<PauseIntraGap>();

    public static AudioBuffer Apply(
        AudioBuffer audio,
        AudioBuffer roomtone,
        IReadOnlyList<SentenceAlign> sentences,
        IReadOnlyDictionary<int, SentenceTiming> updatedTimeline,
        double toneGainLinear,
        double fadeMs = 35.0,
        IReadOnlyList<PauseIntraGap>? intraSentenceGaps = null)
    {
        if (audio is null) throw new ArgumentNullException(nameof(audio));
        if (roomtone is null) throw new ArgumentNullException(nameof(roomtone));
        if (sentences is null) throw new ArgumentNullException(nameof(sentences));
        if (updatedTimeline is null) throw new ArgumentNullException(nameof(updatedTimeline));
        if (updatedTimeline.Count == 0)
        {
            return audio;
        }

        Log.Debug("PauseAudioApplier starting: sampleRate={SampleRate}, timelineCount={TimelineCount}", audio.SampleRate, updatedTimeline.Count);

        var finalTimeline = new Dictionary<int, SentenceTiming>(updatedTimeline);

        bool hasTail = finalTimeline.TryGetValue(PauseTimelineApplier.TailSentinelSentenceId, out var tailTiming);
        if (hasTail)
        {
            finalTimeline.Remove(PauseTimelineApplier.TailSentinelSentenceId);
        }

        var orderedSentences = sentences
            .Where(sentence => finalTimeline.ContainsKey(sentence.Id))
            .OrderBy(sentence => finalTimeline[sentence.Id].StartSec)
            .ToList();

        if (orderedSentences.Count == 0)
        {
            return audio;
        }

        var intraLookup = intraSentenceGaps is not null && intraSentenceGaps.Count > 0
            ? intraSentenceGaps
                .Where(gap => finalTimeline.ContainsKey(gap.SentenceId))
                .GroupBy(gap => gap.SentenceId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(g => g.SourceStartSec).ToList())
            : new Dictionary<int, List<PauseIntraGap>>();

        int sampleRate = audio.SampleRate;

        var overlays = new List<IReadOnlyList<SentenceSegment>>(orderedSentences.Count);
        double maxSegmentEndSec = hasTail ? tailTiming!.EndSec : 0d;

        foreach (var sentence in orderedSentences)
        {
            if (!finalTimeline.TryGetValue(sentence.Id, out var targetTiming))
            {
                continue;
            }

            var hasIntraGaps = intraLookup.TryGetValue(sentence.Id, out var gapList) && gapList is { Count: > 0 };
            var gaps = hasIntraGaps ? gapList! : EmptyIntraGaps;

            double exitThresholdDb = hasIntraGaps ? -55.0 : -60.0;
            double postRollMs = hasIntraGaps ? 6.0 : 10.0;
            double hangoverMs = hasIntraGaps ? 16.0 : 30.0;

            var seed = new TimingRange(sentence.Timing.StartSec, sentence.Timing.EndSec);
            var energy = AudioProcessor.SnapToEnergy(
                audio,
                seed,
                enterThresholdDb: -48.0,
                exitThresholdDb: exitThresholdDb,
                searchWindowSec: 1.0,
                windowMs: 25.0,
                stepMs: 5.0,
                preRollMs: 10.0,
                postRollMs: postRollMs,
                hangoverMs: hangoverMs);

            if (energy.EndSec <= energy.StartSec)
            {
                energy = seed;
            }

            double sourceStartSec = Math.Max(seed.StartSec, energy.StartSec);
            double sourceEndSec = Math.Max(seed.EndSec, energy.EndSec);

            if (sourceEndSec - sourceStartSec < DurationEpsilon)
            {
                sourceStartSec = seed.StartSec;
                sourceEndSec = seed.EndSec;
            }

            if (!IsApproximatelyEqual(sourceEndSec, seed.EndSec))
            {
                Log.Debug(
                    "PauseAudioApplier energy-adjusted sentence {SentenceId}: seedEnd={SeedEnd:F3}s energyEnd={EnergyEnd:F3}s",
                    sentence.Id,
                    seed.EndSec,
                    sourceEndSec);
            }

            if (gaps.Count > 0)
            {
                Log.Debug(
                    "PauseAudioApplier using feature-derived intra-gaps for sentence {SentenceId}: {GapCount} gap(s).",
                    sentence.Id,
                    gaps.Count);
            }
            else
            {
                Log.Debug(
                    "PauseAudioApplier using SnapToEnergy fallback for sentence {SentenceId} (no intra-gap timeline).",
                    sentence.Id);
            }

            var segments = BuildSentenceSegments(
                sentence.Id,
                sourceStartSec,
                sourceEndSec,
                targetTiming,
                gaps);

            if (segments.Count == 0)
            {
                continue;
            }

            foreach (var segment in segments)
            {
                double duration = segment.SourceEndSec - segment.SourceStartSec;
                double end = segment.DestinationStartSec + duration;
                if (end > maxSegmentEndSec)
                {
                    maxSegmentEndSec = end;
                }
            }

            overlays.Add(segments);
        }

        double scheduledEndSec = finalTimeline.Count > 0 ? finalTimeline.Values.Max(t => t.EndSec) : 0d;
        double totalDurationSec = Math.Max(scheduledEndSec, maxSegmentEndSec);
        if (hasTail)
        {
            totalDurationSec = Math.Max(totalDurationSec, tailTiming!.EndSec);
        }

        if (totalDurationSec < DurationEpsilon)
        {
            return audio;
        }

        var tone = roomtone.SampleRate == sampleRate
            ? roomtone
            : AudioProcessor.Resample(roomtone, sampleRate);
        if (tone.Length == 0)
        {
            throw new InvalidOperationException("Roomtone seed must contain samples.");
        }

        RoomtoneUtils.MakeLoopable(tone, seamMs: 60);

        int targetLength = Math.Max((int)Math.Ceiling(totalDurationSec * sampleRate), 1);
        double segmentFadeMs = Math.Clamp(fadeMs, 1.0, SegmentFadeMs);
        var output = BuildRoomtoneBed(tone, audio.Channels, sampleRate, targetLength, toneGainLinear, segmentFadeMs);
        int fadeSamples = Math.Max(1, (int)Math.Round(segmentFadeMs * 0.001 * sampleRate));

        foreach (var segments in overlays)
        {
            OverlaySegments(audio, output, segments, sampleRate, fadeSamples);
        }

        return output;
    }

    private static AudioBuffer BuildRoomtoneBed(AudioBuffer tone, int channels, int sampleRate, int length, double gain, double fadeMs)
    {
        var baseBuffer = new AudioBuffer(channels, sampleRate, length);
        double endSec = length / (double)sampleRate;
        return AudioProcessor.OverlayRoomtone(baseBuffer, tone, 0.0, endSec, gain, fadeMs);
    }

    private static IReadOnlyList<SentenceSegment> BuildSentenceSegments(
        int sentenceId,
        double sourceStartSec,
        double sourceEndSec,
        SentenceTiming targetTiming,
        IReadOnlyList<PauseIntraGap> intraGaps)
    {
        var segments = new List<SentenceSegment>();

        double srcCursor = sourceStartSec;
        double destCursor = targetTiming.StartSec;
        double targetEnd = targetTiming.EndSec;

        if (intraGaps.Count > 0)
        {
            foreach (var gap in intraGaps)
            {
                double gapSourceStart = Math.Clamp(gap.SourceStartSec, sourceStartSec, sourceEndSec);
                double gapSourceEnd = Math.Clamp(gap.SourceEndSec, gapSourceStart, sourceEndSec);

                double gapTargetStart = Math.Clamp(gap.TargetStartSec, targetTiming.StartSec, targetEnd);
                double gapTargetEnd = Math.Clamp(gap.TargetEndSec, gapTargetStart, targetEnd);

                if (gapSourceStart - srcCursor > DurationEpsilon)
                {
                    segments.Add(new SentenceSegment(srcCursor, gapSourceStart, destCursor));
                }

                srcCursor = gapSourceEnd;
                destCursor = gapTargetEnd;
            }
        }

        if (sourceEndSec - srcCursor > DurationEpsilon)
        {
            segments.Add(new SentenceSegment(srcCursor, sourceEndSec, destCursor));
        }

        return segments;
    }

    private static void OverlaySegments(
        AudioBuffer source,
        AudioBuffer destination,
        IReadOnlyList<SentenceSegment> segments,
        int sampleRate,
        int fadeSamples)
    {
        if (segments.Count == 0)
        {
            return;
        }

        int sourceChannels = source.Channels;
        int destChannels = destination.Channels;

        foreach (var segment in segments)
        {
            int srcStart = SecondsToSample(segment.SourceStartSec, sampleRate, source.Length);
            int srcEnd = SecondsToSample(segment.SourceEndSec, sampleRate, source.Length);
            int dstStart = SecondsToSample(segment.DestinationStartSec, sampleRate, destination.Length);

            int length = Math.Min(srcEnd - srcStart, destination.Length - dstStart);
            if (length <= 0)
            {
                continue;
            }

            for (int ch = 0; ch < destChannels; ch++)
            {
                var srcArr = source.Planar[ch % sourceChannels];
                var dstArr = destination.Planar[ch];

                for (int i = 0; i < length; i++)
                {
                    int srcIndex = srcStart + i;
                    int dstIndex = dstStart + i;

                    if (srcIndex >= srcArr.Length || dstIndex >= dstArr.Length)
                    {
                        break;
                    }

                    double envelope = 1.0;

                    if (fadeSamples > 0 && length > 1)
                    {
                        if (i < fadeSamples)
                        {
                            double ratio = (i + 1.0) / (fadeSamples + 1.0);
                            envelope *= Math.Sin(ratio * (Math.PI * 0.5));
                        }

                        if (i >= length - fadeSamples)
                        {
                            double ratio = (length - i) / (fadeSamples + 1.0);
                            envelope *= Math.Sin(ratio * (Math.PI * 0.5));
                        }
                    }

                    double srcSample = srcArr[srcIndex];
                    double dstSample = dstArr[dstIndex];
                    double mixed = srcSample * envelope + dstSample * (1.0 - envelope);
                    dstArr[dstIndex] = (float)mixed;
                }
            }
        }
    }

    private static int SecondsToSample(double seconds, int sampleRate, int maxLength)
    {
        int sample = (int)Math.Round(seconds * sampleRate);
        return Math.Clamp(sample, 0, Math.Max(0, maxLength));
    }

    private static bool IsApproximatelyEqual(double left, double right)
    {
        return Math.Abs(left - right) < DurationEpsilon;
    }

    private readonly struct SentenceSegment
    {
        public SentenceSegment(double sourceStartSec, double sourceEndSec, double destinationStartSec)
        {
            SourceStartSec = sourceStartSec;
            SourceEndSec = sourceEndSec;
            DestinationStartSec = destinationStartSec;
        }

        public double SourceStartSec { get; }
        public double SourceEndSec { get; }
        public double DestinationStartSec { get; }
    }
}
