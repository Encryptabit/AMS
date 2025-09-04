using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public class PlanWindowsStage : StageRunner
{
    private readonly IChunkPlanner _planner;
    private readonly WindowPlanningParams _params;

    public PlanWindowsStage(
        string workDir,
        IChunkPlanner planner,
        WindowPlanningParams parameters)
        : base(workDir, "plan")
    {
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine($"Planning windows (min={_params.Min}s, max={_params.Max}s, target={_params.Target}s)...");

        var timelineDir = Path.Combine(WorkDir, "timeline");
        var silencePath = Path.Combine(timelineDir, "silence.json");
        if (!File.Exists(silencePath))
            throw new InvalidOperationException("Silence timeline not found. Run detect-silence stage first.");

        var timelineJson = await File.ReadAllTextAsync(silencePath, ct);
        var timeline = JsonSerializer.Deserialize<SilenceTimelineV2>(timelineJson) ?? throw new InvalidOperationException("Invalid silence timeline format.");

        var candidates = timeline.Events.Select(e => e.Mid).ToArray();
        var legacyParams = new SegmentationParams(_params.Min, _params.Max, _params.Target, _params.StrictTail);

        var plan = _planner.Plan(manifest.Input.DurationSec, candidates, legacyParams);

        var planV2 = new WindowPlanV2(plan.Spans, _params, plan.TotalCost, plan.TailRelaxed);

        var windowsPath = Path.Combine(stageDir, "windows.json");
        var json = JsonSerializer.Serialize(planV2, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(windowsPath, json, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Planned {plan.Spans.Count} windows (tail relaxed: {plan.TailRelaxed})");

        return new Dictionary<string, string>
        {
            ["windows"] = "windows.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        var inputHash = manifest.Input.Sha256;
        if (manifest.Stages.TryGetValue("timeline", out var silenceStage))
        {
            inputHash = ComputeHash(inputHash + silenceStage.Fingerprint.InputHash + silenceStage.Fingerprint.ParamsHash);
        }
        return Task.FromResult(new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>()));
    }
}

