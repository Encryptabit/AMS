using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Align;
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

        var sectionRange = SentenceRefinementPreparation.TryLoadSectionWordRange(WorkDir, bookIndex);
        if (sectionRange is { } range)
        {
            Console.WriteLine($"[refine] using section window words {range.Start}..{range.End}");
        }

        var (scopedBookIndex, transcriptIndex, mapping) = SentenceRefinementPreparation.BuildTranscriptArtifacts(
            bookIndex,
            asr,
            audioPath,
            bookIndexPath,
            sectionRange);

        var chunkAlignments = await SentenceRefinementPreparation.LoadChunkAlignmentsAsync(
            Path.Combine(WorkDir, "align-chunks", "chunks"),
            s_jsonOptions,
            ct);
        var chapterAlignmentIndex = ChapterAlignmentIndex.Build(chunkAlignments, mapping);

        var silenceEvents = _params.UseSilence
            ? await SentenceRefinementPreparation.LoadSilencesAsync(
                Path.Combine(WorkDir, "timeline", "silence.json"),
                s_jsonOptions,
                ct)
            : Array.Empty<SilenceEvent>();

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
        var outputs = await GenerateStageOutputs(stageDir, scopedBookIndex, refinedSentences, refinedAsr, asr, ct);

        return outputs;
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var inputPaths = new[]
        {
            Path.Combine(WorkDir, "book.index.json"),
            Path.Combine(WorkDir, "transcripts", "asr.json"),
            Path.Combine(WorkDir, "anchors", "anchors.json"),
            Path.Combine(WorkDir, "align-chunks", "chunks"),
            Path.Combine(WorkDir, "timeline", "silence.json")
        };

        var inputContents = new List<string>();
        foreach (var path in inputPaths)
        {
            if (Directory.Exists(path))
            {
                inputContents.AddRange(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .Select(File.ReadAllText));
            }
            else if (File.Exists(path))
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
            { "SentenceRefinementPreparation", "1.0.0" },
            { "FFmpeg", "system" },
            { "Aeneas", "system" }
        };

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private string DetermineAudioPath(ManifestV2 manifest)
    {
        var candidatePaths = new[]
        {
            manifest.Input.Path,
            Path.Combine(WorkDir, "audio.wav"),
            Path.Combine(WorkDir, Path.GetFileName(manifest.Input.Path))
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
        BookIndex scopedBookIndex,
        IReadOnlyList<SentenceRefined> refinedSentences,
        AsrResponse refinedAsr,
        AsrResponse originalAsr,
        CancellationToken ct)
    {
        const int defaultSampleRate = 44100;
        int sampleRate = defaultSampleRate;

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

        var sentencesOutput = GenerateSentencesJson(scopedBookIndex, refinedSentences, sampleRate);
        var sentencesPath = Path.Combine(stageDir, "sentences.json");
        await File.WriteAllTextAsync(sentencesPath, JsonSerializer.Serialize(sentencesOutput, s_jsonOptions), ct);

        var refinedAsrPath = Path.Combine(stageDir, "refined.asr.json");
        await File.WriteAllTextAsync(refinedAsrPath, JsonSerializer.Serialize(refinedAsr, s_jsonOptions), ct);

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

    private object GenerateSentencesJson(BookIndex scopedBookIndex, IReadOnlyList<SentenceRefined> refinedSentences, int sampleRate)
    {
        var words = scopedBookIndex.Words;

        static string ExtractText(BookWord[] bookWords, int start, int end)
        {
            if (start < 0 || end < start || start >= bookWords.Length)
            {
                return string.Empty;
            }

            end = Math.Min(end, bookWords.Length - 1);
            return string.Join(" ", bookWords.Skip(start).Take(end - start + 1).Select(w => w.Text));
        }

        var sentences = refinedSentences.Select((sentence, index) => new
        {
            start_frame = (int)Math.Round(sentence.Start * sampleRate, 0),
            end_frame = (int)Math.Round(sentence.End * sampleRate, 0),
            start_sec = Math.Round(sentence.Start, 6),
            end_sec = Math.Round(sentence.End, 6),
            text = ExtractText(words, sentence.StartWordIdx, sentence.EndWordIdx),
            startwordidx = sentence.StartWordIdx,
            endwordidx = sentence.EndWordIdx,
            conf = Math.Round(sentence.HasFragment ? 1.0 : 0.6, 4)
        }).ToArray();

        return new
        {
            sr = sampleRate,
            sentences,
            details = new
            {
                silences = Array.Empty<object>(),
                notes = "Generated by SentenceRefinementStage"
            },
            refined_asr = new
            {
                modelVersion = "processed",
                tokens = Array.Empty<object>()
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














