using System;
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

        g0 = Math.Clamp(g0, 0, dst.Length);
        g1 = Math.Clamp(g1, g0, dst.Length);
        int gapLen = g1 - g0;
        int leftOv = Math.Min(overlapSamples, g0);
        int rightOv = Math.Min(overlapSamples, dst.Length - g1);

        if (gapLen <= 0 && leftOv == 0 && rightOv == 0)
        {
            return;
        }

        const int wrapXfadeMs = 12;
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
}
