using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class PlanWindowsCommand
{
    public static Command Create()
    {
        var cmd = new Command("plan-windows", "Plan chunk windows using silence boundaries");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");
        var minOption = new Option<double>("--min", () => 60.0, "Minimum window duration in seconds");
        var maxOption = new Option<double>("--max", () => 90.0, "Maximum window duration in seconds");
        var targetOption = new Option<double>("--target", () => 75.0, "Target window duration in seconds");
        var strictTailOption = new Option<bool>("--strict-tail", () => true, "Strict tail constraint");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(minOption);
        cmd.AddOption(maxOption);
        cmd.AddOption(targetOption);
        cmd.AddOption(strictTailOption);

        cmd.SetHandler(async (input, work, paramsFile, force, min, max, target, strictTail) =>
        {
            var workDir = work?.FullName ?? input!.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            WindowPlanningParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<WindowPlanningParams>(json) ?? new WindowPlanningParams(min, max, target, strictTail);
            }
            else
            {
                parameters = new WindowPlanningParams(min, max, target, strictTail);
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("plan", wd => new PlanWindowsStage(wd, new SilenceWindowPlanner(), parameters));

            var ok = await runner.RunAsync(input!.FullName, workDir, fromStage: "plan", toStage: "plan", force: force);
            if (!ok) Environment.Exit(1);
        }, inOption, workOption, paramsOption, forceOption, minOption, maxOption, targetOption, strictTailOption);

        return cmd;
    }
}
