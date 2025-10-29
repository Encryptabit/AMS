using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Alignment.Tx;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Common;

namespace Ams.Core.Prosody;

internal static class PauseAudioApplier
{
    private const double DurationEpsilon = 1e-6;
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

        Log.Info("PauseAudioApplier starting: sampleRate={0}, timelineCount={1}", audio.SampleRate, updatedTimeline.Count);
        var analyzer = new AudioAnalysisService(audio);

        var timeline = new Dictionary<int, SentenceTiming>(updatedTimeline);
        bool hasTailTiming = timeline.TryGetValue(PauseTimelineApplier.TailSentinelSentenceId, out var tailTiming);
        if (hasTailTiming)
        {
            timeline.Remove(PauseTimelineApplier.TailSentinelSentenceId);
        }

        Dictionary<int, List<PauseIntraGap>>? intraGapLookup = null;
        if (intraSentenceGaps is not null && intraSentenceGaps.Count > 0)
        {
            intraGapLookup = intraSentenceGaps
                .Where(gap => timeline.ContainsKey(gap.SentenceId))
                .GroupBy(gap => gap.SentenceId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(g => g.SourceStartSec).ToList());
        }

        var orderedSentences = sentences
            .Where(sentence => timeline.ContainsKey(sentence.Id))
            .OrderBy(sentence => sentence.Timing.StartSec)
            .ToList();

        if (orderedSentences.Count == 0)
        {
            return audio;
        }

        var unreliableSentenceIds = orderedSentences
            .Where(sentence => !IsReliableStatus(sentence.Status))
            .Select(sentence => sentence.Id)
            .ToHashSet();

        int sampleRate = audio.SampleRate;
        double timelineMaxEnd = timeline.Values.Max(t => t.EndSec);
        double durationSec = Math.Max(timelineMaxEnd, audio.Length / (double)sampleRate);
        if (hasTailTiming)
        {
            durationSec = Math.Max(durationSec, tailTiming!.EndSec);
        }
        int targetLength = Math.Max(audio.Length, (int)Math.Ceiling(durationSec * sampleRate));
        var working = new AudioBuffer(audio.Channels, sampleRate, targetLength);

        for (int i = 0; i < orderedSentences.Count; i++)
        {
            var sentence = orderedSentences[i];
            if (!timeline.TryGetValue(sentence.Id, out var targetTiming))
            {
                Log.Warn("PauseAudioApplier missing timeline entry for sentence {0}; preserving original timing {1:F3}-{2:F3}", sentence.Id, sentence.Timing.StartSec, sentence.Timing.EndSec);
                targetTiming = new SentenceTiming(sentence.Timing.StartSec, sentence.Timing.EndSec);
            }

            var seed = new TimingRange(sentence.Timing.StartSec, sentence.Timing.EndSec);
            var energy = analyzer.SnapToEnergy(seed, enterThresholdDb: -48.0, exitThresholdDb: -60.0, searchWindowSec: 1.0, windowMs: 25.0, stepMs: 5.0, preRollMs: 25.0, postRollMs: 30.0, hangoverMs: 15.0);
            if (energy.EndSec <= energy.StartSec)
            {
                energy = seed;
            }

            double nextStart = i + 1 < orderedSentences.Count && timeline.TryGetValue(orderedSentences[i + 1].Id, out var nextTiming)
                ? nextTiming.StartSec
                : durationSec;

            var actualSource = new TimingRange(
                Math.Max(seed.StartSec, energy.StartSec),
                Math.Min(nextStart, Math.Max(seed.EndSec, energy.EndSec)));

            if (actualSource.EndSec - actualSource.StartSec < DurationEpsilon)
            {
                actualSource = seed;
            }

            double gapLeft = Math.Max(actualSource.EndSec, targetTiming.EndSec);
            if (nextStart - gapLeft > DurationEpsilon)
            {
                double refinedTail = analyzer.FindSpeechEndFromGap(gapLeft, nextStart, silenceThresholdDb: -55.0, stepMs: 5.0, backoffMs: 3.0);
                if (refinedTail - actualSource.EndSec > DurationEpsilon)
                {
                    Log.Info("PauseAudioApplier tail-extended sentence {0}: prevEnd={1:F3}s refinedEnd={2:F3}s", sentence.Id, actualSource.EndSec, refinedTail);
                    double clampedTail = Math.Min(refinedTail, nextStart);
                    actualSource = new TimingRange(actualSource.StartSec, Math.Max(actualSource.StartSec, clampedTail));
                }
            }

            if (!IsApproximatelyEqual(actualSource.EndSec, seed.EndSec))
            {
                Log.Info("PauseAudioApplier energy-adjusted sentence {0}: seedEnd={1:F3}s energyEnd={2:F3}s", sentence.Id, seed.EndSec, actualSource.EndSec);
            }

            var sentenceGaps = intraGapLookup is not null && intraGapLookup.TryGetValue(sentence.Id, out var gapList)
                ? (IReadOnlyList<PauseIntraGap>)gapList
                : EmptyIntraGaps;

            CopySentence(audio, working, actualSource, targetTiming, sampleRate, sentenceGaps);
        }

