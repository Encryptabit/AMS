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

        var chunkCount = plan.Chunks.Count;
        var chunkAudioDirectory = PrepareChunkAudioDirectory(chapter);

        // Pre-compute slices, names, and paths for all chunks
        var chunkSlices = new AudioBuffer[chunkCount];
        var chunkAudioEntries = new ChunkAudioEntry[chunkCount];
        for (int i = 0; i < chunkCount; i++)
        {
            var entry = plan.Chunks[i];
            var utteranceName = FormatChunkUtteranceName(i);
            chunkSlices[i] = buffer.Slice(entry.StartSample, entry.LengthSamples);
            chunkAudioEntries[i] = new ChunkAudioEntry(
                ChunkId: entry.ChunkId,
                UtteranceName: utteranceName,
                StartSec: entry.StartSec,
                EndSec: entry.EndSec,
                WavPath: Path.Combine(chunkAudioDirectory, utteranceName + ".wav"));
        }

        // Phase 1: encode all chunk WAVs in parallel (slices are zero-copy views)
        Parallel.For(0, chunkCount,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            },
            i => AudioProcessor.EncodeWav(chunkAudioEntries[i].WavPath, chunkSlices[i]));
        Log.Debug("ASR encoded {ChunkCount} chunk WAVs in parallel", chunkCount);

        // Phase 2: transcribe sequentially (Whisper model context is single-threaded)
        var chunkResults = new List<(AsrResponse Response, double OffsetSec)>(chunkCount);
        for (int i = 0; i < chunkCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await AsrProcessor.TranscribeBufferAsync(chunkSlices[i], options, cancellationToken)
                .ConfigureAwait(false);
            chunkResults.Add((response, plan.Chunks[i].StartSec));
        }

        chapter.Documents.ChunkAudio = new ChunkAudioDocument(
            Version: ChunkAudioDocument.CurrentVersion,
            CreatedAtUtc: DateTime.UtcNow,
            SourceAudioFingerprint: plan.SourceAudioFingerprint,
            SampleRate: buffer.SampleRate,
            Channels: buffer.Channels,
            Chunks: chunkAudioEntries);
        Log.Debug("ASR persisted chunk audio artifact ({ChunkCount} chunks, dir={Directory})",
            chunkAudioEntries.Length, chunkAudioDirectory);

        return MergeChunkResponses(chunkResults);
    }

    public AudioBuffer ResolveAsrReadyBuffer(ChapterContext chapter)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var bufferContext = ResolveAudioBufferContext(chapter);
        var buffer = bufferContext.Buffer ?? throw new InvalidOperationException("Audio buffer could not be loaded.");
        return AsrAudioPreparer.PrepareForAsr(buffer);
    }

    public async Task<AsrResponse> TranscribeChunksAsync(
        ChapterContext chapter,
        IReadOnlyList<int> chunkIndices,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentNullException.ThrowIfNull(chunkIndices);
        ArgumentNullException.ThrowIfNull(options);

        if (chunkIndices.Count == 0)
        {
            throw new ArgumentException(
                "chunkIndices must contain at least one chunk index.", nameof(chunkIndices));
        }

        var existingResponse = chapter.Documents.Asr
            ?? throw new InvalidOperationException(
                "Cannot run scoped re-ASR: no existing AsrResponse on chapter. " +
                "Caller must run a full ASR pass first.");

        var plan = chapter.Documents.ChunkPlan
            ?? throw new InvalidOperationException(
                "Cannot run scoped re-ASR: no chunk plan on chapter. " +
                "Scoped recovery requires a stable chunk plan.");

        var buffer = ResolveAsrReadyBuffer(chapter);
        var audioContext = ResolveAudioBufferContext(chapter);

        // Reject scoped re-ASR if the chunk plan no longer matches the audio. Caller (C4
        // orchestrator) is expected to catch this and fall back to a full chapter re-ASR.
        if (!_chunkPlanningService.IsValid(plan, buffer, audioContext.Descriptor.Path))
        {
            throw new InvalidOperationException(
                "Cannot run scoped re-ASR: chunk plan is invalid for current audio " +
                "(audio fingerprint or chunking policy changed). " +
                "Caller should fall back to full chapter re-transcription.");
        }

        // Validate, dedupe, and sort indices for deterministic processing order.
        var sortedIndices = chunkIndices.Distinct().OrderBy(i => i).ToList();
        foreach (var idx in sortedIndices)
        {
            if (idx < 0 || idx >= plan.Chunks.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunkIndices),
                    $"Chunk index {idx} is out of bounds [0, {plan.Chunks.Count}).");
            }
        }

        Log.Debug(
            "Scoped ASR: re-transcribing {Count} chunk(s) of {Total} (indices=[{Indices}])",
            sortedIndices.Count, plan.Chunks.Count, string.Join(",", sortedIndices));

        var newChunkResults = new List<(int ChunkId, AsrResponse Response, double OffsetSec)>(sortedIndices.Count);
        foreach (var idx in sortedIndices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = plan.Chunks[idx];
            var slice = buffer.Slice(entry.StartSample, entry.LengthSamples);
            var response = await AsrProcessor.TranscribeBufferAsync(slice, options, cancellationToken)
                .ConfigureAwait(false);
            newChunkResults.Add((idx, response, entry.StartSec));
        }

        var spliced = SpliceChunkResponses(existingResponse, plan, sortedIndices, newChunkResults);
        Log.Debug(
            "Scoped ASR splice complete: {OldTokens} → {NewTokens} tokens, {OldSegs} → {NewSegs} segments",
            existingResponse.Tokens?.Length ?? 0, spliced.Tokens.Length,
            existingResponse.Segments?.Length ?? 0, spliced.Segments.Length);

        return spliced;
    }

    // Splices per-chunk re-ASR responses into an existing chapter-level AsrResponse. Tokens
    // and segments whose StartSec falls inside any patched chunk's [StartSec, EndSec) window
    // are removed; the new chunks' tokens/segments are offset-adjusted to absolute time and
    // merged in. The combined arrays are sorted by start time (stable) and a final monotonicity
    // pass clamps any out-of-order entries (matching MergeChunkResponses semantics).
    //
    // ModelVersion is taken from the first new chunk (scoped re-ASR is the source of truth for
    // the patched ranges); falls back to the existing response's modelVersion if no new chunks
    // contributed a value.
    internal static AsrResponse SpliceChunkResponses(
        AsrResponse existingResponse,
        ChunkPlanDocument plan,
        IReadOnlyList<int> patchedChunkIndices,
        IReadOnlyList<(int ChunkId, AsrResponse Response, double OffsetSec)> newChunks)
    {
        ArgumentNullException.ThrowIfNull(existingResponse);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(patchedChunkIndices);
        ArgumentNullException.ThrowIfNull(newChunks);

        var patchedRanges = patchedChunkIndices
            .Select(idx => (Start: plan.Chunks[idx].StartSec, End: plan.Chunks[idx].EndSec))
            .OrderBy(r => r.Start)
            .ToArray();

        bool IsInPatchedRange(double startTime)
        {
            foreach (var range in patchedRanges)
            {
                if (startTime >= range.Start && startTime < range.End)
                {
                    return true;
                }
            }
            return false;
        }

        // Filter out tokens/segments whose StartSec falls in any patched range.
        var keptTokens = (existingResponse.Tokens ?? Array.Empty<AsrToken>())
            .Where(t => !IsInPatchedRange(t.StartTime))
            .ToList();
        var keptSegments = (existingResponse.Segments ?? Array.Empty<AsrSegment>())
            .Where(s => !IsInPatchedRange(s.StartSec))
            .ToList();

        // Apply offsets to new tokens/segments (relative-to-chunk → absolute) AND clip to the
        // patched chunk's own [StartSec, EndSec) window. Whisper can occasionally emit a token
        // exactly at the slice end (or slightly past it); without clipping, that token would
        // land inside the NEXT chunk's range and collide with whichever old token we just
        // preserved at that boundary. Tokens with StartTime outside the chunk window are
        // dropped; segments are clamped (StartSec must be in-window; EndSec is clamped to
        // chunk end so the segment doesn't span into adjacent chunks).
        string? newModelVersion = null;
        foreach (var (chunkId, response, offsetSec) in newChunks)
        {
            var chunkEntry = plan.Chunks[chunkId];
            var chunkStart = chunkEntry.StartSec;
            var chunkEnd = chunkEntry.EndSec;

            newModelVersion ??= response.ModelVersion;
            if (response.Tokens is { Length: > 0 })
            {
                foreach (var token in response.Tokens)
                {
                    var absoluteStart = token.StartTime + offsetSec;
                    if (absoluteStart < chunkStart || absoluteStart >= chunkEnd)
                    {
                        continue;
                    }
                    keptTokens.Add(new AsrToken(absoluteStart, token.Duration, token.Word));
                }
            }
            if (response.Segments is { Length: > 0 })
            {
                foreach (var segment in response.Segments)
                {
                    var absoluteStart = segment.StartSec + offsetSec;
                    if (absoluteStart < chunkStart || absoluteStart >= chunkEnd)
                    {
                        continue;
                    }
                    var absoluteEnd = Math.Min(segment.EndSec + offsetSec, chunkEnd);
                    if (absoluteEnd <= absoluteStart)
                    {
                        absoluteEnd = absoluteStart;
                    }
                    keptSegments.Add(new AsrSegment(absoluteStart, absoluteEnd, segment.Text));
                }
            }
        }

        // Stable sort by start time (LINQ OrderBy is stable). Combined with the original input
        // order, kept tokens precede new tokens at identical timestamps — but in practice the
        // patched ranges and kept ranges don't overlap by construction so ties are irrelevant.
        var orderedTokens = keptTokens.OrderBy(t => t.StartTime).ToArray();
        var orderedSegments = keptSegments.OrderBy(s => s.StartSec).ToArray();

        // Monotonicity guard (mirrors MergeChunkResponses): clamp any out-of-order entries.
        var monotonicTokens = new AsrToken[orderedTokens.Length];
        double lastTokenEnd = 0;
        for (int i = 0; i < orderedTokens.Length; i++)
        {
            var t = orderedTokens[i];
            var adjustedStart = Math.Max(t.StartTime, lastTokenEnd);
            monotonicTokens[i] = adjustedStart == t.StartTime
                ? t
                : new AsrToken(adjustedStart, t.Duration, t.Word);
            lastTokenEnd = adjustedStart + Math.Max(0, t.Duration);
        }

        var monotonicSegments = new AsrSegment[orderedSegments.Length];
        double lastSegmentEnd = 0;
        for (int i = 0; i < orderedSegments.Length; i++)
        {
            var s = orderedSegments[i];
            var adjustedStart = Math.Max(s.StartSec, lastSegmentEnd);
            var adjustedEnd = Math.Max(s.EndSec, adjustedStart);
            monotonicSegments[i] = (adjustedStart == s.StartSec && adjustedEnd == s.EndSec)
                ? s
                : new AsrSegment(adjustedStart, adjustedEnd, s.Text);
            lastSegmentEnd = adjustedEnd;
        }

        return new AsrResponse(
            newModelVersion ?? existingResponse.ModelVersion ?? "whisper",
            monotonicTokens,
            monotonicSegments);
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

    private static string PrepareChunkAudioDirectory(ChapterContext chapter)
    {
        var chapterRoot = chapter.Descriptor.RootPath
            ?? throw new InvalidOperationException("Chapter root path is not configured.");
        var directory = Path.Combine(chapterRoot, "alignment", "chunk-audio");
        Directory.CreateDirectory(directory);

        foreach (var file in Directory.EnumerateFiles(directory, "*.wav"))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Log.Debug("Unable to remove stale ASR chunk audio file {Path}: {Message}", file, ex.Message);
            }
        }

        return directory;
    }

    private static string FormatChunkUtteranceName(int index)
        => $"utt-{index:D4}";
}
