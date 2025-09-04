using System.CommandLine;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AsrRunCommand
{
    public static Command Create()
    {
        var cmd = new Command("run", "Run ASR pipeline from start to finish or between specified stages");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var fromOption = new Option<string>("--from", () => "timeline", "Start stage name");
        var toOption = new Option<string>("--to", () => "validate", "End stage name");
        var resumeOption = new Option<bool>("--resume", "Resume (skip completed stages)");
        var forceOption = new Option<bool>("--force", "Force re-run from --from stage");
        var jobsOption = new Option<int>("--jobs", () => 1, "Parallelism for future stages");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(fromOption);
        cmd.AddOption(toOption);
        cmd.AddOption(resumeOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(jobsOption);

        cmd.SetHandler(async (input, work, from, to, resume, force, jobs) =>
        {
            var workDir = work?.FullName ?? input!.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            var httpClient = new HttpClient();
            var runner = new AsrPipelineRunner();
            
            // Register all stages with default parameters
            runner.RegisterStage("timeline", wd => new DetectSilenceStage(wd, new FfmpegSilenceDetector(), new DefaultProcessRunner(), new SilenceDetectionParams(-30.0, 0.3)));
            runner.RegisterStage("plan", wd => new PlanWindowsStage(wd, new SilenceWindowPlanner(), new WindowPlanningParams(60.0, 90.0, 75.0, true)));
            runner.RegisterStage("chunks", wd => new ChunkAudioStage(wd, new DefaultProcessRunner(), new ChunkingParams("wav", 44100)));
            runner.RegisterStage("transcripts", wd => new TranscribeStage(wd, httpClient, new TranscriptionParams("nvidia/parakeet-ctc-0.6b", "en", 1, 1.0, "http://localhost:8081")));
            runner.RegisterStage("align-chunks", wd => new AlignChunksStage(wd, httpClient, new AlignmentParams("eng", 600, "http://localhost:8082")));
            runner.RegisterStage("refine", wd => new RefineStage(wd, new RefinementParams(-30.0, 0.12)));
            runner.RegisterStage("collate", wd => new CollateStage(wd, new DefaultProcessRunner(), new CollationParams("auto", -50.0, 5, 2000, 60, null)));
            runner.RegisterStage("validate", wd => new ValidateStage(wd, new ValidationParams(0.25, 0.25, null)));

            var ok = await runner.RunAsync(input!.FullName, workDir, from, to, force && !resume);
            if (!ok) Environment.Exit(1);
        }, inOption, workOption, fromOption, toOption, resumeOption, forceOption, jobsOption);

        return cmd;
    }
}