        var updatedSentences = orderedSentences
            .Select(sentence => sentence with { Timing = new TimingRange(timeline[sentence.Id].StartSec, timeline[sentence.Id].EndSec) })
            .ToList();

        var gaps = BuildPlanGaps(updatedSentences, durationSec, unreliableSentenceIds);
        if (intraSentenceGaps is not null && intraSentenceGaps.Count > 0)
        {
            foreach (var gap in intraSentenceGaps)
            {
                double start = Math.Clamp(gap.TargetStartSec, 0.0, durationSec);
                double end = Math.Clamp(gap.TargetEndSec, start, durationSec);
                double gapDuration = end - start;
                if (gapDuration <= DurationEpsilon)
                {
                    continue;
                }

                gaps.Add(new RoomtonePlanGap(
                    start,
                    end,
                    gapDuration,
                    gap.SentenceId,
                    gap.SentenceId,
                    MinRmsDb: -60,
                    MaxRmsDb: -60,
                    MeanRmsDb: -60,
                    SilenceFraction: 1.0,
                    BreathRegions: Array.Empty<RoomtoneBreathRegion>()));
            }

            gaps.Sort((left, right) => left.StartSec.CompareTo(right.StartSec));
        }
        Log.Info("PauseAudioApplier gaps generated: {0}", gaps.Count);

        if (roomtone.SampleRate != sampleRate)
        {
            roomtone = RoomtoneUtils.ResampleLinear(roomtone, sampleRate);
            Log.Info("PauseAudioApplier resampled roomtone seed to {0} Hz", sampleRate);
        }

        var rendered = RoomtoneRenderer.RenderWithSentenceMasks(
            working,
            roomtone,
            gaps,
            updatedSentences,
            sampleRate,
            toneGainLinear,
            fadeMs,
            debugDirectory: null);

        var analysis = new AudioAnalysisService(rendered);
        double lengthSec = rendered.SampleRate > 0 ? rendered.Length / (double)rendered.SampleRate : 0.0;
        var rms = analysis.AnalyzeGap(0.0, lengthSec);
        Log.Info("PauseAudioApplier output RMS mean={0:F2} dB min={1:F2} dB max={2:F2} dB", rms.MeanRmsDb, rms.MinRmsDb, rms.MaxRmsDb);

