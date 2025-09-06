using System.Text.Json;

namespace Ams.Core.Pipeline;

public class ScriptCompareStage : StageRunner
{
    private readonly ScriptCompareParams _params;
    private readonly string _bookIndexPath;

    public ScriptCompareStage(string workDir, string bookIndexPath, ScriptCompareParams parameters)
        : base(workDir, "script-compare")
    {
        _bookIndexPath = bookIndexPath ?? throw new ArgumentNullException(nameof(bookIndexPath));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var segmentsPath = Path.Combine(WorkDir, "collate", "segments.json");
        if (!File.Exists(segmentsPath)) throw new InvalidOperationException("Missing collate/segments.json");
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath)) throw new InvalidOperationException("Missing anchors/anchors.json");
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.json");
        if (!File.Exists(windowsPath)) throw new InvalidOperationException("Missing windows/windows.json");

        var book = JsonSerializer.Deserialize<BookIndex>(await File.ReadAllTextAsync(_bookIndexPath, ct))!;
        var segments = JsonSerializer.Deserialize<CollationSegments>(await File.ReadAllTextAsync(segmentsPath, ct))!;
        var anchors = JsonSerializer.Deserialize<AnchorsArtifact>(await File.ReadAllTextAsync(anchorsPath, ct))!;
        var windows = JsonSerializer.Deserialize<WindowsArtifact>(await File.ReadAllTextAsync(windowsPath, ct))!;

        // MVP metrics (placeholders; replace with real per-window comp later)
        double wer = 0.0;
        double cer = 0.0;
        double retention = ComputeOpeningRetention(segments);

        var global = new ScriptCompareMetrics(
            Wer: wer,
            Cer: cer,
            OpeningRetention0_10s: retention,
            ShortPhraseLoss: 0.0,
            SeamDuplications: 0,
            SeamOmissions: 0,
            AnchorCoverage: windows.Coverage,
            AnchorDriftP50: 0.0,
            AnchorDriftP95: 0.0,
            RuntimeSec: 0.0
        );
        var report = new ScriptCompareReport(
            Global: global,
            PerWindow: new Dictionary<string, ScriptCompareMetrics>(),
            Errors: Array.Empty<string>(),
            Stats: new { windows = windows.Windows.Count, anchors = anchors.Selected.Count },
            GeneratedAt: DateTime.UtcNow
        );

        Directory.CreateDirectory(stageDir);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_params), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "map.jsonl"), string.Empty, ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "errors.csv"), "type,detail\n", ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "diff.txt"), string.Empty, ct);

        return new Dictionary<string, string> {
            ["report"] = "report.json",
            ["params"] = "params.snapshot.json",
            ["map"] = "map.jsonl",
            ["errors"] = "errors.csv",
            ["diff"] = "diff.txt"
        };
    }

    private static double ComputeOpeningRetention(CollationSegments segments)
    {
        const double windowStart = 0.0, windowEnd = 10.0;
        var expected = segments.Sentences.Count(s => s.Start < windowEnd && s.End > windowStart);
        var retained = expected; // Without per-word granularity, assume retained
        return expected > 0 ? Math.Min(1.0, (double)retained / expected) : 1.0;
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        string s = string.Empty, a = string.Empty, w = string.Empty;
        var sPath = Path.Combine(WorkDir, "collate", "segments.json");
        if (File.Exists(sPath)) s = ComputeHash(await File.ReadAllTextAsync(sPath, ct));
        var aPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (File.Exists(aPath)) a = ComputeHash(await File.ReadAllTextAsync(aPath, ct));
        var wPath = Path.Combine(WorkDir, "windows", "windows.json");
        if (File.Exists(wPath)) w = ComputeHash(await File.ReadAllTextAsync(wPath, ct));
        var inputHash = ComputeHash(s + "\n" + a + "\n" + w);
        return new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>());
    }
}
