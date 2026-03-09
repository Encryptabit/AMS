using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts.Alignment;

/// <summary>
/// Canonical chunk plan artifact representing the silence-based audio segmentation
/// for a chapter. This document is shared by both the ASR and MFA execution stages
/// to ensure deterministic, consistent chunk boundaries across the pipeline.
/// <para>
/// Persisted as <c>{chapterStem}.align.chunks.json</c> through the artifact resolver.
/// </para>
/// </summary>
public sealed record ChunkPlanDocument(
    [property: JsonPropertyName("version")]
    int Version,

    [property: JsonPropertyName("createdAtUtc")]
    DateTime CreatedAtUtc,

    [property: JsonPropertyName("sourceAudioPath")]
    string SourceAudioPath,

    [property: JsonPropertyName("sourceAudioFingerprint")]
    string SourceAudioFingerprint,

    [property: JsonPropertyName("policy")]
    ChunkPlanPolicy Policy,

    [property: JsonPropertyName("chunks")]
    IReadOnlyList<ChunkPlanEntry> Chunks)
{
    /// <summary>
    /// Current document schema version. Increment when making breaking changes
    /// to the chunk plan format.
    /// </summary>
    public const int CurrentVersion = 2;
}

/// <summary>
/// Policy metadata capturing the parameters used to generate the chunk plan.
/// Enables downstream stages to verify whether the plan is still valid for
/// their execution requirements.
/// </summary>
public sealed record ChunkPlanPolicy(
    [property: JsonPropertyName("silenceThresholdDb")]
    double SilenceThresholdDb,

    [property: JsonPropertyName("minSilenceDurationMs")]
    double MinSilenceDurationMs,

    [property: JsonPropertyName("minChunkDurationSec")]
    double MinChunkDurationSec,

    [property: JsonPropertyName("maxChunkDurationSec")]
    double MaxChunkDurationSec,

    [property: JsonPropertyName("sampleRate")]
    int SampleRate);

/// <summary>
/// A single contiguous chunk within the chapter audio. Each chunk is defined
/// by its sample range (for precise slicing) and its time range (for alignment
/// offset calculations).
/// </summary>
public sealed record ChunkPlanEntry(
    [property: JsonPropertyName("chunkId")]
    int ChunkId,

    [property: JsonPropertyName("startSample")]
    int StartSample,

    [property: JsonPropertyName("lengthSamples")]
    int LengthSamples,

    [property: JsonPropertyName("startSec")]
    double StartSec,

    [property: JsonPropertyName("endSec")]
    double EndSec);
