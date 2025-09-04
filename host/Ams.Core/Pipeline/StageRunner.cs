using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public abstract class StageRunner
{
    protected readonly string WorkDir;
    protected readonly string StageName;

    protected StageRunner(string workDir, string stageName)
    {
        WorkDir = workDir ?? throw new ArgumentNullException(nameof(workDir));
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
    }

    public async Task<bool> RunAsync(ManifestV2 manifest, CancellationToken ct = default)
    {
        var stageDir = Path.Combine(WorkDir, StageName);
        Directory.CreateDirectory(stageDir);

        if (await ShouldSkipAsync(manifest, stageDir, ct))
        {
            Console.WriteLine($"Skipping {StageName} (up-to-date)");
            return true;
        }

        await UpdateStageStatusAsync(manifest, "in_progress", null, null, ct);

        try
        {
            var artifacts = await RunStageAsync(manifest, stageDir, ct);
            await WriteStageMetadataAsync(stageDir, ct);
            await UpdateStageStatusAsync(manifest, "completed", artifacts, null, ct);
            Console.WriteLine($"Completed {StageName}");
            return true;
        }
        catch (Exception ex)
        {
            await UpdateStageStatusAsync(manifest, "failed", null, ex.Message, ct);
            Console.Error.WriteLine($"Failed {StageName}: {ex.Message}");
            return false;
        }
    }

    protected abstract Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct);
    protected abstract Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct);

    private async Task<bool> ShouldSkipAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        if (!manifest.Stages.TryGetValue(StageName, out var stage) || stage.Status.Status != "completed")
            return false;

        var currentFingerprint = await ComputeFingerprintAsync(manifest, ct);
        return FingerprintsMatch(stage.Fingerprint, currentFingerprint);
    }

    private static bool FingerprintsMatch(StageFingerprint existing, StageFingerprint current)
    {
        if (existing.InputHash != current.InputHash || existing.ParamsHash != current.ParamsHash)
            return false;

        foreach (var (tool, version) in current.ToolVersions)
        {
            if (!existing.ToolVersions.TryGetValue(tool, out var existingVersion) || existingVersion != version)
                return false;
        }

        return true;
    }

    protected async Task UpdateStageStatusAsync(ManifestV2 manifest, string status, Dictionary<string, string>? artifacts = null, string? error = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (!manifest.Stages.TryGetValue(StageName, out var existing))
        {
            var fingerprint = status == "completed" ? await ComputeFingerprintAsync(manifest, ct) : new StageFingerprint("", "", new Dictionary<string, string>());
            existing = new StageEntry(new StageStatus("pending", null, null, 0, null), new Dictionary<string, string>(), fingerprint);
        }

        var newStatus = existing.Status with
        {
            Status = status,
            Started = existing.Status.Started ?? (status == "in_progress" ? now : existing.Status.Started),
            Ended = status is "completed" or "failed" ? now : existing.Status.Ended,
            Attempts = existing.Status.Attempts + (status == "in_progress" ? 1 : 0),
            Error = error
        };

        var newFingerprint = status == "completed" ? await ComputeFingerprintAsync(manifest, ct) : existing.Fingerprint;

        manifest.Stages[StageName] = existing with
        {
            Status = newStatus,
            Artifacts = artifacts ?? existing.Artifacts,
            Fingerprint = newFingerprint
        };

        await SaveManifestAsync(manifest, ct);
    }

    private async Task SaveManifestAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var manifestPath = Path.Combine(WorkDir, "manifest.json");
        var json = JsonSerializer.Serialize(manifest with { Modified = DateTime.UtcNow }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, json, ct);
    }

    protected async Task WriteStageMetadataAsync(string stageDir, CancellationToken ct)
    {
        var meta = new
        {
            gitSha = await GetGitShaAsync(ct),
            os = Environment.OSVersion.ToString(),
            timestamp = DateTime.UtcNow,
            stage = StageName
        };

        var metaPath = Path.Combine(stageDir, "meta.json");
        var metaJson = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metaPath, metaJson, ct);

        var statusPath = Path.Combine(stageDir, "status.json");
        var statusJson = JsonSerializer.Serialize(new { status = "completed", timestamp = DateTime.UtcNow }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(statusPath, statusJson, ct);
    }

    private async Task<string> GetGitShaAsync(CancellationToken ct)
    {
        try
        {
            var runner = new DefaultProcessRunner();
            var result = await runner.RunAsync("git", "rev-parse HEAD", ct);
            return result.ExitCode == 0 ? result.StdOut.Trim() : "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    protected static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    protected static string SerializeParams<T>(T parameters)
    {
        return JsonSerializer.Serialize(parameters, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}

