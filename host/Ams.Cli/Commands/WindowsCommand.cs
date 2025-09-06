using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class WindowsCommand
{
    public static Command Create()
    {
        var cmd = new Command("windows", "Build half-open anchor windows with pads");
        var work = new Option<DirectoryInfo>("--work") { IsRequired = true };
        var pre = new Option<double>("--pre-pad-s", () => 1.0, "Pre pad seconds");
        var pad = new Option<double>("--pad-s", () => 0.6, "Pad seconds");
        cmd.Add(work); cmd.Add(pre); cmd.Add(pad);
        cmd.SetHandler(async ctx =>
        {
            var wd = ctx.ParseResult.GetValueForOption(work)!.FullName;
            var p = new WindowsParams(ctx.ParseResult.GetValueForOption(pre), ctx.ParseResult.GetValueForOption(pad));
            var stage = new WindowsStage(wd, p);
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
