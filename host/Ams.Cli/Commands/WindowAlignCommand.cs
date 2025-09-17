using System;
using System.CommandLine;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class WindowAlignCommand
{
    public static Command Create()
    {
        var descriptionLines = new[]
        {
            "Align anchor windows via Aeneas service.",
            "Required prerequisites:",
            "- <work>/book.index.json.",
            "- windows/windows.json (produced by 'windows' stage).",
            "- transcripts/merged.json under <work>.",
            "- Aeneas service reachable at --service (default http://localhost:8082).",
            "Note: extracts audio slices with FFmpeg; ensure ffmpeg is on PATH.",
            "Outputs microsecond-precision fragments (6 decimal places)."
        };
        var description = string.Join(Environment.NewLine, descriptionLines);
        var cmd = new Command(
            "window-align",
            description
        );
        var work = new Option<DirectoryInfo>("--work"){ IsRequired = true };
        var url = new Option<string>("--service", () => "http://localhost:8082", "Aeneas service URL");
        var bw = new Option<int>("--band-width-ms", () => 600, "Band width milliseconds");
        var lang = new Option<string>("--language", () => "eng", "Language code");
        var timeout = new Option<int>("--timeout-sec", () => 600, "Timeout seconds");
        cmd.Add(work); cmd.Add(url); cmd.Add(bw); cmd.Add(lang); cmd.Add(timeout);
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
            var p = new WindowAlignParams(
                ctx.ParseResult.GetValueForOption(lang) ?? "eng",
                ctx.ParseResult.GetValueForOption(timeout),
                ctx.ParseResult.GetValueForOption(bw),
                ctx.ParseResult.GetValueForOption(url) ?? "http://localhost:8082");
            using var httpClient = new HttpClient();
            var stage = new WindowAlignStage(wd, httpClient, p);
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
