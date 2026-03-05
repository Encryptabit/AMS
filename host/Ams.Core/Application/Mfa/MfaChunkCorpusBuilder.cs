using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Application.Mfa;

/// <summary>
/// Builds a per-chunk MFA corpus (wav + lab files) from a shared chunk plan,
/// hydrated transcript, and chapter audio. Each chunk produces a deterministic
/// <c>utt-NNNN.wav</c> / <c>utt-NNNN.lab</c> pair under the corpus directory.
/// <para>
/// Lab content is always derived from <see cref="HydratedSentence.BookText"/>
/// to maintain book-text lexical truth. The builder maps chunk time ranges to
/// overlapping hydrated sentences and never falls back to raw ASR words.
/// </para>
/// </summary>
internal static class MfaChunkCorpusBuilder
{
    /// <summary>
    /// Result of a chunk corpus build, containing the list of utterance entries
    /// and the corpus directory path.
    /// </summary>
    internal sealed record ChunkCorpusResult(
        string CorpusDirectory,
        IReadOnlyList<UtteranceEntry> Utterances);

    /// <summary>
    /// A single utterance in the chunk corpus with its file paths and
    /// the source chunk offset (for later TextGrid aggregation).
    /// </summary>
    internal sealed record UtteranceEntry(
        int ChunkId,
        string UtteranceName,
        string WavPath,
        string LabPath,
        double ChunkStartSec,
        double ChunkEndSec);

    /// <summary>
    /// Minimum number of pronunciation tokens per utterance lab. Chunks that
    /// produce fewer tokens after fallback expansion are logged and skipped.
    /// </summary>
    private const int MinLabTokenCount = 2;
    private const int MinBoundaryOverlapTokensForTrim = 3;

    private const double ChunkAudioTimingToleranceSec = 0.05;

