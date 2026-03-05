using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services;

/// <summary>
/// Orchestrates Whisper.NET transcription using the shared processor.
/// Pre-chunks audio using the shared chunk-plan artifact to ensure ASR and MFA
/// use identical chunk boundaries, eliminating stage drift.
/// </summary>
public sealed class AsrService : IAsrService
{
    private readonly ChunkPlanningService _chunkPlanningService = new();

    public async Task<AsrResponse> TranscribeAsync(
        ChapterContext chapter,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var buffer = ResolveAsrReadyBuffer(chapter);

        // Rollout control: DisableChunkPlan bypasses chunk planning entirely,
        // reverting to the legacy single-buffer ASR path.
        if (options.DisableChunkPlan)
        {
            Log.Debug("ASR chunk planning disabled by rollout flag; using single-buffer path");
            return await AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken)
                .ConfigureAwait(false);
        }

        var plan = ResolveOrCreateChunkPlan(chapter, buffer);

        // Single chunk: fall through to original single-buffer path
        if (plan.Chunks.Count <= 1)
        {
            return await AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken)
                .ConfigureAwait(false);
        }

        // Multi-chunk: transcribe each independently and merge with timestamp offsets
        var totalDuration = buffer.Length / (double)buffer.SampleRate;
        Log.Debug(
            "ASR chunk-plan driven: {ChunkCount} chunks from {TotalDuration:F1}s audio",
            plan.Chunks.Count, totalDuration);

        var chunkResults = new List<(AsrResponse Response, double OffsetSec)>(plan.Chunks.Count);

        foreach (var entry in plan.Chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var slice = buffer.Slice(entry.StartSample, entry.LengthSamples);
            var response = await AsrProcessor.TranscribeBufferAsync(slice, options, cancellationToken)
                .ConfigureAwait(false);

            chunkResults.Add((response, entry.StartSec));
        }

        return MergeChunkResponses(chunkResults);
    }

    public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var bufferContext = ResolveAudioBufferContext(chapter);
        var buffer = bufferContext.Buffer ?? throw new InvalidOperationException("Audio buffer could not be loaded.");
        return AsrAudioPreparer.PrepareForAsr(buffer);
    }

    /// <summary>
    /// Resolves an existing chunk plan from chapter documents, or generates a new one
    /// via <see cref="ChunkPlanningService"/> and persists it through chapter documents.
    /// Invalidates the plan when the audio fingerprint or policy no longer matches.
    /// </summary>
    private ChunkPlanDocument ResolveOrCreateChunkPlan(ChapterContext chapter, AudioBuffer buffer)
    {
        var existing = chapter.Documents.ChunkPlan;
        var audioContext = ResolveAudioBufferContext(chapter);
        var sourceAudioPath = audioContext.Descriptor.Path;

        if (existing is not null &&
            _chunkPlanningService.IsValid(existing, buffer, sourceAudioPath))
        {
            Log.Debug(
                "ASR reusing existing chunk plan ({ChunkCount} chunks, fingerprint={Fingerprint})",
                existing.Chunks.Count, existing.SourceAudioFingerprint);
            return existing;
        }

        Log.Debug("ASR generating new chunk plan (existing={HasExisting})", existing is not null);
        var plan = _chunkPlanningService.GeneratePlan(buffer, sourceAudioPath);
        chapter.Documents.ChunkPlan = plan;
        Log.Debug(
            "ASR persisted chunk plan ({ChunkCount} chunks, fingerprint={Fingerprint})",
            plan.Chunks.Count, plan.SourceAudioFingerprint);
        return plan;
    }

    /// <summary>
    /// Merges per-chunk ASR responses into a single response with correctly
    /// offset timestamps. Tokens and segments from each chunk are adjusted
    /// by the chunk's start time offset to produce monotonically increasing timestamps.
    /// </summary>
    /// <remarks>
    /// Guardrails enforced:
    /// <list type="bullet">
    ///   <item>Chunks are processed in offset order (deterministic ordering).</item>
    ///   <item>Token timestamps are clamped to maintain monotonic non-decreasing order.</item>
    ///   <item>Segment timestamps are clamped to maintain monotonic non-decreasing order.</item>
    ///   <item>Each chunk's offset is applied exactly once (no duplicate offset application).</item>
    /// </list>
    /// </remarks>
    internal static AsrResponse MergeChunkResponses(
        IReadOnlyList<(AsrResponse Response, double OffsetSec)> chunks)
    {
        var allTokens = new List<AsrToken>();
        var allSegments = new List<AsrSegment>();
        string? modelVersion = null;

        // Track high-water marks for monotonicity enforcement
        double lastTokenEnd = 0;
        double lastSegmentEnd = 0;

        // Process chunks in offset order to guarantee deterministic ordering
        var ordered = chunks.Count <= 1
            ? chunks
            : chunks.OrderBy(c => c.OffsetSec).ToList();

        foreach (var (response, offsetSec) in ordered)
        {
            modelVersion ??= response.ModelVersion;

            if (response.Tokens is { Length: > 0 })
            {
                foreach (var token in response.Tokens)
                {
                    var adjustedStart = Math.Max(token.StartTime + offsetSec, lastTokenEnd);
                    var adjustedDuration = token.Duration;
                    allTokens.Add(new AsrToken(adjustedStart, adjustedDuration, token.Word));
                    lastTokenEnd = adjustedStart + Math.Max(0, adjustedDuration);
                }
            }

            if (response.Segments is { Length: > 0 })
            {
                foreach (var segment in response.Segments)
                {
                    var adjustedStart = Math.Max(segment.StartSec + offsetSec, lastSegmentEnd);
                    var adjustedEnd = Math.Max(segment.EndSec + offsetSec, adjustedStart);
                    allSegments.Add(new AsrSegment(adjustedStart, adjustedEnd, segment.Text));
                    lastSegmentEnd = adjustedEnd;
                }
            }
        }

        return new AsrResponse(
            modelVersion ?? "whisper",
            allTokens.ToArray(),
            allSegments.ToArray());
    }

    private static AudioBufferContext ResolveAudioBufferContext(ChapterContext chapter)
    {
        if (chapter.Descriptor.AudioBuffers.Count == 0)
        {
            throw new InvalidOperationException("No audio buffers are registered for this chapter.");
        }

        var descriptor = chapter.Descriptor.AudioBuffers[0];
        return chapter.Audio.Load(descriptor.BufferId);
    }
}
