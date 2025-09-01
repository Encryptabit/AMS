using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using Ams.Dsp.Native;
using Ams.Cli.Commands;

namespace Ams.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // If no arguments provided, run legacy DSP demo
        if (args.Length == 0)
        {
            return RunLegacyDspDemo();
        }
        
        // Create root command
        var rootCommand = new RootCommand("AMS - Audio Management System CLI");
        
        // Add commands
        rootCommand.AddCommand(AsrCommand.Create());
        rootCommand.AddCommand(ValidateCommand.Create());
        
        // Add legacy DSP command
        var dspCommand = new Command("dsp", "Run DSP processing demo");
        dspCommand.SetHandler(() => RunLegacyDspDemo());
        rootCommand.AddCommand(dspCommand);
        
        return await rootCommand.InvokeAsync(args);
    }

    private static int RunLegacyDspDemo()
    {
        // Params (easy to tweak)
        const float sampleRate = 48000f;
        const uint channels = 2;
        const uint maxBlock = 512;
       // const double seconds = 2.0;
        const double hz = 440.0;      // A4 test tone
        const float inGainDb = -12.0f; // input sine amplitude
        const uint paramIdGain = 0;   // match your Zig param id

        Console.Write("Enter gain (0–1): ");
        float g = float.Parse(Console.ReadLine() ?? "0.5", CultureInfo.InvariantCulture);
        Console.Write("How long?(seconds) ");
        double seconds = double.Parse(Console.ReadLine() ?? "2.0", CultureInfo.InvariantCulture);

        int totalFrames = checked((int)Math.Round(seconds * sampleRate, MidpointRounding.AwayFromZero));

        // Build planar input/output
        var input = NewPlanar(channels, totalFrames);
        var output = NewPlanar(channels, totalFrames);

        // Fill test tone (-12 dBFS)
        float amp = DbToAmp(inGainDb);
        for (int ch = 0; ch < channels; ch++)
        {
            var plane = input[ch];
            for (int n = 0; n < totalFrames; n++)
            {
                plane[n] = amp * (float)Math.Sin(2.0 * Math.PI * hz * n / sampleRate);
            }
        }

        using var dsp = AmsDsp.Create(sampleRate, maxBlock, channels);


        dsp.SetParameter(paramIdGain, g);  

        // Example: set parameter (e.g., smoothed gain at 0.50)
        //dsp.SetParameter(paramIdGain, value01: 0.5f, sampleOffset: 0);

        var sw = Stopwatch.StartNew();
        dsp.ProcessLong(input, output, totalFrames);
        sw.Stop();

        // Save & restore state (sanity)
        var state = dsp.SaveState();
        dsp.LoadState(state);

        // Write a simple IEEE float33 WAV (interleaved)
        string outPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "ams_out.wav"));
        WriteWavInterleavedFloat32(outPath, output, (int)channels, (int)sampleRate);

        Console.WriteLine("=== AMS CLI ===");
        Console.WriteLine($"Frames processed : {totalFrames:n0}");
        Console.WriteLine($"Channels         : {channels}");
        Console.WriteLine($"Sample Rate      : {sampleRate.ToString(CultureInfo.InvariantCulture)} Hz");
        Console.WriteLine($"Max Block        : {maxBlock}");
        Console.WriteLine($"Elapsed          : {sw.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Latency (report) : {dsp.LatencySamples} samples");
        Console.WriteLine($"Output written   : {outPath}");

        Console.WriteLine($"Input RMS: {Rms(input[0]):F6}");
        Console.WriteLine($"Output RMS: {Rms(output[0]):F6}");
        Console.WriteLine($"Delta (dB): {20 * Math.Log10(Rms(output[0]) / Rms(input[0])):F2} dB");  

        return 0;
    }
    static double Rms(float[] buf)
    {
        double sum = 0;
        foreach (var v in buf) sum += v * v;
        return Math.Sqrt(sum / buf.Length);
    }
    private static float[][] NewPlanar(uint channels, int frames)
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
        int bytesPerSample = 4;
        int fmtChunkSize = 16;
        int bitsPerSample = 32;
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
        for (int ch = 0; ch < channels; ch++)
            bw.Write(planar[ch][n]);
    }
}