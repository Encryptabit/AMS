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

        if (tone.Length > 0)
        {
            var zeroCrossings = BuildZeroCrossings(tone);
            int toneStartSample = zeroCrossings.Length > 0
                ? zeroCrossings[Random.Shared.Next(zeroCrossings.Length)]
                : Random.Shared.Next(tone.Length);
            long startCursor = toneStartSample - overlapSamples;
            long mod = tone.Length;
            toneCursor = ((startCursor % mod) + mod) % mod;
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

        const int wrapXfadeMs = 60;
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

        for (int i = 0; i < leftOv; i++)
        {
            int idx = g0 - leftOv + i;
            double ratio = leftOv <= 1 ? (double)(i + 1) / (leftOv + 1) : (double)i / (leftOv - 1);
            double angle = ratio * (Math.PI * 0.5);
            double gTone = Math.Sin(angle);
            double gSrc = Math.Cos(angle);
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
        toneCursor = AlignToNextZeroCrossing(tone, toneCursor);

        if (gapLen > 0)
        {
            int fadeIn = leftOv > 0 ? 0 : Math.Min(gapLen, overlapSamples);
            int fadeOut = rightOv > 0 ? 0 : Math.Min(gapLen, overlapSamples);

            for (int i = 0; i < gapLen; i++)
            {
                double gainScale = 1.0;

                if (fadeIn > 0 && i < fadeIn)
                {
                    double ratio = fadeIn <= 1 ? (double)(i + 1) / (fadeIn + 1) : (double)i / (fadeIn - 1);
                    double angle = ratio * (Math.PI * 0.5);
                    gainScale *= Math.Sin(angle);
                }

                if (fadeOut > 0 && i >= gapLen - fadeOut)
                {
                    double local = i - (gapLen - fadeOut);
                    double ratio = fadeOut <= 1 ? (local + 1) / (fadeOut + 1) : local / (fadeOut - 1);
                    double angle = ratio * (Math.PI * 0.5);
                    gainScale *= Math.Cos(angle);
                }

                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    var d = dst.Planar[ch];
                    var tt = tone.Planar[ch % tone.Channels];
                    float toneSample = ReadToneSample(tt, toneCursor + i);
                    d[g0 + i] = (float)(baseGainLinear * gainScale * toneSample);
                }
            }

            toneCursor += gapLen;
            toneCursor = AlignToNextZeroCrossing(tone, toneCursor);

            if (rightOv == 0 && gapLen > 0)
            {
                int lastIdx = g0 + gapLen - 1;
                for (int ch = 0; ch < dst.Channels; ch++)
                {
                    dst.Planar[ch][lastIdx] = 0f;
                }
            }
        }

        for (int i = 0; i < rightOv; i++)
        {
            int idx = g1 + i;
            double ratio = rightOv <= 1 ? (double)(i + 1) / (rightOv + 1) : (double)i / (rightOv - 1);
            double angle = ratio * (Math.PI * 0.5);
            double gTone = Math.Cos(angle);
            double gSrc = Math.Sin(angle);
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



    private static long AlignToNextZeroCrossing(AudioBuffer tone, long cursor)
    {
        if (tone.Length == 0)
        {
            return cursor;
        }

        float[] samples = tone.Planar[0];
        int len = samples.Length;
        if (len == 0)
        {
            return cursor;
        }

        int start = Mod(cursor, len);
        int bestIndex = start;
        float bestAbs = Math.Abs(samples[start]);
        float prev = samples[start];

        for (int step = 1; step <= len; step++)
        {
            int i = (start + step) % len;
            float cur = samples[i];
            float absCur = Math.Abs(cur);

            if (absCur < bestAbs)
            {
                bestAbs = absCur;
                bestIndex = i;
            }

            if ((prev <= 0f && cur >= 0f) || (prev >= 0f && cur <= 0f))
            {
                float absPrev = Math.Abs(prev);
                if (absPrev < bestAbs)
                {
                    bestIndex = Mod(i - 1, len);
                }

                return bestIndex;
            }

            prev = cur;
        }

        return bestIndex;
    }

    private static int Mod(long value, int modulus)
    {
        long result = value % modulus;
        if (result < 0)
        {
            result += modulus;
        }

        return (int)result;
    }
}
