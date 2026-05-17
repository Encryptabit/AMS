using System.Text;
using Ams.Core.Asr;
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
/// Lab content prefers hydrate word-level book tokens constrained by ASR word
/// timing inside each chunk. This keeps text/audio proportional at chunk
/// boundaries while preserving book-text lexical truth. If word-timed extraction
/// is insufficient, the builder falls back to sentence overlap heuristics.
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
    private const double WordTimingEdgeToleranceSec = 0.03;

    private const double ChunkAudioTimingToleranceSec = 0.05;

    // MFA's text reader treats a leading U+FEFF as part of the first word, turning
    // "chapter" into "﻿chapter" -- which becomes OOV/<unk> and squashes the
    // alignment of the following words. Lab files must be BOM-free.
    private static readonly Encoding LabFileEncoding =
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Builds per-chunk wav and lab files under <paramref name="corpusDirectory"/>.
    /// </summary>
    /// <param name="audioBuffer">Full chapter audio buffer.</param>
    /// <param name="chunkPlan">Shared chunk plan document from ChunkPlanningService.</param>
    /// <param name="hydrate">Hydrated transcript with sentence-level BookText and timing.</param>
    /// <param name="corpusDirectory">Target directory for utterance corpus assets.</param>
    /// <param name="maxConsecutiveDelRun">Maximum consecutive Del-op book-word run to splice
    /// back into the lab via book canonical text. Longer runs are dropped.</param>
    /// <returns>A <see cref="ChunkCorpusResult"/> with the list of emitted utterances.</returns>
    internal static ChunkCorpusResult Build(
        AudioBuffer audioBuffer,
        ChunkPlanDocument chunkPlan,
        HydratedTranscript hydrate,
        string corpusDirectory,
        ChunkAudioDocument? chunkAudio = null,
        bool requireAsrChunkAudio = false,
        AsrResponse? asr = null,
        int maxConsecutiveDelRun = 3)
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
        int wordTimedChunks = 0;
        int sentenceTimedChunks = 0;
        int nearestSentenceFallbackChunks = 0;
        IReadOnlyList<string>? previousLabTokens = null;

        // Pre-index word midpoints sorted by time for O(W log W + C) lookup
        // instead of O(C * W) rescanning per chunk. Del-op book words within
        // tolerance get spliced in with synthetic midpoints so chapter-heading
        // drops ("Chapter Five" -> Whisper "V.") don't leave the lab missing words.
        List<PreIndexedWord>? preIndexedWords = null;
        if (asr is not null && hydrate.Words.Count > 0 && asr.Tokens.Length > 0)
        {
            preIndexedWords = BuildPreIndexedWords(hydrate.Words, asr, maxConsecutiveDelRun);
        }

        for (int i = 0; i < chunkPlan.Chunks.Count; i++)
        {
            var chunk = chunkPlan.Chunks[i];
            var uttName = FormatUtteranceName(i);
            var usedFallback = false;

            // Prefer word-level mapping using pre-indexed midpoints for O(log W + hits)
            // per chunk instead of O(W) rescanning.
            var labText = preIndexedWords is { Count: > 0 }
                ? BuildLabTextFromPreIndexedWords(preIndexedWords, chunk.StartSec, chunk.EndSec)
                : null;

            if (!string.IsNullOrWhiteSpace(labText))
            {
                wordTimedChunks++;
            }

            if (string.IsNullOrWhiteSpace(labText))
            {
                // Fallback: sentence overlap mapping
                labText = BuildLabText(sentences, chunk.StartSec, chunk.EndSec);
                if (!string.IsNullOrWhiteSpace(labText))
                {
                    sentenceTimedChunks++;
                }
            }

            if (string.IsNullOrWhiteSpace(labText))
            {
                // Fallback: expand to nearest sentence window
                labText = BuildLabTextWithFallback(sentences, chunk.StartSec, chunk.EndSec, i);
                usedFallback = labText is not null;
                if (usedFallback)
                {
                    expandedChunks++;
                    nearestSentenceFallbackChunks++;
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
                if (!audioBuffer.TrySliceClamped(chunkStart, chunkEnd, out var slice))
                {
                    skippedNoAudio++;
                    Log.Debug(
                        "Chunk {ChunkId} has no audio after buffer slice " +
                        "(startSec={StartSec:F2}, endSec={EndSec:F2}); skipping",
                        chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                    continue;
                }

                AudioProcessor.EncodeWav(wavPath, slice);
            }

            // Write LAB
            var labPath = Path.Combine(corpusDirectory, uttName + ".lab");
            File.WriteAllText(labPath, labText, LabFileEncoding);

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
            "boundary-dedupe: {DedupedChunks} chunks/{DedupedTokens} tokens; " +
            "lab-source: word-timed={WordTimed}, sentence-overlap={SentenceTimed}, nearest-fallback={NearestFallback})",
            utterances.Count, chunkPlan.Chunks.Count, skippedNoText, skippedNoAudio, expandedChunks, reusedChunkAudio,
            boundaryTrimmedChunks, boundaryTrimmedTokens,
            wordTimedChunks, sentenceTimedChunks, nearestSentenceFallbackChunks);

        return new ChunkCorpusResult(corpusDirectory, utterances);
    }

    /// <summary>
    /// Rebuilds the corpus artifacts (utt-NNNN.wav, utt-NNNN.lab) for a specific subset of
    /// chunks. Used by C-tier scoped recovery: when MFA flags only a few chunks as misaligned,
    /// the orchestrator re-ASRs them, splices the new tokens into asr.json, then calls this
    /// to regenerate just those chunks' lab/wav files instead of cleaning the whole corpus.
    /// <para>
    /// Returns the FULL utterance list (rebuilt + preserved). Preserved chunks have their
    /// existing wav/lab files on disk left untouched; their UtteranceEntry is reconstructed
    /// from the chunk plan's time bounds and the deterministic file naming. Boundary dedupe
    /// uses the previous chunk's existing lab tokens (read from disk) for context, so chunks
    /// at the edge of the rebuild scope still align with their neighbors.
    /// </para>
    /// </summary>
    internal static ChunkCorpusResult RebuildScoped(
        AudioBuffer audioBuffer,
        ChunkPlanDocument chunkPlan,
        HydratedTranscript hydrate,
        string corpusDirectory,
        IReadOnlyList<int> chunkIndices,
        ChunkAudioDocument? chunkAudio = null,
        bool requireAsrChunkAudio = false,
        AsrResponse? asr = null,
        int maxConsecutiveDelRun = 3)
    {
        ArgumentNullException.ThrowIfNull(audioBuffer);
        ArgumentNullException.ThrowIfNull(chunkPlan);
        ArgumentNullException.ThrowIfNull(hydrate);
        ArgumentException.ThrowIfNullOrWhiteSpace(corpusDirectory);
        ArgumentNullException.ThrowIfNull(chunkIndices);

        if (chunkIndices.Count == 0)
        {
            throw new ArgumentException(
                "chunkIndices must contain at least one chunk index.", nameof(chunkIndices));
        }

        var indicesToRebuild = new HashSet<int>();
        foreach (var idx in chunkIndices)
        {
            if (idx < 0 || idx >= chunkPlan.Chunks.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chunkIndices),
                    $"Chunk index {idx} is out of bounds [0, {chunkPlan.Chunks.Count}).");
            }
            indicesToRebuild.Add(idx);
        }

        Directory.CreateDirectory(corpusDirectory);

        var sentences = hydrate.Sentences;
        var chunkAudioByChunkId = BuildChunkAudioLookup(chunkAudio);
        if (requireAsrChunkAudio && chunkPlan.Chunks.Count > 0 && chunkAudioByChunkId is null)
        {
            throw new InvalidOperationException(
                "ASR chunk audio is required for chunked MFA, but no chunk-audio artifact was available.");
        }

        // Pre-index word midpoints for word-timed lab text (same as Build).
        List<PreIndexedWord>? preIndexedWords = null;
        if (asr is not null && hydrate.Words.Count > 0 && asr.Tokens.Length > 0)
        {
            preIndexedWords = BuildPreIndexedWords(hydrate.Words, asr, maxConsecutiveDelRun);
        }

        var utterances = new List<UtteranceEntry>(chunkPlan.Chunks.Count);
        IReadOnlyList<string>? previousLabTokens = null;
        var rebuiltCount = 0;
        var preservedCount = 0;
        var skippedRebuiltCount = 0;

        // Removes stale .wav/.lab for a scoped chunk whose rebuild failed. Keeping them around
        // would let MFA align consume artifacts that don't match the current ASR/hydrate state
        // (the very reason recovery was triggered in the first place).
        void DeleteStaleArtifacts(string wavPath, string labPath, int chunkId)
        {
            foreach (var path in new[] { wavPath, labPath })
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(
                        "Scoped rebuild: failed to delete stale artifact {Path} for chunk {ChunkId}: {Message}",
                        path, chunkId, ex.Message);
                }
            }
        }

        for (int i = 0; i < chunkPlan.Chunks.Count; i++)
        {
            var chunk = chunkPlan.Chunks[i];
            var uttName = FormatUtteranceName(i);
            var wavPath = Path.Combine(corpusDirectory, uttName + ".wav");
            var labPath = Path.Combine(corpusDirectory, uttName + ".lab");

            if (!indicesToRebuild.Contains(i))
            {
                // Preserve: leave files on disk untouched. Reconstruct the UtteranceEntry from
                // the plan's time bounds and update previousLabTokens for the dedupe context of
                // any subsequent rebuilt chunk.
                if (File.Exists(wavPath) && File.Exists(labPath))
                {
                    utterances.Add(new UtteranceEntry(
                        ChunkId: chunk.ChunkId,
                        UtteranceName: uttName,
                        WavPath: wavPath,
                        LabPath: labPath,
                        ChunkStartSec: chunk.StartSec,
                        ChunkEndSec: chunk.EndSec));
                    try
                    {
                        var existingLab = File.ReadAllText(labPath, Encoding.UTF8);
                        previousLabTokens = TokenizeLabText(existingLab);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(
                            "Scoped rebuild: failed to read existing lab for chunk {ChunkId} ({Path}): {Message}",
                            chunk.ChunkId, labPath, ex.Message);
                        previousLabTokens = null;
                    }
                    preservedCount++;
                }
                else
                {
                    Log.Debug(
                        "Scoped rebuild: chunk {ChunkId} preserved but artifact files missing; entry not emitted",
                        chunk.ChunkId);
                    previousLabTokens = null;
                }
                continue;
            }

            // Rebuild this chunk: lab text resolution mirrors Build's 3-tier fallback.
            var labText = preIndexedWords is { Count: > 0 }
                ? BuildLabTextFromPreIndexedWords(preIndexedWords, chunk.StartSec, chunk.EndSec)
                : null;
            if (string.IsNullOrWhiteSpace(labText))
            {
                labText = BuildLabText(sentences, chunk.StartSec, chunk.EndSec);
            }
            if (string.IsNullOrWhiteSpace(labText))
            {
                labText = BuildLabTextWithFallback(sentences, chunk.StartSec, chunk.EndSec, i);
            }

            if (string.IsNullOrWhiteSpace(labText))
            {
                skippedRebuiltCount++;
                Log.Warn(
                    "Scoped rebuild: chunk {ChunkId} ({StartSec:F2}s-{EndSec:F2}s) produced no usable lab text; removing stale artifacts",
                    chunk.ChunkId, chunk.StartSec, chunk.EndSec);
                DeleteStaleArtifacts(wavPath, labPath, chunk.ChunkId);
                previousLabTokens = null;
                continue;
            }

            var labTokens = TokenizeLabText(labText);
            if (labTokens.Count == 0)
            {
                skippedRebuiltCount++;
                Log.Warn(
                    "Scoped rebuild: chunk {ChunkId} produced no tokenized lab text; removing stale artifacts",
                    chunk.ChunkId);
                DeleteStaleArtifacts(wavPath, labPath, chunk.ChunkId);
                previousLabTokens = null;
                continue;
            }

            if (previousLabTokens is { Count: > 0 })
            {
                var overlap = FindBoundaryTokenOverlap(previousLabTokens, labTokens);
                if (overlap >= MinBoundaryOverlapTokensForTrim)
                {
                    labTokens = labTokens.Skip(overlap).ToList();
                }
            }

            if (labTokens.Count < MinLabTokenCount)
            {
                skippedRebuiltCount++;
                Log.Warn(
                    "Scoped rebuild: chunk {ChunkId} has too few lab tokens after boundary dedupe; removing stale artifacts",
                    chunk.ChunkId);
                DeleteStaleArtifacts(wavPath, labPath, chunk.ChunkId);
                previousLabTokens = null;
                continue;
            }

            labText = string.Join(' ', labTokens);

            if (TryCopyPreSlicedChunkAudio(chunk, uttName, wavPath, chunkAudioByChunkId, out var reuseFailureReason))
            {
                // Pre-sliced ASR chunk audio reused — no FFmpeg trim needed.
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
                if (!audioBuffer.TrySliceClamped(chunkStart, chunkEnd, out var slice))
                {
                    skippedRebuiltCount++;
                    Log.Warn(
                        "Scoped rebuild: chunk {ChunkId} has no audio after buffer slice; removing stale artifacts",
                        chunk.ChunkId);
                    DeleteStaleArtifacts(wavPath, labPath, chunk.ChunkId);
                    previousLabTokens = null;
                    continue;
                }

                AudioProcessor.EncodeWav(wavPath, slice);
            }

            File.WriteAllText(labPath, labText, LabFileEncoding);

            utterances.Add(new UtteranceEntry(
                ChunkId: chunk.ChunkId,
                UtteranceName: uttName,
                WavPath: wavPath,
                LabPath: labPath,
                ChunkStartSec: chunk.StartSec,
                ChunkEndSec: chunk.EndSec));

            previousLabTokens = labTokens;
            rebuiltCount++;
        }

        Log.Info(
            "Scoped corpus rebuild complete: rebuilt {Rebuilt}/{Requested} (skipped {Skipped}), preserved {Preserved}, total utterances {Total}",
            rebuiltCount, indicesToRebuild.Count, skippedRebuiltCount, preservedCount, utterances.Count);

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
    /// Builds lab text from hydrate word mappings whose ASR token timing midpoint
    /// falls inside the chunk time range. Prefers book canonical for Match/Sub so
    /// MFA aligns against book truth; splices Del-op book words back in when their
    /// consecutive run length is within <paramref name="maxConsecutiveDelRun"/>.
    /// </summary>
    internal static string? BuildLabTextFromWordTiming(
        IReadOnlyList<HydratedWord> words,
        AsrResponse asr,
        double chunkStartSec,
        double chunkEndSec,
        int maxConsecutiveDelRun = 3)
    {
        ArgumentNullException.ThrowIfNull(words);
        ArgumentNullException.ThrowIfNull(asr);

        if (words.Count == 0 || asr.Tokens.Length == 0 || chunkEndSec <= chunkStartSec)
        {
            return null;
        }

        var preIndexed = BuildPreIndexedWords(words, asr, maxConsecutiveDelRun);
        return preIndexed.Count == 0
            ? null
            : BuildLabTextFromPreIndexedWords(preIndexed, chunkStartSec, chunkEndSec);
    }

    private static IReadOnlyList<string> ResolveAlignmentLexemeParts(HydratedWord word)
    {
        ArgumentNullException.ThrowIfNull(word);

        if (IsInsertOperation(word.Op))
        {
            // Insert: book has no equivalent, narrator added a word.
            // Filler insertions are dropped to keep the lab clean.
            if (string.Equals(word.Reason, "filler", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<string>();
            }

            return ExtractPreferredPronunciationParts(word.AsrWord);
        }

        // Match, Sub, Del: prefer book canonical for MFA forced alignment.
        // MFA aligns text -> audio; the book is the source of truth so
        // chapter-heading drops ("Chapter Five" Whisper-transcribed as "V.")
        // align as the book pronunciation rather than as the ASR's spoken form.
        // Fall back to the ASR word only when the book word produces no
        // pronouncable parts (defensive guard for empty/punctuation-only entries).
        var canonicalParts = ExtractPreferredPronunciationParts(word.BookWord);
        if (canonicalParts.Count > 0)
        {
            return canonicalParts;
        }

        return ExtractPreferredPronunciationParts(word.AsrWord);
    }

    /// <summary>
    /// Builds the pre-indexed word list with two passes that don't depend on
    /// <paramref name="hydrateWords"/> being in book order. Production hydrate
    /// emits anchor ops first then DP ops, so list-order ≠ book-order — any
    /// algorithm that walks the list to find a Del's "previous word" picks the
    /// wrong neighbor and synthesizes a midpoint far from the actual deletion.
    /// <para>Pass 1: emit non-Del words with their ASR midpoint and build a
    /// BookIdx → midpoint anchor map. Pass 2: collect Del words sorted by
    /// BookIdx, group runs of book-adjacent BookIdx values, and splice canonical
    /// pronunciation for runs ≤ <paramref name="maxConsecutiveDelRun"/>. Each
    /// run's prev/next neighbor is the closest anchor by BookIdx, and its
    /// synthetic midpoint is interpolated between those neighbors' ASR midpoints.
    /// Longer runs are dropped wholesale (narrator-skipped passages).</para>
    /// </summary>
    private static List<PreIndexedWord> BuildPreIndexedWords(
        IReadOnlyList<HydratedWord> hydrateWords,
        AsrResponse asr,
        int maxConsecutiveDelRun)
    {
        var result = new List<PreIndexedWord>(hydrateWords.Count);
        var clampedMax = Math.Max(0, maxConsecutiveDelRun);

        // Pass 1: emit every non-Del word with a valid AsrIdx, indexing
        // BookIdx → midpoint along the way for the Del-run neighbor lookup.
        // SortedList keeps anchor BookIdx values in ascending order so we can
        // find prev/next neighbors by binary search regardless of insert order.
        var anchorMidpointByBookIdx = new SortedList<int, double>();
        foreach (var word in hydrateWords)
        {
            if (IsDeleteOperation(word.Op))
            {
                continue;
            }

            if (word.AsrIdx is not int asrIdx || asrIdx < 0 || asrIdx >= asr.Tokens.Length)
            {
                continue;
            }

            var token = asr.Tokens[asrIdx];
            var midpoint = token.StartTime + Math.Max(0d, token.Duration) * 0.5d;

            var lexemeParts = ResolveAlignmentLexemeParts(word);
            if (lexemeParts.Count > 0)
            {
                result.Add(new PreIndexedWord(midpoint, asrIdx, word.BookIdx, lexemeParts));
            }

            // Index by BookIdx for Del-run neighbor lookup. Inserts have no
            // BookIdx and don't anchor a book position. If two ops share the
            // same BookIdx (shouldn't happen, but defensive), keep the first.
            if (word.BookIdx is int bookIdx && !anchorMidpointByBookIdx.ContainsKey(bookIdx))
            {
                anchorMidpointByBookIdx.Add(bookIdx, midpoint);
            }
        }

        if (clampedMax > 0)
        {
            // Pass 2: collect Del-op book words, sort by BookIdx (NOT list order),
            // group runs of consecutive BookIdx values, splice canonical for short runs.
            var dels = new List<HydratedWord>();
            foreach (var word in hydrateWords)
            {
                if (IsDeleteOperation(word.Op) && word.BookIdx is not null)
                {
                    dels.Add(word);
                }
            }
            dels.Sort((a, b) => a.BookIdx!.Value.CompareTo(b.BookIdx!.Value));

            int runStart = 0;
            while (runStart < dels.Count)
            {
                int runEnd = runStart;
                while (runEnd + 1 < dels.Count
                    && dels[runEnd + 1].BookIdx!.Value == dels[runEnd].BookIdx!.Value + 1)
                {
                    runEnd++;
                }
                int runLen = runEnd - runStart + 1;

                if (runLen > clampedMax)
                {
                    runStart = runEnd + 1;
                    continue;
                }

                int firstBookIdx = dels[runStart].BookIdx!.Value;
                int lastBookIdx = dels[runEnd].BookIdx!.Value;

                var (prevMidpoint, nextMidpoint) =
                    FindAnchorNeighbors(anchorMidpointByBookIdx, firstBookIdx, lastBookIdx);

                if (prevMidpoint is null && nextMidpoint is null)
                {
                    // Run with no anchorable neighbors. Skip; sentence-overlap
                    // fallback will produce lab text from BookText if needed.
                    runStart = runEnd + 1;
                    continue;
                }

                for (int j = 0; j < runLen; j++)
                {
                    var delWord = dels[runStart + j];
                    var canonicalParts = ExtractPreferredPronunciationParts(delWord.BookWord);
                    if (canonicalParts.Count == 0)
                    {
                        continue;
                    }

                    double mid;
                    if (prevMidpoint is double pm && nextMidpoint is double nm)
                    {
                        mid = pm + (nm - pm) * ((j + 1.0) / (runLen + 1.0));
                    }
                    else if (prevMidpoint is double pmOnly)
                    {
                        // Stagger after prev so book order is preserved within the run.
                        mid = pmOnly + 1e-3 * (j + 1);
                    }
                    else
                    {
                        var nmOnly = nextMidpoint!.Value;
                        mid = nmOnly - 1e-3 * (runLen - j);
                    }

                    // AsrIdx -1 is a sentinel for synthesized Del entries; consumers
                    // sort by MidpointSec, not AsrIdx, so this only flags provenance.
                    result.Add(new PreIndexedWord(mid, AsrIdx: -1, delWord.BookIdx, canonicalParts));
                }

                runStart = runEnd + 1;
            }
        }

        result.Sort((a, b) => a.MidpointSec.CompareTo(b.MidpointSec));
        return result;
    }

    /// <summary>
    /// Returns the closest anchor midpoints by BookIdx — the largest key &lt;
    /// <paramref name="firstBookIdx"/> (prev) and the smallest key &gt;
    /// <paramref name="lastBookIdx"/> (next). Either may be null if no anchor
    /// exists on that side. Caller passes anchor BookIdx values that bracket a
    /// Del run, so prev/next are always strictly outside the run.
    /// </summary>
    private static (double? Prev, double? Next) FindAnchorNeighbors(
        SortedList<int, double> anchorMidpointByBookIdx,
        int firstBookIdx,
        int lastBookIdx)
    {
        double? prev = null;
        double? next = null;
        foreach (var kvp in anchorMidpointByBookIdx)
        {
            if (kvp.Key < firstBookIdx)
            {
                prev = kvp.Value; // last write wins -> closest-below by BookIdx
            }
            else if (kvp.Key > lastBookIdx)
            {
                next = kvp.Value; // first hit beyond lastBookIdx
                break;
            }
        }

        return (prev, next);
    }

    private static IReadOnlyList<string> ExtractPreferredPronunciationParts(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return PronunciationHelper.ExtractPronunciationParts(value);
    }

    private static bool IsDeleteOperation(string? operation)
        => string.Equals(operation, nameof(AlignOp.Del), StringComparison.OrdinalIgnoreCase);

    private static bool IsInsertOperation(string? operation)
        => string.Equals(operation, nameof(AlignOp.Ins), StringComparison.OrdinalIgnoreCase);

    private static bool IsSubstitutionOperation(string? operation)
        => string.Equals(operation, nameof(AlignOp.Sub), StringComparison.OrdinalIgnoreCase);

    private sealed record AlignmentWordCandidate(
        double MidpointSec,
        int AsrIdx,
        int? BookIdx,
        IReadOnlyList<string> LexemeParts);

    /// <summary>
    /// Pre-computed word with its ASR token midpoint time, for sorted pre-indexing.
    /// </summary>
    private sealed record PreIndexedWord(
        double MidpointSec,
        int AsrIdx,
        int? BookIdx,
        IReadOnlyList<string> LexemeParts);

    /// <summary>
    /// Builds lab text from a pre-sorted list of word midpoints using binary search
    /// to find the start index, then scanning forward until midpoint exceeds chunk end.
    /// O(log W + hits) per chunk instead of O(W) rescanning.
    /// </summary>
    private static string? BuildLabTextFromPreIndexedWords(
        List<PreIndexedWord> sortedWords,
        double chunkStartSec,
        double chunkEndSec)
    {
        if (sortedWords.Count == 0 || chunkEndSec <= chunkStartSec)
        {
            return null;
        }

        // Binary search for the first word with midpoint >= chunkStartSec - tolerance
        double lowerBound = chunkStartSec - WordTimingEdgeToleranceSec;
        int lo = 0, hi = sortedWords.Count - 1;
        int startIndex = sortedWords.Count; // default: no match
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (sortedWords[mid].MidpointSec >= lowerBound)
            {
                startIndex = mid;
                hi = mid - 1;
            }
            else
            {
                lo = mid + 1;
            }
        }

        double upperBound = chunkEndSec + WordTimingEdgeToleranceSec;
        var candidates = new List<AlignmentWordCandidate>();

        for (int i = startIndex; i < sortedWords.Count; i++)
        {
            var pw = sortedWords[i];
            if (pw.MidpointSec >= upperBound)
                break;

            candidates.Add(new AlignmentWordCandidate(pw.MidpointSec, pw.AsrIdx, pw.BookIdx, pw.LexemeParts));
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        // Sort by MidpointSec so synthesized Del entries (AsrIdx=-1) interleave
        // with real ASR-anchored words at their interpolated positions instead
        // of clustering at the front of the lab.
        candidates.Sort((a, b) => a.MidpointSec.CompareTo(b.MidpointSec));
        var seenBookIdx = new HashSet<int>();
        var parts = new List<string>(candidates.Count);

        foreach (var candidate in candidates)
        {
            if (candidate.BookIdx is int bookIdx && !seenBookIdx.Add(bookIdx))
            {
                continue;
            }

            foreach (var part in candidate.LexemeParts)
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

    private static bool IsWithinChunk(double tokenMidpointSec, double chunkStartSec, double chunkEndSec)
    {
        return tokenMidpointSec >= chunkStartSec - WordTimingEdgeToleranceSec &&
               tokenMidpointSec < chunkEndSec + WordTimingEdgeToleranceSec;
    }

}
