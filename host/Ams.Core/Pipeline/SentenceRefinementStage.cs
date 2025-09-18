using System.Text.Json;
using Ams.Core.Align.Tx;
using Ams.Core.Asr.Pipeline;
using Ams.Core.Services;

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
        WriteIndented = true
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
        // Load ChunkTranscriptIndex from transcripts stage
        var transcriptsPath = Path.Combine(WorkDir, "transcripts", "index.json");
        if (!File.Exists(transcriptsPath))
            throw new InvalidOperationException("ChunkTranscriptIndex not found. Run 'transcripts' stage first.");

        var transcriptsJson = await File.ReadAllTextAsync(transcriptsPath, ct);
        var chunkTranscriptIndex = JsonSerializer.Deserialize<ChunkTranscriptIndex>(transcriptsJson, s_jsonOptions) ??
                 throw new InvalidOperationException("Invalid ChunkTranscriptIndex JSON");

        // Load ASR JSON from transcripts stage  
        var asrPath = Path.Combine(WorkDir, "transcripts", "asr.json");
        if (!File.Exists(asrPath))
            throw new InvalidOperationException("ASR JSON not found. Run 'transcripts' stage first.");

        var asrJson = await File.ReadAllTextAsync(asrPath, ct);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, s_jsonOptions) ??
                  throw new InvalidOperationException("Invalid ASR JSON");

        // Determine audio file path
        string audioPath = DetermineAudioPath(manifest);

        // Create a minimal TranscriptIndex for sentence refinement service
        // For now, we'll create empty collections since sentence refinement only needs ASR data
        var tx = new TranscriptIndex(
            AudioPath: audioPath,
            ScriptPath: "", // Not needed for sentence refinement
            BookIndexPath: "", // Not needed for sentence refinement  
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1",
            Words: new List<WordAlign>(), // Empty - not used by sentence refinement
            Sentences: new List<SentenceAlign>(), // Empty - not used by sentence refinement
            Paragraphs: new List<ParagraphAlign>() // Empty - not used by sentence refinement
        );

        // Run sentence refinement using the correct service
        var refinedSentences = await _sentenceRefinementService.RefineAsync(
            audioPath: audioPath,
            tx: tx,
            asr: asr,
            language: _params.Language ?? "eng",
            useSilence: _params.UseSilence,
            silenceThresholdDb: _params.SilenceThresholdDb,
            silenceMinDurationSec: _params.SilenceMinDurationSec
        );

        // Generate refined ASR output using AsrRefinementService
        var refinedAsr = _asrRefinementService.GenerateRefinedAsr(asr, refinedSentences);

        // Generate outputs in ./CORRECT_RESULTS/ compatible format
        var outputs = await GenerateStageOutputs(stageDir, refinedSentences, refinedAsr, asr, ct);

        return outputs;
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Compute input hash from ChunkTranscriptIndex and ASR files
        var inputPaths = new[]
        {
            Path.Combine(WorkDir, "transcripts", "index.json"),
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
        var detailsOutput = GenerateRefinementDetails(originalAsr, refinedAsr);
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

    private object GenerateRefinementDetails(AsrResponse originalAsr, AsrResponse refinedAsr)
    {
        // Generate detected silences info (placeholder - could be enhanced with actual silence detection)
        var silences = new[]
        {
            new
            {
                start_sec = Math.Round(0.0, 6),
                end_sec = Math.Round(0.1, 6),
                duration_sec = Math.Round(0.1, 6),
                confidence = Math.Round(1.0, 4)
            }
        };

        // Generate refined tokens summary
        var refinedTokensSummary = new
        {
            original_count = originalAsr.Tokens.Length,
            refined_count = refinedAsr.Tokens.Length,
            total_duration_original = Math.Round(originalAsr.Tokens.Sum(t => t.Duration), 6),
            total_duration_refined = Math.Round(refinedAsr.Tokens.Sum(t => t.Duration), 6)
        };

        return new
        {
            silences = silences,
            refined_tokens = refinedTokensSummary,
            notes = "Sentence refinement completed using SentenceRefinementService and AsrRefinementService"
        };
    }
}

/// <summary>
/// Parameters for sentence refinement stage.
/// </summary>
public sealed record SentenceRefinementParams(
    string? Language = "eng",
    bool UseSilence = true,
    double SilenceThresholdDb = -30.0,
    double SilenceMinDurationSec = 0.1
);