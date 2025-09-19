using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Asr;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
    public static class RoomtoneRenderer
    {
        /// <summary>
        /// Renders output by filling detected gaps with looped roomtone and restoring sentence regions from the source.
        /// Optional crossfades are applied at sentence boundaries to hide seams.
        /// </summary>
        public static AudioBuffer RenderWithSentenceMasks(
            AudioBuffer input,
            AudioBuffer roomtoneSeed,
            IReadOnlyList<RoomtonePlanGap> gaps,
            IReadOnlyList<SentenceAlign> sentences,
            int targetSampleRate,
            double toneGainLinear,
            double fadeMs)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (roomtoneSeed is null) throw new ArgumentNullException(nameof(roomtoneSeed));
            if (gaps is null) throw new ArgumentNullException(nameof(gaps));
            if (sentences is null) throw new ArgumentNullException(nameof(sentences));
            if (targetSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(targetSampleRate));

            var src = input.SampleRate == targetSampleRate ? input : ResampleLinear(input, targetSampleRate);
            var tone = roomtoneSeed.SampleRate == targetSampleRate ? roomtoneSeed : ResampleLinear(roomtoneSeed, targetSampleRate);

            if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");

            var sentRanges = BuildSentenceRanges(sentences, src.SampleRate, src.Length);
            var gapRanges = BuildGapRanges(gaps, src.SampleRate, src.Length);

            var outBuf = new AudioBuffer(src.Channels, src.SampleRate, src.Length);

            // Start with silence in outBuf; fill gaps with roomtone, then overlay source in sentence regions.
            double toneGain = toneGainLinear;
            long toneCursor = 0;

            foreach (var (gapStart, gapEnd) in gapRanges)
            {
                FillGapWithRoomtone(outBuf, tone, gapStart, gapEnd, toneGain, ref toneCursor);
            }

            foreach (var (startSample, endSample) in sentRanges)
            {
                int len = endSample - startSample;
                if (len <= 0) continue;
                for (int ch = 0; ch < src.Channels; ch++)
                {
                    Array.Copy(src.Planar[ch], startSample, outBuf.Planar[ch], startSample, len);
                }
            }

            int fadeSamples = fadeMs <= 0 ? 0 : (int)Math.Round(fadeMs * 0.001 * src.SampleRate);
            if (fadeSamples > 0)
            {
                foreach (var (s0, s1) in sentRanges)
                {
                    // Fade-in into sentence
                    int fiStart = Math.Max(0, s0 - fadeSamples);
                    int fiEnd = s0;
                    ApplyCrossfade(outBuf, src, fiStart, fiEnd, fadeSamples, directionIn: true);

                    // Fade-out out of sentence
                    int foStart = s1;
                    int foEnd = Math.Min(src.Length, s1 + fadeSamples);
                    ApplyCrossfade(outBuf, src, foStart, foEnd, fadeSamples, directionIn: false);
                }
            }

            RepairSilentRegions(outBuf, src);

            return outBuf;
        }

        private static List<(int Start, int End)> BuildSentenceRanges(
            IReadOnlyList<SentenceAlign> sentences,
            int sampleRate,
            int totalSamples)
        {
            var ranges = new List<(int Start, int End)>(Math.Max(8, sentences.Count));
            foreach (var sentence in sentences)
            {
                var timing = sentence.Timing;
                double startSec = timing.StartSec;
                double endSec = timing.EndSec;
                if (endSec <= startSec) continue;

                int startSample = Math.Clamp((int)Math.Round(startSec * sampleRate), 0, totalSamples);
                int endSample = Math.Clamp((int)Math.Round(endSec * sampleRate), 0, totalSamples);
                if (endSample > startSample)
                {
                    ranges.Add((startSample, endSample));
                }
            }

            if (ranges.Count == 0) return ranges;

            // coalesce any overlaps/adjacents to simplify downstream fades
            ranges.Sort((a, b) => a.Start != b.Start ? a.Start.CompareTo(b.Start) : a.End.CompareTo(b.End));
            var coalesced = new List<(int, int)>(ranges.Count);
            var cur = ranges[0];
            for (int i = 1; i < ranges.Count; i++)
            {
                var next = ranges[i];
                if (next.Start <= cur.End)
                {
                    cur = (cur.Start, Math.Max(cur.End, next.End));
                }
                else
                {
                    coalesced.Add(cur);
                    cur = next;
                }
            }
            coalesced.Add(cur);
            return coalesced;
        }

        private static List<(int Start, int End)> BuildGapRanges(
            IReadOnlyList<RoomtonePlanGap> gaps,
            int sampleRate,
            int totalSamples)
        {
            var ranges = new List<(int, int)>(gaps.Count);
            foreach (var gap in gaps)
            {
                int s = Math.Clamp((int)Math.Round(gap.StartSec * sampleRate), 0, totalSamples);
                int e = Math.Clamp((int)Math.Round(gap.EndSec * sampleRate), 0, totalSamples);
                if (e > s) ranges.Add((s, e));
            }
            return ranges;
        }

        private static void FillGapWithRoomtone(
            AudioBuffer dst,
            AudioBuffer tone,
            int start,
            int end,
            double gain,
            ref long toneCursor)
        {
            int len = end - start;
            if (len <= 0) return;

            for (int ch = 0; ch < dst.Channels; ch++)
            {
                var d = dst.Planar[ch];
                var t = tone.Planar[ch % tone.Channels];

                long cursor = toneCursor;
                int tl = t.Length;
                for (int i = 0; i < len; i++)
                {
                    float sample = t[(int)(cursor % tl)];
                    d[start + i] = (float)(gain * sample);
                    cursor++;
                }
            }

            toneCursor += len;
        }

        private static void RepairSilentRegions(AudioBuffer mix, AudioBuffer src)
        {
            if (mix is null) throw new ArgumentNullException(nameof(mix));
            if (src is null) throw new ArgumentNullException(nameof(src));
            if (mix.Length == 0) return;

            const float epsilon = 1e-7f;
            int segmentStart = -1;
            int length = Math.Min(mix.Length, src.Length);
            int channels = mix.Channels;

            for (int sample = 0; sample < length; sample++)
            {
                bool silent = true;
                for (int ch = 0; ch < channels; ch++)
                {
                    if (MathF.Abs(mix.Planar[ch][sample]) > epsilon)
                    {
                        silent = false;
                        break;
                    }
                }

                if (silent)
                {
                    if (segmentStart < 0)
                    {
                        segmentStart = sample;
                    }
                }
                else if (segmentStart >= 0)
                {
                    CopySourceRange(mix, src, segmentStart, sample);
                    segmentStart = -1;
                }
            }

            if (segmentStart >= 0)
            {
                CopySourceRange(mix, src, segmentStart, length);
            }
        }

        private static void CopySourceRange(AudioBuffer dst, AudioBuffer src, int startSample, int endSample)
        {
            int safeStart = Math.Clamp(startSample, 0, Math.Min(dst.Length, src.Length));
            int safeEnd = Math.Clamp(endSample, safeStart, Math.Min(dst.Length, src.Length));
            int len = safeEnd - safeStart;
            if (len <= 0) return;

            for (int ch = 0; ch < dst.Channels; ch++)
            {
                var dstChannel = dst.Planar[ch];
                var srcChannel = src.Planar[ch % src.Channels];
                Array.Copy(srcChannel, safeStart, dstChannel, safeStart, len);
            }
        }

        private static void ApplyCrossfade(
            AudioBuffer mix,       // mix currently contains roomtone (outside sentences) and sentence audio in sentence regions.
            AudioBuffer src,       // original source audio to blend with
            int start, int end,
            int fadeSamples,
            bool directionIn)
        {
            int s = Math.Max(0, start);
            int e = Math.Max(s, end);
            int n = e - s;
            if (n <= 0 || fadeSamples <= 0) return;

            // Ensure ramp uses exactly n samples (not necessarily equal to fadeSamples at clip edges).
            for (int ch = 0; ch < mix.Channels; ch++)
            {
                var m = mix.Planar[ch];
                var a = src.Planar[ch % src.Channels];

                for (int i = 0; i < n; i++)
                {
                    double t = (n == 1) ? 1.0 : (double)i / (n - 1);
                    double srcGain = directionIn ? t : (1.0 - t);   // fade in or out for source
                    double mixGain = 1.0 - srcGain;                 // complementary gain for roomtone

                    int idx = s + i;
                    double blended = mixGain * m[idx] + srcGain * a[idx];
                    m[idx] = (float)blended;
                }
            }
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
