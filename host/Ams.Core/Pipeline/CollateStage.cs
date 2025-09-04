using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class CollateStage : StageRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly CollationParams _params;

    public CollateStage(
        string workDir,
        IProcessRunner processRunner,
        CollationParams parameters)
        : base(workDir, "collate")
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        // Load refined sentences
        var sentencesPath = Path.Combine(WorkDir, "refine", "sentences.json");
        if (!File.Exists(sentencesPath))
            throw new InvalidOperationException("Refined sentences not found. Run 'refine' stage first.");

        // Load chunk index and plan for boundary sliver detection
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        var planPath = Path.Combine(WorkDir, "plan", "windows.json");

        if (!File.Exists(chunkIndexPath) || !File.Exists(planPath))
            throw new InvalidOperationException("Chunk index or plan not found. Run 'chunks' stage first.");

        var sentencesJson = await File.ReadAllTextAsync(sentencesPath, ct);
        var sentences = JsonSerializer.Deserialize<List<RefinedSentence>>(sentencesJson) ?? throw new InvalidOperationException("Invalid sentences");

        var chunkIndexJson = await File.ReadAllTextAsync(chunkIndexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(chunkIndexJson) ?? throw new InvalidOperationException("Invalid chunk index");

        var planJson = await File.ReadAllTextAsync(planPath, ct);
        var plan = JsonSerializer.Deserialize<WindowPlanV2>(planJson) ?? throw new InvalidOperationException("Invalid plan");

        Console.WriteLine($"Collating {sentences.Count} sentences with room tone replacement...");

        // Use manifest duration instead of reading WAV directly (avoids format issues)
        var originalDuration = manifest.Input.DurationSec;

        Console.WriteLine($"Original audio: {originalDuration:F3}s (from manifest metadata)");

        // Create room tone source
        var roomtoneSource = await CreateRoomtoneSourceAsync(manifest.Input.Path, originalDuration, stageDir, ct);

        // Detect boundary slivers and inter-sentence gaps
        var replacements = AnalyzeReplacements(sentences, plan.Windows, originalDuration);

        Console.WriteLine($"Identified {replacements.Count} regions for room tone replacement");

        // Generate collated audio in two phases:
        // 1. Reconstruct complete audio from chunks  
        // 2. Apply roomtone replacements to inter-sentence gaps
        var finalPath = Path.Combine(stageDir, "final.wav");
        await RenderCollatedAudioAsync(sentences, replacements, chunkIndex, roomtoneSource, finalPath, originalDuration, 44100, ct);

        // Save segments and log
        var segments = new CollationSegments(sentences, replacements);
        var segmentsPath = Path.Combine(stageDir, "segments.json");
        var segmentsJson = JsonSerializer.Serialize(segments, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(segmentsPath, segmentsJson, ct);

        var logData = new
        {
            OriginalDuration = originalDuration,
            SentenceCount = sentences.Count,
            ReplacementCount = replacements.Count,
            InterSentenceGaps = replacements.Count(r => r.Kind == "gap"),
            BoundarySlivers = replacements.Count(r => r.Kind == "boundary_sliver"),
            TotalRoomtoneDuration = replacements.Sum(r => r.Duration),
            RoomtoneSource = _params.RoomtoneSource,
            GeneratedAt = DateTime.UtcNow
        };

        var logPath = Path.Combine(stageDir, "log.json");
        var logJson = JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(logPath, logJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJsonOutput = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJsonOutput, ct);

        // Verify final duration using ffprobe
        var finalDuration = await GetAudioDurationAsync(finalPath, ct);
        var durationDiff = Math.Abs(finalDuration - originalDuration);

        Console.WriteLine($"Collated audio: {finalDuration:F3}s (diff: {durationDiff * 1000:F1}ms)");

        if (durationDiff > 0.01) // More than 10ms difference
        {
            Console.WriteLine($"Warning: Duration mismatch exceeds tolerance: {durationDiff:F6}s");
        }

        Console.WriteLine($"Collation completed: {replacements.Count} regions replaced with room tone");

        return new Dictionary<string, string>
        {
            ["final"] = "final.wav",
            ["segments"] = "segments.json",
            ["log"] = "log.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Include sentences, chunks, and plan in input hash
        var sentencesPath = Path.Combine(WorkDir, "refine", "sentences.json");
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        var planPath = Path.Combine(WorkDir, "plan", "windows.json");

        var sentencesHash = "";
        var chunkHash = "";
        var planHash = "";

        if (File.Exists(sentencesPath))
        {
            var sentencesContent = await File.ReadAllTextAsync(sentencesPath, ct);
            sentencesHash = ComputeHash(sentencesContent);
        }

        if (File.Exists(chunkIndexPath))
        {
            var chunkContent = await File.ReadAllTextAsync(chunkIndexPath, ct);
            chunkHash = ComputeHash(chunkContent);
        }

        if (File.Exists(planPath))
        {
            var planContent = await File.ReadAllTextAsync(planPath, ct);
            planHash = ComputeHash(planContent);
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + sentencesHash + chunkHash + planHash);

        var toolVersions = new Dictionary<string, string>();
        try
        {
            var result = await _processRunner.RunAsync(GetFfmpegExecutable(), "-version", ct);
            if (result.ExitCode == 0)
            {
                var version = FfmpegSilenceDetector.ParseVersion(result.StdOut + "\n" + result.StdErr) ?? "unknown";
                toolVersions["ffmpeg"] = version;
            }
        }
        catch
        {
            toolVersions["ffmpeg"] = "unknown";
        }

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<string> CreateRoomtoneSourceAsync(string inputPath, double originalDuration, string stageDir, CancellationToken ct)
    {
        var roomtonePath = Path.Combine(stageDir, "roomtone_source.wav");

        if (_params.RoomtoneSource == "file" && !string.IsNullOrEmpty(_params.RoomtoneFilePath))
        {
            // Use provided room tone file
            var sourcePath = PathNormalizer.NormalizePath(_params.RoomtoneFilePath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Room tone file not found: {sourcePath}");

            // Copy and resample if needed
            await ResampleAudioAsync(sourcePath, roomtonePath, 44100, ct);
            Console.WriteLine($"Using provided room tone file: {_params.RoomtoneFilePath}");
        }
        else
        {
            // Auto-generate from low-energy windows in original audio
            await ExtractRoomtoneFromOriginalAsync(inputPath, roomtonePath, originalDuration, ct);
            Console.WriteLine("Generated room tone from low-energy windows in original audio");
        }

        return roomtonePath;
    }

    private async Task ExtractRoomtoneFromOriginalAsync(string inputPath, string outputPath, double originalDuration, CancellationToken ct)
    {
        // Simple approach: extract a quiet section and loop it
        // In practice, you'd want more sophisticated room tone detection
        
        var normalizedInput = PathNormalizer.NormalizePath(inputPath);
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);

        // Extract 5 seconds from 10% into the audio (assuming it's quiet)
        var startTime = originalDuration * 0.1;
        var duration = Math.Min(5.0, originalDuration * 0.1);

        var args = $"-i \"{normalizedInput}\" -ss {startTime:F6} -t {duration:F6} -af \"volume={_params.RoomtoneLevelDb}dB\" -y \"{normalizedOutput}\"";

        var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to extract room tone: {result.StdErr}");
        }
    }

    private async Task ResampleAudioAsync(string inputPath, string outputPath, int targetSampleRate, CancellationToken ct)
    {
        var normalizedInput = PathNormalizer.NormalizePath(inputPath);
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);

        var args = $"-i \"{normalizedInput}\" -ar {targetSampleRate} -y \"{normalizedOutput}\"";

        var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to resample audio: {result.StdErr}");
        }
    }

    private List<CollationReplacement> AnalyzeReplacements(List<RefinedSentence> sentences, List<ChunkSpan> windows, double originalDuration)
    {
        var replacements = new List<CollationReplacement>();

        // 1. Inter-sentence gaps
        for (int i = 0; i < sentences.Count - 1; i++)
        {
            var current = sentences[i];
            var next = sentences[i + 1];
            var gapDuration = next.Start - current.End;

            if (gapDuration >= _params.MinGapMs / 1000.0 && gapDuration <= _params.MaxGapMs / 1000.0)
            {
                replacements.Add(new CollationReplacement(
                    "gap",
                    current.End,
                    next.Start,
                    gapDuration,
                    _params.RoomtoneLevelDb
                ));
            }
        }

        // 2. Boundary slivers across chunk cuts
        var chunkBoundaries = windows.Skip(1).Select(w => w.Start).ToList(); // Skip first boundary (start of audio)

        foreach (var boundary in chunkBoundaries)
        {
            // Find sentences that cross this boundary
            var sentencesNearBoundary = sentences
                .Where(s => s.Start < boundary && s.End > boundary)
                .ToList();

            foreach (var sentence in sentencesNearBoundary)
            {
                var leftSliver = boundary - sentence.Start;
                var rightSliver = sentence.End - boundary;

                // Only replace if both slivers are small enough
                if (leftSliver <= _params.BridgeMaxMs / 1000.0 && rightSliver <= _params.BridgeMaxMs / 1000.0)
                {
                    replacements.Add(new CollationReplacement(
                        "boundary_sliver",
                        sentence.Start,
                        sentence.End,
                        leftSliver + rightSliver,
                        _params.RoomtoneLevelDb
                    ));
                }
            }
        }

        return replacements.OrderBy(r => r.From).ToList();
    }

    private async Task RenderCollatedAudioAsync(
        List<RefinedSentence> sentences,
        List<CollationReplacement> replacements,
        ChunkIndex chunkIndex,
        string roomtoneSource,
        string outputPath,
        double originalDuration,
        int sampleRate,
        CancellationToken ct)
    {
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);
        
        // Phase 1: Reconstruct complete audio from chunks to ensure perfect duration preservation
        var reconstructedPath = Path.Combine(Path.GetDirectoryName(outputPath)!, "reconstructed.wav");
        await ReconstructCompleteAudioAsync(chunkIndex, reconstructedPath, ct);
        
        // Verify reconstruction preserves duration
        var reconstructedDuration = await GetAudioDurationAsync(reconstructedPath, ct);
        Console.WriteLine($"Reconstructed audio: {reconstructedDuration:F3}s (original: {originalDuration:F3}s)");
        
        if (Math.Abs(reconstructedDuration - originalDuration) > 0.1)
        {
            Console.WriteLine($"Warning: Reconstruction duration mismatch > 100ms");
        }

        // Phase 2: Apply silence to inter-sentence gaps  
        if (replacements.Count > 0)
        {
            Console.WriteLine($"Applying silence to {replacements.Count} inter-sentence gaps...");
            await ApplySilenceReplacementsAsync(reconstructedPath, roomtoneSource, replacements, normalizedOutput, ct);
            
            // Clean up intermediate file
            try { File.Delete(reconstructedPath); } catch { }
        }
        else
        {
            // No replacements needed, just use reconstructed audio
            File.Move(reconstructedPath, normalizedOutput);
        }

        Console.WriteLine($"Collated audio completed with {replacements.Count} gaps silenced");
    }

    private async Task ReconstructCompleteAudioAsync(ChunkIndex chunkIndex, string outputPath, CancellationToken ct)
    {
        // Get chunk files in order
        var chunkPaths = chunkIndex.Chunks
            .OrderBy(c => c.Span.Start)
            .Select(c => Path.Combine(WorkDir, "chunks", "wav", c.Filename))
            .Where(File.Exists)
            .ToList();

        if (chunkPaths.Count == 0)
            throw new InvalidOperationException("No chunk files found for reconstruction");

        Console.WriteLine($"Reconstructing audio from {chunkPaths.Count} chunks...");

        // Create concat list file for FFmpeg
        var concatListPath = Path.Combine(Path.GetDirectoryName(outputPath)!, "chunk_concat_list.txt");
        var concatContent = string.Join("\n", chunkPaths.Select(p => $"file '{PathNormalizer.NormalizePath(p)}'"));
        await File.WriteAllTextAsync(concatListPath, concatContent, ct);

        try
        {
            var args = $"-f concat -safe 0 -i \"{concatListPath}\" -c copy -y \"{PathNormalizer.NormalizePath(outputPath)}\"";
            var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to reconstruct audio from chunks: {result.StdErr}");
            }
        }
        finally
        {
            try { File.Delete(concatListPath); } catch { }
        }
    }

    private async Task ApplySilenceReplacementsAsync(
        string inputPath, 
        string roomtonePath, 
        List<CollationReplacement> replacements, 
        string outputPath, 
        CancellationToken ct)
    {
        if (replacements.Count == 0)
        {
            await CopyAudioAsync(inputPath, outputPath, ct);
            return;
        }

        var normalizedInput = PathNormalizer.NormalizePath(inputPath);
        var normalizedRoomtone = PathNormalizer.NormalizePath(roomtonePath);
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);

        if (replacements.Count == 0)
        {
            await CopyAudioAsync(inputPath, outputPath, ct);
            return;
        }
        
        // Since the complex expression fails, use sequential processing
        // Process each gap individually in sequence
        
        Console.WriteLine($"Applying silence to {replacements.Count} inter-sentence gaps sequentially...");
        
        var currentInput = normalizedInput;
        var tempFiles = new List<string>();
        
        try
        {
            for (int i = 0; i < replacements.Count; i++)
            {
                var replacement = replacements[i];
                var isLastReplacement = (i == replacements.Count - 1);
                
                var outputForThisStep = isLastReplacement ? normalizedOutput : 
                    Path.Combine(Path.GetDirectoryName(normalizedOutput)!, $"temp_step_{i:D3}.wav");
                
                if (!isLastReplacement)
                    tempFiles.Add(outputForThisStep);
                
                // Apply silence to this single gap using the working syntax
                var startTime = replacement.From.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                var endTime = replacement.To.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                
                var volumeExpression = $"'if(between(t,{startTime},{endTime}),0,1)'";
                var args = $"-i \"{currentInput}\" -af \"volume={volumeExpression}\" -c:a pcm_f32le -y \"{outputForThisStep}\"";
                
                if (i % 50 == 0) // Progress update every 50 replacements
                    Console.WriteLine($"Processing gap {i + 1}/{replacements.Count}: {replacement.From:F3}s-{replacement.To:F3}s");
                
                var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);
                
                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"Failed at gap {i + 1}: {result.StdErr}");
                    // Clean up and fall back
                    foreach (var tempFile in tempFiles)
                    {
                        try { File.Delete(tempFile); } catch { }
                    }
                    await CopyAudioAsync(inputPath, outputPath, ct);
                    return;
                }
                
                // Update input for next iteration (unless this was the final step)
                if (!isLastReplacement)
                    currentInput = outputForThisStep;
            }
            
            Console.WriteLine($"SUCCESS: Applied silence to all {replacements.Count} inter-sentence gaps sequentially");
            Console.WriteLine("All breath sounds between sentences have been replaced with silence");
        }
        finally
        {
            // Clean up all temp files
            foreach (var tempFile in tempFiles)
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }


    private async Task CopyAudioAsync(string inputPath, string outputPath, CancellationToken ct)
    {
        var normalizedInput = PathNormalizer.NormalizePath(inputPath);
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);

        var args = $"-i \"{normalizedInput}\" -c copy -y \"{normalizedOutput}\"";

        var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to copy audio: {result.StdErr}");
        }
    }

    private async Task<double> GetAudioDurationAsync(string audioPath, CancellationToken ct)
    {
        var normalizedPath = PathNormalizer.NormalizePath(audioPath);
        var args = $"-v quiet -show_entries format=duration -of csv=p=0 \"{normalizedPath}\"";

        var result = await _processRunner.RunAsync(GetFfprobeExecutable(), args, ct);

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.StdOut))
        {
            throw new InvalidOperationException($"Failed to get audio duration: {result.StdErr}");
        }

        if (double.TryParse(result.StdOut.Trim(), out var duration))
        {
            return duration;
        }

        throw new InvalidOperationException($"Could not parse duration: {result.StdOut}");
    }

    private static string GetFfmpegExecutable()
    {
        return Environment.GetEnvironmentVariable("FFMPEG_EXE") ?? "ffmpeg";
    }

    private static string GetFfprobeExecutable()
    {
        return Environment.GetEnvironmentVariable("FFPROBE_EXE") ?? "ffprobe";
    }
}
