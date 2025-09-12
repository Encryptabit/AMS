using System.Text.Json;
using Ams.Align.Anchors;
using Ams.Core.Align.Anchors;

namespace Ams.Core.Pipeline;

public class WindowsStage : StageRunner
{
    private readonly WindowsParams _params;

    public WindowsStage(string workDir, WindowsParams parameters) : base(workDir, "windows")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath)) throw new InvalidOperationException("Missing anchors/anchors.json");
        var mergedPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(mergedPath)) throw new InvalidOperationException("Missing transcripts/merged.json");

        var anchorsJson = await File.ReadAllTextAsync(anchorsPath, ct);
        var mergedJson = await File.ReadAllTextAsync(mergedPath, ct);
        var anchors = JsonSerializer.Deserialize<AnchorsArtifact>(anchorsJson) ?? throw new InvalidOperationException("Invalid anchors.json");
        var merged = JsonSerializer.Deserialize<JsonElement>(mergedJson);
        var asrWordCount = merged.TryGetProperty("TotalWords", out var tw) ? tw.GetInt32() : 0;

        // Build half-open windows [BookStart, BookEnd) based on selected anchors, padded
        var selected = anchors.Selected.OrderBy(a => a.Bp).ToList();
        int bookStart = 0, bookEnd = anchors.BookTokenCount; // filtered token count proxy
        var core = selected.Select(a => new Anchor(a.Bp, a.Ap)).ToList();
        var ranges = AnchorDiscovery.BuildWindows(core, bookStart, Math.Max(bookStart, bookEnd - 1), 0, Math.Max(0, asrWordCount - 1));

        var winList = new List<AnchorWindow>();
        for (int i = 0; i < ranges.Count; i++)
        {
            var (bLo, bHi, aLo, aHi) = ranges[i];
            int b0 = Math.Max(bookStart, bLo);
            int b1 = Math.Min(bookEnd, bHi);
            var id = $"win_{i:D4}";
            winList.Add(new AnchorWindow(id, b0, b1, aLo, aHi, i > 0 ? core[i - 1].Bp : null, i < core.Count ? core[i].Bp : null));
        }

        // Coverage: fraction of [bookStart,bookEnd) covered by windows
        var covered = winList.Sum(w => Math.Max(0, w.BookEnd - w.BookStart));
        var coverage = Math.Min(1.0, (double)covered / Math.Max(1.0, (bookEnd - bookStart)));

        var artifact = new WindowsArtifact(
            Windows: winList,
            Params: _params,
            Coverage: coverage,
            LargestGapSec: 0.0,
            ToolVersions: new Dictionary<string, string>()
        );

        Directory.CreateDirectory(stageDir);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "windows.json"), JsonSerializer.Serialize(artifact, new JsonSerializerOptions { WriteIndented = true }), ct);
        await File.WriteAllTextAsync(Path.Combine(stageDir, "params.snapshot.json"), SerializeParams(_params), ct);

        return new Dictionary<string, string> {
            ["windows"] = "windows.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        var hash = File.Exists(anchorsPath) ? ComputeHash(await File.ReadAllTextAsync(anchorsPath, ct)) : string.Empty;
        return new StageFingerprint(hash, paramsHash, new Dictionary<string, string>());
    }
}
