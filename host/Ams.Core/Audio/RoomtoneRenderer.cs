using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Asr;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
    public static class RoomtoneRenderer
    {
        // Return type for structural normalization (entries come back time-shifted)
        public sealed record NormalizeEntriesResult(AudioBuffer Audio, List<SentenceTimelineEntry> Entries);

        /// <summary>
        /// Enforce exact pre-roll/post-chapter/tail by inserting OR trimming roomtone,
        /// then shift all sentence entries so the timeline matches the new audio.
        /// Targets: pre=0.75s, post-chapter=1.50s, tail=3.00s (configurable).
        /// </summary>
        public static NormalizeEntriesResult NormalizeStructureExact(
            AudioBuffer input,
            AudioBuffer roomtoneSeed,
            IReadOnlyList<SentenceTimelineEntry> entries,
            int targetSampleRate,
            double toneGainLinear,
            double preRollSec = 0.75,
            double postChapterPauseSec = 1.50,
            double tailSec = 3.00,
            int overlapMs = 35,
            string? debugDirectory = null)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (roomtoneSeed is null) throw new ArgumentNullException(nameof(roomtoneSeed));
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            var src  = input.SampleRate == targetSampleRate ? input : ResampleLinear(input, targetSampleRate);
            var tone = roomtoneSeed.SampleRate == targetSampleRate ? roomtoneSeed : ResampleLinear(roomtoneSeed, targetSampleRate);
            if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");
            MakeLoopable(tone, seamMs: 15);

            int sr = targetSampleRate;
            int N  = Math.Max(1, (int)Math.Round(overlapMs * 0.001 * sr));

            static bool HasPositiveTiming(SentenceTimelineEntry e) => e.HasTiming && (e.Timing.EndSec - e.Timing.StartSec) > 1e-6;
            static double EffectiveStart(SentenceTimelineEntry e) => HasPositiveTiming(e) ? e.Timing.StartSec : e.Window.StartSec;
            static double EffectiveEnd(SentenceTimelineEntry e) => HasPositiveTiming(e) ? e.Timing.EndSec : e.Window.EndSec;

            var timedEntries = entries.Where(HasPositiveTiming).ToList();

            if (entries.Count == 0)
            {
                int tailTarget0 = (int)Math.Round(tailSec * sr);
                long cursor0 = 0;
                var only = new AudioBuffer(src.Channels, sr, src.Length + tailTarget0);
                for (int ch = 0; ch < src.Channels; ch++) Array.Copy(src.Planar[ch], 0, only.Planar[ch], 0, src.Length);
                if (tailTarget0 > 0)
                    ReplaceGapWithRoomtoneClassic(only, only, tone, src.Length, src.Length + tailTarget0, toneGainLinear, overlapMs, ref cursor0);
                return new NormalizeEntriesResult(only, new List<SentenceTimelineEntry>());
            }

            SentenceTimelineEntry ResolveFirst() => timedEntries.Count > 0 ? timedEntries[0] : entries[0];
            SentenceTimelineEntry? ResolveSecond(SentenceTimelineEntry first) => timedEntries.FirstOrDefault(e => e.SentenceId != first.SentenceId)
                ?? entries.SkipWhile(e => e.SentenceId == first.SentenceId).FirstOrDefault(e => e.SentenceId != first.SentenceId);
            SentenceTimelineEntry ResolveLast() => timedEntries.Count > 0 ? timedEntries[^1] : entries[^1];

            var firstTimed = ResolveFirst();
            var secondTimed = ResolveSecond(firstTimed);
            var lastTimed = ResolveLast();

            static SentenceTimelineEntry ShiftEntry(SentenceTimelineEntry e, double shift, double maxTime)
            {
                var t = e.Timing;
                var w = e.Window;
                double s1 = Math.Clamp(t.StartSec + shift, 0, maxTime);
                double e1 = Math.Clamp(t.EndSec   + shift, 0, maxTime);
                if (e1 < s1) e1 = s1;

                double ws = Math.Clamp(w.StartSec + shift, 0, maxTime);
                double we = Math.Clamp(w.EndSec   + shift, 0, maxTime);
                if (we < ws) we = ws;

                return new SentenceTimelineEntry(
                    e.SentenceId,
                    new SentenceTiming(s1, e1, t.FragmentBacked, t.Confidence),
                    e.HasTiming,
                    new SentenceTiming(ws, we, w.FragmentBacked, w.Confidence));
            }

            var cur = CopyBuffer(src);
            long toneCursor = 0;

            double firstStartSec = EffectiveStart(firstTimed);
            double firstEndSec = EffectiveEnd(firstTimed);
            double secondStartSec = secondTimed is not null ? EffectiveStart(secondTimed) : double.PositiveInfinity;
            double secondStartOriginal = secondStartSec;
            double lastEndSec = EffectiveEnd(lastTimed);

            int preTargetSamples = (int)Math.Round(preRollSec * sr);
            int postTargetSamples = (int)Math.Round(postChapterPauseSec * sr);
            int tailTargetSamples = (int)Math.Round(tailSec * sr);

            int firstStartSamples = (int)Math.Round(firstStartSec * sr);
            int deltaPreSamples = preTargetSamples - firstStartSamples;
            if (deltaPreSamples > 0)
            {
                cur = InsertSilence(cur, 0, deltaPreSamples);
            }
            else if (deltaPreSamples < 0)
            {
                cur = RemoveSamples(cur, 0, -deltaPreSamples);
            }
            ReplaceGapWithRoomtoneClassic(cur, cur, tone, 0, preTargetSamples, toneGainLinear, overlapMs, ref toneCursor);

            double shiftPreSec = deltaPreSamples / (double)sr;
            firstStartSec = preRollSec;
            firstEndSec += shiftPreSec;
            if (!double.IsPositiveInfinity(secondStartSec))
            {
                secondStartSec += shiftPreSec;
            }
            lastEndSec += shiftPreSec;

            double shiftAfterChapterSec = shiftPreSec;
            if (!double.IsPositiveInfinity(secondStartSec))
            {
                int firstEndSamples = (int)Math.Round(firstEndSec * sr);
                int currentSecondStartSamples = (int)Math.Round(secondStartSec * sr);
                int currentGapSamples = currentSecondStartSamples - firstEndSamples;
                int deltaGapSamples = postTargetSamples - currentGapSamples;
                if (deltaGapSamples > 0)
                {
                    cur = InsertSilence(cur, firstEndSamples, deltaGapSamples);
                }
                else if (deltaGapSamples < 0)
                {
                    cur = RemoveSamples(cur, firstEndSamples, -deltaGapSamples);
                }

                ReplaceGapWithRoomtoneClassic(cur, cur, tone, firstEndSamples, firstEndSamples + postTargetSamples, toneGainLinear, overlapMs, ref toneCursor);

                double gapShiftSec = deltaGapSamples / (double)sr;
                if (!double.IsPositiveInfinity(secondStartSec))
                {
                    secondStartSec += gapShiftSec;
                }
                lastEndSec += gapShiftSec;
                shiftAfterChapterSec = shiftPreSec + gapShiftSec;
            }

            int lastEndSamples = (int)Math.Round(lastEndSec * sr);
            int desiredTotalSamples = lastEndSamples + tailTargetSamples;
            int deltaTailSamples = desiredTotalSamples - cur.Length;
            if (deltaTailSamples > 0)
            {
                cur = InsertSilence(cur, cur.Length, deltaTailSamples);
            }
            else if (deltaTailSamples < 0)
            {
                cur = RemoveSamples(cur, desiredTotalSamples, -deltaTailSamples);
            }

            ReplaceGapWithRoomtoneClassic(cur, cur, tone, desiredTotalSamples - tailTargetSamples, desiredTotalSamples, toneGainLinear, overlapMs, ref toneCursor);

            double newDurationSec = cur.Length / (double)sr;
            double secondBoundary = secondStartOriginal;

            var shifted = new List<SentenceTimelineEntry>(entries.Count);
            foreach (var e in entries)
            {
                double probe = EffectiveStart(e);
                double shift = probe >= secondBoundary ? shiftAfterChapterSec : shiftPreSec;
                shifted.Add(ShiftEntry(e, shift, newDurationSec));
            }

            WriteDebug(debugDirectory, "struct_after", cur);
            return new NormalizeEntriesResult(cur, shifted);
        }

        /// <summary>
        /// Fill non-speech regions with looped room tone while preserving speech AND non-ASR performative audio.
        /// Strategy:
        ///   1) Start with a full copy of the source (prevents accidental silence).
        ///   2) Build a keep mask = Sentences ∪ EnergyKeeps (global, adaptive RMS + micro-guards).
        ///   3) Apply structural fills (pre-roll, first inter-sentence pause, tail) regardless of the keep mask.
        ///   4) Interior gaps are the complement of the keep mask (optionally intersected with plan 'gaps').
        ///   5) Replace each interior gap with room tone using in-gap crossfades.
        /// </summary>
        public static AudioBuffer RenderWithSentenceMasks(
            AudioBuffer input,
            AudioBuffer roomtoneSeed,
            IReadOnlyList<RoomtonePlanGap> gaps, // optional plan gaps
            IReadOnlyList<SentenceAlign> sentences,
            int targetSampleRate,
            double toneGainLinear,
            double fadeMs,
            string? debugDirectory = null)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (roomtoneSeed is null) throw new ArgumentNullException(nameof(roomtoneSeed));
            if (sentences is null) throw new ArgumentNullException(nameof(sentences));
            if (targetSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(targetSampleRate));

            // Resample to a common rate
            var src = input.SampleRate == targetSampleRate ? input : ResampleLinear(input, targetSampleRate);
            var tone = roomtoneSeed.SampleRate == targetSampleRate
                ? roomtoneSeed
                : ResampleLinear(roomtoneSeed, targetSampleRate);
            if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");

            MakeLoopable(tone, seamMs: 20); // make roomtone seed loopable

            WriteDebug(debugDirectory, "step0_source", src);
            WriteDebug(debugDirectory, "step0_roomtone_seed", tone);

            // 1) Sentence keep ranges (coalesced)
            var sentRanges = BuildSentenceRanges(sentences, src.SampleRate, src.Length);

            // 2) Global energy keeps (adaptive + micro-guards to reduce false positives)
            var energyKeeps = DetectEnergyKeepsGlobal(
                src,
                windowMs: 25, stepMs: 2,
                enterAboveFloorDb: 10,
                exitBelowEnterDb: 4,
                minKeepMs: 30,
                minHoleMs: 15,
                prePadMs: 8,
                postPadMs: 15,
                modulationDb: 0.8,
                speechBandFloorDeltaDb: -12.0
            );

            // 3) Keep mask = Sentences ∪ EnergyKeeps
            var keepMask = Coalesce(sentRanges.Concat(energyKeeps).ToList());

            // 4) Plan ranges (optional) and interior gaps
            var planGapRanges = (gaps != null && gaps.Count > 0)
                ? BuildGapRanges(gaps, src.SampleRate, src.Length)
                : new List<(int, int)>();

            var (firstStart, lastEnd) = FirstLastSentenceBounds(sentRanges, src.Length);

            var interiorGaps = ComputeInteriorGaps(keepMask, firstStart, lastEnd);
            if (planGapRanges.Count > 0)
            {
                interiorGaps = IntersectRanges(interiorGaps, planGapRanges);
            }

            var finalGaps = Coalesce(interiorGaps);

            // Debug masks
            WriteMaskDebug(debugDirectory, "step0b_keep_energy", energyKeeps, src);
            WriteMaskDebug(debugDirectory, "step0c_sentences", sentRanges, src);
            WriteMaskDebug(debugDirectory, "step0d_gaps_final", finalGaps, src);

            // 5) Out starts as a full copy of source (prevents accidental silence)
            var outBuf = CopyBuffer(src);

            int fadeSamples = fadeMs <= 0 ? 0 : (int)Math.Round(fadeMs * 0.001 * src.SampleRate);
            long toneCursor = 0;

            // 6) Fill structural gaps explicitly
            ApplyStructuralGaps(outBuf, src, tone, sentences, toneGainLinear, fadeSamples, ref toneCursor);

            // 7) Replace interior gaps with room tone
            foreach (var (g0, g1) in finalGaps)
            {
                ReplaceGapWithRoomtone(outBuf, src, tone, g0, g1, toneGainLinear, fadeSamples, ref toneCursor);
            }

            WriteDebug(debugDirectory, "step1_fill_gaps", outBuf);
            return outBuf;
        }

        // ---------------------------------------------------------------------------------
        // Core synthesis
        // ---------------------------------------------------------------------------------

        // Call once when loading roomtoneSeed (before RenderWithSentenceMasks)
        private static void MakeLoopable(AudioBuffer tone, int seamMs = 20)
        {
            int sr = tone.SampleRate;
            int N = Math.Max(1, (int)Math.Round(seamMs * 0.001 * sr));
            if (N * 2 >= tone.Length) return; // seed too short; skip

            for (int ch = 0; ch < tone.Channels; ch++)
            {
                var p = tone.Planar[ch];
                int a0 = 0; // start region [a0 .. a0+N)
                int b0 = tone.Length - N; // end region [b0 .. b0+N)

                for (int i = 0; i < N; i++)
                {
                    double t = (double)(i + 1) / (N + 1); // 0..1
                    double gHead = Math.Sqrt(1.0 - t); // equal-power
                    double gTail = Math.Sqrt(t);

                    // Overwrite the head with an overlap-add of head+tail
                    p[a0 + i] = (float)(gHead * p[a0 + i] + gTail * p[b0 + i]);
                }
            }
        }

        /// <summary>
        /// Replace [g0,g1) with roomtone. If fadeSamples > 0, crossfade from src→tone at the head
        /// and tone→src at the tail, both inside the gap. toneCursor advances continuously.
        /// </summary>
        private static void ReplaceGapWithRoomtone(
            AudioBuffer dst, AudioBuffer src, AudioBuffer tone,
            int g0, int g1, double gain, int fadeSamples, ref long toneCursor)
        {
            int sr = dst.SampleRate;
            int overlapSamples = fadeSamples > 0 ? fadeSamples : Math.Max(1, (int)Math.Round(0.01 * sr));
            int overlapMs = Math.Max(1, (int)Math.Round(overlapSamples * 1000.0 / sr));
            ReplaceGapWithRoomtoneClassic(dst, src, tone, g0, g1, gain, overlapMs, ref toneCursor);
        }

        // Classic overlap-add: blend src↔tone across the OUTSIDE edges of the gap.
        // left overlap region:  [g0 - overlap, g0)
        // core tone-only region:[g0, g1)
        // right overlap region: [g1, g1 + overlap)
        private static void ReplaceGapWithRoomtoneClassic(
            AudioBuffer dst, AudioBuffer src, AudioBuffer tone,
            int g0, int g1,
            double baseGainLinear,
            int overlapMs, // e.g., 30–60 ms
            ref long toneCursor)
        {
            int sr = dst.SampleRate;
            int overlapSamples = Math.Max(1, (int)Math.Round(overlapMs * 0.001 * sr));

            g0 = Math.Clamp(g0, 0, dst.Length);
            g1 = Math.Clamp(g1, g0, dst.Length);
            int gapLen = g1 - g0;
            int leftOv = Math.Min(overlapSamples, g0);
            int rightOv = Math.Min(overlapSamples, dst.Length - g1);

            if (gapLen <= 0 && leftOv == 0 && rightOv == 0) return;

            const int wrapXfadeMs = 12; // 10–20 ms
            int wrapX = Math.Max(1, (int)Math.Round(wrapXfadeMs * 0.001 * sr));

            float ReadToneSample(float[] tt, long curPlusI)
            {
                int tl = tt.Length;
                int idx = (int)(curPlusI % tl);
                if (idx < wrapX && tl >= wrapX)
                {
                    int iHead = idx;
                    int iTail = (tl - wrapX + idx) % tl;
                    double t = wrapX == 1 ? 1.0 : (double)iHead / (wrapX - 1);
                    double gH = Math.Sqrt(t);
                    double gT = Math.Sqrt(1.0 - t);
                    return (float)(gT * tt[iTail] + gH * tt[iHead]);
                }
                return tt[idx];
            }

            // LEFT OVERLAP
            for (int i = 0; i < leftOv; i++)
            {
                int idx = g0 - leftOv + i;
                double t = (double)(i + 1) / (leftOv + 1);
                double gTone = Math.Sqrt(t);
                double gSrc = Math.Sqrt(1.0 - t);
                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    var d = dst.Planar[ch];
                    var s = src.Planar[ch % src.Channels];
                    var tt = tone.Planar[ch % tone.Channels];
                    float toneSample = ReadToneSample(tt, toneCursor + i);
                    d[idx] = (float)(gSrc * s[idx] + gTone * (baseGainLinear * toneSample));
                }
            }
            toneCursor += leftOv;

            // CORE
            if (gapLen > 0)
            {
                for (int i = 0; i < gapLen; i++)
                {
                    for (int ch = 0; ch < dst.Channels; ch++)
                    {
                        var d = dst.Planar[ch];
                        var tt = tone.Planar[ch % tone.Channels];
                        float toneSample = ReadToneSample(tt, toneCursor + i);
                        d[g0 + i] = (float)(baseGainLinear * toneSample);
                    }
                }
                toneCursor += gapLen;
            }

            // RIGHT OVERLAP
            for (int i = 0; i < rightOv; i++)
            {
                int idx = g1 + i;
                double t = (double)(i + 1) / (rightOv + 1);
                double gSrc = Math.Sqrt(t);
                double gTone = Math.Sqrt(1.0 - t);
                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    var d = dst.Planar[ch];
                    var s = src.Planar[ch % src.Channels];
                    var tt = tone.Planar[ch % tone.Channels];
                    float toneSample = ReadToneSample(tt, toneCursor + i);
                    d[idx] = (float)(gTone * (baseGainLinear * toneSample) + gSrc * s[idx]);
                }
            }
            toneCursor += rightOv;
        }

        private static AudioBuffer InsertSilence(AudioBuffer source, int position, int samples)
        {
            if (samples <= 0) return source;
            position = Math.Clamp(position, 0, source.Length);
            var dst = new AudioBuffer(source.Channels, source.SampleRate, source.Length + samples);
            for (int ch = 0; ch < source.Channels; ch++)
            {
                Array.Copy(source.Planar[ch], 0, dst.Planar[ch], 0, position);
                Array.Copy(source.Planar[ch], position, dst.Planar[ch], position + samples, source.Length - position);
            }
            return dst;
        }

        private static AudioBuffer RemoveSamples(AudioBuffer source, int position, int samples)
        {
            if (samples <= 0) return source;
            position = Math.Clamp(position, 0, Math.Max(0, source.Length - samples));
            var dst = new AudioBuffer(source.Channels, source.SampleRate, source.Length - samples);
            for (int ch = 0; ch < source.Channels; ch++)
            {
                Array.Copy(source.Planar[ch], 0, dst.Planar[ch], 0, position);
                Array.Copy(source.Planar[ch], position + samples, dst.Planar[ch], position, source.Length - (position + samples));
            }
            return dst;
        }

        private static void ApplyStructuralGaps(
            AudioBuffer dst,
            AudioBuffer src,
            AudioBuffer tone,
            IReadOnlyList<SentenceAlign> sentences,
            double toneGainLinear,
            int fadeSamples,
            ref long toneCursor)
        {
            if (sentences.Count == 0) return;

            const double preRollSec = 0.75;
            const double postChapterPauseSec = 1.5;
            const double tailSec = 3.0;

            int sampleRate = dst.SampleRate;
            int totalSamples = dst.Length;
            double audioDurationSec = totalSamples / (double)sampleRate;

            int ToSample(double seconds) => Math.Clamp((int)Math.Round(seconds * sampleRate), 0, totalSamples);

            var first = sentences[0];
            double firstStartSec = Math.Clamp(first.Timing.StartSec, 0.0, audioDurationSec);
            double firstPreStartSec = Math.Clamp(firstStartSec - preRollSec, 0.0, audioDurationSec);
            if (firstStartSec > firstPreStartSec)
            {
                int startSample = ToSample(firstPreStartSec);
                int endSample = ToSample(firstStartSec);
                if (endSample > startSample)
                    ReplaceGapWithRoomtone(dst, src, tone, startSample, endSample, toneGainLinear, fadeSamples, ref toneCursor);
            }

            if (sentences.Count > 1)
            {
                var second = sentences[1];
                double gapStartSec = Math.Clamp(first.Timing.EndSec, 0.0, audioDurationSec);
                double gapEndSec = Math.Min(Math.Clamp(second.Timing.StartSec, 0.0, audioDurationSec), gapStartSec + postChapterPauseSec);
                if (gapEndSec > gapStartSec)
                {
                    int startSample = ToSample(gapStartSec);
                    int endSample = ToSample(gapEndSec);
                    if (endSample > startSample)
                        ReplaceGapWithRoomtone(dst, src, tone, startSample, endSample, toneGainLinear, fadeSamples, ref toneCursor);
                }
            }

            var last = sentences[^1];
            double tailStartSec = Math.Clamp(last.Timing.EndSec, 0.0, audioDurationSec);
            double tailEndSec = Math.Min(audioDurationSec, tailStartSec + tailSec);
            if (tailEndSec > tailStartSec)
            {
                int startSample = ToSample(tailStartSec);
                int endSample = ToSample(tailEndSec);
                if (endSample > startSample)
                    ReplaceGapWithRoomtone(dst, src, tone, startSample, endSample, toneGainLinear, fadeSamples, ref toneCursor);
            }
        }

        // ---------------------------------------------------------------------------------
        // Keep detection (global, adaptive) with micro-guards for false positives
        // ---------------------------------------------------------------------------------

        private static List<(int Start, int End)> DetectEnergyKeepsGlobal(
            AudioBuffer src,
            int windowMs, int stepMs,
            double enterAboveFloorDb,
            double exitBelowEnterDb,
            int minKeepMs, int minHoleMs,
            int prePadMs, int postPadMs,
            double modulationDb,
            double speechBandFloorDeltaDb)
        {
            int sr = src.SampleRate;
            int win = Math.Max(1, (int)Math.Round(windowMs * 0.001 * sr));
            int hop = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));
            int minKeep = Math.Max(hop, (int)Math.Round(minKeepMs * 0.001 * sr));
            int minHole = Math.Max(hop, (int)Math.Round(minHoleMs * 0.001 * sr));
            int pre = Math.Max(0, (int)Math.Round(prePadMs * 0.001 * sr));
            int post = Math.Max(0, (int)Math.Round(postPadMs * 0.001 * sr));

            var dbs = new List<double>(Math.Max(16, src.Length / Math.Max(1, hop)));
            for (int p = 0; p < src.Length; p += hop)
            {
                int len = Math.Min(win, src.Length - p);
                if (len <= 0) break;
                dbs.Add(ToDb(RmsAllCh(src, p, len)));
            }

            if (dbs.Count == 0) return new List<(int, int)>();
            dbs.Sort();
            double floorDb = dbs[(int)Math.Floor(0.10 * (dbs.Count - 1))];

            double enterDb = floorDb + enterAboveFloorDb;
            double exitDb = enterDb - exitBelowEnterDb;
            double peakHatchDb = floorDb + 12.0;

            var ranges = new List<(int, int)>();
            bool inKeep = false;
            int keepStart = 0;

            double prevDb = double.NegativeInfinity;

            for (int p = 0; p < src.Length; p += hop)
            {
                int len = Math.Min(win, src.Length - p);
                if (len <= 0) break;

                double rms = RmsAllCh(src, p, len);
                double rDb = ToDb(rms);
                double peak = PeakAllCh(src, p, len);
                double peakDb = ToDb(peak);

                double dDb = double.IsNegativeInfinity(prevDb) ? 0.0 : Math.Abs(rDb - prevDb);
                prevDb = rDb;

                double diff = DiffRmsCh0(src, p, len);
                double diffDb = ToDb(diff);
                double hfRatioDb = diffDb - rDb;

                bool baseEnter = (rDb >= enterDb);
                bool baseStay  = (rDb >= exitDb);
                bool enterGuards = (dDb >= modulationDb && hfRatioDb >= speechBandFloorDeltaDb) || (peakDb >= peakHatchDb);
                bool active = (!inKeep && baseEnter && enterGuards) || (inKeep && baseStay);

                if (!inKeep && active) { inKeep = true; keepStart = p; }
                else if (inKeep && !active)
                {
                    int keepEnd = p;

                    int holeProbe = Math.Min(p + minHole, src.Length);
                    bool resumes = false;
                    for (int q = p; q < holeProbe; q += hop)
                    {
                        int l2 = Math.Min(win, src.Length - q);
                        if (l2 <= 0) break;
                        double rq = ToDb(RmsAllCh(src, q, l2));
                        double dq = ToDb(DiffRmsCh0(src, q, l2)) - rq;
                        double dd = double.IsNegativeInfinity(prevDb) ? 0.0 : Math.Abs(rq - rDb);
                        bool guards = (dd >= modulationDb && dq >= speechBandFloorDeltaDb) ||
                                      (ToDb(PeakAllCh(src, q, l2)) >= peakHatchDb);
                        if (rq >= enterDb && guards) { resumes = true; break; }
                    }

                    if (!resumes)
                    {
                        int s = Math.Max(0, keepStart - pre);
                        int e = Math.Min(src.Length, keepEnd + post);
                        if (e - s >= minKeep) ranges.Add((s, e));
                        inKeep = false;
                    }
                }
            }

            if (inKeep)
            {
                int s = Math.Max(0, keepStart - pre);
                int e = src.Length;
                if (e - s >= minKeep) ranges.Add((s, e));
            }

            return Coalesce(ranges);
        }

        // ---------------- gap construction & utilities (unchanged) ----------------

        private static (int firstStart, int lastEnd) FirstLastSentenceBounds(List<(int Start, int End)> sentences, int total)
        {
            if (sentences.Count == 0) return (0, total);
            int firstStart = sentences[0].Start;
            int lastEnd = sentences[^1].End;
            return (firstStart, lastEnd);
        }

        private static List<(int Start, int End)> ComputeInteriorGaps(List<(int Start, int End)> keepMask, int firstStart, int lastEnd)
        {
            var interiorMask = new List<(int, int)>();
            foreach (var (s, e) in keepMask)
            {
                int cs = Math.Max(s, firstStart);
                int ce = Math.Min(e, lastEnd);
                if (ce > cs) interiorMask.Add((cs, ce));
            }

            interiorMask = Coalesce(interiorMask);

            var gaps = new List<(int, int)>();
            if (lastEnd <= firstStart) return gaps;

            int cursor = firstStart;
            foreach (var (s, e) in interiorMask)
            {
                if (s > cursor) gaps.Add((cursor, s));
                cursor = Math.Max(cursor, e);
            }
            if (cursor < lastEnd) gaps.Add((cursor, lastEnd));
            return gaps;
        }

        private static List<(int Start, int End)> IntersectRanges(List<(int Start, int End)> a, List<(int Start, int End)> b)
        {
            var A = Coalesce(a);
            var B = Coalesce(b);
            var outL = new List<(int, int)>();
            int i = 0, j = 0;
            while (i < A.Count && j < B.Count)
            {
                var (as0, ae0) = A[i];
                var (bs0, be0) = B[j];
                int s = Math.Max(as0, bs0);
                int e = Math.Min(ae0, be0);
                if (e > s) outL.Add((s, e));
                if (ae0 < be0) i++; else j++;
            }
            return outL;
        }

        private static List<(int Start, int End)> Coalesce(List<(int Start, int End)> ranges)
        {
            if (ranges.Count == 0) return ranges;
            ranges.Sort((x, y) => x.Start != y.Start ? x.Start.CompareTo(y.Start) : x.End.CompareTo(y.End));
            var outL = new List<(int, int)>(ranges.Count);
            var cur = ranges[0];
            for (int k = 1; k < ranges.Count; k++)
            {
                var nx = ranges[k];
                if (nx.Start < cur.End) cur = (cur.Start, Math.Max(cur.End, nx.End));
                else { outL.Add(cur); cur = nx; }
            }
            outL.Add(cur);
            return outL;
        }

        private static List<(int Start, int End)> BuildSentenceRanges(IReadOnlyList<SentenceAlign> sentences, int sampleRate, int totalSamples)
        {
            var ranges = new List<(int Start, int End)>(Math.Max(8, sentences.Count));
            foreach (var sentence in sentences)
            {
                var t = sentence.Timing;
                if (t.EndSec <= t.StartSec) continue;
                int s = Math.Clamp((int)Math.Round(t.StartSec * sampleRate), 0, totalSamples);
                int e = Math.Clamp((int)Math.Round(t.EndSec   * sampleRate), 0, totalSamples);
                if (e > s) ranges.Add((s, e));
            }
            return Coalesce(ranges);
        }

        private static List<(int Start, int End)> BuildGapRanges(IReadOnlyList<RoomtonePlanGap> gaps, int sampleRate, int totalSamples)
        {
            var ranges = new List<(int, int)>(gaps.Count);
            foreach (var g in gaps)
            {
                int s = Math.Clamp((int)Math.Round(g.StartSec * sampleRate), 0, totalSamples);
                int e = Math.Clamp((int)Math.Round(g.EndSec   * sampleRate), 0, totalSamples);
                if (e > s) ranges.Add((s, e));
            }
            return Coalesce(ranges);
        }

        private static AudioBuffer CopyBuffer(AudioBuffer src)
        {
            var dst = new AudioBuffer(src.Channels, src.SampleRate, src.Length);
            for (int ch = 0; ch < src.Channels; ch++)
                Array.Copy(src.Planar[ch], 0, dst.Planar[ch], 0, src.Length);
            return dst;
        }

        private static double RmsAllCh(AudioBuffer buf, int start, int len)
        {
            double sum = 0.0; int count = 0;
            int end = Math.Min(buf.Length, start + len);
            for (int ch = 0; ch < buf.Channels; ch++)
            {
                var p = buf.Planar[ch];
                for (int i = start; i < end; i++) { double s = p[i]; sum += s * s; count++; }
            }
            return count == 0 ? 0.0 : Math.Sqrt(sum / count);
        }

        private static double PeakAllCh(AudioBuffer buf, int start, int len)
        {
            int end = Math.Min(buf.Length, start + len);
            float maxAbs = 0f;
            for (int ch = 0; ch < buf.Channels; ch++)
            {
                var p = buf.Planar[ch];
                for (int i = start; i < end; i++)
                {
                    float a = MathF.Abs(p[i]);
                    if (a > maxAbs) maxAbs = a;
                }
            }
            return maxAbs;
        }

        private static double DiffRmsCh0(AudioBuffer buf, int start, int len)
        {
            int end = Math.Min(buf.Length, start + len);
            if (end - start < 2) return 0.0;
            double sum = 0.0;
            var p = buf.Planar[0];
            float prev = p[start];
            for (int i = start + 1; i < end; i++)
            {
                float d = p[i] - prev;
                sum += d * d;
                prev = p[i];
            }
            return Math.Sqrt(sum / (end - start - 1));
        }

        private static double ToDb(double linear) =>
            linear <= 0 ? double.NegativeInfinity : 20.0 * Math.Log10(linear);

        private static void WriteDebug(string? directory, string suffix, AudioBuffer buffer)
        {
            if (string.IsNullOrWhiteSpace(directory) || buffer is null) return;
            try
            {
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, $"roomtone.{suffix}.wav");
                WavIo.WriteFloat32(path, buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Roomtone] Debug write failed for {suffix}: {ex.Message}");
            }
        }

        private static void WriteMaskDebug(string? directory, string suffix, List<(int Start, int End)> ranges, AudioBuffer like)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;
            try
            {
                var m = new AudioBuffer(like.Channels, like.SampleRate, like.Length);
                foreach (var (s, e) in ranges)
                {
                    int len = e - s; if (len <= 0) continue;
                    for (int ch = 0; ch < m.Channels; ch++)
                    {
                        var p = m.Planar[ch];
                        for (int i = s; i < e; i++) p[i] = 0.25f;
                    }
                }
                WriteDebug(directory, suffix, m);
            }
            catch { /* no-op */ }
        }

        /// <summary>Very simple per-channel linear resampler. Enough for roomtone and scratch output.</summary>
        private static AudioBuffer ResampleLinear(AudioBuffer src, int targetSampleRate)
        {
            if (src.SampleRate == targetSampleRate) return src;

            double ratio = (double)targetSampleRate / src.SampleRate;
            int outLen = Math.Max(1, (int)Math.Round(src.Length * ratio));
            var dst = new AudioBuffer(src.Channels, targetSampleRate, outLen);

            for (int ch = 0; ch < src.Channels; ch++)
            {
                var s = src.Planar[ch];
                var d = dst.Planar[ch];
                for (int i = 0; i < outLen; i++)
                {
                    double pos = i / ratio;
                    int i0 = (int)Math.Floor(pos);
                    int i1 = Math.Min(i0 + 1, src.Length - 1);
                    double frac = pos - i0;

                    float y0 = s[Math.Clamp(i0, 0, src.Length - 1)];
                    float y1 = s[i1];
                    d[i] = (float)((1.0 - frac) * y0 + frac * y1);
                }
            }
            return dst;
        }
    }
}

