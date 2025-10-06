using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Audio;

namespace Ams.Core.Prosody;

internal static class PauseAudioApplier
{
    public static AudioBuffer Apply(
        AudioBuffer audio,
        AudioBuffer roomtone,
        IReadOnlyList<PauseAdjust> adjustments,
        double toneGainLinear,
        int overlapMs = 35)
    {
        if (audio is null) throw new ArgumentNullException(nameof(audio));
        if (roomtone is null) throw new ArgumentNullException(nameof(roomtone));
        if (adjustments is null) throw new ArgumentNullException(nameof(adjustments));

        var filtered = adjustments
            .Where(adj => adj is not null && double.IsFinite(adj.TargetDurationSec) && adj.TargetDurationSec >= 0)
            .OrderBy(adj => adj.StartSec)
            .ToList();

        if (filtered.Count == 0)
        {
            return audio;
        }

        int sampleRate = audio.SampleRate;
        int totalDeltaSamples = 0;
        foreach (var adjust in filtered)
        {
            int startSample = Math.Clamp((int)Math.Round(adjust.StartSec * sampleRate), 0, audio.Length);
            int endSample = Math.Clamp((int)Math.Round(adjust.EndSec * sampleRate), startSample, audio.Length);
            int originalSamples = endSample - startSample;
            int targetSamples = Math.Max(0, (int)Math.Round(adjust.TargetDurationSec * sampleRate));
            totalDeltaSamples += targetSamples - originalSamples;
        }

        int finalLength = audio.Length + totalDeltaSamples;
        if (finalLength < 0)
        {
            finalLength = 0;
        }

        var dst = new AudioBuffer(audio.Channels, sampleRate, finalLength);
        var gapPositions = new List<(int Start, int End)>(filtered.Count);

        int readCursor = 0;
        int writeCursor = 0;

        foreach (var adjust in filtered)
        {
            int startSample = Math.Clamp((int)Math.Round(adjust.StartSec * sampleRate), 0, audio.Length);
            int endSample = Math.Clamp((int)Math.Round(adjust.EndSec * sampleRate), startSample, audio.Length);
            int targetSamples = Math.Max(0, (int)Math.Round(adjust.TargetDurationSec * sampleRate));

            if (startSample > readCursor)
            {
                int copyLength = Math.Min(startSample - readCursor, Math.Max(0, dst.Length - writeCursor));
                CopySamples(audio, readCursor, dst, writeCursor, copyLength);
                readCursor += copyLength;
                writeCursor += copyLength;
            }
            else if (startSample < readCursor)
            {
                startSample = readCursor;
            }

            readCursor = endSample;
            int g0 = writeCursor;
            int g1 = Math.Clamp(g0 + targetSamples, g0, dst.Length);
            gapPositions.Add((g0, g1));
            writeCursor = g1;
        }

        if (writeCursor < dst.Length && readCursor < audio.Length)
        {
            int remaining = Math.Min(audio.Length - readCursor, dst.Length - writeCursor);
            CopySamples(audio, readCursor, dst, writeCursor, remaining);
            readCursor += remaining;
            writeCursor += remaining;
        }

        // If rounding left trailing space, it remains silence (zeros).

        var srcCopy = RoomtoneUtils.CopyBuffer(dst);

        if (roomtone.SampleRate != sampleRate)
        {
            roomtone = RoomtoneUtils.ResampleLinear(roomtone, sampleRate);
        }

        RoomtoneUtils.MakeLoopable(roomtone, seamMs: 20);

        int fadeSamples = Math.Max(1, (int)Math.Round(Math.Max(1, overlapMs) * 0.001 * sampleRate));
        long toneCursor = 0;

        foreach (var (start, end) in gapPositions)
        {
            RoomtoneUtils.ReplaceGapWithRoomtone(dst, srcCopy, roomtone, start, end, toneGainLinear, fadeSamples, ref toneCursor);
        }

        return dst;
    }

    private static void CopySamples(AudioBuffer source, int sourceStart, AudioBuffer destination, int destinationStart, int length)
    {
        if (length <= 0)
        {
            return;
        }

        int maxSourceLength = source.Length - sourceStart;
        int maxDestLength = destination.Length - destinationStart;
        int copyLength = Math.Min(length, Math.Min(maxSourceLength, maxDestLength));
        if (copyLength <= 0)
        {
            return;
        }

        for (int ch = 0; ch < destination.Channels; ch++)
        {
            var src = source.Planar[Math.Min(ch, source.Channels - 1)];
            var dst = destination.Planar[ch];
            Array.Copy(src, sourceStart, dst, destinationStart, copyLength);
        }
    }
}
