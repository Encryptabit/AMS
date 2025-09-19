using System.CommandLine;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Alignment.Tx;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class AudioCommand
{
    public static Command Create()
    {
        var audio = new Command("audio", "Audio rendering utilities");
        audio.AddCommand(CreateRoomtone());
        return audio;
    }

    private static Command CreateRoomtone()
    {
        var cmd = new Command("roomtone", "Render WAV with roomtone-filled gaps (sentence-level) and 5 ms crossfades");

        var txOption = new Option<FileInfo>("--tx-json", description: "Path to TranscriptIndex (*.tx.json)") { IsRequired = true };
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo>("--out-wav", description: "Output WAV path") { IsRequired = true };
        outOption.AddAlias("-o");

        var srOption = new Option<int>("--sample-rate", () => 44100, "Output sample rate (Hz)");
        var bitOption = new Option<int>("--bit-depth", () => 16, "Output bit depth (currently 16 only)");
        var fadeMsOption = new Option<double>("--fade-ms", () => 5.0, "Crossfade length at boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Roomtone RMS level (dBFS)");

        cmd.AddOption(txOption);
        cmd.AddOption(outOption);
        cmd.AddOption(srOption);
        cmd.AddOption(bitOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);

        cmd.SetHandler(async (context) =>
        {
            var txFile = context.ParseResult.GetValueForOption(txOption)!;
            var outWav = context.ParseResult.GetValueForOption(outOption)!;
            var sr = context.ParseResult.GetValueForOption(srOption);
            var bit = context.ParseResult.GetValueForOption(bitOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);

            try
            {
                await RunRenderAsync(txFile, outWav, sr, bit, fadeMs, toneDb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunRenderAsync(FileInfo txFile, FileInfo outWav, int sampleRate, int bitDepth, double fadeMs, double toneDb)
    {
        if (!txFile.Exists) throw new FileNotFoundException($"TranscriptIndex not found: {txFile.FullName}");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var txJson = await File.ReadAllTextAsync(txFile.FullName);
        var tx = JsonSerializer.Deserialize<TranscriptIndex>(txJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse TranscriptIndex");

        string audioPath = NormalizePath(tx.AudioPath);
        if (!File.Exists(audioPath)) throw new FileNotFoundException($"Audio file not found: {audioPath}");

        if (bitDepth != 16)
            throw new NotSupportedException("Currently only 16-bit PCM output is supported in MVP.");

        string workDir = outWav.DirectoryName ?? Directory.GetCurrentDirectory();
        string chapterId = Path.GetFileNameWithoutExtension(txFile.Name);

        var manifest = new ManifestV2(
            ChapterId: string.IsNullOrWhiteSpace(chapterId) ? "chapter" : chapterId,
            WorkDirectory: workDir,
            AudioPath: audioPath,
            TranscriptIndexPath: txFile.FullName);

        var stage = new RoomToneInsertionStage(sampleRate, toneDb, fadeMs);
        var outputs = await stage.RunAsync(manifest, CancellationToken.None);

        var producedWav = outputs.TryGetValue("roomtoneWav", out var path) ? path : throw new InvalidOperationException("Stage did not produce roomtone WAV");

        EnsureDirectory(outWav.DirectoryName);
        if (!string.Equals(Path.GetFullPath(producedWav), Path.GetFullPath(outWav.FullName), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(producedWav, outWav.FullName, overwrite: true);
        }

        Console.WriteLine($"Roomtone WAV: {outWav.FullName}");
        if (outputs.TryGetValue("timeline", out var timeline)) Console.WriteLine($"Timeline JSON: {timeline}");
        if (outputs.TryGetValue("meta", out var meta)) Console.WriteLine($"Meta JSON: {meta}");
        if (outputs.TryGetValue("params", out var snapshot)) Console.WriteLine($"Params Snapshot: {snapshot}");
    }

    private static void EnsureDirectory(string? dir)
    {
        if (string.IsNullOrWhiteSpace(dir)) return;
        Directory.CreateDirectory(dir);
    }

    private static string NormalizePath(string path)
    {
        // Accept Windows paths (e.g., C:\foo\bar.wav) when running on Linux/WSL by mapping to /mnt/c/...
        if (OperatingSystem.IsWindows()) return path;
        if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
        {
            var drive = char.ToLowerInvariant(path[0]);
            var rest = path.Substring(2).Replace('\\', '/');
            return $"/mnt/{drive}{rest}";
        }
        return path;
    }
}



