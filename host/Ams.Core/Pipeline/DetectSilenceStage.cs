using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public class DetectSilenceStage : StageRunner
{
    private readonly ISilenceDetector _detector;
    private readonly IProcessRunner _processRunner;
    private readonly SilenceDetectionParams _params;

    public DetectSilenceStage(
        string workDir,
        ISilenceDetector detector,
        IProcessRunner processRunner,
        SilenceDetectionParams parameters)
        : base(workDir, "timeline")
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine($"Detecting silences (db-floor={_params.DbFloor}dB, min-dur={_params.MinSilenceDur}s)...");

        var legacyParams = new SilenceParams(_params.DbFloor, _params.MinSilenceDur);
        var timeline = await _detector.DetectAsync(manifest.Input.Path, legacyParams, _processRunner, ct);

        var toolVersions = new Dictionary<string, string>
        {
            ["ffmpeg"] = timeline.FfmpegVersion
        };

        var timelineV2 = new SilenceTimelineV2(
            timeline.AudioSha256,
            _params,
            timeline.Events,
            toolVersions
        );

        var silencePath = Path.Combine(stageDir, "silence.json");
        var json = JsonSerializer.Serialize(timelineV2, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(silencePath, json, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Found {timeline.Events.Count} silence events");

        return new Dictionary<string, string>
        {
            ["timeline"] = "silence.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        var toolVersions = new Dictionary<string, string>();
        try
        {
            var result = await _processRunner.RunAsync("ffmpeg", "-version", ct);
            if (result.ExitCode == 0)
            {
                var version = FfmpegSilenceDetector.ParseVersion(result.StdOut + "\n" + result.StdErr) ?? "unknown";
                toolVersions["ffmpeg"] = version;
            }
        }
        catch
        {
            toolVersions["ffmpeg"] = "unknown";
        }

        return new StageFingerprint(
            manifest.Input.Sha256,
            paramsHash,
            toolVersions
        );
    }
}

