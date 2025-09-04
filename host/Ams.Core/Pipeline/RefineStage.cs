using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public class RefineStage : StageRunner
{
    private readonly RefinementParams _params;

    public RefineStage(
        string workDir,
        RefinementParams parameters)
        : base(workDir, "refine")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
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
        var timeline = JsonSerializer.Deserialize<SilenceTimelineV2>(timelineJson) ?? throw new InvalidOperationException("Invalid silence timeline");

        var chunkIndexJson = await File.ReadAllTextAsync(chunkIndexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(chunkIndexJson) ?? throw new InvalidOperationException("Invalid chunk index");

        var planJson = await File.ReadAllTextAsync(planPath, ct);
        var plan = JsonSerializer.Deserialize<WindowPlanV2>(planJson) ?? throw new InvalidOperationException("Invalid window plan");

        Console.WriteLine($"Refining sentence boundaries using snap-to-silence.start rule...");
        Console.WriteLine($"Silence threshold: {_params.SilenceThresholdDb}dB, Min duration: {_params.MinSilenceDurSec}s");

        // Load all chunk alignments
        var alignmentsDir = Path.Combine(WorkDir, "align-chunks", "chunks");
        var chunkAlignments = await LoadChunkAlignmentsAsync(alignmentsDir, chunkIndex.Chunks, ct);

        // Convert chunk-relative alignment fragments to chapter-relative sentences
        var rawSentences = ConvertAlignmentsToSentences(chunkAlignments);

        Console.WriteLine($"Found {rawSentences.Count} raw sentences from alignment");

        // Apply snap-to-silence.start refinement rule
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, timeline.Events, _params);

        Console.WriteLine($"Refined {refinedSentences.Count} sentences");

        // Save refined sentences
        var sentencesPath = Path.Combine(stageDir, "sentences.json");
        var sentencesJson = JsonSerializer.Serialize(refinedSentences, new JsonSerializerOptions { WriteIndented = true });
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

    private async Task<List<ChunkAlignment>> LoadChunkAlignmentsAsync(string alignmentsDir, List<ChunkInfo> chunks, CancellationToken ct)
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
                    fragment.End + alignment.OffsetSec,   // Convert to chapter time
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

    private List<RefinedSentence> ApplySnapToSilenceRule(
        List<RefinedSentence> rawSentences,
        List<SilenceEvent> silenceEvents,
        RefinementParams parameters)
    {
        if (rawSentences.Count == 0)
            return rawSentences;

        // Filter silence events by our threshold parameters
        var qualifiedSilences = silenceEvents
            .Where(s => s.Duration >= parameters.MinSilenceDurSec)
            .OrderBy(s => s.Start)
            .ToList();

        Console.WriteLine($"Using {qualifiedSilences.Count} qualified silence events (>= {parameters.MinSilenceDurSec}s)");

        var refinedSentences = new List<RefinedSentence>();

        for (int i = 0; i < rawSentences.Count; i++)
        {
            var sentence = rawSentences[i];
            var nextSentenceStart = i + 1 < rawSentences.Count ? rawSentences[i + 1].Start : double.MaxValue;

            // BUSINESS RULE: Find earliest silence.start >= sentence.end and < nextSentence.start
            var candidateSilences = qualifiedSilences
                .Where(s => s.Start >= sentence.End && s.Start < nextSentenceStart)
                .OrderBy(s => s.Start)
                .ToList();

            double refinedEnd;
            string refinedSource;

            if (candidateSilences.Count > 0)
            {
                // Snap to earliest qualifying silence.start
                var targetSilence = candidateSilences.First();
                refinedEnd = targetSilence.Start;
                refinedSource = "aeneas+silence.start";
                
                Console.WriteLine($"Sentence {sentence.Id}: {sentence.End:F3}s -> {refinedEnd:F3}s (snapped to silence)");
            }
            else
            {
                // No qualifying silence found, keep original end
                refinedEnd = sentence.End;
                refinedSource = "aeneas+no-snap";
                
                Console.WriteLine($"Sentence {sentence.Id}: {sentence.End:F3}s (no qualifying silence)");
            }

            var refinedSentence = sentence with 
            { 
                End = refinedEnd, 
                Source = refinedSource 
            };

            refinedSentences.Add(refinedSentence);
        }

        // Final pass: ensure monotonic non-overlap and minimum length
        var finalSentences = EnforceConstraints(refinedSentences);

        return finalSentences;
    }

    private List<RefinedSentence> EnforceConstraints(List<RefinedSentence> sentences)
    {
        const double MinSentenceDuration = 0.05; // 50ms minimum
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
                adjustedEnd = Math.Max(nextSentence.Start - 0.001, minEnd);
                Console.WriteLine($"Sentence {sentence.Id}: adjusted end to avoid overlap: {sentence.End:F3}s -> {adjustedEnd:F3}s");
            }

            var constrainedSentence = sentence with { End = adjustedEnd };
            constrained.Add(constrainedSentence);
        }

        return constrained;
    }
}