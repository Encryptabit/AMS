using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ams.Core;

public sealed record SilenceParams(
    double DbFloor,
    double MinSilenceDur
);

public sealed record SegmentationParams(
    double Min,
    double Max,
    double Target,
    bool StrictTail
);

public sealed record SilenceEvent(
    double Start,
    double End,
    double Duration,
    double Mid
);

public sealed record SilenceTimeline(
    string AudioSha256,
    string FfmpegVersion,
    SilenceParams Params,
    List<SilenceEvent> Events
);

public sealed record ChunkSpan(
    double Start,
    double End
)
{
    [JsonIgnore]
    public double Length => Math.Max(0, End - Start);
}

public sealed record ChunkPlan(
    List<ChunkSpan> Spans,
    double TotalCost,
    bool TailRelaxed
);

public sealed record ToolingMeta(
    string Name,
    string Version,
    string CommandLine
);

public sealed record ChunkEntry(
    ChunkSpan Span,
    string Id,
    string? ChunkWav,
    string? OutJson,
    string Status,
    int Attempts,
    string? Error
);

public sealed record AsrManifest(
    string AudioPath,
    string AudioSha256,
    string OutDir,
    SegmentationParams Segmentation,
    SilenceParams Silence,
    string SilencesFile,
    ToolingMeta Ffmpeg,
    List<ChunkEntry> Chunks
);

// Intentionally no ValidationReport here to avoid conflict with Ams.Core.Validation.ValidationReport

// ======= ManifestV2 Models for Staged Pipeline =======

public sealed record InputMetadata(
    string Path,
    string Sha256,
    double DurationSec,
    long SizeBytes,
    DateTime ModifiedUtc
);

public sealed record StageFingerprint(
    string InputHash,
    string ParamsHash,
    Dictionary<string, string> ToolVersions
);

public sealed record StageStatus(
    string Status, // "pending", "in_progress", "completed", "failed"
    DateTime? Started,
    DateTime? Ended,
    int Attempts,
    string? Error
);

public sealed record StageEntry(
    StageStatus Status,
    Dictionary<string, string> Artifacts,
    StageFingerprint Fingerprint
);

public sealed record ManifestV2(
    string Schema, // "asr-manifest/v2"
    InputMetadata Input,
    Dictionary<string, StageEntry> Stages,
    DateTime Created,
    DateTime Modified
)
{
    public static ManifestV2 CreateNew(InputMetadata input)
    {
        var now = DateTime.UtcNow;
        return new ManifestV2(
            "asr-manifest/v2",
            input,
            new Dictionary<string, StageEntry>(),
            now,
            now
        );
    }
}

// Stage-specific artifact models
public sealed record SilenceDetectionParams(
    double DbFloor,
    double MinSilenceDur
);

public sealed record WindowPlanningParams(
    double Min,
    double Max,
    double Target,
    bool StrictTail
);

public sealed record ChunkingParams(
    string Format = "wav", // output format for chunks
    int SampleRate = 44100
);

// Updated silence timeline for v2
public sealed record SilenceTimelineV2(
    string AudioSha256,
    SilenceDetectionParams Params,
    List<SilenceEvent> Events,
    Dictionary<string, string> ToolVersions
);

// Window plan for v2
public sealed record WindowPlanV2(
    List<ChunkSpan> Windows,
    WindowPlanningParams Params,
    double TotalCost,
    bool TailRelaxed
);

// Chunk index for managing chunk artifacts
public sealed record ChunkInfo(
    string Id,
    ChunkSpan Span,
    string Filename,
    string Sha256,
    double DurationSec
);

public sealed record ChunkIndex(
    List<ChunkInfo> Chunks,
    string AudioSha256,
    ChunkingParams Params
);
