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

// Transcription models
public sealed record TranscriptionParams(
    string Model = "nvidia/parakeet-ctc-0.6b",
    string Language = "en",
    int BeamSize = 1,
    double TemperatureSampling = 1.0,
    string ServiceUrl = "http://localhost:8081"
);

public sealed record TranscriptWord(
    string Word,
    double Start,
    double End,
    double Confidence
);

public sealed record ChunkTranscript(
    string ChunkId,
    string Text,
    List<TranscriptWord> Words,
    double DurationSec,
    Dictionary<string, string> ToolVersions,
    DateTime GeneratedAt
);

public sealed record TranscriptIndex(
    List<string> ChunkIds,
    Dictionary<string, string> ChunkToJsonMap,
    TranscriptionParams Params,
    Dictionary<string, string> ToolVersions
);

// Alignment models
public sealed record AlignmentParams(
    string Language = "eng",
    int TimeoutSec = 600,
    string ServiceUrl = "http://localhost:8082"
);

public sealed record AlignmentFragment(
    double Begin,
    double End
);

public sealed record ChunkAlignment(
    string ChunkId,
    double OffsetSec,
    string Language,
    string TextDigest,
    List<AlignmentFragment> Fragments,
    Dictionary<string, string> ToolVersions,
    DateTime GeneratedAt
);

// Refinement models  
public sealed record RefinementParams(
    double SilenceThresholdDb = -30.0,
    double MinSilenceDurSec = 0.12
);

public sealed record RefinedSentence(
    string Id,
    double Start,
    double End,
    int? StartWordIdx,
    int? EndWordIdx,
    string Source = "aeneas+silence.start"
);

// Collation models
public sealed record CollationParams(
    string RoomtoneSource = "auto", // "auto" or "file"
    double RoomtoneLevelDb = -50.0,
    int MinGapMs = 5,
    int MaxGapMs = 2000,
    int BridgeMaxMs = 60,
    string? RoomtoneFilePath = null
);

public sealed record CollationReplacement(
    string Kind, // "gap" or "boundary_sliver"
    double From,
    double To,
    double Duration,
    double RoomtoneLevelDb
);

public sealed record CollationSegments(
    List<RefinedSentence> Sentences,
    List<CollationReplacement> Replacements
);

// Validation models
public sealed record ValidationParams(
    double WerThreshold = 0.25,
    double CerThreshold = 0.25,
    Dictionary<string, double>? EditCosts = null
);
