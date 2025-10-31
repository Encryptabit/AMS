using System;
using System.Collections.Generic;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

internal static class RoomtoneUtils
{
    public static void MakeLoopable(AudioBuffer tone, int seamMs = 20)
    {
        if (tone is null) throw new ArgumentNullException(nameof(tone));

        int sr = tone.SampleRate;
        int N = Math.Max(1, (int)Math.Round(seamMs * 0.001 * sr));
        if (N * 2 >= tone.Length)
        {
            return;
        }

        for (int ch = 0; ch < tone.Channels; ch++)
        {
            var p = tone.Planar[ch];
            int a0 = 0;
            int b0 = tone.Length - N;

            for (int i = 0; i < N; i++)
            {
                double t = (double)(i + 1) / (N + 1);
                double gHead = Math.Sqrt(1.0 - t);
                double gTail = Math.Sqrt(t);
                p[a0 + i] = (float)(gHead * p[a0 + i] + gTail * p[b0 + i]);
            }
        }
    }

    public static void ReplaceGapWithRoomtone(
        AudioBuffer dst,
        AudioBuffer src,
        AudioBuffer tone,
        int g0,
        int g1,
        double gain,
        int fadeSamples,
        ref long toneCursor)
    {
        if (dst is null) throw new ArgumentNullException(nameof(dst));
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (tone is null) throw new ArgumentNullException(nameof(tone));

        int sr = dst.SampleRate;
        int overlapSamples = fadeSamples > 0 ? fadeSamples : Math.Max(1, (int)Math.Round(0.01 * sr));
        int overlapMs = Math.Max(1, (int)Math.Round(overlapSamples * 1000.0 / sr));
        ReplaceGapWithRoomtoneClassic(dst, src, tone, g0, g1, gain, overlapMs, ref toneCursor);
    }

