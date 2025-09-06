using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AnchorsCommand
{
    public static Command Create()
    {
        var cmd = new Command("anchors", "Generate deterministic n-gram anchors between BookIndex and ASR tokens");

        var bookOption = new Option<FileInfo>("--book", "Path to book index JSON file") { IsRequired = true };
        var asrOption = new Option<FileInfo>("--asr", "Path to ASR JSON file") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory") { IsRequired = true };
        var paramsOption = new Option<FileInfo>("--params", "Parameters JSON file");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");
        var ngramOption = new Option<int>("--ngram", () => 3, "N-gram size for anchor mining");
        var minSepOption = new Option<int>("--min-separation", () => 50, "Minimum separation between anchors in tokens");
        var targetOption = new Option<double>("--target-per-tokens", () => 0.02, "Target anchors per tokens ratio");

        cmd.AddOption(bookOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(workOption);
        cmd.AddOption(paramsOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(ngramOption);
        cmd.AddOption(minSepOption);
        cmd.AddOption(targetOption);

        cmd.SetHandler(async (book, asr, work, paramsFile, force, ngram, minSep, target) =>
        {
            // Refactorv2: Step 5 - Implement anchors command handler
            var workDir = work!.FullName;
            Directory.CreateDirectory(workDir);

            AnchorsParams parameters;
            if (paramsFile?.Exists == true)
            {
                var json = await File.ReadAllTextAsync(paramsFile.FullName);
                parameters = System.Text.Json.JsonSerializer.Deserialize<AnchorsParams>(json) ?? 
                    new AnchorsParams(ngram, 2, target, minSep, "en-basic", true);
            }
            else
            {
                parameters = new AnchorsParams(ngram, 2, target, minSep, "en-basic", true);
            }

            var runner = new AsrPipelineRunner();
            runner.RegisterStage("anchors", wd => new AnchorsStage(wd, parameters, book!.FullName, asr!.FullName));

            // Refactorv2: Step 5 - Use book file as input placeholder for manifest
            var ok = await runner.RunAsync(book!.FullName, workDir, fromStage: "anchors", toStage: "anchors", force: force);
            if (!ok) Environment.Exit(1);
        }, bookOption, asrOption, workOption, paramsOption, forceOption, ngramOption, minSepOption, targetOption);

        return cmd;
    }
}