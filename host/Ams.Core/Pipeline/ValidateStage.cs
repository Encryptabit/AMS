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
        Console.WriteLine("Validating manifest and artifacts (gates)...");

        var issues = new List<string>();
        var warnings = new List<string>();

        string[] mustHave =
        {
            Path.Combine(WorkDir, "book.index.json"),
            Path.Combine(WorkDir, "timeline", "silence.json"),
            Path.Combine(WorkDir, "plan", "windows.json"),
            Path.Combine(WorkDir, "refine", "sentences.json")
        };
        foreach (var p in mustHave) if (!File.Exists(p)) issues.Add($"Missing: {p}");

        var collated = Path.Combine(WorkDir, "collate", "final.wav");
        if (!File.Exists(collated)) warnings.Add($"Missing: {collated} (run collate)");

        // Completed stages: always require book-index, timeline, plan, chunks, transcripts, refine.
        var requiredAlways = new[] { "book-index", "timeline", "plan", "chunks", "transcripts", "refine" };
        foreach (var s in requiredAlways)
        {
            if (!manifest.Stages.TryGetValue(s, out var entry) || entry.Status.Status != "completed")
                issues.Add($"Stage not completed: {s}");
        }

        // Alignment can be via chunk alignment OR anchor window alignment.
        bool hasChunkAlign = manifest.Stages.TryGetValue("align-chunks", out var ac) && ac.Status.Status == "completed";
        bool hasWindowAlign = manifest.Stages.TryGetValue("window-align", out var wa) && wa.Status.Status == "completed";
        if (!hasChunkAlign && !hasWindowAlign)
        {
            issues.Add("Alignment not completed: run either 'align-chunks' or 'window-align'");
        }

        // Try script-compare/report.json for metrics
        var compareReport = Path.Combine(WorkDir, "script-compare", "report.json");
        double wer = double.NaN, cer = double.NaN, opening = double.NaN;
        if (File.Exists(compareReport))
        {
            var json = await File.ReadAllTextAsync(compareReport, ct);
            var root = JsonSerializer.Deserialize<JsonElement>(json);
            if (root.TryGetProperty("Global", out var g))
            {
                if (g.TryGetProperty("Wer", out var jw)) wer = jw.GetDouble();
                if (g.TryGetProperty("Cer", out var jc)) cer = jc.GetDouble();
                if (g.TryGetProperty("OpeningRetention0_10s", out var jr)) opening = jr.GetDouble();
            }
        }

        var gates = new ValidationGates(
            MinOpeningRetention: 0.995,
            MaxSeamDup: 0,
            MaxSeamOmit: 0,
            MaxShortPhraseLoss: 0.005,
            MaxAnchorDriftP95: 0.8,
            MinAnchorCoverage: 0.85,
            MaxWer: _params.WerThreshold,
            MaxCer: _params.CerThreshold
        );
        var reasons = new List<string>(issues);
        if (!double.IsNaN(wer) && wer > gates.MaxWer) reasons.Add($"WER {wer:F3} > {gates.MaxWer:F3}");
        if (!double.IsNaN(cer) && cer > gates.MaxCer) reasons.Add($"CER {cer:F3} > {gates.MaxCer:F3}");
        if (!double.IsNaN(opening) && opening < gates.MinOpeningRetention) reasons.Add($"Opening retention {opening:F4} < {gates.MinOpeningRetention:F4}");

        var okAll = reasons.Count == 0;
        object reportObj = new
        {
            ok = okAll,
            input = manifest.Input.Path,
            stages = manifest.Stages.Keys.OrderBy(k => k).ToArray(),
            issues = reasons,
            warnings,
            metrics = new { wer, cer, opening_retention_0_10s = opening },
            gates,
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

