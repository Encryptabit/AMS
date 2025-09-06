using System.CommandLine;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class WindowAlignCommand
{
    public static Command Create()
    {
        var cmd = new Command("window-align", "Align anchor windows via Aeneas service");
        var work = new Option<DirectoryInfo>("--work"){ IsRequired = true };
        var url = new Option<string>("--service", () => "http://localhost:8082", "Aeneas service URL");
        var bw = new Option<int>("--band-width-ms", () => 600, "Band width milliseconds");
        var lang = new Option<string>("--language", () => "eng", "Language code");
        var timeout = new Option<int>("--timeout-sec", () => 600, "Timeout seconds");
        cmd.Add(work); cmd.Add(url); cmd.Add(bw); cmd.Add(lang); cmd.Add(timeout);
        cmd.SetHandler(async ctx =>
        {
            var wd = ctx.ParseResult.GetValueForOption(work)!.FullName;
            var p = new WindowAlignParams(ctx.ParseResult.GetValueForOption(lang), ctx.ParseResult.GetValueForOption(timeout), ctx.ParseResult.GetValueForOption(bw), ctx.ParseResult.GetValueForOption(url));
            var stage = new WindowAlignStage(wd, new HttpClient(), p);
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
