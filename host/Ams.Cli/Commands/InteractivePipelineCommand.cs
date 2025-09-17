using System;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine;
using System.Globalization;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Asr.Pipeline;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class InteractivePipelineCommand
{
    private static readonly VolumeAnalysisParams DefaultVolumeParams = new(
        DbFloor: -45.0,
        SpeechFloorDb: -35.0,
        MinProbeSec: 0.080,
        ProbeWindowSec: 0.050,
        HfBandLowHz: 3500.0,
        HfBandHighHz: 12000.0,
        HfMarginDb: 5.0,
        WeakMarginDb: 2.5,
        NudgeStepSec: 0.003,
        MaxLeftNudges: 8,
        MaxRightNudges: 3,
        GuardLeftSec: 0.012,
        GuardRightSec: 0.015);

    public static Command Create()
    {
        var cmd = new Command("interactive", "Run the AMS pipeline in an interactive loop");
        cmd.SetHandler(async () => await RunInteractiveLoopAsync());
        return cmd;
    }

    public static async Task<int> RunInteractiveLoopAsync()
    {
        Console.WriteLine("AMS Interactive Pipeline Shell");
        Console.WriteLine("Type 'run' to execute, 'help' for options, or 'quit' to exit.\n");

        using var httpClient = new HttpClient();

        while (true)
        {
            Console.Write("interactive> ");
            var line = Console.ReadLine();
            if (line is null)
            {
                Console.WriteLine();
                break;
            }

            var command = line.Trim();
            if (command.Length == 0)
                continue;

            switch (command.ToLowerInvariant())
            {
                case "quit":
                case "exit":
                    Console.WriteLine("Exiting interactive shell.");
                    return 0;
                case "help":
                    PrintHelp();
                    continue;
                case "run":
                    var context = PromptRunContext();
                    if (context is null)
                    {
                        Console.WriteLine("Run cancelled.");
                        continue;
                    }

                    Directory.CreateDirectory(context.WorkDir);
                    var runner = ConfigureRunner(context, httpClient);

                    try
                    {
                        var force = context.Force && !context.Resume;
                        var ok = await runner.RunAsync(context.AudioPath, context.WorkDir, context.FromStage, context.ToStage, force);
                        Console.WriteLine(ok ? "Pipeline run completed." : "Pipeline run failed. Check logs for details.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pipeline execution error: {ex.Message}");
                    }

                    continue;
                default:
                    Console.WriteLine("Unknown command. Type 'help' to see available options.");
                    continue;
            }
        }

        return 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  run   - Prompt for parameters and execute the pipeline");
        Console.WriteLine("  help  - Show this message");
        Console.WriteLine("  quit  - Exit the interactive shell");
    }

    private static InteractiveContext? PromptRunContext()
    {
        string audioPath = PromptRequired("Input audio file path");
        if (!File.Exists(audioPath))
        {
            Console.WriteLine("Audio file does not exist.");
            return null;
        }

        string bookPath = PromptRequired("Book file path");
        if (!File.Exists(bookPath))
        {
            Console.WriteLine("Book file does not exist.");
            return null;
        }

        var workDirInput = PromptOptional("Work directory (leave blank to use <audio>.ams)");
        var workDir = string.IsNullOrWhiteSpace(workDirInput) ? audioPath + ".ams" : workDirInput;

        var fromStage = PromptOptional("From stage [timeline]") ?? "timeline";
        var toStage = PromptOptional("To stage [validate]") ?? "validate";
        var resume = PromptYesNo("Resume previous run? [y/N]", defaultValue: false);
        var force = PromptYesNo("Force re-run from from-stage? [y/N]", defaultValue: false);

        var silenceThreshold = PromptDouble("Silence threshold dBFS [-38]", -38.0);
        var silenceMin = PromptDouble("Minimum silence duration seconds [0.30]", 0.30);
        var roomtoneFile = PromptOptional("Roomtone file (leave blank for auto)");
        if (!string.IsNullOrWhiteSpace(roomtoneFile) && !File.Exists(roomtoneFile!))
        {
            Console.WriteLine("Roomtone file not found; using auto generation.");
            roomtoneFile = null;
        }

        var asrService = PromptOptional("ASR service URL [http://localhost:8081]") ?? "http://localhost:8081";
        var alignService = PromptOptional("Alignment service URL [http://localhost:8082]") ?? "http://localhost:8082";

        return new InteractiveContext(
            audioPath,
            Path.GetFullPath(bookPath),
            Path.GetFullPath(workDir),
            fromStage,
            toStage,
            resume,
            force,
            silenceThreshold,
            silenceMin,
            roomtoneFile,
            asrService,
            alignService);
    }

    private static string PromptRequired(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            Console.WriteLine("Value required.");
        }
    }

    private static string? PromptOptional(string label)
    {
        Console.Write($"{label}: ");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input.Trim();
    }

    private static bool PromptYesNo(string label, bool defaultValue)
    {
        Console.Write($"{label}: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;

        return input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) ||
               input.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private static double PromptDouble(string label, double defaultValue)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            if (double.TryParse(input.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            Console.WriteLine("Invalid number. Try again.");
        }
    }

    private static AsrPipelineRunner ConfigureRunner(InteractiveContext context, HttpClient httpClient)
    {
        var runner = new AsrPipelineRunner();

        runner.RegisterStage("book-index", wd => new BookIndexStage(wd, context.BookPath, new BookIndexOptions { AverageWpm = 200.0 }));
        runner.RegisterStage("timeline", wd => new DetectSilenceStage(wd, new FfmpegSilenceDetector(), new DefaultProcessRunner(), new SilenceDetectionParams(context.SilenceThresholdDb, context.SilenceMinDurationSec)));
        runner.RegisterStage("plan", wd => new PlanWindowsStage(wd, new SilenceWindowPlanner(), new WindowPlanningParams(60.0, 90.0, 75.0, true)));
        runner.RegisterStage("chunks", wd => new ChunkAudioStage(wd, new DefaultProcessRunner(), new ChunkingParams("wav", 44100, DefaultVolumeParams)));
        runner.RegisterStage("transcripts", wd => new TranscribeStage(wd, httpClient, new TranscriptionParams("nvidia/parakeet-ctc-0.6b", "en", 1, 1.0, context.AsrServiceUrl)));
        runner.RegisterStage("align-chunks", wd => new AlignChunksStage(wd, httpClient, new AlignmentParams("eng", 600, context.AlignServiceUrl)));
        runner.RegisterStage("anchors", wd =>
        {
            var bookIndexPath = Path.Combine(wd, "book.index.json");
            var asrMerged = Path.Combine(wd, "transcripts", "merged.json");
            return new AnchorsStage(wd, bookIndexPath, asrMerged, new AnchorsParams(3, 50, 2, 50, "v1", "english+domain"));
        });
        runner.RegisterStage("refine", wd => new RefineStage(wd, new RefinementParams(context.SilenceThresholdDb, context.SilenceMinDurationSec)));
        runner.RegisterStage("collate", wd => new CollateStage(wd, new DefaultProcessRunner(), new CollationParams(
            context.RoomtoneFile is null ? "auto" : "file",
            -50.0,
            5,
            2000,
            60,
            context.RoomtoneFile,
            context.SilenceThresholdDb,
            false,
            null,
            5.0,
            15.0,
            true)));
        runner.RegisterStage("script-compare", wd =>
        {
            var bookIndexPath = Path.Combine(wd, "book.index.json");
            var rules = new ComparisonRules(true, true, true, Array.Empty<(string, string)>(), Array.Empty<string>(), "cmp-rules/v1");
            return new ScriptCompareStage(wd, bookIndexPath, new ScriptCompareParams(rules, "default"));
        });
        runner.RegisterStage("validate", wd => new ValidateStage(wd, new ValidationParams(0.25, 0.25, null)));

        return runner;
    }

    private sealed record InteractiveContext(
        string AudioPath,
        string BookPath,
        string WorkDir,
        string FromStage,
        string ToStage,
        bool Resume,
        bool Force,
        double SilenceThresholdDb,
        double SilenceMinDurationSec,
        string? RoomtoneFile,
        string AsrServiceUrl,
        string AlignServiceUrl);
}
