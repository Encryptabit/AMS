using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Align.Tx;

namespace Ams.Core;

public static class RoomtoneRenderer
{
    public static AudioBuffer RenderWithSentenceMasks(
        AudioBuffer input,
        AsrResponse asr,
        IReadOnlyList<SentenceAlign> sentences,
        int targetSampleRate,
        double toneGainDb,
        double fadeMs)
    {
        // 1) Optionally resample input to target SR (planar, float32)
        var src = input.SampleRate == targetSampleRate ? input : ResampleLinear(input, targetSampleRate);

        // 2) Build sentence time ranges from ScriptRange token indices
        var sentRanges = new List<(int startSample, int endSample)>();
        foreach (var s in sentences)
        {
            if (s.ScriptRange == null || !s.ScriptRange.Start.HasValue || !s.ScriptRange.End.HasValue) continue;
            int si = Math.Clamp(s.ScriptRange.Start.Value, 0, asr.Tokens.Length - 1);
            int ei = Math.Clamp(s.ScriptRange.End.Value, 0, asr.Tokens.Length - 1);
            if (ei < si) continue;
            var t0 = asr.Tokens[si].StartTime;
            var t1 = asr.Tokens[ei].StartTime + asr.Tokens[ei].Duration;
            int s0 = (int)Math.Round(t0 * src.SampleRate);
            int s1 = (int)Math.Round(t1 * src.SampleRate);
            s0 = Math.Clamp(s0, 0, src.Length);
            s1 = Math.Clamp(s1, 0, src.Length);
            if (s1 > s0) sentRanges.Add((s0, s1));
        }
        sentRanges = MergeRanges(sentRanges);

        // 3) Prepare output buffer and roomtone generator
        var outBuf = new AudioBuffer(src.Channels, src.SampleRate, src.Length);
        double toneAmp = Math.Pow(10.0, toneGainDb / 20.0); // dBFS -> linear
        var rnd = new Random(0xC0FFEE);
        Span<float> tmpNoise = stackalloc float[4096];

        // 4) Fill entire output with room tone (TPDF noise)
        int total = src.Length;
        int idx = 0;
        while (idx < total)
        {
            int n = Math.Min(tmpNoise.Length, total - idx);
            for (int i = 0; i < n; i++)
            {
                // TPDF noise in [-1,1] scaled by toneAmp
                double u = rnd.NextDouble() - rnd.NextDouble();
                tmpNoise[i] = (float)(u * toneAmp);
            }
            for (int ch = 0; ch < src.Channels; ch++)
            {
                var dst = outBuf.Planar[ch];
                tmpNoise[..n].CopyTo(dst.AsSpan(idx, n));
            }
            idx += n;
        }

        // 5) Overlay source audio on sentence ranges
        foreach (var (s0, s1) in sentRanges)
        {
            int len = s1 - s0;
            for (int ch = 0; ch < src.Channels; ch++)
            {
                Array.Copy(src.Planar[ch], s0, outBuf.Planar[ch], s0, len);
            }
        }

        // 6) Crossfade at boundaries (speech<->roomtone) using linear 5 ms ramps
        int fadeSamples = (int)Math.Round(fadeMs * 0.001 * src.SampleRate);
        fadeSamples = Math.Max(1, fadeSamples);

        foreach (var (s0, s1) in sentRanges)
        {
            // Fade-in at start (roomtone -> speech)
            int fiStart = Math.Max(0, s0 - fadeSamples);
            int fiEnd = s0;
            ApplyCrossfade(outBuf, src, fiStart, fiEnd, fadeSamples, directionIn: true);

            // Fade-out at end (speech -> roomtone)
            int foStart = s1;
            int foEnd = Math.Min(src.Length, s1 + fadeSamples);
            ApplyCrossfade(outBuf, src, foStart, foEnd, fadeSamples, directionIn: false);
        }

        return outBuf;
    }

    private static void ApplyCrossfade(AudioBuffer dstWithTone, AudioBuffer src, int aStart, int aEnd, int fadeSamples, bool directionIn)
    {
        // Crossfade is implemented by mixing a linear ramp between the two layers across overlap window:
        // directionIn=true: roomtone -> speech (gainTone: 1..0, gainSrc: 0..1)
        // directionIn=false: speech -> roomtone (gainSrc: 1..0, gainTone: 0..1)
        int n = Math.Max(0, aEnd - aStart);
        if (n == 0) return;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / Math.Max(1, fadeSamples);
            t = Math.Clamp(t, 0.0, 1.0);
            double gSrc = directionIn ? t : (1.0 - t);
            double gTone = 1.0 - gSrc;
            int si = aStart + i;
            for (int ch = 0; ch < src.Channels; ch++)
            {
                float s = (si >= 0 && si < src.Length) ? src.Planar[ch][si] : 0f;
                float d = dstWithTone.Planar[ch][si]; // currently holds roomtone
                dstWithTone.Planar[ch][si] = (float)(s * gSrc + d * gTone);
            }
        }
    }

    private static List<(int, int)> MergeRanges(List<(int s0, int s1)> ranges)
    {
        if (ranges.Count == 0) return new();
        ranges.Sort((a, b) => a.s0 != b.s0 ? a.s0.CompareTo(b.s0) : a.s1.CompareTo(b.s1));
        var merged = new List<(int, int)> { ranges[0] };
        for (int i = 1; i < ranges.Count; i++)
        {
            var (s0, s1) = ranges[i];
            var last = merged[^1];
            if (s0 <= last.Item2)
                merged[^1] = (last.Item1, Math.Max(last.Item2, s1));
            else
                merged.Add((s0, s1));
        }
        return merged;
    }

    private static AudioBuffer ResampleLinear(AudioBuffer src, int targetSampleRate)
    {
        double ratio = (double)targetSampleRate / src.SampleRate;
        int outLen = (int)Math.Round(src.Length * ratio);
        var outBuf = new AudioBuffer(src.Channels, targetSampleRate, outLen);
        for (int ch = 0; ch < src.Channels; ch++)
        {
            var s = src.Planar[ch];
            var d = outBuf.Planar[ch];
            for (int i = 0; i < outLen; i++)
            {
                double x = i / ratio;
                int i0 = (int)Math.Floor(x);
                int i1 = Math.Min(src.Length - 1, i0 + 1);
                double frac = x - i0;
                float y = (float)((1.0 - frac) * s[Math.Clamp(i0, 0, src.Length - 1)] + frac * s[i1]);
                d[i] = y;
            }
        }
        return outBuf;
    }
}

public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }

    public AudioBuffer(int channels, int sampleRate, int length)
    {
        Channels = channels;
        SampleRate = sampleRate;
        Length = length;
        Planar = new float[channels][];
        for (int ch = 0; ch < channels; ch++) Planar[ch] = new float[length];
    }
}

