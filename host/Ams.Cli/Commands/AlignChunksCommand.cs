using System.CommandLine;
using System.Net.Http;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AlignChunksCommand
{
    public static Command Create()
    {
        var cmd = new Command("align-chunks", "Run forced alignment on audio chunks using Aeneas service");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var serviceOption = new Option<string>("--service", () => "http://localhost:8082", "Aeneas service URL");
        var langOption = new Option<string>("--lang", () => "eng", "Language code for alignment");
        var timeoutOption = new Option<int>("--timeout", () => 600, "Timeout in seconds per chunk");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(serviceOption);
        cmd.AddOption(langOption);
        cmd.AddOption(timeoutOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (input, work, service, lang, timeout, force) =>
        {
            var workDir = work?.FullName ?? input!.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            var httpClient = new HttpClient();
            var runner = new AsrPipelineRunner();
            
            runner.RegisterStage("align-chunks", wd => new AlignChunksStage(wd, httpClient, new AlignmentParams(lang, timeout, service)));

            var manifest = await LoadManifestAsync(workDir);
            if (manifest == null)
            {
                Console.Error.WriteLine("Manifest not found. Run pipeline from earlier stage first.");
                Environment.Exit(1);
                return;
            }

            var stage = new AlignChunksStage(workDir, httpClient, new AlignmentParams(lang, timeout, service));
            if (force)
            {
                // Clear existing stage status
                manifest.Stages.Remove("align-chunks");
            }

            var success = await stage.RunAsync(manifest);
            if (!success)
            {
                Environment.Exit(1);
            }
        }, inOption, workOption, serviceOption, langOption, timeoutOption, forceOption);

        return cmd;
    }

    private static async Task<ManifestV2?> LoadManifestAsync(string workDir)
    {
        try
        {
            var path = Path.Combine(workDir, "manifest.json");
            if (!File.Exists(path)) return null;
            var json = await File.ReadAllTextAsync(path);
            return System.Text.Json.JsonSerializer.Deserialize<ManifestV2>(json);
        }
        catch
        {
            return null;
        }
    }
}