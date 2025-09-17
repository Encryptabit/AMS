using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Asr.Pipeline;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class ChunkAudioStage : StageRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly ChunkingParams _params;
    private readonly AudioAnalysisService _analysisService;
    private readonly VolumeAnalysisParams _analysisParams;

    public ChunkAudioStage(
        string workDir,
        IProcessRunner processRunner,
        ChunkingParams parameters)
        : base(workDir, "chunks")
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _analysisService = new AudioAnalysisService(_processRunner, _params.SampleRate);
        _analysisParams = parameters.VolumeAnalysis ?? new VolumeAnalysisParams(
            DbFloor: -45.0,
            SpeechFloorDb: -35.0,
            MinProbeSec: 0.080,
            ProbeWindowSec: 0.050,
            HfBandLowHz: 3500.0,
            HfBandHighHz: 12000.0,
            HfMarginDb: 5.0,
            WeakMarginDb: 2.5,
            NudgeStepSec: 0.003,
            MaxLeftNudges: 8,
            MaxRightNudges: 3,
            GuardLeftSec: 0.012,
            GuardRightSec: 0.015);
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var planPath = Path.Combine(WorkDir, "plan", "windows.json");
        if (!File.Exists(planPath))
            throw new InvalidOperationException("Window plan not found. Run 'plan' stage first.");

        var planJson = await File.ReadAllTextAsync(planPath, ct);
        var plan = JsonSerializer.Deserialize<WindowPlanV2>(planJson) ?? throw new InvalidOperationException("Invalid window plan");

        Console.WriteLine($"Chunking audio into {plan.Windows.Count} chunks...");

        var chunksDir = Path.Combine(stageDir, "wav");
        Directory.CreateDirectory(chunksDir);

        var chunkInfos = new List<ChunkInfo>();

        for (int i = 0; i < plan.Windows.Count; i++)
        {
            var window = plan.Windows[i];
            var chunkId = $"chunk_{i:D3}";
            var chunkFilename = $"{chunkId}.wav";
            var chunkPath = Path.Combine(chunksDir, chunkFilename);

            Console.WriteLine($"Creating chunk {chunkId}: {window.Start:F2}s - {window.End:F2}s ({window.Length:F2}s)");

            var adjustedSpan = await ExtractChunkWithVolumeNudging(manifest.Input.Path, chunkPath, window, ct);

            var chunkInfo = await CreateChunkInfoAsync(chunkId, adjustedSpan, chunkFilename, chunkPath, ct);
            chunkInfos.Add(chunkInfo);

            Console.WriteLine($"Chunk {chunkId}: {chunkInfo.DurationSec:F2}s, SHA256: {chunkInfo.Sha256[..8]}...");
        }

        var chunkIndex = new ChunkIndex(chunkInfos, manifest.Input.Sha256, _params);
        var indexPath = Path.Combine(stageDir, "index.json");
        var indexJson = JsonSerializer.Serialize(chunkIndex, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(indexPath, indexJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Created {chunkInfos.Count} audio chunks");

        return new Dictionary<string, string>
        {
            ["index"] = "index.json",
            ["chunks_dir"] = "wav",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        var planPath = Path.Combine(WorkDir, "plan", "windows.json");
        var planHash = "";
        if (File.Exists(planPath))
        {
            var planContent = await File.ReadAllTextAsync(planPath, ct);
            planHash = ComputeHash(planContent);
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + planHash);

        var toolVersions = new Dictionary<string, string>();
        try
        {
            var result = await _processRunner.RunAsync(GetFfmpegExecutable(), "-version", ct);
            if (result.ExitCode == 0)
            {
                var version = FfmpegSilenceDetector.ParseVersion(result.StdOut + "\n" + result.StdErr) ?? "unknown";
                toolVersions["ffmpeg"] = version;
            }
        }
        catch
        {
            toolVersions["ffmpeg"] = "unknown";
        }

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<ChunkSpan> ExtractChunkWithVolumeNudging(
        string inputAudioPath,
        string outputChunkPath,
        ChunkSpan window,
        CancellationToken ct)
    {
        var analysis = await _analysisService
            .GetVolumeAnalysis(inputAudioPath, window.Start, window.Length, _analysisParams, ct)
            .ConfigureAwait(false);

        var suggestedStart = Math.Max(window.Start, Math.Min(window.End - 0.002, analysis.SuggestedStart));
        var suggestedEnd = Math.Max(suggestedStart + 0.002, Math.Min(window.End + 0.050, analysis.SuggestedEnd));
        var adjustedSpan = new ChunkSpan(suggestedStart, suggestedEnd);

        if (analysis.LeftNudges > 0 || analysis.RightNudges > 0)
        {
            Console.WriteLine(
                $"  Nudged boundaries -> start {window.Start:F3}s ? {adjustedSpan.Start:F3}s, end {window.End:F3}s ? {adjustedSpan.End:F3}s");
        }

        await ExtractChunkAsync(inputAudioPath, outputChunkPath, adjustedSpan, ct).ConfigureAwait(false);
        return adjustedSpan;
    }

    private async Task ExtractChunkAsync(string inputAudioPath, string outputChunkPath, ChunkSpan window, CancellationToken ct)
    {
        var normalizedInputPath = PathNormalizer.NormalizePath(inputAudioPath);
        var normalizedOutputPath = PathNormalizer.NormalizePath(outputChunkPath);

        PathNormalizer.EnsureDirectory(normalizedOutputPath);

        var ffmpegExe = GetFfmpegExecutable();
        var args = BuildFfmpegArgs(normalizedInputPath, normalizedOutputPath, window);

        var result = await _processRunner.RunAsync(ffmpegExe, args, ct);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg failed to extract chunk: {result.StdErr}");
        }

        if (!File.Exists(normalizedOutputPath))
        {
            throw new InvalidOperationException($"Chunk file was not created: {normalizedOutputPath}");
        }
    }

    private string BuildFfmpegArgs(string inputPath, string outputPath, ChunkSpan window)
    {
        var args = new List<string>
        {
            "-i", $"\"{inputPath}\"",
            "-ss", window.Start.ToString("F6"),
            "-t", window.Length.ToString("F6"),
            "-c", "copy"
        };

        if (_params.SampleRate != 44100)
        {
            args.AddRange(new[] { "-ar", _params.SampleRate.ToString() });
            args.Remove("-c");
            args.Remove("copy");
        }

        args.AddRange(new[] { "-y", $"\"{outputPath}\"" });

        return string.Join(" ", args);
    }

    private async Task<ChunkInfo> CreateChunkInfoAsync(string chunkId, ChunkSpan window, string filename, string chunkPath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(chunkPath);
        var hash = await SHA256.HashDataAsync(stream, ct);
        var sha256 = Convert.ToHexString(hash);

        double duration;
        try
        {
            var audioData = WavIo.ReadPcmOrFloat(chunkPath);
            duration = audioData.Length / (double)audioData.SampleRate;
        }
        catch
        {
            duration = window.Length;
        }

        return new ChunkInfo(chunkId, window, filename, sha256, duration);
    }

    private static string GetFfmpegExecutable()
    {
        return Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";
    }
}
