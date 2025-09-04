using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class CollateCommand
{
    public static Command Create()
    {
        var cmd = new Command("collate", "Collate sentences with room tone replacement");

        var inOption = new Option<FileInfo>("--in", "Path to input audio file (WAV format)") { IsRequired = true };
        var workOption = new Option<DirectoryInfo>("--work", "Work directory (default: <input>.ams)");
        var roomtoneOption = new Option<string>("--roomtone", () => "auto", "Room tone source: 'auto' or path to room tone file");
        var levelOption = new Option<double>("--level-db", () => -50.0, "Room tone level in dB");
        var bridgeMaxOption = new Option<int>("--bridge-max-ms", () => 60, "Maximum cross-chunk boundary sliver duration to bridge (ms)");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(roomtoneOption);
        cmd.AddOption(levelOption);
        cmd.AddOption(bridgeMaxOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (input, work, roomtone, levelDb, bridgeMaxMs, force) =>
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

            string? roomtoneFilePath = null;
            string roomtoneSource = "auto";
            
            if (roomtone != "auto" && File.Exists(roomtone))
            {
                roomtoneSource = "file";
                roomtoneFilePath = roomtone;
            }

            var collationParams = new CollationParams(
                roomtoneSource,
                levelDb,
                5, // minGapMs
                2000, // maxGapMs  
                bridgeMaxMs,
                roomtoneFilePath
            );

            var stage = new CollateStage(workDir, new DefaultProcessRunner(), collationParams);
            
            if (force)
            {
                // Clear existing stage status
                manifest.Stages.Remove("collate");
            }

            var success = await stage.RunAsync(manifest);
            if (!success)
            {
                Environment.Exit(1);
            }
        }, inOption, workOption, roomtoneOption, levelOption, bridgeMaxOption, forceOption);

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