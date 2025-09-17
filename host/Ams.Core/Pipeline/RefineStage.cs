using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Models;

namespace Ams.Core.Pipeline;

public class RefineStage : StageRunner
{
    private readonly RefinementParams _params;

    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
        { PropertyNameCaseInsensitive = true };

    public RefineStage(
        string workDir,
        RefinementParams parameters)
        : base(workDir, "refine")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir,
        CancellationToken ct)
    {
        // Load silence timeline
        var timelinePath = Path.Combine(WorkDir, "timeline", "silence.json");
        if (!File.Exists(timelinePath))
            throw new InvalidOperationException("Silence timeline not found. Run 'timeline' stage first.");

        // Load chunk index and plan
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        var planPath = Path.Combine(WorkDir, "plan", "windows.json");

        if (!File.Exists(chunkIndexPath) || !File.Exists(planPath))
            throw new InvalidOperationException("Chunk index or plan not found. Run 'chunks' stage first.");

        var timelineJson = await File.ReadAllTextAsync(timelinePath, ct);
        var timeline = JsonSerializer.Deserialize<SilenceTimelineV2>(timelineJson) ??
                       throw new InvalidOperationException("Invalid silence timeline");

        var chunkIndexJson = await File.ReadAllTextAsync(chunkIndexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(chunkIndexJson) ??
                         throw new InvalidOperationException("Invalid chunk index");

        var planJson = await File.ReadAllTextAsync(planPath, ct);
        var plan = JsonSerializer.Deserialize<WindowPlanV2>(planJson) ??
                   throw new InvalidOperationException("Invalid window plan");

        Console.WriteLine($"Refining sentence boundaries using snap-to-silence.start rule...");
        Console.WriteLine(
            $"Silence threshold: {_params.SilenceThresholdDb}dB, Min duration: {_params.MinSilenceDurSec}s");

        // Build alignment-derived sentences from chunk-level alignment
        var alignmentsDir = Path.Combine(WorkDir, "align-chunks", "chunks");
        var transcriptsDir = Path.Combine(WorkDir, "transcripts", "raw");
        var chunkAlignments = await LoadChunkAlignmentsAsync(alignmentsDir, chunkIndex.Chunks, ct);
        var tokenIndex = await LoadChunkTokenIndexAsync(transcriptsDir, chunkIndex.Chunks, ct);
        var mergedWords = await LoadMergedWordsAsync(ct);
        var anchorMap = await LoadAnchorTokenMapAsync(mergedWords, ct);
        var rawSentences =
            ConvertAlignmentsToSentences_AsrInformed(chunkAlignments, tokenIndex, mergedWords, anchorMap);

        Console.WriteLine($"Found {rawSentences.Count} raw sentences from alignment");

        // Apply snap-to-silence.start refinement rule
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, timeline.Events, _params);

        Console.WriteLine($"Refined {refinedSentences.Count} sentences");

        // Save refined sentences
        var sentencesPath = Path.Combine(stageDir, "sentences.json");
        var sentencesJson =
            JsonSerializer.Serialize(refinedSentences, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(sentencesPath, sentencesJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJsonOutput = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJsonOutput, ct);

        // Create refinement log
        var logData = new
        {
            InputSentences = rawSentences.Count,
            OutputSentences = refinedSentences.Count,
            SnapRule = "earliest silence.start >= aeneas.end and < next.start",
            SilenceThresholdDb = _params.SilenceThresholdDb,
            MinSilenceDurSec = _params.MinSilenceDurSec,
            SilenceEvents = timeline.Events.Count,
            GeneratedAt = DateTime.UtcNow
        };

        var logPath = Path.Combine(stageDir, "refinement.log.json");
        var logJson = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(logPath, logJson, ct);

        Console.WriteLine($"Sentence refinement completed: {refinedSentences.Count} sentences");

        return new Dictionary<string, string>
        {
            ["sentences"] = "sentences.json",
            ["log"] = "refinement.log.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Include timeline, chunks, and alignments in input hash
        var timelinePath = Path.Combine(WorkDir, "timeline", "silence.json");
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");

        var timelineHash = "";
        var chunkHash = "";
        var alignmentHash = "";

        if (File.Exists(timelinePath))
        {
            var timelineContent = await File.ReadAllTextAsync(timelinePath, ct);
            timelineHash = ComputeHash(timelineContent);
        }

        if (File.Exists(chunkIndexPath))
        {
            var chunkContent = await File.ReadAllTextAsync(chunkIndexPath, ct);
            chunkHash = ComputeHash(chunkContent);
        }

        // Hash all alignment files
        var alignmentsDir = Path.Combine(WorkDir, "align-chunks", "chunks");
        if (Directory.Exists(alignmentsDir))
        {
            var alignmentFiles = Directory.GetFiles(alignmentsDir, "*.aeneas.json").OrderBy(f => f);
            var alignmentContents = new List<string>();
            foreach (var file in alignmentFiles)
            {
                var content = await File.ReadAllTextAsync(file, ct);
                alignmentContents.Add(content);
            }

            alignmentHash = ComputeHash(string.Join("", alignmentContents));
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + timelineHash + chunkHash + alignmentHash);

        return new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>());
    }

    private async Task<List<ChunkAlignment>> LoadChunkAlignmentsAsync(string alignmentsDir, List<ChunkInfo> chunks,
        CancellationToken ct)
    {
        var alignments = new List<ChunkAlignment>();

        foreach (var chunk in chunks)
        {
            var alignmentPath = Path.Combine(alignmentsDir, $"{chunk.Id}.aeneas.json");
            if (File.Exists(alignmentPath))
            {
                var alignmentJson = await File.ReadAllTextAsync(alignmentPath, ct);
                var alignment = JsonSerializer.Deserialize<ChunkAlignment>(alignmentJson);
                if (alignment != null)
                {
                    alignments.Add(alignment);
                }
            }
            else
            {
                Console.WriteLine($"Warning: Alignment file not found for chunk {chunk.Id}");
            }
        }

        return alignments;
    }

    private List<RefinedSentence> ConvertAlignmentsToSentences(List<ChunkAlignment> chunkAlignments)
    {
        var sentences = new List<RefinedSentence>();
        int sentenceCounter = 0;

        foreach (var alignment in chunkAlignments.OrderBy(a => a.OffsetSec))
        {
            for (int i = 0; i < alignment.Fragments.Count; i++)
            {
                var fragment = alignment.Fragments[i];
                var sentenceId = $"sentence_{sentenceCounter:D3}";

                var sentence = new RefinedSentence(
                    sentenceId,
                    fragment.Begin + alignment.OffsetSec, // Convert to chapter time
                    fragment.End + alignment.OffsetSec, // Convert to chapter time
                    null, // StartWordIdx not available from Aeneas
                    null, // EndWordIdx not available from Aeneas
                    "aeneas+pre-snap"
                );

                sentences.Add(sentence);
                sentenceCounter++;
            }
        }

        return sentences;
    }

    private sealed record TokenTime(double Start, double End);

    private sealed record AnchorTokenMap(
        int[] FilteredToOriginal,
        int[] OriginalToFiltered,
        int[] AnchorFilteredIndices);

    private async Task<Dictionary<string, List<TokenTime>>> LoadChunkTokenIndexAsync(string transcriptsDir,
        List<ChunkInfo> chunks, CancellationToken ct)
    {
        var map = new Dictionary<string, List<TokenTime>>(StringComparer.Ordinal);
        foreach (var ch in chunks)
        {
            var path = Path.Combine(transcriptsDir, $"{ch.Id}.json");
            if (!File.Exists(path)) continue;
            var json = await File.ReadAllTextAsync(path, ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            var list = new List<TokenTime>();
            if (doc.TryGetProperty("Words", out var words))
            {
                foreach (var w in words.EnumerateArray())
                {
                    double s = w.GetProperty("Start").GetDouble();
                    double e = w.GetProperty("End").GetDouble();
                    list.Add(new TokenTime(s, e));
                }
            }

            map[ch.Id] = list;
        }

        return map;
    }


    private async Task<List<TranscriptWord>> LoadMergedWordsAsync(CancellationToken ct)
    {
        var mergedPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(mergedPath))
            throw new InvalidOperationException("Missing transcripts/merged.json");

        var mergedJson = await File.ReadAllTextAsync(mergedPath, ct);
        var merged = JsonSerializer.Deserialize<MergedTranscript>(mergedJson, s_jsonOptions)
                     ?? throw new InvalidOperationException("Invalid merged transcript");

        return merged.Words?.OrderBy(w => w.Start).ToList() ?? new List<TranscriptWord>();
    }

    private async Task<AnchorTokenMap> LoadAnchorTokenMapAsync(List<TranscriptWord> mergedWords, CancellationToken ct)
    {
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath) || mergedWords.Count == 0)
        {
            return new AnchorTokenMap(Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>());
        }

        var anchorsJson = await File.ReadAllTextAsync(anchorsPath, ct);
        var anchors = JsonSerializer.Deserialize<AnchorsArtifact>(anchorsJson, s_jsonOptions)
                      ?? throw new InvalidOperationException("Invalid anchors artifact");

        var asrTokens = mergedWords
            .Select(w => new AsrToken(w.Start, Math.Max(0.0, w.End - w.Start), w.Word))
            .ToArray();
        var asrView = AnchorPreprocessor.BuildAsrView(new AsrResponse("merged/derived", asrTokens));

        var filteredToOriginal = asrView.FilteredToOriginalToken.ToArray();
        var originalToFiltered = Enumerable.Repeat(-1, mergedWords.Count).ToArray();
        for (int i = 0; i < filteredToOriginal.Length; i++)
        {
            var originalIndex = filteredToOriginal[i];
            if (originalIndex >= 0 && originalIndex < originalToFiltered.Length)
            {
                originalToFiltered[originalIndex] = i;
            }
        }

        var anchorFilteredIndices = anchors.Selected
            .Select(a => a.Ap)
            .Where(ap => ap >= 0)
            .OrderBy(ap => ap)
            .ToArray();

        return new AnchorTokenMap(filteredToOriginal, originalToFiltered, anchorFilteredIndices);
    }

    private sealed record MergedTranscript(List<TranscriptWord> Words);

    private List<RefinedSentence> ConvertAlignmentsToSentences_AsrInformed(List<ChunkAlignment> chunkAlignments,
        Dictionary<string, List<TokenTime>> tokenIndex, List<TranscriptWord> mergedWords, AnchorTokenMap anchorMap)
    {
        var sentences = new List<RefinedSentence>();
        if (mergedWords is null)
        {
            mergedWords = new List<TranscriptWord>();
        }

        var orderedWords = mergedWords;
        int sentenceCounter = 0;
        int wordCursor = 0;
        const double tokenEpsilon = 1e-4;

        foreach (var alignment in chunkAlignments.OrderBy(a => a.OffsetSec))
        {
            tokenIndex.TryGetValue(alignment.ChunkId, out var toks);
            var tokens = toks ?? new List<TokenTime>();

            foreach (var fragment in alignment.Fragments)
            {
                var sChapter = fragment.Begin + alignment.OffsetSec;
                var eChapterAeneas = fragment.End + alignment.OffsetSec;

                double eChapterAsr = eChapterAeneas;
                var within = tokens.Where(t => t.Start >= fragment.Begin - 1e-3 && t.End <= fragment.End + 1e-3)
                    .ToList();
                if (within.Count > 0)
                {
                    var maxEnd = within.Max(t => t.End);
                    eChapterAsr = Math.Max(eChapterAsr, alignment.OffsetSec + maxEnd);
                }

                while (wordCursor < orderedWords.Count && orderedWords[wordCursor].End <= sChapter + tokenEpsilon)
                {
                    wordCursor++;
                }

                int firstWordIndex = -1;
                int lastWordIndex = -1;
                int scanIndex = wordCursor;
                while (scanIndex < orderedWords.Count && orderedWords[scanIndex].Start < eChapterAsr + tokenEpsilon)
                {
                    if (firstWordIndex < 0 && orderedWords[scanIndex].End > sChapter - tokenEpsilon)
                    {
                        firstWordIndex = scanIndex;
                    }

                    if (orderedWords[scanIndex].End <= eChapterAsr + tokenEpsilon)
                    {
                        lastWordIndex = scanIndex;
                    }

                    scanIndex++;
                }

                int anchorStartIndex = ResolveAnchorAlignedWordIndex(anchorMap, firstWordIndex);
                int anchorEndIndex = ResolveAnchorAlignedWordIndex(anchorMap, lastWordIndex);

                if (anchorEndIndex >= 0 && anchorEndIndex < orderedWords.Count)
                {
                    eChapterAsr = Math.Min(eChapterAsr, orderedWords[anchorEndIndex].End);
                }

                if (lastWordIndex >= 0)
                {
                    wordCursor = Math.Max(wordCursor, lastWordIndex);
                }

                var sentenceId = $"sentence_{sentenceCounter:D3}";
                int? startWordIdx = anchorStartIndex >= 0
                    ? anchorStartIndex
                    : (firstWordIndex >= 0 ? firstWordIndex : null);
                int? endWordIdx = anchorEndIndex >= 0 ? anchorEndIndex : (lastWordIndex >= 0 ? lastWordIndex : null);

                if (startWordIdx is not null && endWordIdx is not null && startWordIdx > endWordIdx)
                {
                    startWordIdx = endWordIdx;
                }

                var source = anchorEndIndex >= 0 ? "anchor+asr" : "asr+next-word";

                var sentence = new RefinedSentence(
                    sentenceId,
                    sChapter,
                    eChapterAsr,
                    startWordIdx,
                    endWordIdx,
                    source
                );
                sentences.Add(sentence);
                sentenceCounter++;
            }
        }

        return sentences.OrderBy(s => s.Start).ToList();
    }


    private static int ResolveAnchorAlignedWordIndex(AnchorTokenMap anchorMap, int fallbackWordIndex)
    {
        if (fallbackWordIndex < 0)
            return -1;

        var originalToFiltered = anchorMap.OriginalToFiltered;
        if (originalToFiltered.Length == 0)
            return fallbackWordIndex;

        if (fallbackWordIndex >= originalToFiltered.Length)
        {
            fallbackWordIndex = originalToFiltered.Length - 1;
        }

        int filteredIndex = fallbackWordIndex >= 0 ? originalToFiltered[fallbackWordIndex] : -1;
        int probe = fallbackWordIndex;
        while (filteredIndex < 0 && probe >= 0)
        {
            filteredIndex = originalToFiltered[probe];
            if (filteredIndex >= 0)
            {
                fallbackWordIndex = probe;
                break;
            }

            probe--;
        }

        if (filteredIndex < 0)
            return fallbackWordIndex;

        var anchorFiltered = anchorMap.AnchorFilteredIndices;
        if (anchorFiltered.Length == 0)
            return fallbackWordIndex;

        int anchorIdx = BinarySearchLessOrEqual(anchorFiltered, filteredIndex);
        if (anchorIdx >= 0)
        {
            var filteredToOriginal = anchorMap.FilteredToOriginal;
            int filteredTokenIndex = anchorFiltered[anchorIdx];
            if (filteredTokenIndex >= 0 && filteredTokenIndex < filteredToOriginal.Length)
            {
                return filteredToOriginal[filteredTokenIndex];
            }
        }

        return fallbackWordIndex;
    }

    private static int BinarySearchLessOrEqual(int[] values, int target)
    {
        int lo = 0;
        int hi = values.Length - 1;
        int result = -1;
        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            if (values[mid] <= target)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }

    private List<RefinedSentence> ApplySnapToSilenceRule(
        List<RefinedSentence> rawSentences,
        List<SilenceEvent> silenceEvents,
        RefinementParams parameters)
    {
        if (rawSentences.Count == 0)
            return rawSentences;

        var qualifiedSilences = silenceEvents
            .Where(s => s.Duration >= parameters.MinSilenceDurSec)
            .OrderBy(s => s.Start)
            .ToList();

        Console.WriteLine(
            $"Using {qualifiedSilences.Count} qualified silence events (>= {parameters.MinSilenceDurSec}s)");

        const double CoverageSlackSec = 0.05; // allow 50ms tolerance when matching silence windows

        var refinedSentences = new List<RefinedSentence>(rawSentences.Count);

        for (int i = 0; i < rawSentences.Count; i++)
        {
            var sentence = rawSentences[i];
            var nextSentenceStart = i + 1 < rawSentences.Count ? rawSentences[i + 1].Start : double.MaxValue;
            var upperBound = nextSentenceStart - 1e-6;
            var lowerBound = sentence.Start;

            var clampedRawEnd = Math.Clamp(sentence.End, lowerBound, upperBound);
            var candidates = new List<(double Value, string Source, SilenceEvent? Silence)>
            {
                (clampedRawEnd, "aeneas+no-snap", null)
            };

            var coveringSilences = qualifiedSilences
                .Where(s =>
                    s.Start >= sentence.End - CoverageSlackSec && // CORRECT: silence must start after/at sentence end
                    s.Start < nextSentenceStart && // CORRECT: silence must start before next sentence
                    s.End < nextSentenceStart + CoverageSlackSec) // CORRECT: silence must end before next sentence
                .ToList();

            foreach (var silence in coveringSilences)
            {
                var startCandidate = Math.Clamp(silence.Start, lowerBound, upperBound);
                var endCandidate = Math.Clamp(silence.End, lowerBound, upperBound);

                if (startCandidate > lowerBound + 1e-6)
                {
                    candidates.Add((startCandidate, "aeneas+silence.start", silence));
                }

                if (endCandidate > lowerBound + 1e-6)
                {
                    candidates.Add((endCandidate, "aeneas+silence.end", silence));
                }
            }

            var chosen = candidates
                .OrderByDescending(c => c.Value)
                .ThenBy(c => c.Source switch // Then by preference
                {
                    "aeneas+silence.start" => 1, // Prefer silence start
                    "aeneas+silence.end" => 2, // Then silence end
                    "aeneas+no-snap" => 3, // Last resort: original
                    _ => 4
                })
                .First();

            var refinedEnd = chosen.Value;
            var refinedSource = chosen.Source;

            if (chosen.Silence is SilenceEvent selectedSilence)
            {
                Console.WriteLine(
                    $"Sentence {sentence.Id}: {sentence.End:F3}s -> {refinedEnd:F3}s ({refinedSource}) using silence [{selectedSilence.Start:F3}, {selectedSilence.End:F3}]");
            }
            else
            {
                Console.WriteLine($"Sentence {sentence.Id}: {sentence.End:F3}s -> {refinedEnd:F3}s ({refinedSource})");
            }

            var refinedSentence = sentence with
            {
                End = refinedEnd,
                Source = refinedSource
            };

            refinedSentences.Add(refinedSentence);
        }

        var finalSentences = EnforceConstraints(refinedSentences);
        return finalSentences;
    }

    private List<RefinedSentence> EnforceConstraints(List<RefinedSentence> sentences)
    {
        const double MinSentenceDuration = 0.01; // 50ms minimum
        var constrained = new List<RefinedSentence>();

        for (int i = 0; i < sentences.Count; i++)
        {
            var sentence = sentences[i];
            var nextSentence = i + 1 < sentences.Count ? sentences[i + 1] : null;

            // Ensure minimum duration
            var minEnd = sentence.Start + MinSentenceDuration;
            var adjustedEnd = Math.Max(sentence.End, minEnd);

            // Ensure no overlap with next sentence
            if (nextSentence != null && adjustedEnd >= nextSentence.Start)
            {
                adjustedEnd = Math.Max(nextSentence.Start - 0.020, minEnd);
                Console.WriteLine(
                    $"Sentence {sentence.Id}: adjusted end to avoid overlap: {sentence.End:F3}s -> {adjustedEnd:F3}s");
            }

            var constrainedSentence = sentence with { End = adjustedEnd };
            constrained.Add(constrainedSentence);
        }

        return constrained;
    }
}