    /// <summary>
    /// Builds per-chunk wav and lab files under <paramref name="corpusDirectory"/>.
    /// </summary>
    /// <param name="audioBuffer">Full chapter audio buffer.</param>
    /// <param name="chunkPlan">Shared chunk plan document from ChunkPlanningService.</param>
    /// <param name="hydrate">Hydrated transcript with sentence-level BookText and timing.</param>
    /// <param name="corpusDirectory">Target directory for utterance corpus assets.</param>
    /// <returns>A <see cref="ChunkCorpusResult"/> with the list of emitted utterances.</returns>
    internal static ChunkCorpusResult Build(
        AudioBuffer audioBuffer,
        ChunkPlanDocument chunkPlan,
        HydratedTranscript hydrate,
        string corpusDirectory,
        ChunkAudioDocument? chunkAudio = null,
        bool requireAsrChunkAudio = false)
    {
        ArgumentNullException.ThrowIfNull(audioBuffer);
        ArgumentNullException.ThrowIfNull(chunkPlan);
        ArgumentNullException.ThrowIfNull(hydrate);
        ArgumentException.ThrowIfNullOrWhiteSpace(corpusDirectory);

        Directory.CreateDirectory(corpusDirectory);

        var sentences = hydrate.Sentences;
        var utterances = new List<UtteranceEntry>(chunkPlan.Chunks.Count);
        var chunkAudioByChunkId = BuildChunkAudioLookup(chunkAudio);
        if (requireAsrChunkAudio && chunkPlan.Chunks.Count > 0 && chunkAudioByChunkId is null)
        {
            throw new InvalidOperationException(
                "ASR chunk audio is required for chunked MFA, but no chunk-audio artifact was available.");
        }

        int skippedNoText = 0;
        int skippedNoAudio = 0;
        int expandedChunks = 0;
        int reusedChunkAudio = 0;
        int boundaryTrimmedChunks = 0;
        int boundaryTrimmedTokens = 0;
        IReadOnlyList<string>? previousLabTokens = null;

        for (int i = 0; i < chunkPlan.Chunks.Count; i++)
        {
            var chunk = chunkPlan.Chunks[i];
            var uttName = FormatUtteranceName(i);
            var usedFallback = false;

            // Find overlapping sentences for this chunk
            var labText = BuildLabText(sentences, chunk.StartSec, chunk.EndSec);

            if (string.IsNullOrWhiteSpace(labText))
            {
                // Fallback: expand to nearest sentence window
                labText = BuildLabTextWithFallback(sentences, chunk.StartSec, chunk.EndSec, i);
                usedFallback = labText is not null;
                if (usedFallback)
                {
                    expandedChunks++;
                }
            }

            if (string.IsNullOrWhiteSpace(labText))
            {
                skippedNoText++;
                Log.Debug(
                    "Chunk {ChunkId} ({StartSec:F2}s-{EndSec:F2}s) produced no usable lab text after fallback; skipping",
                    chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                continue;
            }

            var labTokens = TokenizeLabText(labText);
            if (labTokens.Count == 0)
            {
                skippedNoText++;
                Log.Debug(
                    "Chunk {ChunkId} ({StartSec:F2}s-{EndSec:F2}s) produced no tokenized lab text; skipping",
                    chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                continue;
            }

            if (previousLabTokens is { Count: > 0 })
            {
                var overlap = FindBoundaryTokenOverlap(previousLabTokens, labTokens);
                if (overlap >= MinBoundaryOverlapTokensForTrim)
                {
                    labTokens = labTokens.Skip(overlap).ToList();
                    boundaryTrimmedChunks++;
                    boundaryTrimmedTokens += overlap;
                    Log.Debug(
                        "Chunk {ChunkId} ({Utterance}) trimmed {Overlap} boundary-duplicate tokens",
                        chunk.ChunkId, uttName, overlap);
                }
            }

            if (labTokens.Count < MinLabTokenCount)
            {
                skippedNoText++;
                Log.Debug(
                    "Chunk {ChunkId} ({StartSec:F2}s-{EndSec:F2}s) has too few lab tokens after boundary dedupe; skipping",
                    chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                continue;
            }

            labText = string.Join(' ', labTokens);

            // Write WAV by preferring ASR-emitted chunk audio when available,
            // otherwise falling back to FFmpeg time-domain trim.
            var wavPath = Path.Combine(corpusDirectory, uttName + ".wav");
            if (TryCopyPreSlicedChunkAudio(chunk, uttName, wavPath, chunkAudioByChunkId, out var reuseFailureReason))
            {
                reusedChunkAudio++;
            }
            else
            {
                if (requireAsrChunkAudio)
                {
                    throw new InvalidOperationException(
                        $"ASR chunk audio is required for chunked MFA, but chunk {chunk.ChunkId} " +
                        $"({uttName}) could not be reused: {reuseFailureReason}");
                }

                var chunkStart = TimeSpan.FromSeconds(Math.Max(0d, chunk.StartSec));
                var chunkEnd = TimeSpan.FromSeconds(Math.Max(chunk.StartSec, chunk.EndSec));
                var slice = AudioProcessor.Trim(audioBuffer, chunkStart, chunkEnd);

                if (slice.Length <= 0)
                {
                    skippedNoAudio++;
                    Log.Debug(
                        "Chunk {ChunkId} has no audio after FFmpeg trim " +
                        "(startSec={StartSec:F2}, endSec={EndSec:F2}); skipping",
                        chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                    continue;
                }

                AudioProcessor.EncodeWav(wavPath, slice);
            }

            // Write LAB
            var labPath = Path.Combine(corpusDirectory, uttName + ".lab");
            File.WriteAllText(labPath, labText, Encoding.UTF8);

            utterances.Add(new UtteranceEntry(
                ChunkId: chunk.ChunkId,
                UtteranceName: uttName,
                WavPath: wavPath,
                LabPath: labPath,
                ChunkStartSec: chunk.StartSec,
                ChunkEndSec: chunk.EndSec));

            previousLabTokens = labTokens;
        }

        Log.Info(
            "Built chunk corpus: {Count} utterances from {Total} chunks " +
            "(skipped: {SkippedText} no-text, {SkippedAudio} no-audio; expanded: {Expanded}; reused-asr-audio: {Reused}; " +
            "boundary-dedupe: {DedupedChunks} chunks/{DedupedTokens} tokens)",
            utterances.Count, chunkPlan.Chunks.Count, skippedNoText, skippedNoAudio, expandedChunks, reusedChunkAudio,
            boundaryTrimmedChunks, boundaryTrimmedTokens);

        return new ChunkCorpusResult(corpusDirectory, utterances);
    }

    /// <summary>
    /// Builds lab text from hydrated sentences whose timing overlaps
    /// the chunk time range [chunkStart, chunkEnd).
    /// </summary>
    internal static string? BuildLabText(
        IReadOnlyList<HydratedSentence> sentences,
        double chunkStartSec,
        double chunkEndSec)
    {
        var overlapping = FindOverlappingSentences(sentences, chunkStartSec, chunkEndSec);
        if (overlapping.Count == 0)
        {
            return null;
        }

        return PrepareLabFromSentences(overlapping);
    }

    /// <summary>
    /// Fallback when no sentences overlap the chunk timing window.
    /// Expands to the nearest sentence(s) by proximity, ensuring at least
    /// one sentence is included. Never switches to raw ASR words.
    /// </summary>
    internal static string? BuildLabTextWithFallback(
        IReadOnlyList<HydratedSentence> sentences,
        double chunkStartSec,
        double chunkEndSec,
        int chunkIndex)
    {
        if (sentences.Count == 0)
        {
            return null;
        }

        var chunkMidpoint = (chunkStartSec + chunkEndSec) / 2.0;

        // Find the sentence with the closest timing to the chunk midpoint
        HydratedSentence? closest = null;
        double closestDistance = double.MaxValue;

        foreach (var sentence in sentences)
        {
            if (sentence.Timing is null)
            {
                continue;
            }

            var sentMid = (sentence.Timing.StartSec + sentence.Timing.EndSec) / 2.0;
            var distance = Math.Abs(sentMid - chunkMidpoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = sentence;
            }
        }

        if (closest is null)
        {
            // No timed sentences at all: use positional fallback based on chunk index
            // Map chunk index proportionally to sentence list
            var proportionalIndex = sentences.Count > 1
                ? (int)Math.Round((double)chunkIndex / Math.Max(1, chunkIndex + 1) * (sentences.Count - 1))
                : 0;
            proportionalIndex = Math.Clamp(proportionalIndex, 0, sentences.Count - 1);
            closest = sentences[proportionalIndex];
        }

        Log.Debug(
            "Chunk {ChunkIndex} ({StartSec:F2}s-{EndSec:F2}s) expanded to nearest sentence {SentenceId}",
            chunkIndex, chunkStartSec, chunkEndSec, closest.Id);

        var labText = PrepareLabFromSentences(new[] { closest });
        return string.IsNullOrWhiteSpace(labText) ? null : labText;
    }

    /// <summary>
    /// Finds sentences whose timing overlaps [chunkStart, chunkEnd).
    /// A sentence overlaps if its timing range intersects the chunk range.
    /// </summary>
    internal static List<HydratedSentence> FindOverlappingSentences(
        IReadOnlyList<HydratedSentence> sentences,
        double chunkStartSec,
        double chunkEndSec)
    {
        var result = new List<HydratedSentence>();

        foreach (var sentence in sentences)
        {
            if (sentence.Timing is null)
            {
                continue;
            }

            // Standard interval overlap: [a, b) intersects [c, d) iff a < d && c < b
            if (sentence.Timing.StartSec < chunkEndSec &&
                chunkStartSec < sentence.Timing.EndSec)
            {
                result.Add(sentence);
            }
        }

        return result;
    }

    /// <summary>
    /// Prepares a single lab line from the BookText of the given sentences.
    /// Uses PronunciationHelper to normalize text for MFA consumption.
    /// </summary>
    private static string? PrepareLabFromSentences(IEnumerable<HydratedSentence> sentences)
    {
        var parts = new List<string>();

        foreach (var sentence in sentences)
        {
            var bookText = sentence.BookText;
            if (string.IsNullOrWhiteSpace(bookText))
            {
                continue;
            }

            var pronunciationParts = PronunciationHelper.ExtractPronunciationParts(bookText);
            foreach (var part in pronunciationParts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    parts.Add(part);
                }
            }
        }

        if (parts.Count < MinLabTokenCount)
        {
            return null;
        }

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Formats a zero-padded utterance name for deterministic ordering.
    /// </summary>
    internal static string FormatUtteranceName(int index)
        => $"utt-{index:D4}";

    internal static int FindBoundaryTokenOverlap(
        IReadOnlyList<string> previousTokens,
        IReadOnlyList<string> currentTokens)
    {
        ArgumentNullException.ThrowIfNull(previousTokens);
        ArgumentNullException.ThrowIfNull(currentTokens);

        var maxLength = Math.Min(previousTokens.Count, currentTokens.Count);
        for (int length = maxLength; length >= 1; length--)
        {
            var startInPrevious = previousTokens.Count - length;
            var isMatch = true;
            for (int i = 0; i < length; i++)
            {
                if (!string.Equals(previousTokens[startInPrevious + i], currentTokens[i], StringComparison.Ordinal))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                return length;
            }
        }

        return 0;
    }

    internal static bool IsChunkAudioEntryCompatible(
        ChunkPlanEntry chunk,
        ChunkAudioEntry entry,
        string expectedUtteranceName)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedUtteranceName);

        if (chunk.ChunkId != entry.ChunkId)
        {
            return false;
        }

        if (!string.Equals(entry.UtteranceName, expectedUtteranceName, StringComparison.Ordinal))
        {
            return false;
        }

        return Math.Abs(chunk.StartSec - entry.StartSec) <= ChunkAudioTimingToleranceSec &&
               Math.Abs(chunk.EndSec - entry.EndSec) <= ChunkAudioTimingToleranceSec;
    }

    private static Dictionary<int, ChunkAudioEntry>? BuildChunkAudioLookup(ChunkAudioDocument? chunkAudio)
    {
        if (chunkAudio is null || chunkAudio.Chunks.Count == 0)
        {
            return null;
        }

        var map = new Dictionary<int, ChunkAudioEntry>();
        foreach (var entry in chunkAudio.Chunks)
        {
            map[entry.ChunkId] = entry;
        }

        return map;
    }

    private static bool TryCopyPreSlicedChunkAudio(
        ChunkPlanEntry chunk,
        string utteranceName,
        string destinationWavPath,
        Dictionary<int, ChunkAudioEntry>? chunkAudioByChunkId,
        out string failureReason)
    {
        failureReason = "unknown";

        if (chunkAudioByChunkId is null)
        {
            failureReason = "chunk-audio artifact not loaded";
            return false;
        }

        if (!chunkAudioByChunkId.TryGetValue(chunk.ChunkId, out var entry))
        {
            failureReason = $"no chunk-audio entry for chunk id {chunk.ChunkId}";
            return false;
        }

        if (!IsChunkAudioEntryCompatible(chunk, entry, utteranceName))
        {
            failureReason = "chunk-audio entry metadata is incompatible (chunk id/timing/utterance mismatch)";
            return false;
        }

        if (!File.Exists(entry.WavPath))
        {
            failureReason = $"chunk-audio WAV file does not exist ({entry.WavPath})";
            return false;
        }

        try
        {
            var info = new FileInfo(entry.WavPath);
            if (info.Length == 0)
            {
                failureReason = $"chunk-audio WAV file is empty ({entry.WavPath})";
                return false;
            }

            File.Copy(entry.WavPath, destinationWavPath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to reuse ASR chunk audio {Path} for chunk {ChunkId}: {Message}",
                entry.WavPath, chunk.ChunkId, ex.Message);
            failureReason = $"failed to copy chunk-audio WAV ({ex.Message})";
            return false;
        }
    }

    private static IReadOnlyList<string> TokenizeLabText(string labText)
    {
        if (string.IsNullOrWhiteSpace(labText))
        {
            return Array.Empty<string>();
        }

        return labText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();
    }

}
