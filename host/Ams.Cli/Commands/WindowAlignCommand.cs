using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class WindowAlignCommand
{
    public static Command Create()
    {
        var cmd = new Command("window-align", "Force alignment within each window using Aeneas");

        var workOption = new Option<DirectoryInfo>("--work", "Work directory") { IsRequired = true };
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");
        var serviceOption = new Option<string>("--service", () => "http://localhost:8082", "Aeneas service URL");
        var bandWidthOption = new Option<int>("--band-width-ms", () => 600, "DP band width in milliseconds");
        var timeoutOption = new Option<int>("--timeout-sec", () => 600, "Alignment timeout in seconds");

        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(serviceOption);
        cmd.AddOption(bandWidthOption);
        cmd.AddOption(timeoutOption);

        cmd.SetHandler(async (work, paramsFile, force, service, bandWidth, timeout) =>
        {
            // Refactorv2: Step 7 - Implement window-align command handler
            var workDir = work!.FullName;

            WindowAlignParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<WindowAlignParams>(json) ?? 
                    new WindowAlignParams("eng", service, timeout, bandWidth, true, true);
            }
            else
            {
                parameters = new WindowAlignParams("eng", service, timeout, bandWidth, true, true);
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("window-align", wd => new WindowAlignStage(wd, parameters));

            // Refactorv2: Step 7 - Use dummy input for manifest (window-align uses existing chunks)
            var ok = await runner.RunAsync(workDir + "/dummy", workDir, fromStage: "window-align", toStage: "window-align", force: force);
            if (!ok) Environment.Exit(1);
        }, workOption, paramsOption, forceOption, serviceOption, bandWidthOption, timeoutOption);

        return cmd;
    }
}