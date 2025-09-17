using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Models;

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

        // Prefer window-align if available; fallback to align-chunks (ASR-informed variant)
        List<RefinedSentence> rawSentences;
        var winIndexPath = Path.Combine(WorkDir, "window-align", "index.json");
        if (File.Exists(winIndexPath))
        {
            var winIndexJson = await File.ReadAllTextAsync(winIndexPath, ct);
            var winIndex = JsonSerializer.Deserialize<WindowAlignIndex>(winIndexJson) ?? throw new InvalidOperationException("Invalid window-align index");
            var winDir = Path.Combine(WorkDir, "window-align", "windows");
            rawSentences = await LoadWindowAlignmentsAsSentencesAsync(winDir, winIndex.WindowIds, winIndex.WindowToJsonMap, ct);
        }
        else
        {
            var alignmentsDir = Path.Combine(WorkDir, "align-chunks", "chunks");
            var transcriptsDir = Path.Combine(WorkDir, "transcripts", "raw");
            var chunkAlignments = await LoadChunkAlignmentsAsync(alignmentsDir, chunkIndex.Chunks, ct);
            var tokenIndex = await LoadChunkTokenIndexAsync(transcriptsDir, chunkIndex.Chunks, ct);
            rawSentences = ConvertAlignmentsToSentences_AsrInformed(chunkAlignments, tokenIndex);
        }

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

    private async Task<List<RefinedSentence>> LoadWindowAlignmentsAsSentencesAsync(string winDir, IReadOnlyList<string> windowIds, IReadOnlyDictionary<string,string> map, CancellationToken ct)
    {
        var sentences = new List<RefinedSentence>();
        int sentenceCounter = 0;
        foreach (var id in windowIds)
        {
            if (!map.TryGetValue(id, out var file)) continue;
            var path = Path.Combine(winDir, file);
            if (!File.Exists(path)) continue;
            var json = await File.ReadAllTextAsync(path, ct);
            var entry = JsonSerializer.Deserialize<WindowAlignEntry>(json);
            if (entry == null) continue;
            foreach (var f in entry.Fragments)
            {
                var sId = $"sentence_{sentenceCounter:D3}";
                sentences.Add(new RefinedSentence(
                    sId,
                    entry.OffsetSec + f.Begin,
                    entry.OffsetSec + f.End,
                    null,
                    null,
                    "aeneas-window+pre-snap"
                ));
                sentenceCounter++;
            }
        }
        return sentences.OrderBy(s => s.Start).ToList();
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

    private sealed record TokenTime(double Start, double End);

    private async Task<Dictionary<string, List<TokenTime>>> LoadChunkTokenIndexAsync(string transcriptsDir, List<ChunkInfo> chunks, CancellationToken ct)
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

    private List<RefinedSentence> ConvertAlignmentsToSentences_AsrInformed(List<ChunkAlignment> chunkAlignments, Dictionary<string, List<TokenTime>> tokenIndex)
    {
        var sentences = new List<RefinedSentence>();
        int sentenceCounter = 0;

        foreach (var alignment in chunkAlignments.OrderBy(a => a.OffsetSec))
        {
            tokenIndex.TryGetValue(alignment.ChunkId, out var toks);
            var tokens = toks ?? new List<TokenTime>();

            for (int i = 0; i < alignment.Fragments.Count; i++)
            {
                var fragment = alignment.Fragments[i];
                var sChapter = fragment.Begin + alignment.OffsetSec;
                var eChapterAeneas = fragment.End + alignment.OffsetSec;

                double eChapterAsr = eChapterAeneas;
                // Find ASR tokens inside the fragment window (chunk-relative)
                var within = tokens.Where(t => t.Start >= fragment.Begin - 1e-3 && t.End <= fragment.End + 1e-3).ToList();
                if (within.Count > 0)
                {
                    var maxEnd = within.Max(t => t.End);
                    eChapterAsr = Math.Max(eChapterAeneas, alignment.OffsetSec + maxEnd);
                }

                var sentenceId = $"sentence_{sentenceCounter:D3}";
                var sentence = new RefinedSentence(
                    sentenceId,
                    sChapter,
                    eChapterAsr, // provisional end from ASR tokens
                    null,
                    null,
                    "aeneas+asr-dur+pre-snap"
                );
                sentences.Add(sentence);
                sentenceCounter++;
            }
        }

        return sentences.OrderBy(s => s.Start).ToList();
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
                .Where(s => s.Start >= sentence.End - 1e-6 && s.Start < nextSentenceStart - 1e-6)
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
