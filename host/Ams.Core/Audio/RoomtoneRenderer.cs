using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Asr;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public static class RoomtoneRenderer
{
    public static AudioBuffer RenderWithSentenceMasks(
        AudioBuffer input,
        AudioBuffer roomtoneSeed,
        AsrResponse asr,
        IReadOnlyList<SentenceAlign> sentences,
        int targetSampleRate,
        double toneGainDb,
        double fadeMs)
    {
        var src = input.SampleRate == targetSampleRate ? input : ResampleLinear(input, targetSampleRate);
        var tone = roomtoneSeed.SampleRate == targetSampleRate ? roomtoneSeed : ResampleLinear(roomtoneSeed, targetSampleRate);
        if (tone.Length == 0) throw new InvalidOperationException("Roomtone seed must contain samples.");

        var sentRanges = BuildSentenceRanges(sentences, asr, src.SampleRate, src.Length);
        var gapRanges = BuildGapRanges(sentRanges, src.Length);

        var outBuf = CloneBuffer(src);

        double toneGain = Math.Pow(10.0, toneGainDb / 20.0);
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
                int fiStart = Math.Max(0, s0 - fadeSamples);
                int fiEnd = s0;
                ApplyCrossfade(outBuf, src, fiStart, fiEnd, fadeSamples, directionIn: true);

                int foStart = s1;
                int foEnd = Math.Min(src.Length, s1 + fadeSamples);
                ApplyCrossfade(outBuf, src, foStart, foEnd, fadeSamples, directionIn: false);
            }
        }

        return outBuf;
    }

    private static List<(int Start, int End)> BuildSentenceRanges(
        IReadOnlyList<SentenceAlign> sentences,
        AsrResponse asr,
        int sampleRate,
        int totalSamples)
    {
        var ranges = new List<(int Start, int End)>(sentences.Count);
        foreach (var sentence in sentences)
        {
            var timing = sentence.Timing;
            double startSec = timing.StartSec;
            double endSec = timing.EndSec;

            if (endSec <= startSec && sentence.ScriptRange is { Start: { } startIdx, End: { } endIdx })
            {
                var fallback = ComputeTimingFromTokens(asr, startIdx, endIdx);
                startSec = fallback.StartSec;
                endSec = fallback.EndSec;
            }

            if (endSec <= startSec) continue;

            int startSample = Math.Clamp((int)Math.Round(startSec * sampleRate), 0, totalSamples);
            int endSample = Math.Clamp((int)Math.Round(endSec * sampleRate), 0, totalSamples);
            if (endSample > startSample)
            {
                ranges.Add((startSample, endSample));
            }
        }

        if (ranges.Count == 0) return ranges;
        ranges.Sort((a, b) => a.Start != b.Start ? a.Start.CompareTo(b.Start) : a.End.CompareTo(b.End));

        var merged = new List<(int Start, int End)> { ranges[0] };
        for (int i = 1; i < ranges.Count; i++)
        {
            var current = ranges[i];
            var last = merged[^1];
            if (current.Start <= last.End)
                merged[^1] = (last.Start, Math.Max(last.End, current.End));
            else
                merged.Add(current);
        }

        return merged;
    }

    private static List<(int Start, int End)> BuildGapRanges(
        List<(int Start, int End)> sentenceRanges,
        int totalSamples)
    {
        var gaps = new List<(int Start, int End)>();
        int cursor = 0;
        foreach (var range in sentenceRanges)
        {
            if (range.Start > cursor)
            {
                gaps.Add((cursor, range.Start));
            }
            cursor = Math.Max(cursor, range.End);
        }
        if (cursor < totalSamples)
        {
            gaps.Add((cursor, totalSamples));
        }
        return gaps;
    }

    private static void FillGapWithRoomtone(
        AudioBuffer dest,
        AudioBuffer tone,
        int gapStart,
        int gapEnd,
        double gain,
        ref long toneCursor)
    {
        int len = gapEnd - gapStart;
        if (len <= 0) return;
        int toneChannels = tone.Channels;
        int toneLength = tone.Length;
        if (toneLength == 0) return;

        for (int offset = 0; offset < len; offset++, toneCursor++)
        {
            int toneIndex = (int)(toneCursor % toneLength);
            for (int ch = 0; ch < dest.Channels; ch++)
            {
                int toneCh = toneChannels == 1 ? 0 : ch % toneChannels;
                dest.Planar[ch][gapStart + offset] = (float)(gain * tone.Planar[toneCh][toneIndex]);
            }
        }
    }

    private static TimingRange ComputeTimingFromTokens(AsrResponse asr, int startIdx, int endIdx)
    {
        if (asr.Tokens.Length == 0) return TimingRange.Empty;
        var start = Math.Clamp(startIdx, 0, asr.Tokens.Length - 1);
        var end = Math.Clamp(endIdx, start, asr.Tokens.Length - 1);
        var startToken = asr.Tokens[start];
        var endToken = asr.Tokens[end];
        return new TimingRange(startToken.StartTime, endToken.StartTime + endToken.Duration);
    }

    private static void ApplyCrossfade(AudioBuffer dstWithTone, AudioBuffer src, int aStart, int aEnd, int fadeSamples, bool directionIn)
    {
        int n = Math.Max(0, aEnd - aStart);
        if (n == 0 || fadeSamples <= 0) return;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / Math.Max(1, fadeSamples);
            t = Math.Clamp(t, 0.0, 1.0);
            double gSrc = directionIn ? t : (1.0 - t);
            double gTone = 1.0 - gSrc;
            int si = aStart + i;
            if (si < 0 || si >= dstWithTone.Length) continue;
            for (int ch = 0; ch < src.Channels; ch++)
            {
                float speech = (si >= 0 && si < src.Length) ? src.Planar[ch][si] : 0f;
                float tone = dstWithTone.Planar[ch][si];
                dstWithTone.Planar[ch][si] = (float)(speech * gSrc + tone * gTone);
            }
        }
    }

    private static AudioBuffer CloneBuffer(AudioBuffer source)
    {
        var clone = new AudioBuffer(source.Channels, source.SampleRate, source.Length);
        for (int ch = 0; ch < source.Channels; ch++)
        {
            Array.Copy(source.Planar[ch], clone.Planar[ch], source.Length);
        }
        return clone;
    }

    private static AudioBuffer ResampleLinear(AudioBuffer src, int targetSampleRate)
    {
        double ratio = (double)targetSampleRate / src.SampleRate;
        int outLen = Math.Max(1, (int)Math.Round(src.Length * ratio));
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
