using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Book;

namespace Ams.Core.Pipeline;

public sealed class RoomToneInsertionStage
{
    private readonly int _targetSampleRate;
    private readonly double _toneGainDb;
    private readonly double _fadeMs;

    public RoomToneInsertionStage(int targetSampleRate = 44100, double toneGainDb = -60.0, double fadeMs = 5.0)
    {
        if (targetSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(targetSampleRate));
        _targetSampleRate = targetSampleRate;
        _toneGainDb = toneGainDb;
        _fadeMs = fadeMs;
    }

    public async Task<IDictionary<string, string>> RunAsync(ManifestV2 manifest, CancellationToken ct, bool renderAudio = true)
    {
        if (manifest is null) throw new ArgumentNullException(nameof(manifest));
        ct.ThrowIfCancellationRequested();

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var transcript = await LoadTranscriptAsync(manifest.TranscriptIndexPath, jsonOptions, ct);
        var asr = await LoadAsrAsync(transcript.ScriptPath, jsonOptions, ct);
        var bookIndex = await LoadBookIndexAsync(transcript.BookIndexPath, jsonOptions, ct);
        var roomtonePath = RequireRoomtone(manifest.AudioPath, bookIndex.SourceFile);

        if (!File.Exists(manifest.AudioPath))
            throw new FileNotFoundException("Audio file not found", manifest.AudioPath);

        var inputAudio = WavIo.ReadPcmOrFloat(manifest.AudioPath);
        var roomtoneSeed = WavIo.ReadPcmOrFloat(roomtonePath);
        var analyzer = new AudioAnalysisService(inputAudio);
        var seedAnalyzer = new AudioAnalysisService(roomtoneSeed);
        var roomtoneSeedStats = seedAnalyzer.AnalyzeGap(0.0, roomtoneSeed.SampleRate > 0 ? roomtoneSeed.Length / (double)roomtoneSeed.SampleRate : 0.0);
        double toneGainLinear = ComputeToneGainLinear(roomtoneSeedStats.MeanRmsDb, _toneGainDb);
        double appliedGainDb = ToDb(toneGainLinear);
        Console.WriteLine($"[Roomtone] Seed RMS {roomtoneSeedStats.MeanRmsDb:F2} dB, target {_toneGainDb:F2} dB, applied gain {appliedGainDb:F2} dB");

        var timelineEntries = SentenceTimelineBuilder.Build(transcript.Sentences, analyzer, asr);
        double audioDurationSec = inputAudio.SampleRate > 0 ? inputAudio.Length / (double)inputAudio.SampleRate : 0.0;
        var gaps = BuildGaps(timelineEntries, analyzer, audioDurationSec);
        Console.WriteLine($"[Roomtone] Gap segments retained: {gaps.Count}");

        var entryMap = timelineEntries.ToDictionary(e => e.SentenceId);
        var updatedSentences = transcript.Sentences
            .Select(s => entryMap.TryGetValue(s.Id, out var entry) ? s with { Timing = entry.Timing } : s)
            .ToList();

        var stageDir = manifest.ResolveStageDirectory("roomtone");
        EnsureDirectory(stageDir);
        var planPath = Path.Combine(stageDir, "plan.json");
        var timelinePath = Path.Combine(stageDir, "timeline.json");
        var metaPath = Path.Combine(stageDir, "meta.json");
        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");

        var plan = BuildPlan(manifest, inputAudio, roomtonePath, timelineEntries, gaps, roomtoneSeedStats.MeanRmsDb, appliedGainDb, _targetSampleRate, _toneGainDb, _fadeMs);

        string? wavPath = null;
        if (renderAudio)
        {
            var rendered = RoomtoneRenderer.RenderWithSentenceMasks(
                input: inputAudio,
                roomtoneSeed: roomtoneSeed,
                gaps: plan.Gaps,
                sentences: updatedSentences,
                targetSampleRate: _targetSampleRate,
                toneGainLinear: toneGainLinear,
                fadeMs: _fadeMs);

            wavPath = Path.Combine(stageDir, "roomtone.wav");
            WavIo.WriteFloat32(wavPath, rendered);
        }

        await WriteTimelineAsync(timelineEntries, manifest, timelinePath, ct);
        await WritePlanAsync(plan, planPath, ct);
        await WriteMetaAsync(manifest, wavPath, roomtonePath, planPath, metaPath, ct);
        await WriteParamsAsync(paramsPath, _toneGainDb, roomtoneSeedStats.MeanRmsDb, appliedGainDb, _fadeMs, ct);

        var outputs = new Dictionary<string, string>
        {
            ["plan"] = planPath,
            ["timeline"] = timelinePath,
            ["meta"] = metaPath,
            ["params"] = paramsPath
        };

        if (wavPath is not null)
        {
            outputs["roomtoneWav"] = wavPath;
        }

        return outputs;
    }

