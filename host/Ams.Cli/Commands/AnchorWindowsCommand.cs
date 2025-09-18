using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AnchorWindowsCommand
{
    public static Command Create()
    {
        var cmd = new Command(
            "anchor-windows",
            "Build half-open anchor windows (token search windows) with pads.\n" +
            "Required prerequisites:\n" +
            "- <work>/book.index.json (build-index or asr run --book).\n" +
            "- anchors/anchors.json under <work> (produced by AnchorsStage via 'asr run' or anchors command)."
        );
        var work = new Option<DirectoryInfo>("--work") { IsRequired = true };
        var pre = new Option<double>("--pre-pad-s", () => 1.0, "Pre pad seconds");
        var pad = new Option<double>("--pad-s", () => 0.6, "Pad seconds");
        cmd.Add(work); cmd.Add(pre); cmd.Add(pad);
        cmd.SetHandler(async ctx =>
        {
            var wd = ctx.ParseResult.GetValueForOption(work)!.FullName;
            var canonicalBook = Path.Combine(wd, "book.index.json");
            if (!File.Exists(canonicalBook))
            {
                Console.Error.WriteLine("Missing book.index.json in work directory.");
                Console.Error.WriteLine("Build it first: dotnet run --project host/Ams.Cli -- build-index --book <book.docx|.txt|.md|.rtf> --out <work>/book.index.json");
                Console.Error.WriteLine("Or run the pipeline with: asr run --book <bookfile> --in <audio.wav> --work <work>");
                Environment.Exit(2);
                return;
            }
            var p = new AnchorWindowParams(ctx.ParseResult.GetValueForOption(pre), ctx.ParseResult.GetValueForOption(pad));
            var stage = new AnchorWindowsStage(wd, p);
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
