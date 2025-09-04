using System.CommandLine;
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
        var toOption = new Option<string>("--to", () => "export", "End stage name");
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

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("timeline", wd => new DetectSilenceStage(wd, new FfmpegSilenceDetector(), new DefaultProcessRunner(), new SilenceDetectionParams(-30.0, 0.3)));
            runner.RegisterStage("plan", wd => new PlanWindowsStage(wd, new SilenceWindowPlanner(), new WindowPlanningParams(60.0, 90.0, 75.0, true)));
            // TODO: add chunk-audio, transcribe, collate, align, refine, export

            var ok = await runner.RunAsync(input!.FullName, workDir, from, to, force && !resume);
            if (!ok) Environment.Exit(1);
        }, inOption, workOption, fromOption, toOption, resumeOption, forceOption, jobsOption);

        return cmd;
    }
}
