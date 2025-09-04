using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class ChunkAudioStage : StageRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly ChunkingParams _params;

    public ChunkAudioStage(
        string workDir,
        IProcessRunner processRunner,
        ChunkingParams parameters)
        : base(workDir, "chunks")
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
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

            await ExtractChunkAsync(manifest.Input.Path, chunkPath, window, ct);

            // Compute chunk metadata
            var chunkInfo = await CreateChunkInfoAsync(chunkId, window, chunkFilename, chunkPath, ct);
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

        // Include plan hash in input hash since chunks depend on the plan
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
            "-c", "copy" // Copy without re-encoding for speed
        };

        // Apply sample rate conversion if needed
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
        // Compute SHA256 hash
        await using var stream = File.OpenRead(chunkPath);
        var hash = await SHA256.HashDataAsync(stream, ct);
        var sha256 = Convert.ToHexString(hash);

        // Get actual duration from the file
        double duration;
        try
        {
            var audioData = WavIo.ReadPcmOrFloat(chunkPath);
            duration = audioData.Length / (double)audioData.SampleRate;
        }
        catch
        {
            // Fallback to window length if we can't read the file
            duration = window.Length;
        }

        return new ChunkInfo(chunkId, window, filename, sha256, duration);
    }

    private static string GetFfmpegExecutable()
    {
        return Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";
    }
}