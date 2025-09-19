using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Align.Anchors;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Align.Anchors;
using Ams.Core.Align.Tx;
using Ams.Core.Asr.Pipeline;
using Ams.Core.Services;
using Ams.Core.Util;

namespace Ams.Core.Pipeline;

/// <summary>
/// Pipeline stage that replaces RefineStage with SentenceRefinementService-based implementation.
/// Produces ./CORRECT_RESULTS-compatible artifacts including sentences.json, refined.asr.json, and refinement-details.json.
/// </summary>
public sealed class SentenceRefinementStage : StageRunner
{
    private readonly SentenceRefinementService _sentenceRefinementService;
    private readonly AsrRefinementService _asrRefinementService;
    private readonly SentenceRefinementParams _params;

    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SentenceRefinementStage(
        string workDir,
        SentenceRefinementParams parameters,
        SentenceRefinementService? sentenceRefinementService = null,
        AsrRefinementService? asrRefinementService = null)
        : base(workDir, "refine")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _sentenceRefinementService = sentenceRefinementService ?? new SentenceRefinementService();
        _asrRefinementService = asrRefinementService ?? new AsrRefinementService();
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var bookIndexPath = Path.Combine(WorkDir, "book.index.json");
        if (!File.Exists(bookIndexPath))
            throw new InvalidOperationException("BookIndex not found. Run 'book-index' stage first.");

        var bookIndexJson = await File.ReadAllTextAsync(bookIndexPath, ct);
        var bookIndex = JsonSerializer.Deserialize<BookIndex>(bookIndexJson, s_jsonOptions) ??
                        throw new InvalidOperationException("Invalid BookIndex JSON");

        var asrPath = Path.Combine(WorkDir, "transcripts", "asr.json");
        if (!File.Exists(asrPath))
            throw new InvalidOperationException("ASR JSON not found. Run 'transcripts' stage first.");