        return rendered;
    }

    private static void CopySentence(
        AudioBuffer source,
        AudioBuffer destination,
        TimingRange originalTiming,
        SentenceTiming targetTiming,
        int sampleRate,
        IReadOnlyList<PauseIntraGap> intraGaps)
    {
        int origStart = Math.Clamp((int)Math.Round(originalTiming.StartSec * sampleRate), 0, source.Length);
        int origEnd = Math.Clamp((int)Math.Round(originalTiming.EndSec * sampleRate), origStart, source.Length);
        if (origEnd <= origStart)
        {
            return;
        }

        int targetStart = Math.Clamp((int)Math.Round(targetTiming.StartSec * sampleRate), 0, destination.Length);
        int targetEnd = Math.Clamp((int)Math.Round(targetTiming.EndSec * sampleRate), targetStart, destination.Length);
        if (targetEnd <= targetStart)
        {
            return;
        }

        if (intraGaps.Count == 0)
        {
            int length = Math.Min(origEnd - origStart, targetEnd - targetStart);
            CopyBlock(source, destination, origStart, targetStart, length);
            return;
        }

        int srcCursor = origStart;
        int dstCursor = targetStart;

        foreach (var gap in intraGaps)
        {
            if (gap.SourceEndSec <= originalTiming.StartSec || gap.SourceStartSec >= originalTiming.EndSec)
            {
                continue;
            }

            double gapSourceStartSec = Math.Max(originalTiming.StartSec, gap.SourceStartSec);
            double gapSourceEndSec = Math.Min(originalTiming.EndSec, gap.SourceEndSec);
            int gapSourceStart = Math.Clamp((int)Math.Round(gapSourceStartSec * sampleRate), origStart, origEnd);
            int gapSourceEnd = Math.Clamp((int)Math.Round(gapSourceEndSec * sampleRate), gapSourceStart, origEnd);
            if (gapSourceStart >= origEnd)
            {
                break;
            }

            double gapTargetStartSec = Math.Clamp(gap.TargetStartSec, targetTiming.StartSec, targetTiming.EndSec);
            double gapTargetEndSec = Math.Clamp(gap.TargetEndSec, gapTargetStartSec, targetTiming.EndSec);
            int gapTargetStart = Math.Clamp((int)Math.Round(gapTargetStartSec * sampleRate), targetStart, targetEnd);
            int gapTargetEnd = Math.Clamp((int)Math.Round(gapTargetEndSec * sampleRate), gapTargetStart, targetEnd);

            if (gapTargetStart >= targetEnd)
            {
                break;
            }

            int copyLen = Math.Min(gapSourceStart - srcCursor, targetEnd - dstCursor);
            if (copyLen > 0)
            {
                CopyBlock(source, destination, srcCursor, dstCursor, copyLen);
                srcCursor += copyLen;
                dstCursor += copyLen;
            }

            srcCursor = Math.Min(gapSourceEnd, origEnd);
            dstCursor = Math.Min(Math.Max(dstCursor, gapTargetStart), targetEnd);
            dstCursor = Math.Min(gapTargetEnd, targetEnd);
            if (srcCursor >= origEnd || dstCursor >= targetEnd)
            {
                break;
            }
        }

        int remainingLen = Math.Min(origEnd - srcCursor, targetEnd - dstCursor);
        if (remainingLen > 0)
        {
            CopyBlock(source, destination, srcCursor, dstCursor, remainingLen);
        }
    }

    private static void CopyBlock(AudioBuffer source, AudioBuffer destination, int srcStart, int dstStart, int length)
    {
        if (length <= 0)
        {
            return;
        }

        length = Math.Min(length, source.Length - srcStart);
        length = Math.Min(length, destination.Length - dstStart);
        if (length <= 0)
        {
            return;
        }

        for (int ch = 0; ch < destination.Channels; ch++)
        {
            var src = source.Planar[Math.Min(ch, source.Channels - 1)];
            var dst = destination.Planar[ch];
            Array.Copy(src, srcStart, dst, dstStart, length);
        }
    }

    private static bool IsReliableStatus(string? status)
    {
        return string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private static List<RoomtonePlanGap> BuildPlanGaps(IReadOnlyList<SentenceAlign> updatedSentences, double durationSec, ISet<int>? unreliableSentenceIds)
    {
        var gaps = new List<RoomtonePlanGap>();
        if (updatedSentences.Count == 0)
        {
            return gaps;
        }

        double previousEnd = 0.0;
        int? previousId = null;

        foreach (var sentence in updatedSentences)
        {
            double gapStart = previousEnd;
            double gapEnd = sentence.Timing.StartSec;

            bool leftUnreliable = previousId.HasValue && unreliableSentenceIds is not null && unreliableSentenceIds.Contains(previousId.Value);
            bool rightUnreliable = unreliableSentenceIds is not null && unreliableSentenceIds.Contains(sentence.Id);
            bool skipFill = leftUnreliable && rightUnreliable;

            if (!skipFill && gapEnd - gapStart > 1e-6)
            {
                gaps.Add(new RoomtonePlanGap(
                    gapStart,
                    gapEnd,
                    gapEnd - gapStart,
                    previousId,
                    sentence.Id,
                    MinRmsDb: -60,
                    MaxRmsDb: -60,
                    MeanRmsDb: -60,
                    SilenceFraction: 1.0,
                    BreathRegions: Array.Empty<RoomtoneBreathRegion>()));
            }

            previousEnd = sentence.Timing.EndSec;
            previousId = sentence.Id;
        }

        double finalGapDuration = Math.Max(0.0, durationSec - previousEnd);
        if (finalGapDuration > 1e-6)
        {
            gaps.Add(new RoomtonePlanGap(
                previousEnd,
                previousEnd + finalGapDuration,
                finalGapDuration,
                previousId,
                null,
                MinRmsDb: -60,
                MaxRmsDb: -60,
                MeanRmsDb: -60,
                SilenceFraction: 1.0,
                BreathRegions: Array.Empty<RoomtoneBreathRegion>()));
        }

        return gaps;
    }

    private static bool IsApproximatelyEqual(double a, double b) => Math.Abs(a - b) < DurationEpsilon;
}
