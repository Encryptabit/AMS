using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class RefineCommand
{
    public static Command Create()
    {
        var cmd = new Command("refine", "Refine sentence boundaries using snap-to-silence rule");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var silenceThresholdOption = new Option<double>("--silence-threshold-db", () => -30.0, "Silence threshold in dB");
        var silenceMinDurOption = new Option<double>("--silence-min-dur", () => 0.12, "Minimum silence duration in seconds");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(silenceThresholdOption);
        cmd.AddOption(silenceMinDurOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (input, work, silenceThresholdDb, silenceMinDur, force) =>
        {
            var workDir = work?.FullName ?? input!.FullName + ".ams";
            Directory.CreateDirectory(workDir);

            var manifest = await LoadManifestAsync(workDir);
            if (manifest == null)
            {
                Console.Error.WriteLine("Manifest not found. Run pipeline from earlier stage first.");
                Environment.Exit(1);
                return;
            }

            var stage = new RefineStage(workDir, new RefinementParams(silenceThresholdDb, silenceMinDur));
            
            if (force)
            {
                // Clear existing stage status
                manifest.Stages.Remove("refine");
            }

            var success = await stage.RunAsync(manifest);
            if (!success)
            {
                Environment.Exit(1);
            }
        }, inOption, workOption, silenceThresholdOption, silenceMinDurOption, forceOption);

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