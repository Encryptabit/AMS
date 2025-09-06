using System.Text.Json;
using System.Text.RegularExpressions;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class CollateStage : StageRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly CollationParams _params;

    // Processing constants
    private const int TargetRate = 44100; // audiobook-friendly sample rate
    private const double FadeSecDefault = 0.005; // 5 ms default crossfade
    private const double GuardHotL = 0.012; // keep in sync with ApplyRoomtoneSeamsAsync
    private const double GuardHotR = 0.015; // keep in sync with ApplyRoomtoneSeamsAsync
    private const double HfProbeWindowSec = 0.050; // probe 30 ms windows
    private const int HfCutHz = 1500; // high-pass cutoff for HF probe (was 00)
    
    private const int    MaxLeftNudges  = 8;      // tighter cap
    private const int    MaxRightNudges = 3;
    private const double NudgeStepSec   = 0.003;  // 3 ms
    private const double ProbeWinMinSec = 0.080;  // stable left/right probes
    private const double HfBandLowHz   = 3500.0;
    private const double HfBandHighHz  = 12000.0;
    private const double HfMarginDb    = 5.0;   // band - fullband must exceed this
    private const double WeakMarginDb  = 2.5;   // treat as "not hot" if below this (hysteresis)
    private const double MinProbeSec   = 0.080; // longer window stabilizes RMS
    
    public CollateStage(
        string workDir,
        IProcessRunner processRunner,
        CollationParams parameters)
        : base(workDir, "collate")
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir,
        CancellationToken ct)
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
        var sentences = JsonSerializer.Deserialize<List<RefinedSentence>>(sentencesJson) ??
                        throw new InvalidOperationException("Invalid sentences");

        var chunkIndexJson = await File.ReadAllTextAsync(chunkIndexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(chunkIndexJson) ??
                         throw new InvalidOperationException("Invalid chunk index");

        var planJson = await File.ReadAllTextAsync(planPath, ct);
        var plan = JsonSerializer.Deserialize<WindowPlanV2>(planJson) ??
                   throw new InvalidOperationException("Invalid plan");

        Console.WriteLine($"Collating {sentences.Count} sentences with room tone replacement + qsin crossfades...");

        // Use manifest duration instead of reading WAV directly (avoids format issues)
        var originalDuration = manifest.Input.DurationSec;

        Console.WriteLine($"Original audio: {originalDuration:F3}s (from manifest metadata)");

        // Create room tone source
        var roomtoneSource = await CreateRoomtoneSourceAsync(manifest.Input.Path, originalDuration, stageDir, ct);

        // Detect boundary slivers and inter-sentence gaps
        var replacements = AnalyzeReplacements(sentences, plan.Windows, originalDuration);

        Console.WriteLine($"Identified {replacements.Count} candidate regions for room tone replacement");

        // Generate collated audio in two phases:
        // 1. Reconstruct complete audio from chunks
        // 2. Apply roomtone replacements to gaps/slivers with crossfades
        var finalPath = Path.Combine(stageDir, "final.wav");
        await RenderCollatedAudioAsync(sentences, replacements, chunkIndex, roomtoneSource, finalPath, originalDuration,
            TargetRate, ct);

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

        Console.WriteLine($"Collation completed: {replacements.Count} regions processed with room tone + crossfades");

        CleanupCollateTempFiles(stageDir);

        return new Dictionary<string, string>
        {
            ["final"] = "final.wav",
            ["segments"] = "segments.json",
            ["log"] = "log.json",
            ["params"] = "params.snapshot.json"
        };
    }


    private void CleanupCollateTempFiles(string stageDir)
    {
        var patterns = new[]
        {
            "reconstructed.wav",
            "roomtone_source.wav",
            "collate_step_*.wav",
            "collate_filter.txt",
            "chunk_concat_list.txt"
        };

        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.GetFiles(stageDir, pattern))
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted temp file: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete {file}: {ex.Message}");
                }
            }
        }
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

    private async Task<string> CreateRoomtoneSourceAsync(string inputPath, double originalDuration, string stageDir,
        CancellationToken ct)
    {
        var roomtonePath = Path.Combine(stageDir, "roomtone_source.wav");

        if (_params.RoomtoneSource == "file" && !string.IsNullOrEmpty(_params.RoomtoneFilePath))
        {
            // Use provided room tone file
            var sourcePath = PathNormalizer.NormalizePath(_params.RoomtoneFilePath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Room tone file not found: {sourcePath}");

            // Copy and resample if needed
            await ResampleAudioAsync(sourcePath, roomtonePath, TargetRate, ct);
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

    private async Task ExtractRoomtoneFromOriginalAsync(string inputPath, string outputPath, double originalDuration,
        CancellationToken ct)
    {
        // Simple approach: extract a quiet section and loop it
        // In practice, you'd want more sophisticated room tone detection

        var normalizedInput = PathNormalizer.NormalizePath(inputPath);
        var normalizedOutput = PathNormalizer.NormalizePath(outputPath);

        // Extract 5 seconds from 10% into the audio (assuming it's quiet)
        var startTime = originalDuration * 0.1;
        var duration = Math.Min(5.0, originalDuration * 0.1);

        var args =
            $"-i \"{normalizedInput}\" -ss {startTime:F6} -t {duration:F6} -af \"volume={_params.RoomtoneLevelDb}dB\" -y \"{normalizedOutput}\"";

        var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to extract room tone: {result.StdErr}");
        }
    }

    private async Task ResampleAudioAsync(string inputPath, string outputPath, int targetSampleRate,
        CancellationToken ct)
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

    private List<CollationReplacement> AnalyzeReplacements(List<RefinedSentence> sentences, List<ChunkSpan> windows,
        double originalDuration)
    {
        var replacements = new List<CollationReplacement>();

        // 1. Inter-sentence gaps
        for (int i = 0; i < sentences.Count - 1; i++)
        {
            var current = sentences[i];
            var next = sentences[i + 1];
            var gapDuration = next.Start - current.End;
            
            Console.WriteLine($"Gap: {gapDuration:F3} Sentence: {sentences[i].StartWordIdx} ");

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

        // Phase 2: Apply room tone + crossfades seam-by-seam
        // Build seams from both 'gap' and 'boundary_sliver' (you can filter if desired)
        var seams = replacements
            .Where(r => r.To > r.From)
            .Select(r => (From: r.From, To: r.To))
            .OrderBy(s => s.From)
            .ToList();

        if (seams.Count > 0)
        {
            Console.WriteLine($"Applying room tone with crossfades to {seams.Count} seams...");
            await ApplyRoomtoneSeamsAsync(reconstructedPath, roomtoneSource, seams, normalizedOutput,
                reconstructedDuration, ct);
            try
            {
                File.Delete(reconstructedPath);
            }
            catch
            {
            }
        }
        else
        {
            File.Move(reconstructedPath, normalizedOutput, true);
        }

        Console.WriteLine($"Collated audio completed with {seams.Count} seams processed");
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
            var args =
                $"-f concat -safe 0 -i \"{concatListPath}\" -c copy -y \"{PathNormalizer.NormalizePath(outputPath)}\"";
            var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to reconstruct audio from chunks: {result.StdErr}");
            }
        }
        finally
        {
            try
            {
                File.Delete(concatListPath);
            }
            catch
            {
            }
        }
    }


// Scales fades down if gap is tiny, leaving 1 ms safety
    private static (double fadeL, double fadeR) FitFadesIntoPause(double P, double fadeL, double fadeR)
    {
        double sum = fadeL + fadeR;
        if (P <= 0.001 || sum <= 0.0) return (0.0, 0.0);
        if (sum <= P - 0.001) return (fadeL, fadeR);

        double scale = (P - 0.001) / sum;
        scale = Math.Max(0.0, Math.Min(1.0, scale));
        return (fadeL * scale, fadeR * scale);
    }


    /// <summary>
    /// Replaces each seam (From, To) with room tone and applies crossfades (qsin).
    /// Uses HF (fricative) probe to nudge cut points and adapt fade length.
    /// Processes iteratively to avoid massive filter graphs / arg-length limits.
    /// Keeps timing exact by compensating for crossfade consumption.
    /// </summary>
    private async Task ApplyRoomtoneSeamsAsync(
        string inputPath,
        string roomtonePath,
        List<(double From, double To)> seams,
        string outputPath,
        double totalDuration,
        CancellationToken ct)
    {
        if (seams.Count == 0)
        {
            await CopyAudioAsync(inputPath, outputPath, ct);
            return;
        }

        // onset/tail guards for risky edges
        const double GuardHotL = 0.012; // protect trailing fricatives on the left
        const double GuardHotR = 0.015; // pre-roll/protect onset on the right

        var ci = System.Globalization.CultureInfo.InvariantCulture;
        string currentIn = PathNormalizer.NormalizePath(inputPath);
        string room = PathNormalizer.NormalizePath(roomtonePath);
        string stageDir = Path.GetDirectoryName(PathNormalizer.NormalizePath(outputPath))!;

        for (int i = 0; i < seams.Count; i++)
        {
            var (ta0, tb0) = seams[i];
            double ta = Math.Max(0.0, ta0);
            double tb = Math.Min(totalDuration, tb0);

            // --- use your WITH-COUNTS nudge function ---
            var (finalTa, finalTb, hfLeft, hfRight, leftNudges, rightNudges) =
                await NudgeUntilNoHighbandAsync_WithCounts(currentIn, ta, tb, totalDuration, ct);

            // Desired pause (keep adjusted original)
            double P = Math.Max(0.0, finalTb - finalTa);

            // Risk assessment: either HF still present or nudges were needed
            bool tailRiskL = hfLeft || leftNudges > 0;
            bool onsetRiskR = hfRight || rightNudges > 0;

            // Base fades
            double baseFade = FadeSecDefault; // class constant you already have (e.g., 0.005)

            // LEFT: do NOT fade out [a]; instead fade IN roomtone by a small amount if risky
            double fadeL = tailRiskL ? Math.Max(baseFade, GuardHotL) : baseFade;

            // RIGHT: onset-safe — pre-roll right clip by guardR and ensure fadeR >= guardR
            double guardR = onsetRiskR ? GuardHotR : 0.0;
            double fadeR = onsetRiskR ? Math.Max(baseFade, guardR) : baseFade;

            // If pause is tiny, scale fades to fit, leaving ~1 ms safety
            double sum = fadeL + fadeR;
            if (P <= 0.001 || sum <= 0.0)
            {
                fadeL = 0.0;
                fadeR = 0.0;
            }
            else if (sum > P - 0.001)
            {
                double scale = (P - 0.001) / sum;
                scale = Math.Max(0.0, Math.Min(1.0, scale));
                fadeL *= scale;
                fadeR *= scale;
            }

            // Roomtone slice: pause + RIGHT acrossfade (left is a simple fade-in, no time consumption)
            double roomLen = P + fadeR;

            // Start the right program a bit early if risky, so onset lives under the xfade
            double rightStart = Math.Max(0.0, finalTb - guardR);

            string stepOut = (i == seams.Count - 1)
                ? PathNormalizer.NormalizePath(outputPath)
                : Path.Combine(stageDir, $"collate_step_{i:D3}.wav");

            // --- Filter graph ---
            // [a]  = program up to finalTa (preserved tail)
            // [b]  = program from rightStart
            // [r]  = roomtone of roomLen, faded IN by fadeL
            // [ar] = [a] then [r] (concat, no overlap)
            // out  = crossfade [ar] -> [b] by fadeR (qsin curves)
            string filter = $@"
[0:a]aformat=sample_rates={TargetRate}:channel_layouts=mono,
     atrim=0:{finalTa.ToString("F6", ci)},asetpts=PTS-STARTPTS[a];
[0:a]aformat=sample_rates={TargetRate}:channel_layouts=mono,
     atrim={rightStart.ToString("F6", ci)},asetpts=PTS-STARTPTS[b];

[1:a]aformat=sample_rates={TargetRate}:channel_layouts=mono,
     atrim=0:{roomLen.ToString("F6", ci)},asetpts=PTS-STARTPTS[rpre];
[rpre]afade=t=in:st=0:d={fadeL.ToString("F3", ci)}[r];

[a][r]concat=n=2:v=0:a=1[ar];
[ar][b]acrossfade=d={fadeR.ToString("F3", ci)}:curve1=qsin:curve2=qsin[out]";

            var args =
                $"-y -hide_banner -i \"{currentIn}\" -stream_loop -1 -i \"{room}\" " +
                $"-filter_complex \"{filter}\" -map \"[out]\" -c:a pcm_f32le \"{stepOut}\"";

            Console.WriteLine(
                $"Seam {i + 1}/{seams.Count}: ta={finalTa:F3}s, tb={finalTb:F3}s, P={P * 1000:F1}ms, " +
                $"fadeL={fadeL * 1000:F1}ms, fadeR={fadeR * 1000:F1}ms, rightStart={rightStart:F3}s " +
                $"(HF L={hfLeft}, R={hfRight}, nudges L={leftNudges}, R={rightNudges})");

            var result = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);
            if (result.ExitCode != 0)
                throw new InvalidOperationException($"FFmpeg seam {i + 1} failed:\n{result.StdErr}");

            currentIn = stepOut; // chain
        }
    }


    /// <summary>
    /// Iteratively nudges cut points until highband detection returns false for both sides.
    /// Returns the final adjusted times and the last detected HF status.
    /// </summary>
    private async Task<(double ta, double tb, bool hfLeft, bool hfRight, int leftNudges, int rightNudges)>
        NudgeUntilNoHighbandAsync_WithCounts(
            string audioPath,
            double initialTa,
            double initialTb,
            double totalDuration,
            CancellationToken ct)
    {
        double ta = initialTa;
        double tb = initialTb;

        int leftNudges = 0;
        int rightNudges = 0;
        bool hfLeft, hfRight;

        Console.WriteLine($"Initial seam: ta={ta:F3}s, tb={tb:F3}s");

        // ---- LEFT EDGE ----
        // Probe a window that ENDS at ta (so the probe "listens" to what we'd cut off)
        // If hot, move ta LATER (to the right) so the cut lands deeper into the quiet.
        double leftWin = Math.Max(ProbeWinMinSec, Math.Max(GuardHotL, HfProbeWindowSec));
        double? prevBandDb = null;

        while (leftNudges < MaxLeftNudges && ta + NudgeStepSec < tb)
        {
            double probeStart = Math.Max(0.0, ta - leftWin);
            var band = await GetBandRmsAsync(audioPath, probeStart, leftWin, HfBandLowHz, HfBandHighHz, ct);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, leftWin, ct);

            if (band is null || full is null)
            {
                hfLeft = false;
                break;
            }

            double delta = band.Value - full.Value;
            bool hot = (band.Value > _params.DbFloor) && (delta >= HfMarginDb);

            // Weak-hot hysteresis: don't chase Δ ~ 2–3 dB
            if (!hot || delta < WeakMarginDb)
            {
                hfLeft = false;
                break;
            }

            // Monotonic guard: if moving has been making it LOUDER, stop
            if (prevBandDb.HasValue && band.Value > prevBandDb.Value + 0.5) // +0.5 dB louder
            {
                Console.WriteLine(
                    $"  Left nudge aborted (monotonic louder): band {band.Value:F1} dB > prev {prevBandDb.Value:F1} dB");
                break;
            }

            prevBandDb = band.Value;

            ta = Math.Min(tb - 0.002, ta + NudgeStepSec); // move later, keep safety from tb
            leftNudges++;
            Console.WriteLine(
                $"  Left nudge #{leftNudges}: ta -> {ta:F3}s (band={band.Value:F1}, full={full.Value:F1}, Δ={delta:F1})");
        }

        // final left status
        {
            double probeStart = Math.Max(0.0, ta - leftWin);
            var band = await GetBandRmsAsync(audioPath, probeStart, leftWin, HfBandLowHz, HfBandHighHz, ct);
            var full = await GetFullbandRmsAsync(audioPath, probeStart, leftWin, ct);
            hfLeft = (band is not null && full is not null) &&
                     (band.Value > _params.DbFloor) &&
                     (band.Value - full.Value >= HfMarginDb);
        }

        // ---- RIGHT EDGE ----
        // Prefer minimal movement. If hot, move tb slightly later.
        double rightWin = Math.Max(ProbeWinMinSec, Math.Max(GuardHotR, HfProbeWindowSec));
        int tries = 0;

        while (tries < MaxRightNudges && tb < totalDuration)
        {
            double rs = Math.Max(0.0, tb); // start at tb, look forward
            double rd = Math.Min(rightWin, Math.Max(0.020, totalDuration - rs));
            var band = await GetBandRmsAsync(audioPath, rs, rd, HfBandLowHz, HfBandHighHz, ct);
            var full = await GetFullbandRmsAsync(audioPath, rs, rd, ct);

            if (band is null || full is null)
            {
                hfRight = false;
                break;
            }

            double delta = band.Value - full.Value;
            bool hot = (band.Value > _params.DbFloor) && (delta >= HfMarginDb);

            if (!hot || delta < WeakMarginDb)
            {
                hfRight = false;
                break;
            }

            tb = Math.Min(totalDuration, tb + NudgeStepSec);
            tries++;
            rightNudges++;
            Console.WriteLine(
                $"  Right nudge #{rightNudges}: tb -> {tb:F3}s (band={band.Value:F1}, full={full.Value:F1}, Δ={delta:F1})");
        }

        // final right status
        {
            double rs = Math.Max(0.0, tb);
            double rd = Math.Min(rightWin, Math.Max(0.020, totalDuration - rs));
            var band = await GetBandRmsAsync(audioPath, rs, rd, HfBandLowHz, HfBandHighHz, ct);
            var full = await GetFullbandRmsAsync(audioPath, rs, rd, ct);
            hfRight = (band is not null && full is not null) &&
                      (band.Value > _params.DbFloor) &&
                      (band.Value - full.Value >= HfMarginDb);
        }

        // Log gap change
        if (leftNudges > 0 || rightNudges > 0)
        {
            double originalGap = initialTb - initialTa;
            double finalGap = tb - ta;
            Console.WriteLine(
                $"  Nudging complete: L={leftNudges}, R={rightNudges}. Gap: {originalGap * 1000:F1}ms -> {finalGap * 1000:F1}ms");
        }

        return (ta, tb, hfLeft, hfRight, leftNudges, rightNudges);
    }

    private async Task<bool> IsHighbandHotAsync(string audioPath, double start, double dur, CancellationToken ct)
    {
        start = Math.Max(0.0, start);
        dur = Math.Max(MinProbeSec, dur);

        // Band-limited RMS (LP -> HP = bandpass)
        var band = await GetBandRmsAsync(audioPath, start, dur, HfBandLowHz, HfBandHighHz, ct);
        var full = await GetFullbandRmsAsync(audioPath, start, dur, ct);
        if (band is null || full is null) return false;

        double delta = band.Value - full.Value;
        bool hotAbs = band.Value > _params.DbFloor;
        bool hotRel = delta >= HfMarginDb;

        // Hysteresis: weak-hot isn’t hot
        bool hot = hotAbs && hotRel;

        Console.WriteLine(
            $"HF probe @ {start:F3}s/{dur * 1000:F0}ms: Band={band:F1} dB, Full={full:F1} dB, Δ={delta:F1} dB, " +
            $"floor={_params.DbFloor:F1} → hot={hot}");

        return hot;
    }

    private async Task<double?> GetBandRmsAsync(string audioPath, double start, double dur, double lowHz, double highHz,
        CancellationToken ct)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);

        string afilter =
            $"aformat=sample_fmts=flt:sample_rates={TargetRate}:channel_layouts=mono," +
            $"lowpass=f={highHz.ToString("F0", ci)}," +
            $"highpass=f={lowHz.ToString("F0", ci)}," +
            $"volumedetect";

        string args =
            $"-v info -ss {start.ToString("F6", ci)} -t {dur.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilter}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);
        var stderr = res.StdErr ?? string.Empty;

        var mv = System.Text.RegularExpressions.Regex.Match(stderr, @"mean_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mv.Success && double.TryParse(mv.Groups[1].Value, out var meanDb)) return meanDb;

        var mx = System.Text.RegularExpressions.Regex.Match(stderr, @"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mx.Success && double.TryParse(mx.Groups[1].Value, out var maxDb)) return maxDb;

        return null;
    }

    private async Task<double?> GetFullbandRmsAsync(string audioPath, double start, double dur, CancellationToken ct)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);

        string afilter =
            $"aformat=sample_fmts=flt:sample_rates={TargetRate}:channel_layouts=mono,volumedetect";

        string args =
            $"-v info -ss {start.ToString("F6", ci)} -t {dur.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilter}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), args, ct);
        var stderr = res.StdErr ?? string.Empty;

        var mv = System.Text.RegularExpressions.Regex.Match(stderr, @"mean_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mv.Success && double.TryParse(mv.Groups[1].Value, out var meanDb)) return meanDb;

        var mx = System.Text.RegularExpressions.Regex.Match(stderr, @"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mx.Success && double.TryParse(mx.Groups[1].Value, out var maxDb)) return maxDb;

        return null;
    }


    /// <summary>HF-emphasized RMS via volumedetect (HP×2) with astats fallback.</summary>
    private async Task<double?> GetHighbandRmsAsync(string audioPath, double start, double dur, CancellationToken ct)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var normalized = PathNormalizer.NormalizePath(audioPath);

        string afilterHp2 =
            $"aformat=sample_fmts=flt:sample_rates={TargetRate}:channel_layouts=mono," +
            $"highpass=f={HfCutHz},highpass=f={HfCutHz}," + // stack HP twice, no 'order'
            $"volumedetect";

        string argsHp2 =
            $"-v info -ss {start.ToString("F6", ci)} -t {dur.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilterHp2}\" -f null -";

        var res = await _processRunner.RunAsync(GetFfmpegExecutable(), argsHp2, ct);
        var stderr = res.StdErr ?? string.Empty;

        // mean_volume (prefer) then max_volume
        var mv = System.Text.RegularExpressions.Regex.Match(stderr, @"mean_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mv.Success && double.TryParse(mv.Groups[1].Value, out var meanDb))
            return meanDb;

        var mx = System.Text.RegularExpressions.Regex.Match(stderr, @"max_volume:\s*(-?\d+(?:\.\d+)?)\s*dB");
        if (mx.Success && double.TryParse(mx.Groups[1].Value, out var maxDb))
            return maxDb;

        // Fallback: astats
        string afilterAst =
            $"aformat=sample_fmts=flt:sample_rates={TargetRate}:channel_layouts=mono," +
            $"highpass=f={HfCutHz},highpass=f={HfCutHz}," +
            $"astats=metadata=1:reset=1:measure_overall=1:measure_perchannel=0:length=1";

        string argsAst =
            $"-v info -ss {start.ToString("F6", ci)} -t {dur.ToString("F6", ci)} " +
            $"-i \"{normalized}\" -af \"{afilterAst}\" -f null -";

        var res2 = await _processRunner.RunAsync(GetFfmpegExecutable(), argsAst, ct);
        var text2 = (res2.StdErr ?? "") + "\n" + (res2.StdOut ?? "");

        var overall = System.Text.RegularExpressions.Regex
            .Matches(text2, @"Overall\.RMS_level\s*:\s*(-?\d+(?:\.\d+)?)")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value).LastOrDefault();

        if (overall != null && double.TryParse(overall, out var db))
            return db;

        var frame = System.Text.RegularExpressions.Regex
            .Matches(text2, @"\bRMS_level\s*:\s*(-?\d+(?:\.\d+)?)")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Groups[1].Value).LastOrDefault();

        return (frame != null && double.TryParse(frame, out var fdb)) ? fdb : (double?)null;
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