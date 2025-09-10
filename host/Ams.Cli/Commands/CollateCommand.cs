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
        var dbFloorOption = new Option<double>("--db-floor", () => -45.0, "Highband detection floor threshold in dB (for fricative avoidance)");
        // Chapter-wide interword options (feature-flagged)
        var enableInterwordOption = new Option<bool>("--enable-interword", () => false, "Enable chapter-wide interword roomtone over long silences");
        var interwordMinSilenceOption = new Option<double>("--interword-min-silence", () => 0.12, "Minimum silence duration (sec) to consider as interword gap");
        var interwordMaxSilenceOption = new Option<double>("--interword-max-silence", () => 5.0, "Maximum silence duration (sec) to consider as interword gap");
        var interwordGuardMsOption = new Option<double>("--interword-guard-ms", () => 15.0, "Guard (ms) around sentence and window boundaries to avoid speech erasure");
        var interwordRespectSentencesOption = new Option<bool>("--interword-respect-sentences", () => true, "If true, only fill gaps outside sentence spans");
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(inOption);
        cmd.AddOption(workOption);
        cmd.AddOption(roomtoneOption);
        cmd.AddOption(levelOption);
        cmd.AddOption(bridgeMaxOption);
        cmd.AddOption(dbFloorOption);
        cmd.AddOption(enableInterwordOption);
        cmd.AddOption(interwordMinSilenceOption);
        cmd.AddOption(interwordMaxSilenceOption);
        cmd.AddOption(interwordGuardMsOption);
        cmd.AddOption(interwordRespectSentencesOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inOption)!;
            var work = context.ParseResult.GetValueForOption(workOption);
            var roomtone = context.ParseResult.GetValueForOption(roomtoneOption)!;
            var levelDb = context.ParseResult.GetValueForOption(levelOption);
            var bridgeMaxMs = context.ParseResult.GetValueForOption(bridgeMaxOption);
            var dbFloor = context.ParseResult.GetValueForOption(dbFloorOption);
            var enableInterword = context.ParseResult.GetValueForOption(enableInterwordOption);
            var interwordMinSilence = context.ParseResult.GetValueForOption(interwordMinSilenceOption);
            var interwordMaxSilence = context.ParseResult.GetValueForOption(interwordMaxSilenceOption);
            var interwordGuardMs = context.ParseResult.GetValueForOption(interwordGuardMsOption);
            var interwordRespectSentences = context.ParseResult.GetValueForOption(interwordRespectSentencesOption);
            var force = context.ParseResult.GetValueForOption(forceOption);

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
                roomtoneFilePath,
                dbFloor,
                enableInterword,
                interwordMinSilence,
                interwordMaxSilence,
                interwordGuardMs,
                interwordRespectSentences
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
        });

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
