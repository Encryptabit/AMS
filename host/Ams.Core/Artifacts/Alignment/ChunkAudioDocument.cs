using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts.Alignment;

/// <summary>
/// Persisted record of chunk audio WAV files emitted during ASR chunk transcription.
/// MFA can reuse these files to guarantee identical chunk audio boundaries.
/// </summary>
public sealed record ChunkAudioDocument(
    [property: JsonPropertyName("version")]
    int Version,

    [property: JsonPropertyName("createdAtUtc")]
    DateTime CreatedAtUtc,

    [property: JsonPropertyName("sourceAudioFingerprint")]
    string SourceAudioFingerprint,

    [property: JsonPropertyName("sampleRate")]
    int SampleRate,

    [property: JsonPropertyName("channels")]
    int Channels,

    [property: JsonPropertyName("chunks")]
    IReadOnlyList<ChunkAudioEntry> Chunks)
{
    /// <summary>Current schema version.</summary>
    public const int CurrentVersion = 1;
}

/// <summary>
/// Chunk audio file metadata keyed by chunk id and deterministic utterance name.
/// </summary>
public sealed record ChunkAudioEntry(
    [property: JsonPropertyName("chunkId")]
    int ChunkId,

    [property: JsonPropertyName("utteranceName")]
    string UtteranceName,

    [property: JsonPropertyName("startSec")]
    double StartSec,

    [property: JsonPropertyName("endSec")]
    double EndSec,

    [property: JsonPropertyName("wavPath")]
    string WavPath);
