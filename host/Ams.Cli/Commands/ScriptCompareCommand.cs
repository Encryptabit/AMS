using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class ScriptCompareCommand
{
    public static Command Create()
    {
        var cmd = new Command("script-compare", "Compare collated transcript vs BookIndex with window-scoped scoring");

        var bookOption = new Option<FileInfo>("--book", "Path to book index JSON file") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory") { IsRequired = true };
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(bookOption);
        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (book, work, paramsFile, force) =>
        {
            // Refactorv2: Step 10 - Implement script-compare command handler
            var workDir = work!.FullName;

            ScriptCompareParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<ScriptCompareParams>(json) ?? 
                    new ScriptCompareParams();
            }
            else
            {
                parameters = new ScriptCompareParams();
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("script-compare", wd => new ScriptCompareStage(wd, parameters, book!.FullName));

            // Refactorv2: Step 10 - Use book file as input
            var ok = await runner.RunAsync(book!.FullName, workDir, fromStage: "script-compare", toStage: "script-compare", force: force);
            if (!ok) Environment.Exit(1);
        }, bookOption, workOption, paramsOption, forceOption);

        return cmd;
    }
}