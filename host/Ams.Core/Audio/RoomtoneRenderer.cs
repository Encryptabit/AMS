using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Asr;
using Ams.Core.Common;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
    public static class RoomtoneRenderer
    {
        internal const double DefaultPreRollSec = 0.75;
        internal const double DefaultPostChapterPauseSec = 1.50;
        internal const double DefaultTailSec = 3.00;

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
            double preRollSec = DefaultPreRollSec,
            double postChapterPauseSec = DefaultPostChapterPauseSec,
            double tailSec = DefaultTailSec,
            int overlapMs = 65,
            string? debugDirectory = null)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (roomtoneSeed is null) throw new ArgumentNullException(nameof(roomtoneSeed));
            if (entries is null) throw new ArgumentNullException(nameof(entries));

            // Work at the target rate the rest of the pipeline uses
            var src  = input.SampleRate == targetSampleRate ? input : RoomtoneUtils.ResampleLinear(input, targetSampleRate);
            var tone = roomtoneSeed.SampleRate == targetSampleRate ? roomtoneSeed : RoomtoneUtils.ResampleLinear(roomtoneSeed, targetSampleRate);
            if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");
            MakeLoopable(tone, seamMs: 60);

            int sr = targetSampleRate;
            int N  = Math.Max(1, (int)Math.Round(overlapMs * 0.001 * sr));

            if (entries.Count == 0)
            {
                // No sentences: enforce exact tail only
                int tailTarget0 = (int)Math.Round(tailSec * sr);
                long cursor = 0;
                var only = new AudioBuffer(src.Channels, sr, src.Length + tailTarget0);
                for (int ch = 0; ch < src.Channels; ch++) Array.Copy(src.Planar[ch], 0, only.Planar[ch], 0, src.Length);
                if (tailTarget0 > 0)
                    ReplaceGapWithRoomtoneClassic(only, only, tone, src.Length, src.Length + tailTarget0, toneGainLinear, overlapMs, ref cursor);
                return new NormalizeEntriesResult(only, new List<SentenceTimelineEntry>());
            }

            // Helpers to shift timeline entries
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

            // Current working audio
            var cur = RoomtoneUtils.CopyBuffer(src);
            long toneCursor = 0;

            // Original markers (seconds)
            var first  = entries[0];
            var last   = entries[^1];
            var second = entries.Count > 1 ? entries[1] : null;

            double firstStartSec  = first.Timing.StartSec;
            double firstEndSec    = first.Timing.EndSec;
            double secondStartSec = second?.Timing.StartSec ?? firstEndSec;
            double lastEndSec     = last.Timing.EndSec;

            // Targets (samples)
            int preTarget    = (int)Math.Round(preRollSec          * sr);
            int postTarget   = (int)Math.Round(postChapterPauseSec * sr);
            int tailTarget   = (int)Math.Round(tailSec             * sr);

            // Existing (samples)
            int preExisting  = (int)Math.Round(firstStartSec * sr);
            int postExisting = (int)Math.Round((secondStartSec - firstEndSec) * sr);
            int tailExisting = src.Length - (int)Math.Round(lastEndSec * sr);

            // Deltas we want to apply (samples) (+ insert, − trim)
            int dPre   = preTarget  - preExisting;
            int dPost  = postTarget - postExisting;
            int dTail  = tailTarget - tailExisting;

            // ------------------ PRE-ROLL ------------------
            if (dPre > 0)
            {
                // Insert roomtone at the very start; crossfade into original
                var dst = new AudioBuffer(cur.Channels, sr, cur.Length + dPre);
                for (int ch = 0; ch < cur.Channels; ch++)
                    Array.Copy(cur.Planar[ch], 0, dst.Planar[ch], dPre, cur.Length);

                ReplaceGapWithRoomtoneClassic(dst, dst, tone, 0, dPre, toneGainLinear, overlapMs, ref toneCursor);
                cur = dst;
            }
            else if (dPre < 0)
            {
                // Trim, but keep 'ov' samples for a clean equal-power crossfade at the new boundary.
                int trim = -dPre;
                int ov   = Math.Min(N, preTarget);
                int core = Math.Min(preExisting, trim + ov);

                var dst = new AudioBuffer(cur.Channels, sr, cur.Length - core);
                for (int ch = 0; ch < cur.Channels; ch++)
                    Array.Copy(cur.Planar[ch], core, dst.Planar[ch], 0, cur.Length - core);

                ReplaceGapWithRoomtoneClassic(dst, dst, tone, 0, ov, toneGainLinear, overlapMs, ref toneCursor);
                cur = dst;
            }

            // All times shift by dPre/sr from here forward
            double shiftPreSec = dPre / (double)sr;

            // ---------------- POST-CHAPTER (gap between 1st & 2nd) ----------------
            if (second is not null)
            {
                // Position of the join after pre-roll step
                int firstEndAfter  = (int)Math.Round((firstEndSec + shiftPreSec) * sr);

                if (dPost > 0)
                {
                    // Insert roomtone between first end and second start
                    int insertPos = firstEndAfter;
                    var dst = new AudioBuffer(cur.Channels, sr, cur.Length + dPost);

                    // Left
                    for (int ch = 0; ch < cur.Channels; ch++)
                        Array.Copy(cur.Planar[ch], 0, dst.Planar[ch], 0, insertPos);
                    // Right (shifted)
                    for (int ch = 0; ch < cur.Channels; ch++)
                        Array.Copy(cur.Planar[ch], insertPos, dst.Planar[ch], insertPos + dPost, cur.Length - insertPos);

                    ReplaceGapWithRoomtoneClassic(dst, dst, tone, insertPos, insertPos + dPost, toneGainLinear, overlapMs, ref toneCursor);
                    cur = dst;
                }
                else if (dPost < 0)
                {
                    // Trim down the gap; reserve 'ov' to crossfade tone between the halves.
                    int trim = -dPost;
                    int ov   = Math.Min(N, postTarget);
                    int core = Math.Min(postExisting, trim + ov);

                    int joinL = firstEndAfter;          // left join index in current audio
                    var dst = new AudioBuffer(cur.Channels, sr, cur.Length - core);

                    // Left part unchanged
                    for (int ch = 0; ch < cur.Channels; ch++)
                        Array.Copy(cur.Planar[ch], 0, dst.Planar[ch], 0, joinL);

                    // Right part jumps ahead by 'core'; we leave 'ov' samples as the crossfade gap
                    for (int ch = 0; ch < cur.Channels; ch++)
                        Array.Copy(cur.Planar[ch], joinL + core, dst.Planar[ch], joinL, cur.Length - (joinL + core));

                    // Crossfade 'ov' samples of tone between halves (keeps total duration exact)
                    ReplaceGapWithRoomtoneClassic(dst, dst, tone, joinL, joinL + ov, toneGainLinear, overlapMs, ref toneCursor);
                    cur = dst;
                }
            }

            // Sentences at or after the 2nd get this additional shift
            double shiftAfterChapterSec = shiftPreSec + (dPost / (double)sr);

            // ------------------ TAIL ------------------
            // Recompute last end after previous steps
            int lastEndAfter = (int)Math.Round((lastEndSec + shiftAfterChapterSec) * sr);
            int tailAfter    = cur.Length - lastEndAfter;
            int dTailNow     = tailTarget - tailAfter;

            if (dTailNow > 0)
            {
                int add = dTailNow;
                var dst = new AudioBuffer(cur.Channels, sr, cur.Length + add);
                for (int ch = 0; ch < cur.Channels; ch++)
                    Array.Copy(cur.Planar[ch], 0, dst.Planar[ch], 0, cur.Length);

                ReplaceGapWithRoomtoneClassic(dst, dst, tone, cur.Length, cur.Length + add, toneGainLinear, overlapMs, ref toneCursor);
                cur = dst;
            }
            else if (dTailNow < 0)
            {
                int trim = -dTailNow;
                int ov   = Math.Min(N, tailTarget);
                int core = trim + ov; // always safe; we're trimming from the very end
                var dst = new AudioBuffer(cur.Channels, sr, cur.Length - core);

                // Copy everything but the 'core' from end
                for (int ch = 0; ch < cur.Channels; ch++)
                    Array.Copy(cur.Planar[ch], 0, dst.Planar[ch], 0, dst.Length);

                // Equal-power fade of 'ov' at the new tail
                ReplaceGapWithRoomtoneClassic(dst, dst, tone, lastEndAfter, lastEndAfter + ov, toneGainLinear, overlapMs, ref toneCursor);
                cur = dst;
            }

            // ---- Shift all entries so they match the normalized audio ----
            double newDurationSec = cur.Length / (double)sr;
            var shifted = new List<SentenceTimelineEntry>(entries.Count);
            double secondStart0 = second?.Timing.StartSec ?? double.PositiveInfinity;

            foreach (var e in entries)
            {
                double shift = (e.Timing.StartSec >= secondStart0) ? shiftAfterChapterSec : shiftPreSec;
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
        ///   3) Final gaps:
        ///        - pre-roll  [0, firstSentenceStart) and post-chapter (lastSentenceEnd, N) are ALWAYS filled;
        ///        - interior gaps are the complement of the keep mask (optionally intersected with plan 'gaps').
        ///   4) Replace each final gap with room tone, using crossfades inside the gap.
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
            var src = input.SampleRate == targetSampleRate ? input : RoomtoneUtils.ResampleLinear(input, targetSampleRate);
            var tone = roomtoneSeed.SampleRate == targetSampleRate
                ? roomtoneSeed
                : RoomtoneUtils.ResampleLinear(roomtoneSeed, targetSampleRate);
            if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");

            MakeLoopable(tone, seamMs: 75); // make roomtone seed loopable

            WriteDebug(debugDirectory, "step0_source", src);
            WriteDebug(debugDirectory, "step0_roomtone_seed", tone);

            // 1) Sentence keep ranges (coalesced)
            var sentRanges = BuildSentenceRanges(sentences, src.SampleRate, src.Length);

            // 2) Global energy keeps (adaptive + micro-guards to reduce false positives)
            var energyKeeps = DetectEnergyKeepsGlobal(
                src,
                windowMs: 25, stepMs: 2,
                enterAboveFloorDb: 10, // enter ≈ global floor + 8 dB
                exitBelowEnterDb: 4,   // hysteresis
                minKeepMs: 30,         // ignore very short blips
                minHoleMs: 15,         // close tiny holes
                prePadMs: 8,           // safety pads to protect onsets
                postPadMs: 15,
                modulationDb: 0.8,     // micro-guard: require small energy modulation to enter
                speechBandFloorDeltaDb: -12.0 // micro-guard: simple HF hint (first-difference vs total)
            );

            // 3) Keep mask = Sentences ∪ EnergyKeeps
            var keepMask = Coalesce(sentRanges.Concat(energyKeeps).ToList());

            // 4) Compute gaps with edge protection
            var planGapRanges = (gaps != null && gaps.Count > 0)
                ? BuildGapRanges(gaps, src.SampleRate, src.Length)
                : new List<(int, int)>();

            var (firstStart, lastEnd) = FirstLastSentenceBounds(sentRanges, src.Length);

            // Force-fill edges no matter what the energy keep says.
            var edgeGaps = new List<(int, int)>();
            if (firstStart > 0) edgeGaps.Add((0, firstStart));
            if (lastEnd < src.Length) edgeGaps.Add((lastEnd, src.Length));
            edgeGaps = Coalesce(edgeGaps);

            // Interior gaps = complement of keep mask inside [firstStart, lastEnd]
            var interiorGaps = ComputeInteriorGaps(keepMask, firstStart, lastEnd);
            var autoGaps = Coalesce(edgeGaps.Concat(interiorGaps).ToList());

            List<(int, int)> finalGaps;
            if (planGapRanges.Count > 0)
            {
                // When explicit gaps are provided (TextGrid/plan), honor them directly.
                finalGaps = Coalesce(planGapRanges);
            }
            else
            {
                finalGaps = autoGaps;
            }

            // Debug masks
            WriteMaskDebug(debugDirectory, "step0b_keep_energy", energyKeeps, src);
            WriteMaskDebug(debugDirectory, "step0c_sentences", sentRanges, src);
            WriteMaskDebug(debugDirectory, "step0d_gaps_final", finalGaps, src);

            // 5) Out starts as a full copy of source (prevents accidental silence)
            var outBuf = RoomtoneUtils.CopyBuffer(src);

            // 6) Replace gaps with room tone, with in-gap crossfades
            int fadeSamples = fadeMs <= 0 ? 0 : (int)Math.Round(fadeMs * 0.001 * src.SampleRate);
            long toneCursor = 0;
            foreach (var (g0, g1) in finalGaps)
            {
                RoomtoneUtils.ReplaceGapWithRoomtone(outBuf, src, tone, g0, g1, toneGainLinear, fadeSamples, ref toneCursor);
            }

            WriteDebug(debugDirectory, "step1_fill_gaps", outBuf);
            return outBuf;
        }

        // ---------------------------------------------------------------------------------
        // Core synthesis
        // ---------------------------------------------------------------------------------

        // Call once when loading roomtoneSeed (before RenderWithSentenceMasks)
        private static void MakeLoopable(AudioBuffer tone, int seamMs = 20) => RoomtoneUtils.MakeLoopable(tone, seamMs);

        /// <summary>
        /// Replace [g0,g1) with roomtone. If fadeSamples > 0, crossfade from src→tone at the head
        /// and tone→src at the tail, both inside the gap. toneCursor advances continuously.
        /// </summary>
        private static void ReplaceGapWithRoomtone(
            AudioBuffer dst,
            AudioBuffer src,
            AudioBuffer tone,
            int g0,
            int g1,
            double gain,
            int fadeSamples,
            ref long toneCursor) => RoomtoneUtils.ReplaceGapWithRoomtone(dst, src, tone, g0, g1, gain, fadeSamples, ref toneCursor);

        private static void ReplaceGapWithRoomtoneClassic(
            AudioBuffer dst,
            AudioBuffer src,
            AudioBuffer tone,
            int g0,
            int g1,
            double baseGainLinear,
            int overlapMs,
            ref long toneCursor) => RoomtoneUtils.ReplaceGapWithRoomtoneClassic(dst, src, tone, g0, g1, baseGainLinear, overlapMs, ref toneCursor);

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

            int sampleRate = dst.SampleRate;
            int totalSamples = dst.Length;
            double audioDurationSec = totalSamples / (double)sampleRate;

            int ToSample(double seconds) => Math.Clamp((int)Math.Round(seconds * sampleRate), 0, totalSamples);

            var first = sentences[0];
            double firstStartSec = Math.Clamp(first.Timing.StartSec, 0.0, audioDurationSec);
            double firstPreStartSec = Math.Clamp(firstStartSec - DefaultPreRollSec, 0.0, audioDurationSec);
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
                double gapEndSec = Math.Min(Math.Clamp(second.Timing.StartSec, 0.0, audioDurationSec), gapStartSec + DefaultPostChapterPauseSec);
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
            double tailEndSec = Math.Min(audioDurationSec, tailStartSec + DefaultTailSec);
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
                Log.Warn("[Roomtone] Debug write failed for {Suffix}: {Message}", suffix, ex.Message);
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

    }
}