        var asrJson = await File.ReadAllTextAsync(asrPath, ct);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, s_jsonOptions) ??
                  throw new InvalidOperationException("Invalid ASR JSON");

        var audioPath = DetermineAudioPath(manifest);

        var sectionRange = LoadSectionWordRange(bookIndex);
        if (sectionRange is { } range)
        {
            Console.WriteLine($"[refine] using section window words {range.Start}..{range.End}");
        }

        var (transcriptIndex, mapping) = BuildTranscriptArtifacts(bookIndex, asr, audioPath, bookIndexPath, sectionRange);

        var chunkAlignments = await LoadChunkAlignmentsAsync(ct);
        var chapterAlignmentIndex = ChapterAlignmentIndex.Build(chunkAlignments, mapping);

        var silences = await LoadSilenceTimelineAsync(ct);
        var silenceEvents = _params.UseSilence ? silences : Array.Empty<SilenceEvent>();

        Console.WriteLine($"[refine] sentences={transcriptIndex.Sentences.Count}, fragments={chapterAlignmentIndex.Fragments.Count}, silences={silenceEvents.Count}");

        var context = new SentenceRefinementContext(
            Fragments: chapterAlignmentIndex.Fragments.ToDictionary(
                kvp => kvp.Key.ToString(CultureInfo.InvariantCulture),
                kvp => kvp.Value),
            Silences: silenceEvents,
            MinTailSec: Math.Max(0.05, _params.MinTailSec),
            MaxSnapAheadSec: _params.MaxSnapAheadSec
        );

        var refinedSentences = await _sentenceRefinementService.RefineAsync(
            audioPath,
            transcriptIndex,
            asr,
            context,
            ct);

        var refinedAsr = _asrRefinementService.GenerateRefinedAsr(asr, refinedSentences);
        var outputs = await GenerateStageOutputs(stageDir, refinedSentences, refinedAsr, asr, ct);

        return outputs;
    }

    private (int Start, int End)? LoadSectionWordRange(BookIndex bookIndex)
    {
        var anchorsPath = Path.Combine(WorkDir, "anchors", "anchors.json");
        if (!File.Exists(anchorsPath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(anchorsPath);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("Stats", out var statsElement))
            {
                return null;
            }

            if (!statsElement.TryGetProperty("Section", out var sectionProperty))
            {
                return null;
            }

            var sectionName = sectionProperty.GetString();
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                return null;
            }

            var sections = bookIndex.Sections;
            if (sections is null || sections.Length == 0)
            {
                return null;
            }

            SectionRange? match = sections.FirstOrDefault(s => string.Equals(s.Title, sectionName, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                Console.WriteLine($"[refine] section '{sectionName}' not found in book index; using full book");
                return null;
            }

            return (match.StartWord, match.EndWord);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[refine] failed to read anchors/anchors.json: {ex.Message}");
            return null;
        }
    }

    private static (TranscriptIndex Transcript, TokenMapping Mapping) BuildTranscriptArtifacts(
        BookIndex bookIndex,
        AsrResponse asr,
        string audioPath,
        string bookIndexPath,
        (int Start, int End)? sectionWordRange)
    {
        var defaultStart = bookIndex.Words.Length > 0 ? bookIndex.Words[0].WordIndex : 0;
        var defaultEnd = bookIndex.Words.Length > 0 ? bookIndex.Words[^1].WordIndex : int.MaxValue;
        var (startWord, endWord) = sectionWordRange ?? (defaultStart, defaultEnd);

        var filteredWords = bookIndex.Words
            .Where(w => w.WordIndex >= startWord && w.WordIndex <= endWord)
            .ToArray();
        if (filteredWords.Length == 0)
        {
            filteredWords = bookIndex.Words;
        }

        var filteredSentences = bookIndex.Sentences
            .Where(s => s.Start >= startWord && s.End <= endWord)
            .ToArray();
        if (filteredSentences.Length == 0)
        {
            filteredSentences = bookIndex.Sentences;
        }

        var filteredParagraphs = bookIndex.Paragraphs?
            .Where(p => p.Start >= startWord && p.End <= endWord)
            .ToArray();
        if (filteredParagraphs is { Length: 0 })
        {
            filteredParagraphs = bookIndex.Paragraphs;
        }

        var filteredSections = bookIndex.Sections?
            .Where(s => s.StartWord >= startWord && s.EndWord <= endWord)
            .ToArray();

        var wordOffset = filteredWords.Length > 0 ? filteredWords[0].WordIndex : 0;
        var sentenceOffset = filteredSentences.Length > 0 ? filteredSentences[0].Index : 0;
        var paragraphOffset = filteredParagraphs is { Length: > 0 } ? filteredParagraphs[0].Index : 0;

        static int NormalizeIndex(int value, int offset)
            => value >= 0 ? value - offset : value;

        var remappedWords = filteredWords
            .Select((w, idx) => new BookWord(
                Text: w.Text,
                WordIndex: idx,
                SentenceIndex: NormalizeIndex(w.SentenceIndex, sentenceOffset),
                ParagraphIndex: NormalizeIndex(w.ParagraphIndex, paragraphOffset),
                SectionIndex: w.SectionIndex >= 0 ? 0 : w.SectionIndex))
            .ToArray();

        var remappedSentences = filteredSentences
            .Select((s, idx) => new BookSentence(
                Index: idx,
                Start: s.Start - wordOffset,
                End: s.End - wordOffset))
            .ToArray();

        var remappedParagraphs = filteredParagraphs is null
            ? Array.Empty<BookParagraph>()
            : filteredParagraphs
                .Select((p, idx) => new BookParagraph(
                    Index: idx,
                    Start: p.Start - wordOffset,
                    End: p.End - wordOffset,
                    Kind: p.Kind,
                    Style: p.Style))
                .ToArray();

        var remappedSections = filteredSections is null
            ? null
            : filteredSections
                .Select((s, idx) => new SectionRange(
                    Id: idx,
                    Title: s.Title,
                    Level: s.Level,
                    Kind: s.Kind,
                    StartWord: s.StartWord - wordOffset,
                    EndWord: s.EndWord - wordOffset,
                    StartParagraph: NormalizeIndex(s.StartParagraph, paragraphOffset),
                    EndParagraph: NormalizeIndex(s.EndParagraph, paragraphOffset)))
                .ToArray();

        var scopedBookIndex = bookIndex with
        {
            Words = remappedWords,
            Sentences = remappedSentences,
            Paragraphs = remappedParagraphs,
            Sections = remappedSections
        };

        var bookView = AnchorPreprocessor.BuildBookView(scopedBookIndex);
        var asrView = AnchorPreprocessor.BuildAsrView(asr);

        var policy = new AnchorPolicy(Stopwords: StopwordSets.EnglishPlusDomain);
        var anchorResult = AnchorPipeline.ComputeAnchors(
            scopedBookIndex,
            asr,
            policy,
            new SectionDetectOptions(false, 0),
            includeWindows: true);

        var windows = anchorResult.Windows ?? new List<(int bLo, int bHi, int aLo, int aHi)>
        {
            (0, bookView.Tokens.Count, 0, asrView.Tokens.Count)
        };

        var equivalences = new Dictionary<string, string>(StringComparer.Ordinal);
        var fillers = new HashSet<string>(StringComparer.Ordinal) { "uh", "um" };

        var ops = TranscriptAligner.AlignWindows(
            bookView.Tokens,
            asrView.Tokens,
            windows,
            equivalences,
            fillers);

        var wordAligns = new List<WordAlign>(ops.Count);
        foreach (var (bi, aj, op, reason, score) in ops)
        {
            int? bookIdx = bi.HasValue && bi.Value < bookView.FilteredToOriginalWord.Count
                ? bookView.FilteredToOriginalWord[bi.Value]
                : null;
            int? asrIdx = aj.HasValue && aj.Value < asrView.FilteredToOriginalToken.Count
                ? asrView.FilteredToOriginalToken[aj.Value]
                : null;
            wordAligns.Add(new WordAlign(bookIdx, asrIdx, op, reason, score));
        }

        var bookSentences = scopedBookIndex.Sentences
            .Select(s => (s.Index, s.Start, s.End))
            .ToList();

        var bookParagraphs = scopedBookIndex.Paragraphs?.Select(p => (p.Index, p.Start, p.End)).ToList()
                            ?? new List<(int Id, int Start, int End)>();

        var (sentenceAligns, paragraphAligns) = TranscriptAligner.Rollup(
            wordAligns,
            bookSentences,
            bookParagraphs);

        var transcript = new TranscriptIndex(
            audioPath,
            bookIndex.SourceFile,
            bookIndexPath,
            DateTime.UtcNow,
            "v1.0",
            wordAligns,
            sentenceAligns,
            paragraphAligns);

        var sentenceRanges = sentenceAligns
            .Select(s => new SentenceTokenRange(
                s.Id,
                s.ScriptRange?.Start,
                s.ScriptRange?.End))
            .ToList();

        var mapping = new TokenMapping(asr, sentenceRanges, wordAligns);
        return (transcript, mapping);
    }

    private async Task<List<ChunkAlignment>> LoadChunkAlignmentsAsync(CancellationToken ct)
    {
        var alignmentDir = Path.Combine(WorkDir, "align-chunks", "chunks");
        if (!Directory.Exists(alignmentDir))
            throw new InvalidOperationException("Chunk alignments not found. Run 'align-chunks' stage first.");

        var files = Directory.EnumerateFiles(alignmentDir, "*.aeneas.json", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            throw new InvalidOperationException("No chunk alignment fragments found under align-chunks/chunks.");

        var result = new List<ChunkAlignment>(files.Count);
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var alignment = JsonSerializer.Deserialize<ChunkAlignment>(json, s_jsonOptions);
            if (alignment is null)
                throw new InvalidOperationException($"Invalid chunk alignment JSON: {Path.GetFileName(file)}");

            result.Add(alignment);
        }

        return result;
    }

    private async Task<IReadOnlyList<SilenceEvent>> LoadSilenceTimelineAsync(CancellationToken ct)
    {
        var timelinePath = Path.Combine(WorkDir, "timeline", "silence.json");
        if (!File.Exists(timelinePath))
            return Array.Empty<SilenceEvent>();

        try
        {
            var timelineJson = await File.ReadAllTextAsync(timelinePath, ct);
            var timeline = JsonSerializer.Deserialize<SilenceTimelineV2>(timelineJson, s_jsonOptions);
            if (timeline?.Events is null || timeline.Events.Count == 0)
                return Array.Empty<SilenceEvent>();

            return timeline.Events.OrderBy(e => e.Start).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[refine] failed to parse timeline/silence.json: {ex.Message}");
            return Array.Empty<SilenceEvent>();
        }
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Compute input hash from BookIndex and ASR files
        var inputPaths = new[]
        {
            Path.Combine(WorkDir, "book.index.json"),
            Path.Combine(WorkDir, "transcripts", "asr.json")
        };

        var inputContents = new List<string>();
        foreach (var path in inputPaths)
        {
            if (File.Exists(path))
            {
                inputContents.Add(await File.ReadAllTextAsync(path, ct));
            }
        }

        var inputHash = ComputeHash(string.Join("\n", inputContents));
        var paramsHash = ComputeHash(SerializeParams(_params));

        var toolVersions = new Dictionary<string, string>
        {
            { "SentenceRefinementService", "1.0.0" },
            { "AsrRefinementService", "1.0.0" },
            { "BookIndexToTranscriptTransformer", "1.0.0" },
            { "FFmpeg", "system" },
            { "Aeneas", "system" }
        };

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private string DetermineAudioPath(ManifestV2 manifest)
    {
        // Try multiple possible audio file locations
        var candidatePaths = new[]
        {
            manifest.Input.Path, // Manifest audio path
            Path.Combine(WorkDir, "audio.wav"), // Common convention
            Path.Combine(WorkDir, Path.GetFileName(manifest.Input.Path)), // Workdir + filename
        };

        foreach (var path in candidatePaths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                return path;
            }
        }

        throw new InvalidOperationException($"Audio file not found. Tried paths: {string.Join(", ", candidatePaths)}");
    }

    private async Task<Dictionary<string, string>> GenerateStageOutputs(
        string stageDir,
        IReadOnlyList<SentenceRefined> refinedSentences,
        AsrResponse refinedAsr,
        AsrResponse originalAsr,
        CancellationToken ct)
    {
        // Determine sample rate - assume 44100 if not available from audio analysis
        const int defaultSampleRate = 44100;
        int sampleRate = defaultSampleRate;

        // Try to get actual sample rate from timeline/silence.json if available
        var timelinePath = Path.Combine(WorkDir, "timeline", "silence.json");
        if (File.Exists(timelinePath))
        {
            try
            {
                var timelineJson = await File.ReadAllTextAsync(timelinePath, ct);
                var timeline = JsonSerializer.Deserialize<JsonElement>(timelineJson);
                if (timeline.TryGetProperty("sr", out var srProperty))
                {
                    sampleRate = srProperty.GetInt32();
                }
            }
            catch
            {
                // Use default if timeline parsing fails
            }
        }

        // Generate sentences.json - ./CORRECT_RESULTS compatible format
        var sentencesOutput = GenerateSentencesJson(refinedSentences, sampleRate);
        var sentencesPath = Path.Combine(stageDir, "sentences.json");
        await File.WriteAllTextAsync(sentencesPath, JsonSerializer.Serialize(sentencesOutput, s_jsonOptions), ct);

        // Generate refined.asr.json
        var refinedAsrPath = Path.Combine(stageDir, "refined.asr.json");
        await File.WriteAllTextAsync(refinedAsrPath, JsonSerializer.Serialize(refinedAsr, s_jsonOptions), ct);

        // Generate refinement-details.json with detected silences and refined tokens
        var detailsOutput = GenerateRefinementDetails(refinedSentences, originalAsr, refinedAsr);
        var detailsPath = Path.Combine(stageDir, "refinement-details.json");
        await File.WriteAllTextAsync(detailsPath, JsonSerializer.Serialize(detailsOutput, s_jsonOptions), ct);

        return new Dictionary<string, string>
        {
            { "sentences", sentencesPath },
            { "refined_asr", refinedAsrPath },
            { "refinement_details", detailsPath }
        };
    }

    private object GenerateSentencesJson(IReadOnlyList<SentenceRefined> refinedSentences, int sampleRate)
    {
        var sentences = refinedSentences.Select((sentence, index) => new
        {
            start_frame = (int)Math.Round(sentence.Start * sampleRate, 0),
            end_frame = (int)Math.Round(sentence.End * sampleRate, 0),
            start_sec = Math.Round(sentence.Start, 6), // 6 decimal places precision
            end_sec = Math.Round(sentence.End, 6),     // 6 decimal places precision
            text = $"Sentence {index + 1}", // Placeholder - could be enhanced with actual text
            startwordidx = sentence.StartWordIdx,
            endwordidx = sentence.EndWordIdx,
            conf = Math.Round(1.0, 4) // Default confidence - 4 decimal places precision
        }).ToArray();

        return new
        {
            sr = sampleRate,
            sentences = sentences,
            details = new
            {
                silences = new object[0], // Placeholder for silence detection results
                notes = "Generated by SentenceRefinementStage"
            },
            refined_asr = new
            {
                modelVersion = "processed",
                tokens = new object[0] // Will be populated from refined ASR
            }
        };
    }

    private RefinementDetails GenerateRefinementDetails(
        IReadOnlyList<SentenceRefined> refinedSentences,
        AsrResponse originalAsr,
        AsrResponse refinedAsr)
    {
        var refinedTokenDetails = new List<RefinedTokenDetail>(refinedAsr.Tokens.Length);
        var originalTokens = originalAsr.Tokens;

        if (refinedAsr.Tokens.Length > 0)
        {
            var lastOriginalIndex = Math.Max(0, originalTokens.Length - 1);
            var originalIndex = 0;

            foreach (var token in refinedAsr.Tokens)
            {
                var mappedOriginal = originalTokens.Length == 0
                    ? null
                    : originalTokens[Math.Min(originalIndex, lastOriginalIndex)];

                refinedTokenDetails.Add(new RefinedTokenDetail(
                    StartTime: Precision.RoundToMicroseconds(token.StartTime),
                    Duration: Precision.RoundToMicroseconds(token.Duration),
                    Word: token.Word,
                    OriginalStartTime: mappedOriginal is null ? 0.0 : Precision.RoundToMicroseconds(mappedOriginal.StartTime),
                    OriginalDuration: mappedOriginal is null ? 0.0 : Precision.RoundToMicroseconds(mappedOriginal.Duration),
                    Confidence: 1.0
                ));

                if (originalIndex < lastOriginalIndex)
                {
                    originalIndex++;
                }
            }
        }

        var detectedSilences = new List<SilenceInfo>();
        for (var i = 0; i < refinedSentences.Count - 1; i++)
        {
            var current = refinedSentences[i];
            var next = refinedSentences[i + 1];
            var gap = Precision.RoundToMicroseconds(next.Start - current.End);
            if (gap > 0.0)
            {
                var start = Precision.RoundToMicroseconds(current.End);
                var end = Precision.RoundToMicroseconds(next.Start);
                detectedSilences.Add(new SilenceInfo(
                    Start: start,
                    End: end,
                    Duration: Precision.RoundToMicroseconds(end - start),
                    Confidence: 1.0
                ));
            }
        }

        var totalDurationSeconds = refinedTokenDetails.Count == 0
            ? 0.0
            : Precision.RoundToMicroseconds(refinedTokenDetails[^1].StartTime + refinedTokenDetails[^1].Duration);

        return new RefinementDetails(
            ModelVersion: refinedAsr.ModelVersion,
            RefinedTokens: refinedTokenDetails,
            DetectedSilences: detectedSilences,
            TotalDurationSeconds: totalDurationSeconds
        );
    }
}

/// <summary>
/// Parameters for sentence refinement stage.
/// </summary>
public sealed record SentenceRefinementParams(
    string? Language = "eng",
    bool UseSilence = true,
    double SilenceThresholdDb = -30.0,
    double SilenceMinDurationSec = 0.1,
    double MinTailSec = 0.05,
    double MaxSnapAheadSec = 1.0
);





