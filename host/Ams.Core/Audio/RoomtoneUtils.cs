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
            var samples = tone.Planar[ch];
            int headIndex = 0;
            int tailIndex = tone.Length - N;

            for (int i = 0; i < N; i++)
            {
                double t = (double)(i + 1) / (N + 1);
                double gHead = Math.Sqrt(1.0 - t);
                double gTail = Math.Sqrt(t);
                samples[headIndex + i] = (float)(gHead * samples[headIndex + i] + gTail * samples[tailIndex + i]);
            }
        }
    }
}
