using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

// Lean manifest/artifact validator (no dependency on Validation types)
public class ValidateStage : StageRunner
{
    private readonly ValidationParams _params;

    public ValidateStage(string workDir, ValidationParams parameters) : base(workDir, "validate")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine("Validating manifest and artifacts (lean checks)...");

        var issues = new List<string>();
        var warnings = new List<string>();

        string[] mustHave =
        {
            Path.Combine(WorkDir, "timeline", "silence.json"),
            Path.Combine(WorkDir, "plan", "windows.json"),
            Path.Combine(WorkDir, "refine", "sentences.json")
        };
        foreach (var p in mustHave) if (!File.Exists(p)) issues.Add($"Missing: {p}");

        var collated = Path.Combine(WorkDir, "collate", "final.wav");
        if (!File.Exists(collated)) warnings.Add($"Missing: {collated} (run collate)");

        var requiredStages = new[] { "timeline", "plan", "chunks", "transcripts", "align-chunks", "refine" };
        foreach (var s in requiredStages)
        {
            if (!manifest.Stages.TryGetValue(s, out var entry) || entry.Status.Status != "completed")
                issues.Add($"Stage not completed: {s}");
        }

        var reportObj = new
        {
            ok = issues.Count == 0,
            input = manifest.Input.Path,
            stages = manifest.Stages.Keys.OrderBy(k => k).ToArray(),
            issues,
            warnings,
            thresholds = new { _params.WerThreshold, _params.CerThreshold },
            generatedAt = DateTime.UtcNow
        };

        Directory.CreateDirectory(stageDir);
        var reportPath = Path.Combine(stageDir, "report.json");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(reportObj, new JsonSerializerOptions { WriteIndented = true }), ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        await File.WriteAllTextAsync(paramsPath, SerializeParams(_params), ct);

        return new Dictionary<string, string>
        {
            ["report"] = "report.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        var inputHash = manifest.Input.Sha256;
        return Task.FromResult(new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>()));
    }
}

