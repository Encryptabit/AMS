using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Align.Anchors;

namespace Ams.Core.Pipeline;

public class WindowsStage : StageRunner
{
    private readonly WindowsParams _params;

    public WindowsStage(string workDir, WindowsParams parameters)
        : base(workDir, "windows")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine($"Building half-open windows with pads (pre={_params.PrePadSec}s, pad={_params.PadSec}s)...");

        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.v2.json");
        if (!File.Exists(anchorsPath))
        {
            throw new InvalidOperationException("Anchors not found. Run 'anchors' stage first.");
        }

        var anchorsJson = await File.ReadAllTextAsync(anchorsPath, ct);
        var anchors = JsonSerializer.Deserialize<AnchorsResult>(anchorsJson) ?? throw new InvalidOperationException("Invalid anchors.v2.json");

        // Map AnchorsResult.Selected → list of (Bp,Ap)
        var pairs = anchors.Selected
            .OrderBy(a => a.BookPos)
            .Select(a => (Bp: a.BookPos, Ap: a.AsrPos))
            .ToList();

        // Derive ranges in token coordinates (inclusive on ends at source; BuildWindows uses half-open math internally)
        int bookStart = 0;
        int bookEnd = Math.Max(0, (anchors.Tokens.TryGetValue("book", out var bt) ? bt : 0) - 1);
        int asrStart = 0;
        int asrEnd = Math.Max(0, (anchors.Tokens.TryGetValue("asr", out var at) ? at : 0) - 1);

        var windowsRaw = AnchorDiscovery.BuildWindows(
            pairs.Select(p => new Anchor(p.Bp, p.Ap)).ToList(),
            bookStart, bookEnd, asrStart, asrEnd);

        // Build output entries and meta
        var entries = new List<WindowEntry>(windowsRaw.Count);
        double coveredBook = 0.0;

        for (int i = 0; i < windowsRaw.Count; i++)
        {
            var (bLo, bHi, aLo, aHi) = windowsRaw[i];
            var id = $"window_{i + 1:D4}";

            // Identify prev/next anchors for this window (i maps to anchors i..i+1 after sentinel insertion)
            AnchorCandidate? prev = null;
            AnchorCandidate? next = null;
            if (i < anchors.Selected.Count)
            {
                var pa = anchors.Selected[Math.Max(0, i)];
                prev = new AnchorCandidate(pa.Ngram, pa.BookPos, pa.AsrPos, 1.0, true);
            }
            if (i + 1 < anchors.Selected.Count)
            {
                var na = anchors.Selected[i + 1];
                next = new AnchorCandidate(na.Ngram, na.BookPos, na.AsrPos, 1.0, true);
            }

            entries.Add(new WindowEntry(id, bLo, bHi, aLo, aHi, prev, next));
            coveredBook += Math.Max(0, bHi - bLo);
        }

        double coverage = (bookEnd - bookStart + 1) > 0 ? coveredBook / (bookEnd - bookStart + 1) : 0.0;

        var windowsResult = new WindowsResult(
            Meta: new Dictionary<string, object>
            {
                ["coverage"] = coverage,
                ["largestGapSec"] = 0.0 // token-space only at this stage; computed in later stages if needed
            },
            Params: _params,
            Windows: entries
        );

        Directory.CreateDirectory(stageDir);
        var windowsPath = Path.Combine(stageDir, "windows.v2.json");
        var json = JsonSerializer.Serialize(windowsResult, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(windowsPath, json, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Created {windowsResult.Windows.Count} windows (coverage={coverage:P2})");

        return new Dictionary<string, string>
        {
            ["windows"] = "windows.v2.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Hash anchors result content
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.v2.json");
        var anchorsHash = "missing";
        if (File.Exists(anchorsPath))
        {
            var anchorsContent = await File.ReadAllTextAsync(anchorsPath, ct);
            anchorsHash = ComputeHash(anchorsContent);
        }

        var toolVersions = new Dictionary<string, string>
        {
            ["windows"] = "1.0.0"
        };

        return new StageFingerprint(
            anchorsHash,
            paramsHash,
            toolVersions
        );
    }
}
