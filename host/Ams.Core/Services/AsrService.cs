using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services;

/// <summary>
/// Orchestrates Whisper.NET transcription using the shared processor.
/// Pre-chunks audio at silence boundaries before sending to Whisper for
/// improved word-level timestamp accuracy.
/// </summary>
public sealed class AsrService : IAsrService
{
    public async Task<AsrResponse> TranscribeAsync(
        ChapterContext chapter,
        AsrOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        var buffer = ResolveAsrReadyBuffer(chapter);

        var chunks = SilenceChunker.FindChunkBoundaries(buffer);

        // Single chunk: fall through to original single-buffer path
        if (chunks.Count <= 1)
        {
            return await AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken)
                .ConfigureAwait(false);
        }

        // Multi-chunk: transcribe each independently and merge with timestamp offsets
        var totalDuration = buffer.Length / (double)buffer.SampleRate;
        Log.Debug(
            "Pre-chunking: {ChunkCount} chunks from {TotalDuration:F1}s audio",
            chunks.Count, totalDuration);

        var chunkResults = new List<(AsrResponse Response, double OffsetSec)>(chunks.Count);

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var slice = buffer.Slice(chunk.StartSample, chunk.Length);
            var offsetSec = chunk.StartSample / (double)buffer.SampleRate;
            var response = await AsrProcessor.TranscribeBufferAsync(slice, options, cancellationToken)
                .ConfigureAwait(false);

            chunkResults.Add((response, offsetSec));
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
    /// Merges per-chunk ASR responses into a single response with correctly
    /// offset timestamps. Tokens and segments from each chunk are adjusted
    /// by the chunk's start time offset to produce monotonically increasing timestamps.
    /// </summary>
    private static AsrResponse MergeChunkResponses(
        IReadOnlyList<(AsrResponse Response, double OffsetSec)> chunks)
    {
        var allTokens = new List<AsrToken>();
        var allSegments = new List<AsrSegment>();
        string? modelVersion = null;

        foreach (var (response, offsetSec) in chunks)
        {
            modelVersion ??= response.ModelVersion;

            if (response.Tokens is { Length: > 0 })
            {
                foreach (var token in response.Tokens)
                {
                    allTokens.Add(new AsrToken(
                        token.StartTime + offsetSec,
                        token.Duration,
                        token.Word));
                }
            }

            if (response.Segments is { Length: > 0 })
            {
                foreach (var segment in response.Segments)
                {
                    allSegments.Add(new AsrSegment(
                        segment.StartSec + offsetSec,
                        segment.EndSec + offsetSec,
                        segment.Text));
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