using System.CommandLine;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AsrRunCommand
{
    public static Command Create()
    {
        var cmd = new Command(
            "run",
            "Run ASR pipeline from start to finish or a specific span.\n" +
            "Required prerequisites:\n" +
            "- --book <file> (DOCX/TXT/MD/RTF). This command writes <work>/book.index.json via the 'book-index' stage.\n" +
            "- FFmpeg in PATH (silence detection, slicing).\n" +
            "- Services: ASR at --asr-service (default http://localhost:8081); Aeneas at --align-service (default http://localhost:8082) if running alignment stages.\n" +
            "Tips: use --from/--to to run a 1..n span (e.g., --from book-index --to validate)."
        );

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var bookOption = new Option<FileInfo>("--book", "Path to the book file (DOCX, TXT, MD, RTF)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var fromOption = new Option<string>("--from", () => "timeline", "Start stage name");
        var toOption = new Option<string>("--to", () => "validate", "End stage name");
        var resumeOption = new Option<bool>("--resume", "Resume (skip completed stages)");
        var forceOption = new Option<bool>("--force", "Force re-run from --from stage");
        var jobsOption = new Option<int>("--jobs", () => 1, "Parallelism for future stages");
        var asrServiceOption = new Option<string>("--asr-service", () => "http://localhost:8081", "ASR service URL");
        var alignServiceOption = new Option<string>("--align-service", () => "http://localhost:8082", "Aeneas alignment service URL");
        var dbFloorOption = new Option<double>("--db-floor", () => -38.0, "Silence detection threshold in dBFS (lower = stricter)");
        var minDurOption = new Option<double>("--min-dur", () => 0.12, "Minimum silence duration in seconds");
        var roomtoneFileOption = new Option<string>("--roomtone-file", "Path to roomtone file (if not provided, will auto-generate)");

        cmd.AddOption(inOption);
        cmd.AddOption(bookOption);
        cmd.AddOption(workOption);
        cmd.AddOption(fromOption);
        cmd.AddOption(toOption);
        cmd.AddOption(resumeOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(jobsOption);
        cmd.AddOption(asrServiceOption);
        cmd.AddOption(alignServiceOption);
        cmd.AddOption(dbFloorOption);
        cmd.AddOption(minDurOption);
        cmd.AddOption(roomtoneFileOption);

        cmd.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inOption)!;
            var book  = context.ParseResult.GetValueForOption(bookOption)!;
            var work = context.ParseResult.GetValueForOption(workOption);
            var from = context.ParseResult.GetValueForOption(fromOption)!;
            var to = context.ParseResult.GetValueForOption(toOption)!;
            var resume = context.ParseResult.GetValueForOption(resumeOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var jobs = context.ParseResult.GetValueForOption(jobsOption);
            var asrService = context.ParseResult.GetValueForOption(asrServiceOption)!;
            var alignService = context.ParseResult.GetValueForOption(alignServiceOption)!;
            var dbFloor = context.ParseResult.GetValueForOption(dbFloorOption);
            var minDur = context.ParseResult.GetValueForOption(minDurOption);
            var roomtoneFile = context.ParseResult.GetValueForOption(roomtoneFileOption);
            
            var workDir = work?.FullName ?? input.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            var httpClient = new HttpClient();
            var runner = new AsrPipelineRunner();

            // Register all stages with user-provided parameters
            runner.RegisterStage("book-index", wd => new BookIndexStage(wd, book.FullName, new BookIndexOptions { AverageWpm = 200.0 }));
            runner.RegisterStage("timeline", wd => new DetectSilenceStage(wd, new FfmpegSilenceDetector(), new DefaultProcessRunner(), new SilenceDetectionParams(dbFloor, minDur)));
            runner.RegisterStage("plan", wd => new PlanWindowsStage(wd, new SilenceWindowPlanner(), new WindowPlanningParams(60.0, 90.0, 75.0, true)));
            runner.RegisterStage("chunks", wd => new ChunkAudioStage(wd, new DefaultProcessRunner(), new ChunkingParams("wav", 44100)));
            runner.RegisterStage("transcripts", wd => new TranscribeStage(wd, httpClient, new TranscriptionParams("nvidia/parakeet-ctc-0.6b", "en", 1, 1.0, asrService)));
            // Optional legacy: align-chunks (kept for experiments)
            runner.RegisterStage("align-chunks", wd => new AlignChunksStage(wd, httpClient, new AlignmentParams("eng", 600, alignService)));
            // New v2 stages
            runner.RegisterStage("anchors", wd =>
            {
                var bookIndexPath = Path.Combine(wd, "book.index.json"); // expects user-provided
                var asrMerged = Path.Combine(wd, "transcripts", "merged.json");
                return new AnchorsStage(wd, bookIndexPath, asrMerged, new AnchorsParams(3, 50, 2, 50, "v1", "english+domain"));
            });
            runner.RegisterStage("windows", wd => new WindowsStage(wd, new WindowsParams(1.0, 0.6)));
            runner.RegisterStage("window-align", wd => new WindowAlignStage(wd, httpClient, new WindowAlignParams("eng", 600, 600, alignService)));
            runner.RegisterStage("refine", wd => new RefineStage(wd, new RefinementParams(dbFloor, minDur)));
            runner.RegisterStage("collate", wd => new CollateStage(wd, new DefaultProcessRunner(), new CollationParams(
                roomtoneFile != null ? "file" : "auto", 
                -50.0,
                5,
                2000,
                60,
                roomtoneFile,
                dbFloor,
                // Interword defaults (feature off)
                false,
                null,
                5.0,
                15.0,
                true)));
            runner.RegisterStage("script-compare", wd =>
            {
                var bookIndexPath = Path.Combine(wd, "book.index.json");
                var rules = new ComparisonRules(true, true, true, new (string,string)[]{}, new string[]{}, "cmp-rules/v1");
                return new ScriptCompareStage(wd, bookIndexPath, new ScriptCompareParams(rules, "default"));
            });
            runner.RegisterStage("validate", wd => new ValidateStage(wd, new ValidationParams(0.25, 0.25, null)));

            var ok = await runner.RunAsync(input.FullName, workDir, from, to, force && !resume);
            if (!ok) Environment.Exit(1);
        });

        return cmd;
    }
}