    private static string RequireRoomtone(string audioPath, string? docPath)
    {
        var directories = new List<string>();

        AddCandidate(Path.GetDirectoryName(audioPath));
        AddCandidate(Path.GetDirectoryName(docPath));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in directories)
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;
            var full = Path.GetFullPath(dir);
            if (!seen.Add(full)) continue;
            if (!Directory.Exists(full)) continue;

            var match = Directory.EnumerateFiles(full, "roomtone.wav", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), "roomtone.wav", StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }
        }

        var searched = directories
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var message = "roomtone.wav not found in expected directories.";
        if (searched.Length > 0)
        {
            message += $" Checked: {string.Join(", ", searched)}";
        }

        throw new FileNotFoundException(message, "roomtone.wav");

        void AddCandidate(string? dir)
        {
            if (!string.IsNullOrWhiteSpace(dir))
            {
                directories.Add(dir);
            }
        }
    }

    private static async Task<TranscriptIndex> LoadTranscriptAsync(string path, JsonSerializerOptions options, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("TranscriptIndex not found", path);
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<TranscriptIndex>(json, options)
               ?? throw new InvalidOperationException("Failed to parse TranscriptIndex");
    }

    private static async Task<AsrResponse> LoadAsrAsync(string path, JsonSerializerOptions options, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("ASR JSON not found", path);
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<AsrResponse>(json, options)
               ?? throw new InvalidOperationException("Failed to parse ASR JSON");
    }

    private static async Task<BookIndex> LoadBookIndexAsync(string path, JsonSerializerOptions options, CancellationToken ct)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("BookIndex not found", path);
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<BookIndex>(json, options)
               ?? throw new InvalidOperationException("Failed to parse BookIndex");
    }

    private static async Task WriteTimelineAsync(
        IReadOnlyList<SentenceTimelineEntry> entries,
        ManifestV2 manifest,
        string path,
        CancellationToken ct)
    {
        var payload = new
        {
            manifest.ChapterId,
            audio = manifest.AudioPath,
            transcript = manifest.TranscriptIndexPath,
            generatedAtUtc = DateTime.UtcNow,
            sentences = entries.Select(e => new
            {
                sentenceId = e.SentenceId,
                startSec = e.Timing.StartSec,
                endSec = e.Timing.EndSec,
                windowStartSec = e.Window.StartSec,
                windowEndSec = e.Window.EndSec,
                fragmentBacked = e.Window.FragmentBacked,
                hasTiming = e.HasTiming,
                confidence = e.Window.Confidence
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static async Task WritePlanAsync(RoomtonePlan plan, string path, CancellationToken ct)
    {
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(plan, options);
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static RoomtonePlan BuildPlan(
        ManifestV2 manifest,
        AudioBuffer audio,
        string roomtonePath,
        IReadOnlyList<SentenceTimelineEntry> entries,
        IReadOnlyList<RoomtonePlanGap> gaps,
        double roomtoneSeedRmsDb,
        double appliedGainDb,
        int targetSampleRate,
        double targetRmsDb,
        double fadeMs)
    {
        double durationSec = audio.SampleRate > 0 ? audio.Length / (double)audio.SampleRate : 0.0;

        var orderedEntries = entries
            .OrderBy(e => e.Timing.StartSec)
            .ThenBy(e => e.Timing.EndSec)
            .ToList();

        var sentences = orderedEntries
            .Select(e => new RoomtonePlanSentence(
                e.SentenceId,
                e.Timing.StartSec,
                e.Timing.EndSec,
                e.Window.StartSec,
                e.Window.EndSec,
                e.HasTiming,
                e.Window.FragmentBacked,
                e.Window.Confidence))
            .ToList();

        return new RoomtonePlan(
            RoomtonePlanVersion.Current,
            manifest.ChapterId,
            manifest.AudioPath,
            manifest.TranscriptIndexPath,
            roomtonePath,
            durationSec,
            targetSampleRate,
            targetRmsDb,
            appliedGainDb,
            roomtoneSeedRmsDb,
            fadeMs,
            DateTime.UtcNow,
            sentences,
            gaps);
    }

    private static IReadOnlyList<RoomtonePlanGap> BuildGaps(
        IReadOnlyList<SentenceTimelineEntry> entries,
        AudioAnalysisService analyzer,
        double audioDurationSec)
    {
        const double epsilon = 1e-6;
        const double stepSec = 0.005;
        const double backoffSec = 0.005;
        const double speechThresholdDb = -40.0;

        var gaps = new List<RoomtonePlanGap>();
        if (audioDurationSec <= epsilon)
        {
            Console.WriteLine("[Roomtone] Audio too short for gap analysis.");
            return gaps;
        }

        var orderedEntries = entries
            .OrderBy(e => e.Timing.StartSec)
            .ThenBy(e => e.Timing.EndSec)
            .ToList();

        if (orderedEntries.Count == 0)
        {
            Console.WriteLine("[Roomtone] No sentence timings; treating entire chapter as a single gap.");
            var stats = analyzer.AnalyzeGap(0.0, audioDurationSec);
            gaps.Add(CreateGap(0.0, audioDurationSec, null, null, stats));
            return gaps;
        }

        SentenceTimelineEntry? previous = null;
        foreach (var current in orderedEntries)
        {
            gaps.AddRange(ProcessGap(previous, current));
            previous = current;
        }
        gaps.AddRange(ProcessGap(previous, null));

        return gaps;

        IEnumerable<RoomtonePlanGap> ProcessGap(SentenceTimelineEntry? leftEntry, SentenceTimelineEntry? rightEntry)
        {
            double timingStart = leftEntry?.Timing.EndSec ?? 0.0;
            double windowStart = leftEntry?.Window.EndSec ?? 0.0;
            double initialStart = Math.Min(timingStart, windowStart);

            double timingEnd = rightEntry?.Timing.StartSec ?? audioDurationSec;
            double windowEnd = rightEntry?.Window.StartSec ?? audioDurationSec;
            double initialEnd = Math.Max(timingEnd, windowEnd);

            initialStart = Math.Clamp(initialStart, 0.0, audioDurationSec);
            initialEnd = Math.Clamp(initialEnd, 0.0, audioDurationSec);
            if (initialEnd - initialStart <= epsilon)
            {
                yield break;
            }

            string label = $"prev={(leftEntry?.SentenceId.ToString() ?? "null")} next={(rightEntry?.SentenceId.ToString() ?? "null")}";
            Console.WriteLine($"[Roomtone] Gap candidate {label} base=({initialStart:F3},{initialEnd:F3}) dur={(initialEnd - initialStart):F3}");

            double midpoint = (initialStart + initialEnd) / 2.0;
            midpoint = Math.Clamp(midpoint, initialStart + epsilon, initialEnd - epsilon);

            if (midpoint <= initialStart + epsilon || initialEnd <= midpoint + epsilon)
            {
                Console.WriteLine("[Roomtone]   midpoint collapsed; skipping gap.");
                yield break;
            }

            bool leftAccepted = TryCalibrateLeft(initialStart, midpoint, out double leftStart);
            bool rightAccepted = TryCalibrateRight(midpoint, initialEnd, out double rightEnd);

            if (leftAccepted && midpoint - leftStart > epsilon)
            {
                var stats = analyzer.AnalyzeGap(leftStart, midpoint);
                Console.WriteLine($"[Roomtone]   left accepted [{leftStart:F3},{midpoint:F3}] meanRms={stats.MeanRmsDb:F2} dB");
                yield return CreateGap(leftStart, midpoint, leftEntry?.SentenceId, rightEntry?.SentenceId, stats);
            }
            else
            {
                Console.WriteLine("[Roomtone]   left rejected or collapsed.");
            }

            if (rightAccepted && rightEnd - midpoint > epsilon)
            {
                var stats = analyzer.AnalyzeGap(midpoint, rightEnd);
                Console.WriteLine($"[Roomtone]   right accepted [{midpoint:F3},{rightEnd:F3}] meanRms={stats.MeanRmsDb:F2} dB");
                yield return CreateGap(midpoint, rightEnd, leftEntry?.SentenceId, rightEntry?.SentenceId, stats);
            }
            else
            {
                Console.WriteLine("[Roomtone]   right rejected or collapsed.");
            }
        }

        bool TryCalibrateLeft(double initialStart, double end, out double result)
        {
            double boundary = initialStart;
            result = double.NaN;
            while (boundary + epsilon < end)
            {
                double rms = analyzer.MeasureRms(boundary, end);
                Console.WriteLine($"[Roomtone]     test left [{boundary:F3},{end:F3}] rms={rms:F2} dB");
                if (rms <= speechThresholdDb)
                {
                    double candidate = boundary;
                    double backoff = Math.Max(initialStart, candidate - backoffSec);
                    if (backoff < candidate)
                    {
                        double backoffRms = analyzer.MeasureRms(backoff, end);
                        if (backoffRms <= speechThresholdDb)
                        {
                            candidate = backoff;
                        }
                    }
                    result = candidate;
                    return true;
                }
                boundary += stepSec;
            }
            return false;
        }

        bool TryCalibrateRight(double start, double initialEnd, out double result)
        {
            double boundary = initialEnd;
            result = double.NaN;
            while (boundary - epsilon > start)
            {
                double rms = analyzer.MeasureRms(start, boundary);
                Console.WriteLine($"[Roomtone]     test right [{start:F3},{boundary:F3}] rms={rms:F2} dB");
                if (rms <= speechThresholdDb)
                {
                    double candidate = boundary;
                    double backoff = Math.Min(initialEnd, candidate + backoffSec);
                    if (backoff > candidate)
                    {
                        double backoffRms = analyzer.MeasureRms(start, backoff);
                        if (backoffRms <= speechThresholdDb)
                        {
                            candidate = backoff;
                        }
                    }
                    result = candidate;
                    return true;
                }
                boundary -= stepSec;
            }
            return false;
        }
    }

    private static double ExpandLeft(
        AudioAnalysisService analyzer,
        double startSec,
        double limitSec,
        double stepSec,
        double backoffSec,
        double speechThresholdDb,
        double maxExtendSec)
    {
        double current = startSec;
        double best = startSec;
        double maxDistance = Math.Min(maxExtendSec, startSec - limitSec);
        double travelled = 0.0;

        while (travelled + stepSec <= maxDistance)
        {
            current -= stepSec;
            travelled += stepSec;
            double windowStart = Math.Max(limitSec, current);
            double windowEnd = windowStart + stepSec;
            if (windowEnd <= windowStart) break;

            double rmsDb = analyzer.MeasureRms(windowStart, windowEnd);
            if (rmsDb <= speechThresholdDb)
            {
                best = windowStart;
            }
            else
            {
                best = Math.Min(startSec, windowStart + backoffSec);
                break;
            }
        }

        return Math.Max(limitSec, best);
    }

    private static double ExpandRight(
        AudioAnalysisService analyzer,
        double endSec,
        double limitSec,
        double stepSec,
        double backoffSec,
        double speechThresholdDb,
        double maxExtendSec,
        double audioDurationSec)
    {
        double current = endSec;
        double best = endSec;
        double maxDistance = Math.Min(maxExtendSec, limitSec - endSec);
        double travelled = 0.0;

        while (travelled + stepSec <= maxDistance)
        {
            double windowEnd = Math.Min(audioDurationSec, current + stepSec);
            double windowStart = windowEnd - stepSec;
            if (windowStart >= audioDurationSec) break;
            travelled += stepSec;

            double rmsDb = analyzer.MeasureRms(windowStart, windowEnd);
            if (rmsDb <= speechThresholdDb)
            {
                best = windowEnd;
                current = windowEnd;
            }
            else
            {
                best = Math.Max(endSec, windowEnd - backoffSec);
                break;
            }
        }

        return Math.Min(limitSec, best);
    }

    private static RoomtonePlanGap CreateGap(double startSec, double endSec, int? previousId, int? nextId, GapRmsStats stats)
    {
        double duration = Math.Max(0.0, endSec - startSec);
        return new RoomtonePlanGap(
            startSec,
            endSec,
            duration,
            previousId,
            nextId,
            stats.MinRmsDb,
            stats.MaxRmsDb,
            stats.MeanRmsDb,
            stats.SilenceFraction);
    }

    private static double ComputeToneGainLinear(double seedMeanRmsDb, double targetRmsDb)
    {
        double seedLinear = DbToLinear(seedMeanRmsDb);
        double targetLinear = DbToLinear(targetRmsDb);
        if (seedLinear <= 0)
        {
            return 1.0;
        }
        return targetLinear / seedLinear;
    }

    private static double DbToLinear(double db) => Math.Pow(10.0, db / 20.0);

    private static double ToDb(double linear) => linear <= 0 ? double.NegativeInfinity : 20.0 * Math.Log10(linear);

    private static async Task WriteMetaAsync(ManifestV2 manifest, string? wavPath, string sourceRoomtone, string planPath, string path, CancellationToken ct)
    {
        object outputs = wavPath is null
            ? new { plan = planPath }
            : new { plan = planPath, roomtone = wavPath };

        var meta = new
        {
            stage = "roomtone",
            chapter = manifest.ChapterId,
            generatedAtUtc = DateTime.UtcNow,
            outputs,
            sourceRoomtone
        };

        var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static async Task WriteParamsAsync(string path, double targetRmsDb, double roomtoneSeedRmsDb, double appliedGainDb, double fadeMs, CancellationToken ct)
    {
        var snapshot = new
        {
            parameters = new
            {
                targetRmsDb,
                roomtoneSeedRmsDb,
                appliedGainDb,
                fadeMs,
                usedExistingRoomtone = true
            }
        };
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static void EnsureDirectory(string? dir)
    {
        if (string.IsNullOrWhiteSpace(dir)) return;
        Directory.CreateDirectory(dir);
    }
}




