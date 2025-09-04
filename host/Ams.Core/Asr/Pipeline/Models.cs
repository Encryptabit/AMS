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