    public static void ReplaceGapWithRoomtoneClassic(
        AudioBuffer dst,
        AudioBuffer src,
        AudioBuffer tone,
        int g0,
        int g1,
        double baseGainLinear,
        int overlapMs,
        ref long toneCursor)
    {
        if (dst is null) throw new ArgumentNullException(nameof(dst));
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (tone is null) throw new ArgumentNullException(nameof(tone));

        int sr = dst.SampleRate;
        int overlapSamples = Math.Max(1, (int)Math.Round(overlapMs * 0.001 * sr));
        int toneLength = tone.Planar[0].Length;
        if (toneLength == 0)
        {
            return;
        }

        g0 = Math.Clamp(g0, 0, dst.Length);
        g1 = Math.Clamp(g1, g0, dst.Length);
        int gapLen = g1 - g0;
        int leftOv = Math.Min(overlapSamples, g0);
        int rightOv = Math.Min(overlapSamples, dst.Length - g1);

        if (gapLen <= 0 && leftOv == 0 && rightOv == 0)
        {
            return;
        }

        float initialMatch = 0f;
        if (src.Length > 0)
        {
            int targetSample = g0 > 0 ? g0 - 1 : 0;
            targetSample = Math.Clamp(targetSample, 0, src.Planar[0].Length - 1);
            initialMatch = src.Planar[0][targetSample];
        }

        AlignCursorToMatch(tone.Planar[0], toneLength, ref toneCursor, initialMatch);

        // Left overlap crossfade
        for (int i = 0; i < leftOv; i++)
        {
            int dstIndex = g0 - leftOv + i;
            double ratio = leftOv <= 1 ? (double)(i + 1) / (leftOv + 1) : (double)i / (leftOv - 1);
            double angle = ratio * (Math.PI * 0.5);
            double gTone = Math.Sin(angle);
            double gSrc = Math.Cos(angle);

            int toneIdx = NormalizeIndex(toneCursor, toneLength);
            float toneSample0 = tone.Planar[0][toneIdx];

            for (int ch = 0; ch < dst.Channels; ch++)
            {
                var d = dst.Planar[ch];
                var s = src.Planar[ch % src.Channels];
                var tt = tone.Planar[ch % tone.Channels];
                float toneSample = tt[toneIdx];
                d[dstIndex] = (float)(gSrc * s[dstIndex] + gTone * (baseGainLinear * toneSample));
            }

            toneCursor++;
            if (NormalizeIndex(toneCursor, toneLength) == 0)
            {
                AlignCursorToMatch(tone.Planar[0], toneLength, ref toneCursor, toneSample0);
            }
        }

        // Gap interior
        if (gapLen > 0)
        {
            int fadeIn = leftOv > 0 ? 0 : Math.Min(gapLen, overlapSamples);
            int fadeOut = rightOv > 0 ? 0 : Math.Min(gapLen, overlapSamples);

            var prevOutputs = new double[dst.Channels];
            if (leftOv == 0)
            {
                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    int sampleIndex = g0 > 0 ? Math.Clamp(g0 - 1, 0, dst.Planar[ch].Length - 1) : 0;
                    prevOutputs[ch] = dst.Planar[ch][sampleIndex];
                }
            }

            for (int i = 0; i < gapLen; i++)
            {
                double gainScale = 1.0;

                if (fadeIn > 0 && i < fadeIn)
                {
                    double ratio = ComputeRamp(i, fadeIn);
                    double angle = ratio * (Math.PI * 0.5);
                    gainScale *= Math.Sin(angle);
                }

                if (fadeOut > 0 && i >= gapLen - fadeOut)
                {
                    double local = i - (gapLen - fadeOut);
                    double ratio = ComputeRamp((int)local, fadeOut);
                    double angle = (1.0 - ratio) * (Math.PI * 0.5);
                    gainScale *= Math.Sin(angle);
                }

                int toneIdx = NormalizeIndex(toneCursor, toneLength);
                float toneSample0 = tone.Planar[0][toneIdx];

                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    var d = dst.Planar[ch];
                    var tt = tone.Planar[ch % tone.Channels];
                    double toneValue = baseGainLinear * gainScale * tt[toneIdx];

                    if (leftOv == 0)
                    {
                        double prevOut = prevOutputs[ch];
                        if (fadeIn > 0)
                        {
                            toneValue = (1.0 - gainScale) * prevOut + toneValue;
                        }
                        else if (i == 0)
                        {
                            toneValue = 0.5 * (prevOut + toneValue);
                        }

                        prevOutputs[ch] = toneValue;
                    }

                    d[g0 + i] = (float)toneValue;
                }

                toneCursor++;
                if (NormalizeIndex(toneCursor, toneLength) == 0)
                {
                    AlignCursorToMatch(tone.Planar[0], toneLength, ref toneCursor, toneSample0);
                }
            }
        }

        // Right overlap crossfade
        for (int i = 0; i < rightOv; i++)
        {
            int dstIndex = g1 + i;
            double ratio = ComputeRamp(i, rightOv);
            double angle = ratio * (Math.PI * 0.5);
            double gTone = Math.Cos(angle);
            double gSrc = Math.Sin(angle);

            int toneIdx = NormalizeIndex(toneCursor, toneLength);
            float toneSample0 = tone.Planar[0][toneIdx];

            for (int ch = 0; ch < dst.Channels; ch++)
            {
                var d = dst.Planar[ch];
                var s = src.Planar[ch % src.Channels];
                var tt = tone.Planar[ch % tone.Channels];
                float toneSample = tt[toneIdx];
                d[dstIndex] = (float)(gTone * (baseGainLinear * toneSample) + gSrc * s[dstIndex]);
            }

            toneCursor++;
            if (NormalizeIndex(toneCursor, toneLength) == 0)
            {
                AlignCursorToMatch(tone.Planar[0], toneLength, ref toneCursor, toneSample0);
            }
        }
    }

    private static int[] BuildZeroCrossings(AudioBuffer tone)
    {
        if (tone is null) throw new ArgumentNullException(nameof(tone));
        if (tone.Length < 2)
        {
            return Array.Empty<int>();
        }

        var result = new List<int>();
        var samples = tone.Planar[0];
        float prev = samples[0];
        for (int i = 1; i < samples.Length; i++)
        {
            float cur = samples[i];
            if ((prev <= 0 && cur > 0) || (prev >= 0 && cur < 0))
            {
                result.Add(i);
            }

            prev = cur;
        }

        return result.ToArray();
    }

    public static AudioBuffer ResampleLinear(AudioBuffer src, int targetSampleRate)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (targetSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(targetSampleRate));

        if (src.SampleRate == targetSampleRate)
        {
            return src;
        }

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

    public static AudioBuffer CopyBuffer(AudioBuffer src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var dst = new AudioBuffer(src.Channels, src.SampleRate, src.Length);
        for (int ch = 0; ch < src.Channels; ch++)
        {
            Array.Copy(src.Planar[ch], 0, dst.Planar[ch], 0, src.Length);
        }

        return dst;
    }



    private static int NormalizeIndex(long value, int length)
    {
        int idx = (int)(value % length);
        return idx < 0 ? idx + length : idx;
    }

    private static void AlignCursorToMatch(float[] samples, int length, ref long cursor, float match)
    {
        if (samples.Length == 0 || length <= 0)
        {
            return;
        }

        int best = FindBestMatch(samples, match);
        long baseCursor = cursor - NormalizeIndex(cursor, length);
        cursor = baseCursor + best;
    }

    private static int FindBestMatch(float[] samples, float target)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        int best = 0;
        float bestDiff = Math.Abs(samples[0] - target);
        for (int i = 1; i < samples.Length; i++)
        {
            float diff = Math.Abs(samples[i] - target);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
                if (bestDiff <= 1e-6f)
                {
                    break;
                }
            }
        }

        return best;
    }

    private static double ComputeRamp(int index, int count)
    {
        if (count <= 0)
        {
            return 1.0;
        }

        if (count == 1)
        {
            return index >= 1 ? 1.0 : 0.0;
        }

        return Math.Clamp(index / (double)(count - 1), 0.0, 1.0);
    }
}
