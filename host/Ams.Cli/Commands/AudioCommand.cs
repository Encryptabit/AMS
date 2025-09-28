using System.CommandLine;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Alignment.Tx;
using Ams.Core;
using Ams.Core.Pipeline;
using Ams.Core.Common;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class AudioCommand
{
    public static Command Create()
    {
        var audio = new Command("audio", "Audio rendering utilities");
        audio.AddCommand(CreateRoomtone());
        audio.AddCommand(CreateRoomtonePlan());
        return audio;
    }

    private static Command CreateRoomtone()
    {
        var cmd = new Command("roomtone", "Render WAV with roomtone-filled gaps (sentence-level) and 5 ms crossfades");

        var txOption = new Option<FileInfo?>("--tx-json", description: "Path to TranscriptIndex (*.tx.json)");
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo?>("--out-wav", description: "Output WAV path (defaults to chapter.treated.wav)");
        outOption.AddAlias("-o");

        var srOption = new Option<int>("--sample-rate", () => 44100, "Output sample rate (Hz)");
        var bitOption = new Option<int>("--bit-depth", () => 32, "Output bit depth");
        var fadeMsOption = new Option<double>("--fade-ms", () => 10.0, "Crossfade length at boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Roomtone RMS level (dBFS)");
        var diagnosticsOption = new Option<bool>("--emit-diagnostics", () => false, "Write intermediate diagnostic WAV snapshots");
        var adaptiveGainOption = new Option<bool>("--adaptive-gain", () => false, "Scale roomtone seed to the target RMS");

        cmd.AddOption(txOption);
        cmd.AddOption(outOption);
        cmd.AddOption(srOption);
        cmd.AddOption(bitOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);
        cmd.AddOption(diagnosticsOption);
        cmd.AddOption(adaptiveGainOption);

        cmd.SetHandler(async (context) =>
        {
            var txFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(txOption), "align.tx.json");
            var outWav = CommandInputResolver.ResolveOutput(context.ParseResult.GetValueForOption(outOption), "treated.wav");
            var sr = context.ParseResult.GetValueForOption(srOption);
            var bit = context.ParseResult.GetValueForOption(bitOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);
            var emitDiagnostics = context.ParseResult.GetValueForOption(diagnosticsOption);
            var adaptiveGain = context.ParseResult.GetValueForOption(adaptiveGainOption);

            try
            {
                await RunRenderAsync(txFile, outWav, sr, bit, fadeMs, toneDb, emitDiagnostics, adaptiveGain);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "audio roomtone command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static Command CreateRoomtonePlan()
    {
        var cmd = new Command("roomtone-plan", "Generate roomtone plan metadata without rendering audio");

        var txOption = new Option<FileInfo?>("--tx-json", description: "Path to TranscriptIndex (*.tx.json)");
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo?>("--out-json", () => null, "Output roomtone plan path (defaults to tx.roomtone.json)");
        outOption.AddAlias("-o");

        var srOption = new Option<int>("--sample-rate", () => 44100, "Target sample rate used when rendering");
        var fadeMsOption = new Option<double>("--fade-ms", () => 5.0, "Crossfade length at boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Roomtone RMS level (dBFS)");
        var diagnosticsOption = new Option<bool>("--emit-diagnostics", () => false, "Write intermediate diagnostic WAV snapshots when rendering");
        var adaptiveGainOption = new Option<bool>("--adaptive-gain", () => false, "Scale roomtone seed to the target RMS");

        cmd.AddOption(txOption);
        cmd.AddOption(outOption);
        cmd.AddOption(srOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);
        cmd.AddOption(diagnosticsOption);
        cmd.AddOption(adaptiveGainOption);

        cmd.SetHandler(async context =>
        {
            var txFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(txOption), "align.tx.json");
            var outJsonOpt = context.ParseResult.GetValueForOption(outOption);
            var sr = context.ParseResult.GetValueForOption(srOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);
            var emitDiagnostics = context.ParseResult.GetValueForOption(diagnosticsOption);
            var adaptiveGain = context.ParseResult.GetValueForOption(adaptiveGainOption);

            var outJson = outJsonOpt ?? new FileInfo(Path.Combine(txFile.DirectoryName ?? Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(txFile.Name) + ".roomtone.json"));

            try
            {
                await RunPlanAsync(txFile, outJson, sr, fadeMs, toneDb, emitDiagnostics, adaptiveGain);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "audio roomtone-plan command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    internal static async Task RunRenderAsync(FileInfo txFile, FileInfo outWav, int sampleRate, int bitDepth, double fadeMs, double toneDb, bool emitDiagnostics, bool adaptiveGain)
    {
        if (!txFile.Exists) throw new FileNotFoundException($"TranscriptIndex not found: {txFile.FullName}");

        Log.Info("Rendering roomtone for {TranscriptIndex} -> {OutputWav}", txFile.FullName, outWav.FullName);

        var manifest = await PrepareManifestAsync(txFile, outWav.DirectoryName);

        var stage = new RoomToneInsertionStage(sampleRate, toneDb, fadeMs, emitDiagnostics, adaptiveGain);
        var outputs = await stage.RunAsync(manifest, CancellationToken.None);

        var producedWav = outputs.TryGetValue("roomtoneWav", out var path) ? path : throw new InvalidOperationException("Stage did not produce roomtone WAV");

        EnsureDirectory(outWav.DirectoryName);
        if (!string.Equals(Path.GetFullPath(producedWav), Path.GetFullPath(outWav.FullName), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(producedWav, outWav.FullName, overwrite: true);
        }

        Log.Info("Roomtone WAV saved to {OutputWav}", outWav.FullName);
        if (outputs.TryGetValue("plan", out var plan)) Log.Info("Roomtone plan saved to {PlanPath}", plan);
        if (outputs.TryGetValue("timeline", out var timeline)) Log.Info("Roomtone timeline saved to {TimelinePath}", timeline);
        if (outputs.TryGetValue("meta", out var meta)) Log.Info("Roomtone metadata saved to {MetaPath}", meta);
        if (outputs.TryGetValue("params", out var snapshot)) Log.Info("Roomtone params snapshot saved to {ParamsPath}", snapshot);
    }

    private static async Task RunPlanAsync(FileInfo txFile, FileInfo outJson, int sampleRate, double fadeMs, double toneDb, bool emitDiagnostics, bool adaptiveGain)
    {
        if (!txFile.Exists) throw new FileNotFoundException($"TranscriptIndex not found: {txFile.FullName}");

        Log.Info("Generating roomtone plan for {TranscriptIndex} -> {OutputJson}", txFile.FullName, outJson.FullName);

        var manifest = await PrepareManifestAsync(txFile, outJson.DirectoryName);

        var stage = new RoomToneInsertionStage(sampleRate, toneDb, fadeMs, emitDiagnostics, adaptiveGain);
        var outputs = await stage.RunAsync(manifest, CancellationToken.None, renderAudio: false);

        var planPath = outputs.TryGetValue("plan", out var plan) ? plan : throw new InvalidOperationException("Stage did not produce roomtone plan");

        EnsureDirectory(outJson.DirectoryName);
        if (!string.Equals(Path.GetFullPath(planPath), Path.GetFullPath(outJson.FullName), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(planPath, outJson.FullName, overwrite: true);
        }

        Log.Info("Roomtone plan saved to {OutputJson}", outJson.FullName);
        if (outputs.TryGetValue("timeline", out var timeline)) Log.Info("Roomtone timeline saved to {TimelinePath}", timeline);
        if (outputs.TryGetValue("meta", out var meta)) Log.Info("Roomtone metadata saved to {MetaPath}", meta);
        if (outputs.TryGetValue("params", out var snapshot)) Log.Info("Roomtone params snapshot saved to {ParamsPath}", snapshot);
    }

    private static async Task<ManifestV2> PrepareManifestAsync(FileInfo txFile, string? workDirectory)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var txJson = await File.ReadAllTextAsync(txFile.FullName);
        var tx = JsonSerializer.Deserialize<TranscriptIndex>(txJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse TranscriptIndex");

        var audioPath = NormalizePath(tx.AudioPath);
        if (!File.Exists(audioPath)) throw new FileNotFoundException($"Audio file not found: {audioPath}");

        string workDir = string.IsNullOrWhiteSpace(workDirectory) ? Directory.GetCurrentDirectory() : workDirectory;
        string chapterId = Path.GetFileNameWithoutExtension(txFile.Name);
        if (string.IsNullOrWhiteSpace(chapterId)) chapterId = "chapter";

        return new ManifestV2(chapterId, workDir, audioPath, txFile.FullName);
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










