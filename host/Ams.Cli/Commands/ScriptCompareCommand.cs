using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class ScriptCompareCommand
{
    public static Command Create()
    {
        var cmd = new Command("script-compare", "Compare script vs collated segments per anchor-window");
        var work = new Option<DirectoryInfo>("--work"){ IsRequired = true };
        var book = new Option<FileInfo>("--book"){ IsRequired = true };
        cmd.Add(work); cmd.Add(book);
        cmd.SetHandler(async ctx =>
        {
            var wd = ctx.ParseResult.GetValueForOption(work)!.FullName;
            var bk = ctx.ParseResult.GetValueForOption(book)!.FullName;
            var rules = new ComparisonRules(true, true, true, new (string,string)[]{}, new string[]{}, "cmp-rules/v1");
            var stage = new ScriptCompareStage(wd, bk, new ScriptCompareParams(rules, "default"));
            var manifestPath = Path.Combine(wd, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Console.Error.WriteLine("Manifest not found. Use `asr run` or create manifest first.");
                Environment.Exit(2);
                return;
            }
            var json = await File.ReadAllTextAsync(manifestPath);
            var manifest = System.Text.Json.JsonSerializer.Deserialize<ManifestV2>(json)!;
            await stage.RunAsync(manifest);
        });
        return cmd;
    }
}
