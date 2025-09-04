using System.CommandLine;
using System.Globalization;
using Ams.Cli.Commands;
using Ams.Core;

namespace Ams.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // If no arguments provided, run legacy DSP demo (interactive)
        if (args.Length == 0)
        {
            return RunLegacyDspDemoInteractive();
        }
        
        // Create root command
        var rootCommand = new RootCommand("AMS - Audio Management System CLI");
        
        // Add commands
        rootCommand.AddCommand(AsrCommand.Create());
        rootCommand.AddCommand(EnvCommand.Create());
        rootCommand.AddCommand(ValidateCommand.Create());
        rootCommand.AddCommand(ValidateManifestCommand.Create());
        rootCommand.AddCommand(TextCommand.Create());
        rootCommand.AddCommand(BuildIndexCommand.Create());
        rootCommand.AddCommand(BookCommand.Create());
        rootCommand.AddCommand(AlignCommand.Create());
        rootCommand.AddCommand(AudioCommand.Create());
        rootCommand.AddCommand(RefineSentencesCommand.Create());
        
        // New staged-pipeline commands (top-level helpers)
        rootCommand.AddCommand(Ams.Cli.Commands.AlignChunksCommand.Create());
        rootCommand.AddCommand(Ams.Cli.Commands.RefineCommand.Create());
        rootCommand.AddCommand(Ams.Cli.Commands.CollateCommand.Create());
        
        // Add legacy DSP command (interactive)
        var dspCommand = new Command("dsp", "Run DSP processing demo (interactive)");
        dspCommand.SetHandler(() => RunLegacyDspDemoInteractive());
        rootCommand.AddCommand(dspCommand);
        
        return await rootCommand.InvokeAsync(args);
    }

    private static int RunLegacyDspDemoInteractive()
    {
        const float sampleRate = 48000f;
        const int channels = 2;
        const uint maxBlock = 512;
        const double hz = 440.0;       // A4 test tone
        const float inGainDb = -12.0f; // input sine amplitude

        Console.Write("Enter gain (0-1): ");
        float gain = float.Parse(Console.ReadLine() ?? "0.5", CultureInfo.InvariantCulture);

        Console.Write("How long? (seconds): ");
        double seconds = double.Parse(Console.ReadLine() ?? "2.0", CultureInfo.InvariantCulture);

        // var result = DspDemoRunner.RunDemo(sampleRate, channels, maxBlock, hz, inGainDb, seconds, gain);

        Console.WriteLine("=== AMS CLI ===");
        Console.WriteLine("DSP Demo temporarily disabled");
        // Console.WriteLine($"Frames processed : {result.FramesProcessed:n0}");
        // Console.WriteLine($"Channels         : {result.Channels}");
        // Console.WriteLine($"Sample Rate      : {result.SampleRate.ToString(CultureInfo.InvariantCulture)} Hz");
        // Console.WriteLine($"Max Block        : {result.MaxBlock}");
        // Console.WriteLine($"Elapsed          : {result.ElapsedMilliseconds:F3} ms");
        // Console.WriteLine($"Latency (report) : {result.LatencySamples} samples");
        // Console.WriteLine($"Output written   : {result.OutputPath}");
        // Console.WriteLine($"Input RMS        : {result.InputRms:F6}");
        // Console.WriteLine($"Output RMS       : {result.OutputRms:F6}");
        // Console.WriteLine($"Delta (dB)       : {result.DeltaDb:F2} dB");

        return 0;
    }
}
