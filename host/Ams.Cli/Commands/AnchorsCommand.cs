using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AnchorsCommand
{
    public static Command Create()
    {
        var cmd = new Command("anchors", "Compute deterministic anchors between BookIndex and ASR merged words");
        var work = new Option<DirectoryInfo>("--work", "Work dir (.ams)") { IsRequired = true };
        var book = new Option<FileInfo>("--book", "Path to book.index.json") { IsRequired = true };
        var n = new Option<int>("--ngram", () => 3, "N-gram size");
        var minSep = new Option<int>("--min-separation", () => 50, "Min token separation for duplicates");
        var relax = new Option<int>("--relax-down", () => 2, "Relaxation steps if sparse");
        var target = new Option<int>("--target-per-tokens", () => 50, "Anchor density target (1 per N tokens)");
        cmd.Add(work); cmd.Add(book); cmd.Add(n); cmd.Add(minSep); cmd.Add(relax); cmd.Add(target);
        cmd.SetHandler(async ctx =>
        {
            var wd = ctx.ParseResult.GetValueForOption(work)!.FullName;
            var bk = ctx.ParseResult.GetValueForOption(book)!.FullName;
            var p = new AnchorsParams(ctx.ParseResult.GetValueForOption(n), ctx.ParseResult.GetValueForOption(minSep), ctx.ParseResult.GetValueForOption(relax), ctx.ParseResult.GetValueForOption(target), "v1", "english+domain");
            var stage = new AnchorsStage(wd, bk, Path.Combine(wd, "transcripts", "merged.json"), p);
            // Ad-hoc invocation: require manifest to exist; recommend using `asr run`
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
