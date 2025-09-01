using System.Diagnostics;
using System.Globalization;
using Ams.Dsp.Native;

namespace Ams.Core;

public sealed class DspProcessResult
{
    public required int FramesProcessed { get; init; }
    public required int Channels { get; init; }
    public required float SampleRate { get; init; }
    public required uint MaxBlock { get; init; }
    public required double ElapsedMilliseconds { get; init; }
    public required uint LatencySamples { get; init; }
    public required string OutputPath { get; init; }
    public required double InputRms { get; init; }
    public required double OutputRms { get; init; }
    public double DeltaDb => 20 * Math.Log10(OutputRms / Math.Max(InputRms, 1e-20));
}

/// <summary>
/// Core DSP demo/processing routine extracted from CLI. No console I/O.
/// </summary>
public static class DspDemoRunner
{
    /// <summary>
    /// Runs a legacy DSP demo: generate sine, process with AmsDsp, write WAV.
    /// </summary>
    /// <param name="sampleRate">Sample rate in Hz.</param>
    /// <param name="channels">Number of channels.</param>
    /// <param name="maxBlock">Max processing block size.</param>
    /// <param name="hz">Sine frequency in Hz.</param>
    /// <param name="inputGainDb">Input sine amplitude in dBFS.</param>
    /// <param name="seconds">Duration to render in seconds.</param>
    /// <param name="gain01">Normalized gain parameter [0,1] for the DSP.</param>
    /// <param name="outputPath">Optional output path. Defaults to ams_out.wav next to app.</param>
    /// <returns>A <see cref="DspProcessResult"/> with metrics.</returns>
    public static DspProcessResult RunDemo(
        float sampleRate,
        int channels,
        uint maxBlock,
        double hz,
        float inputGainDb,
        double seconds,
        float gain01,
        string? outputPath = null)
    {
        if (channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));

        const uint ParamIdGain = 0; // matches Zig param id

        int totalFrames = checked((int)Math.Round(seconds * sampleRate, MidpointRounding.AwayFromZero));

        // Build planar input/output
        var input = NewPlanar(channels, totalFrames);
        var output = NewPlanar(channels, totalFrames);

        // Fill test tone
        float amp = DbToAmp(inputGainDb);
        for (int ch = 0; ch < channels; ch++)
        {
            var plane = input[ch];
            for (int n = 0; n < totalFrames; n++)
            {
                plane[n] = amp * (float)Math.Sin(2.0 * Math.PI * hz * n / sampleRate);
            }
        }

        using var dsp = AmsDsp.Create(sampleRate, maxBlock, (uint)channels);
        dsp.SetParameter(ParamIdGain, gain01);

        var sw = Stopwatch.StartNew();
        dsp.ProcessLong(input, output, totalFrames);
        sw.Stop();

        // Round-trip state (sanity)
        var state = dsp.SaveState();
        dsp.LoadState(state);

        // Write WAV
        string outPath = outputPath ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "ams_out.wav"));
        WriteWavInterleavedFloat32(outPath, output, channels, (int)sampleRate);

        double inRms = Rms(input[0]);
        double outRms = Rms(output[0]);

        return new DspProcessResult
        {
            FramesProcessed = totalFrames,
            Channels = channels,
            SampleRate = sampleRate,
            MaxBlock = maxBlock,
            ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds,
            LatencySamples = dsp.LatencySamples,
            OutputPath = outPath,
            InputRms = inRms,
            OutputRms = outRms,
        };
    }

    private static double Rms(float[] buf)
    {
        double sum = 0;
        for (int i = 0; i < buf.Length; i++) sum += buf[i] * buf[i];
        return Math.Sqrt(sum / Math.Max(buf.Length, 1));
    }

    private static float[][] NewPlanar(int channels, int frames)
    {
        var arr = new float[channels][];
        for (int ch = 0; ch < channels; ch++) arr[ch] = new float[frames];
        return arr;
    }

    private static float DbToAmp(float db) => (float)Math.Pow(10.0, db / 20.0);

    // Minimal IEEE float32 WAV writer (interleaves planar into [LRLR...])
    private static void WriteWavInterleavedFloat32(string path, float[][] planar, int channels, int sampleRate)
    {
        int frames = planar[0].Length;
        long dataSamples = (long)frames * channels;
        const int bytesPerSample = 4;
        const int fmtChunkSize = 16;
        const int bitsPerSample = 32;
        int blockAlign = channels * bytesPerSample;
        int byteRate = sampleRate * blockAlign;
        long dataBytes = dataSamples * bytesPerSample;
        long riffSize = 4 + (8 + fmtChunkSize) + (8 + dataBytes);

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        void WriteAscii(string s) => bw.Write(System.Text.Encoding.ASCII.GetBytes(s));

        // RIFF header
        WriteAscii("RIFF");
        bw.Write((uint)riffSize);
        WriteAscii("WAVE");

        // fmt chunk (PCM float)
        WriteAscii("fmt ");
        bw.Write((uint)fmtChunkSize);
        bw.Write((ushort)3); // WAVE_FORMAT_IEEE_FLOAT
        bw.Write((ushort)channels);
        bw.Write((uint)sampleRate);
        bw.Write((uint)byteRate);
        bw.Write((ushort)blockAlign);
        bw.Write((ushort)bitsPerSample);

        // data chunk
        WriteAscii("data");
        bw.Write((uint)dataBytes);

        // interleave
        for (int n = 0; n < frames; n++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                bw.Write(planar[ch][n]);
            }
        }
    }
}

