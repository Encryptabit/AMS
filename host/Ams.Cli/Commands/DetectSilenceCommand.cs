using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class DetectSilenceCommand
{
    public static Command Create()
    {
        var cmd = new Command("detect-silence", "Detect silence windows using ffmpeg silencedetect");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        var silenceThresholdOption = new Option<double>("--silence-threshold-db", () => -30.0, "Silence noise floor in dBFS");
        silenceThresholdOption.AddAlias("--db-floor");

        var silenceMinOption = new Option<double>("--silence-min-dur", () => 0.3, "Minimum silence duration in seconds");
        silenceMinOption.AddAlias("--min-silence-dur");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(silenceThresholdOption);
        cmd.AddOption(silenceMinOption);

        cmd.SetHandler(async (input, work, paramsFile, force, silenceThresholdDb, silenceMinDur) =>
        {
            var workDir = work?.FullName ?? input!.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            SilenceDetectionParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<SilenceDetectionParams>(json) ?? new SilenceDetectionParams(silenceThresholdDb, silenceMinDur);
            }
            else
            {
                parameters = new SilenceDetectionParams(silenceThresholdDb, silenceMinDur);
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("timeline", wd => new DetectSilenceStage(wd, new FfmpegSilenceDetector(), new DefaultProcessRunner(), parameters));

            var ok = await runner.RunAsync(input!.FullName, workDir, fromStage: "timeline", toStage: "timeline", force: force);
            if (!ok) Environment.Exit(1);
        }, inOption, workOption, paramsOption, forceOption, silenceThresholdOption, silenceMinOption);

        return cmd;
    }
}
