using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class WindowsCommand
{
    public static Command Create()
    {
        var cmd = new Command("windows", "Build half-open windows between anchors with pads");

        var workOption = new Option<DirectoryInfo>("--work", "Work directory") { IsRequired = true };
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");
        var prePadOption = new Option<double>("--pre-pad-s", () => 1.0, "Pre-pad duration in seconds");
        var padOption = new Option<double>("--pad-s", () => 0.6, "Pad duration in seconds");

        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(prePadOption);
        cmd.AddOption(padOption);

        cmd.SetHandler(async (work, paramsFile, force, prePad, pad) =>
        {
            // Refactorv2: Step 6 - Implement windows command handler
            var workDir = work!.FullName;

            WindowsParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<WindowsParams>(json) ?? 
                    new WindowsParams(prePad, pad);
            }
            else
            {
                parameters = new WindowsParams(prePad, pad);
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("windows", wd => new WindowsStage(wd, parameters));

            // Refactorv2: Step 6 - Use dummy input for manifest (windows stage doesn't need audio input)
            var ok = await runner.RunAsync(workDir + "/dummy", workDir, fromStage: "windows", toStage: "windows", force: force);
            if (!ok) Environment.Exit(1);
        }, workOption, paramsOption, forceOption, prePadOption, padOption);

        return cmd;
    }
}