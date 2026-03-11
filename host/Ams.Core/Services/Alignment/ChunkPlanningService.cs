using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Audio;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Configuration knobs controlling how chapter audio is segmented into chunks.
/// These values parameterize the SilenceChunker algorithm and are stored in
/// the resulting <see cref="ChunkPlanDocument"/> for reproducibility.
/// </summary>
public record ChunkPlanningPolicy
{
    /// <summary>
    /// Silence threshold in decibels. Signals below this level are treated
    /// as silence for boundary detection.
    /// Default: <see cref="AudioDefaults.SilenceThresholdDb"/>.
    /// </summary>
    public double SilenceThresholdDb { get; init; } = AudioDefaults.SilenceThresholdDb;

    /// <summary>
    /// Minimum silence duration to qualify as a split point.
    /// Default: <see cref="AudioDefaults.MinimumSilenceDuration"/>.
    /// </summary>
    public TimeSpan MinSilenceDuration { get; init; } = AudioDefaults.MinimumSilenceDuration;

    /// <summary>
    /// Minimum chunk duration to prevent excessive fragmentation.
    /// Default: 15 seconds to prevent tiny fragments around short pauses.
    /// </summary>
    public TimeSpan MinChunkDuration { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Maximum chunk duration to keep segments inside Whisper's stable 30 second
    /// context window.
    /// Default: 29.5 seconds to leave a small safety margin.
    /// </summary>
    public TimeSpan MaxChunkDuration { get; init; } = TimeSpan.FromSeconds(29.5);

    /// <summary>
    /// Returns the shared default policy derived from <see cref="AudioDefaults"/>.
    /// </summary>
    public static ChunkPlanningPolicy Default => new();
}

/// <summary>
/// Computes a reusable chapter chunk plan from chapter audio using silence-based
/// boundary detection. The resulting <see cref="ChunkPlanDocument"/> is shared by
/// both ASR and MFA execution stages to guarantee deterministic, consistent
/// chunk boundaries across the pipeline.
/// <para>
/// This service is the single source of chunk-plan generation. No stage should
/// compute its own independent boundary logic.
/// </para>
/// </summary>
public sealed class ChunkPlanningService
{
    /// <summary>
    /// Generates a chunk plan for the given chapter audio buffer and policy.
    /// </summary>
    /// <param name="buffer">Chapter audio loaded through AudioBufferManager/AudioBufferContext.</param>
    /// <param name="sourceAudioPath">Path to the source audio file for fingerprint tracking.</param>
    /// <param name="policy">Chunk planning policy. If null, <see cref="ChunkPlanningPolicy.Default"/> is used.</param>
    /// <returns>A deterministic <see cref="ChunkPlanDocument"/> with stable chunk ids in source order.</returns>
    public ChunkPlanDocument GeneratePlan(
        AudioBuffer buffer,
        string sourceAudioPath,
        ChunkPlanningPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAudioPath);

        var effectivePolicy = policy ?? ChunkPlanningPolicy.Default;

        // Invoke SilenceChunker with policy parameters
        var boundaries = SilenceChunker.FindChunkBoundaries(
            buffer,
            silenceThresholdDb: effectivePolicy.SilenceThresholdDb,
            minSilenceDuration: effectivePolicy.MinSilenceDuration,
            minChunkDuration: effectivePolicy.MinChunkDuration,
            maxChunkDuration: effectivePolicy.MaxChunkDuration);

        // Convert sample boundaries to ChunkPlanEntry list with stable chunk ids
        var entries = new List<ChunkPlanEntry>(boundaries.Count);
        for (int i = 0; i < boundaries.Count; i++)
        {
            var boundary = boundaries[i];
            entries.Add(new ChunkPlanEntry(
                ChunkId: i,
                StartSample: boundary.StartSample,
                LengthSamples: boundary.Length,
                StartSec: (double)boundary.StartSample / buffer.SampleRate,
                EndSec: (double)(boundary.StartSample + boundary.Length) / buffer.SampleRate));
        }

        // Compute source audio fingerprint: combine path, sample count, and sample rate
        // for a lightweight but sufficient identity for invalidation decisions.
        var fingerprint = ComputeAudioFingerprint(buffer, sourceAudioPath);

        return new ChunkPlanDocument(
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioPath: sourceAudioPath,
            SourceAudioFingerprint: fingerprint,
            Policy: new ChunkPlanPolicy(
                SilenceThresholdDb: effectivePolicy.SilenceThresholdDb,
                MinSilenceDurationMs: effectivePolicy.MinSilenceDuration.TotalMilliseconds,
                MinChunkDurationSec: effectivePolicy.MinChunkDuration.TotalSeconds,
                MaxChunkDurationSec: effectivePolicy.MaxChunkDuration.TotalSeconds,
                SampleRate: buffer.SampleRate),
            Chunks: entries);
    }

    /// <summary>
    /// Generates a chunk plan for a chapter context by loading its audio through
    /// the chapter's AudioBufferManager.
    /// </summary>
    /// <param name="chapter">Chapter context with audio runtime abstractions.</param>
    /// <param name="policy">Chunk planning policy. If null, <see cref="ChunkPlanningPolicy.Default"/> is used.</param>
    /// <returns>A deterministic <see cref="ChunkPlanDocument"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the chapter audio buffer is not available or cannot be loaded.
    /// </exception>
    public ChunkPlanDocument GeneratePlan(
        ChapterContext chapter,
        ChunkPlanningPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var audioContext = chapter.Audio.Current;
        var buffer = audioContext.Buffer
            ?? throw new InvalidOperationException(
                $"Audio buffer for chapter '{chapter.Descriptor.ChapterId}' could not be loaded.");

        return GeneratePlan(buffer, audioContext.Descriptor.Path, policy);
    }

    /// <summary>
    /// Determines whether an existing chunk plan needs regeneration based on
    /// source audio identity and policy differences.
    /// </summary>
    /// <param name="existing">The existing chunk plan document to evaluate.</param>
    /// <param name="buffer">Current chapter audio buffer.</param>
    /// <param name="sourceAudioPath">Current source audio path.</param>
    /// <param name="policy">Current chunk planning policy.</param>
    /// <returns>True if the existing plan is still valid; false if regeneration is needed.</returns>
    public bool IsValid(
        ChunkPlanDocument existing,
        AudioBuffer buffer,
        string sourceAudioPath,
        ChunkPlanningPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(buffer);

        var effectivePolicy = policy ?? ChunkPlanningPolicy.Default;

        var regenerated = GeneratePlan(buffer, sourceAudioPath, effectivePolicy);

        return string.Equals(existing.SourceAudioFingerprint, regenerated.SourceAudioFingerprint, StringComparison.Ordinal) &&
               existing.Policy == regenerated.Policy &&
               existing.Chunks.SequenceEqual(regenerated.Chunks);
    }

    /// <summary>
    /// Computes a lightweight fingerprint for audio identity tracking.
    /// Combines the file path, sample count, and sample rate to produce a
    /// deterministic identity string for invalidation decisions.
    /// </summary>
    /// <remarks>
    /// This is not a cryptographic hash of audio content -- it trades collision
    /// resistance for speed. Regenerate the chunk plan when source audio changes
    /// (different file, re-encoded, or truncated).
    /// </remarks>
    private static string ComputeAudioFingerprint(AudioBuffer buffer, string sourceAudioPath)
    {
        // Normalize path to forward slashes for cross-platform consistency
        var normalizedPath = sourceAudioPath.Replace('\\', '/');
        return $"{normalizedPath}|{buffer.Length}|{buffer.SampleRate}|{buffer.Channels}";
    }
}
