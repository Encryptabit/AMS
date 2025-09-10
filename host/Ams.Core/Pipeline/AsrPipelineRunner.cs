using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core; // for WavIo

namespace Ams.Core.Pipeline;

public class AsrPipelineRunner
{
    private readonly Dictionary<string, Func<string, StageRunner>> _stageFactories = new();
    private readonly List<string> _stageOrder = new();

    public AsrPipelineRunner()
    {
        _stageOrder.AddRange(new[]
        {
            "book-index",   // 0 (required pre-req)
            "timeline",      // 1
            "plan",          // 2 (chunk plan)
            "chunks",        // 3
            "transcripts",   // 4
            "anchors",       // 5
            "windows",       // 6 (anchor windows)
            "window-align",  // 7
            "refine",        // 8
            "collate",       // 9
            "script-compare",// 10
            "validate"       // 11
        });
    }

    public void RegisterStage(string stageName, Func<string, StageRunner> factory)
    {
        _stageFactories[stageName] = factory;
    }

    public async Task<bool> RunAsync(
        string inputPath,
        string? workDir = null,
        string? fromStage = null,
        string? toStage = null,
        bool force = false,
        CancellationToken ct = default)
    {
        workDir ??= inputPath + ".ams";
        Directory.CreateDirectory(workDir);

        var manifest = await LoadOrCreateManifestAsync(inputPath, workDir, ct);

        if (force)
        {
            ClearStageStatuses(manifest, fromStage);
        }

        var startIndex = fromStage != null ? _stageOrder.IndexOf(fromStage) : 0;
        var endIndex = toStage != null ? _stageOrder.IndexOf(toStage) : _stageOrder.Count - 1;
        if (startIndex < 0) throw new ArgumentException($"Unknown from-stage: {fromStage}");
        if (endIndex < 0) throw new ArgumentException($"Unknown to-stage: {toStage}");
        if (startIndex > endIndex) throw new ArgumentException("from-stage must come before to-stage");

        Console.WriteLine($"Running ASR pipeline from {_stageOrder[startIndex]} to {_stageOrder[endIndex]}");
        for (int i = startIndex; i <= endIndex; i++)
        {
            var stageName = _stageOrder[i];
            if (!_stageFactories.TryGetValue(stageName, out var factory))
            {
                Console.WriteLine($"Skipping {stageName} (not registered)");
                continue;
            }

            var stage = factory(workDir);
            if (!await stage.RunAsync(manifest, ct))
            {
                Console.Error.WriteLine($"Pipeline failed at stage: {stageName}");
                return false;
            }
            manifest = await LoadManifestAsync(workDir, ct) ?? manifest;
        }
        Console.WriteLine("Pipeline completed successfully");
        return true;
    }

    private async Task<ManifestV2> LoadOrCreateManifestAsync(string inputPath, string workDir, CancellationToken ct)
    {
        var manifestPath = Path.Combine(workDir, "manifest.json");
        if (File.Exists(manifestPath))
        {
            var existing = await LoadManifestAsync(workDir, ct);
            if (existing != null)
            {
                var currentInput = await CreateInputMetadataAsync(inputPath, ct);
                if (existing.Input.Sha256 == currentInput.Sha256)
                    return existing;
            }
        }
        var input = await CreateInputMetadataAsync(inputPath, ct);
        var manifest = ManifestV2.CreateNew(input);
        await SaveManifestAsync(manifest, workDir, ct);
        return manifest;
    }

    private async Task<ManifestV2?> LoadManifestAsync(string workDir, CancellationToken ct)
    {
        try
        {
            var path = Path.Combine(workDir, "manifest.json");
            if (!File.Exists(path)) return null;
            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<ManifestV2>(json);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveManifestAsync(ManifestV2 manifest, string workDir, CancellationToken ct)
    {
        var path = Path.Combine(workDir, "manifest.json");
        var json = JsonSerializer.Serialize(manifest with { Modified = DateTime.UtcNow }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private async Task<InputMetadata> CreateInputMetadataAsync(string inputPath, CancellationToken ct)
    {
        var fi = new FileInfo(inputPath);
        if (!fi.Exists) throw new FileNotFoundException($"Input file not found: {inputPath}");
        await using var fs = fi.OpenRead();
        var hash = await SHA256.HashDataAsync(fs, ct);
        var sha = Convert.ToHexString(hash);

        double duration;
        try
        {
            var audio = WavIo.ReadPcmOrFloat(inputPath);
            duration = audio.Length / (double)audio.SampleRate;
        }
        catch
        {
            duration = 0;
        }

        return new InputMetadata(fi.FullName, sha, duration, fi.Length, fi.LastWriteTimeUtc);
    }

    private void ClearStageStatuses(ManifestV2 manifest, string? fromStage)
    {
        var startIndex = 0;
        if (fromStage != null)
        {
            startIndex = _stageOrder.IndexOf(fromStage);
            if (startIndex == -1)
            {
                var validStages = string.Join(", ", _stageOrder);
                throw new ArgumentException($"Invalid stage name '{fromStage}'. Valid stages are: {validStages}", nameof(fromStage));
            }
        }
        
        for (int i = startIndex; i < _stageOrder.Count; i++)
        {
            var name = _stageOrder[i];
            if (manifest.Stages.ContainsKey(name))
                manifest.Stages.Remove(name);
        }
    }
}

