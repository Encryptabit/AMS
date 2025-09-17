using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using Ams.Core.Align;
using Ams.Core.Io;
using Ams.Core.Models;

namespace Ams.Core.Pipeline;

public class WindowAlignStage : StageRunner
{
    private readonly HttpClient _http;
    private readonly WindowAlignParams _params;
    private readonly WindowAlignService _service;

    public WindowAlignStage(string workDir, HttpClient httpClient, WindowAlignParams parameters)
        : base(workDir, "window-align")
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _service = new WindowAlignService(_http, new DefaultProcessRunner());
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.json");
        if (!File.Exists(windowsPath)) throw new InvalidOperationException("Missing windows/windows.json");

        var transcriptMerged = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(transcriptMerged)) throw new InvalidOperationException("Missing transcripts/merged.json");

        var windowsJson = await File.ReadAllTextAsync(windowsPath, ct);
        var windows = JsonSerializer.Deserialize<WindowsArtifact>(windowsJson) ?? throw new InvalidOperationException("Invalid windows artifact");

        var transcriptsJson = await File.ReadAllTextAsync(transcriptMerged, ct);
        var merged = JsonSerializer.Deserialize<JsonElement>(transcriptsJson);

        var winDir = Path.Combine(stageDir, "windows");
        Directory.CreateDirectory(winDir);
        var audioDir = Path.Combine(winDir, "audio");
        Directory.CreateDirectory(audioDir);

        var request = new WindowAlignRequest(
            PathNormalizer.NormalizePath(manifest.Input.Path),
            manifest.Input.DurationSec,
            windows,
            merged,
            _params,
            audioDir
        );

        var result = await _service.AlignAsync(request, ct);

        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var alignment in result.Alignments)
        {
            var digest = ComputeHash(alignment.DigestInput)[..16];
            var entry = new WindowAlignEntry(
                alignment.WindowId,
                alignment.OffsetSec,
                digest,
                alignment.Fragments,
                new Dictionary<string, string>(alignment.ToolVersions, StringComparer.OrdinalIgnoreCase),
                alignment.GeneratedAt
            );

            var entryPath = Path.Combine(winDir, $"{alignment.WindowId}.aeneas.json");
            var entryJson = JsonSerializer.Serialize(entry, serializerOptions);
            await File.WriteAllTextAsync(entryPath, entryJson, ct);
            map[alignment.WindowId] = $"{alignment.WindowId}.aeneas.json";
        }

        var index = new WindowAlignIndex(
            result.Alignments.Select(a => a.WindowId).ToList(),
            map,
            _params,
            new Dictionary<string, string>(result.ServiceToolVersions, StringComparer.OrdinalIgnoreCase)
        );

        await File.WriteAllTextAsync(Path.Combine(stageDir, "index.json"), JsonSerializer.Serialize(index, serializerOptions), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_params), ct);

        return new Dictionary<string, string>
        {
            ["index"] = "index.json",
            ["windows_dir"] = "windows",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        var windowsPath = Path.Combine(WorkDir, "windows", "windows.json");
        var transcriptsPath = Path.Combine(WorkDir, "transcripts", "merged.json");

        var windowsHash = File.Exists(windowsPath) ? ComputeHash(await File.ReadAllTextAsync(windowsPath, ct)) : string.Empty;
        var transcriptsHash = File.Exists(transcriptsPath) ? ComputeHash(await File.ReadAllTextAsync(transcriptsPath, ct)) : string.Empty;
        var inputHash = ComputeHash(windowsHash + "\n" + transcriptsHash);

        var toolVersions = await GetToolVersionsAsync(ct);
        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<Dictionary<string, string>> GetToolVersionsAsync(CancellationToken ct)
    {
        try
        {
            using var resp = await _http.GetAsync($"{_params.ServiceUrl}/v1/health", ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new Dictionary<string, string> { ["python"] = "unknown", ["aeneas"] = "unknown" };
            }

            var json = await resp.Content.ReadAsStringAsync(ct);
            var el = JsonSerializer.Deserialize<JsonElement>(json);
            var tool = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (el.TryGetProperty("python_version", out var py)) tool["python"] = py.GetString() ?? "unknown";
            if (el.TryGetProperty("aeneas_version", out var ae)) tool["aeneas"] = ae.GetString() ?? "unknown";
            return tool;
        }
        catch
        {
            return new Dictionary<string, string> { ["python"] = "unknown", ["aeneas"] = "unknown" };
        }
    }
}
