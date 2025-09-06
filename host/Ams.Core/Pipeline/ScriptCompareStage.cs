using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public class ScriptCompareStage : StageRunner
{
    private readonly ScriptCompareParams _params;
    private readonly string _bookIndexPath;

    public ScriptCompareStage(string workDir, ScriptCompareParams parameters, string bookIndexPath)
        : base(workDir, "script-compare")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _bookIndexPath = bookIndexPath ?? throw new ArgumentNullException(nameof(bookIndexPath));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine("Comparing collated transcript vs BookIndex with window-scoped scoring...");

        // Refactorv2: Step 10 - Scaffold script comparison with placeholder metrics
        // TODO: Load collated segments, anchors, windows and perform actual comparison
        var collatedPath = Path.Combine(WorkDir, "collate", "segments.v2.json");
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.v2.json");
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.v2.json");

        // Back-compat: fall back to segments.json if v2 not present
        if (!File.Exists(collatedPath))
            collatedPath = Path.Combine(WorkDir, "collate", "segments.json");

        if (!File.Exists(collatedPath) || !File.Exists(anchorsPath) || !File.Exists(windowsPath))
        {
            throw new InvalidOperationException("Required inputs not found. Run preceding stages first.");
        }

        // Minimal metrics based on available timing data
        var segmentsJson = await File.ReadAllTextAsync(collatedPath, ct);
        var segments = JsonSerializer.Deserialize<CollationSegments>(segmentsJson) ?? throw new InvalidOperationException("Invalid collate segments");

        double openingRetention = ComputeOpeningRetention(segments.Sentences, 0.0, 10.0);
        int seamDuplications = 0; // not inferable from current data; assume none
        int seamOmissions = 0;    // not inferable from current data; assume none

        // Anchor coverage proxy: anchors per 1k tokens
        var anchorsJson = await File.ReadAllTextAsync(anchorsPath, ct);
        var anchors = JsonSerializer.Deserialize<AnchorsResult>(anchorsJson) ?? throw new InvalidOperationException("Invalid anchors");
        double anchorCoverage = anchors.Tokens.TryGetValue("book", out var bt) && bt > 0 ? (double)Math.Max(1, anchors.Selected.Count - 1) / bt : 0.0;

        // Windows file for per-window metrics (placeholder WER/CER not computed without ASR text mapping)
        var windowsJson = await File.ReadAllTextAsync(windowsPath, ct);
        var windows = JsonSerializer.Deserialize<WindowsResult>(windowsJson) ?? throw new InvalidOperationException("Invalid windows");

        var windowMetrics = new List<WindowMetrics>();
        foreach (var w in windows.Windows)
        {
            // Compute opening retention contribution only for windows overlapping [0,10]s — approximated by global retention
            windowMetrics.Add(new WindowMetrics(w.Id, Wer: 0.0, Cer: 0.0, OpeningRetention: openingRetention,
                ShortPhraseLossRate: 0.0, SeamDuplications: 0, SeamOmissions: 0,
                AnchorCoverage: anchorCoverage, AnchorDriftP50: 0.0, AnchorDriftP95: 0.0));
        }

        var report = new ScriptCompareReport(
            Wer: 0.0,
            Cer: 0.0,
            OpeningRetention: openingRetention,
            ShortPhraseLossRate: 0.0,
            SeamDuplications: seamDuplications,
            SeamOmissions: seamOmissions,
            AnchorCoverage: anchorCoverage,
            AnchorDriftP50: 0.0,
            AnchorDriftP95: 0.0,
            WindowMetrics: windowMetrics,
            Stats: new Dictionary<string, object>
            {
                ["comparisonRulesVersion"] = _params.ComparisonRulesVersion ?? "1.0"
            }
        );

        var reportPath = Path.Combine(stageDir, "report.json");
        var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(reportPath, reportJson, ct);

        // Create placeholder diff artifacts
        var mapPath = Path.Combine(stageDir, "map.jsonl");
        await File.WriteAllTextAsync(mapPath, "// Placeholder word-level alignment map\n", ct);

        var errorsPath = Path.Combine(stageDir, "errors.csv");
        await File.WriteAllTextAsync(errorsPath, "window_id,type,expected,actual,position\n", ct);

        var diffPath = Path.Combine(stageDir, "diff.txt");
        await File.WriteAllTextAsync(diffPath, "// Placeholder human-readable diff\n", ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"WER: {report.Wer:F3}, CER: {report.Cer:F3}, Opening retention: {report.OpeningRetention:F3}");

        return new Dictionary<string, string>
        {
            ["report"] = "report.json",
            ["map"] = "map.jsonl", 
            ["errors"] = "errors.csv",
            ["diff"] = "diff.txt",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Refactorv2: Step 10 - Compute fingerprint based on segments, anchors, windows, and comparison rules
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Hash all input artifacts
        var segmentsPath = Path.Combine(WorkDir, "collate", "segments.v2.json");
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.v2.json");
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.v2.json");
        // Back-compat
        if (!File.Exists(segmentsPath)) segmentsPath = Path.Combine(WorkDir, "collate", "segments.json");

        var segmentsHash = File.Exists(segmentsPath) ? ComputeHash(await File.ReadAllTextAsync(segmentsPath)) : "missing";
        var anchorsHash = File.Exists(anchorsPath) ? ComputeHash(await File.ReadAllTextAsync(anchorsPath)) : "missing";
        var windowsHash = File.Exists(windowsPath) ? ComputeHash(await File.ReadAllTextAsync(windowsPath)) : "missing";
        
        var inputHash = ComputeHash(segmentsHash + anchorsHash + windowsHash);

        var toolVersions = new Dictionary<string, string>
        {
            ["comparison"] = "1.0.0",
            ["rules"] = _params.ComparisonRulesVersion ?? "1.0"
        };

        return new StageFingerprint(
            inputHash,
            paramsHash,
            toolVersions
        );
    }

    private static double ComputeOpeningRetention(List<RefinedSentence> sentences, double startSec, double endSec)
    {
        if (endSec <= startSec) return 1.0;
        double window = endSec - startSec;
        // Measure coverage of sentences over [startSec, endSec]
        var intervals = sentences
            .Select(s => (Start: Math.Max(startSec, s.Start), End: Math.Min(endSec, s.End)))
            .Where(x => x.End > x.Start)
            .OrderBy(x => x.Start)
            .ToList();

        double covered = 0.0;
        double curS = -1, curE = -1;
        foreach (var iv in intervals)
        {
            if (curS < 0)
            {
                curS = iv.Start; curE = iv.End; continue;
            }
            if (iv.Start <= curE)
            {
                curE = Math.Max(curE, iv.End);
            }
            else
            {
                covered += curE - curS; curS = iv.Start; curE = iv.End;
            }
        }
        if (curS >= 0) covered += curE - curS;
        return Math.Max(0.0, Math.Min(1.0, covered / window));
    }
}
