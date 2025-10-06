using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Alignment.Mfa;
using Ams.Core.Artifacts;
using Ams.Core.Common;
using Ams.Core.Audio;
using Ams.Core.Book;
using Ams.Core.Hydrate;

namespace Ams.Core.Pipeline;

public sealed class RoomToneInsertionStage
{
    private readonly int _targetSampleRate;
    private readonly double _toneGainDb;
    private readonly double _fadeMs;
    private readonly bool _emitDiagnostics;
    private readonly bool _useAdaptiveGain;
    private readonly bool _verbose;
    private readonly double _gapLeftThresholdDb;
    private readonly double _gapRightThresholdDb;
    private readonly double _gapStepSec;
    private readonly double _gapBackoffSec;

    private const double DefaultGapThresholdDb = -30.0;
    private const double DefaultGapStepMs = 5.0;
    private const double DefaultGapBackoffMs = 5.0;
    private const double TextGridGapToleranceSec = 0.02;
    private const double TextGridMinGapLengthSec = 0.10;

    private sealed record TextGridGapHint(double StartSec, double EndSec, int? PreviousSentenceId, int? NextSentenceId, string Text);

    public RoomToneInsertionStage(
        int targetSampleRate = 44100,
        double toneGainDb = -74.0,
        double fadeMs = 5.0,
        bool emitDiagnostics = true,
        bool useAdaptiveGain = true,
        bool verbose = false,
        double gapLeftThresholdDb = DefaultGapThresholdDb,
        double gapRightThresholdDb = DefaultGapThresholdDb,
        double gapStepMs = DefaultGapStepMs,
        double gapBackoffMs = DefaultGapBackoffMs)
    {
        if (targetSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(targetSampleRate));
        if (gapStepMs <= 0) throw new ArgumentOutOfRangeException(nameof(gapStepMs));
        if (gapBackoffMs <= 0) throw new ArgumentOutOfRangeException(nameof(gapBackoffMs));
        _targetSampleRate = targetSampleRate;
        _toneGainDb = toneGainDb;
        _fadeMs = fadeMs;
        _emitDiagnostics = emitDiagnostics;
        _useAdaptiveGain = useAdaptiveGain;
        _verbose = verbose;
        _gapLeftThresholdDb = gapLeftThresholdDb;
        _gapRightThresholdDb = gapRightThresholdDb;
        _gapStepSec = gapStepMs / 1000.0;
        _gapBackoffSec = gapBackoffMs / 1000.0;
    }

    public async Task<IDictionary<string, string>> RunAsync(ManifestV2 manifest, CancellationToken ct, bool renderAudio = true)
    {
        if (manifest is null) throw new ArgumentNullException(nameof(manifest));
        ct.ThrowIfCancellationRequested();

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var transcript = await LoadTranscriptAsync(manifest.TranscriptIndexPath, jsonOptions, ct);
        var hydrate = await TryLoadHydrateAsync(manifest.TranscriptIndexPath, jsonOptions, ct);
        var asr = await LoadAsrAsync(transcript.ScriptPath, jsonOptions, ct);
        var bookIndex = await LoadBookIndexAsync(transcript.BookIndexPath, jsonOptions, ct);
        var roomtonePath = RequireRoomtone(manifest.AudioPath, bookIndex.SourceFile);

        if (!File.Exists(manifest.AudioPath))
            throw new FileNotFoundException("Audio file not found", manifest.AudioPath);

        var inputAudio   = WavIo.ReadPcmOrFloat(manifest.AudioPath);
        var roomtoneSeed = WavIo.ReadPcmOrFloat(roomtonePath);

        var seedAnalyzer = new AudioAnalysisService(roomtoneSeed);
        var roomtoneSeedStats = seedAnalyzer.AnalyzeGap(
            0.0,
            roomtoneSeed.SampleRate > 0 ? roomtoneSeed.Length / (double)roomtoneSeed.SampleRate : 0.0);

        double toneGainLinear;
        double appliedGainDb;
        if (_useAdaptiveGain)
        {
            toneGainLinear = ComputeToneGainLinear(roomtoneSeedStats.MeanRmsDb, _toneGainDb);
            appliedGainDb  = ToDb(toneGainLinear);
            if (_verbose)
            {
                Log.Info($"[Roomtone] Seed RMS {roomtoneSeedStats.MeanRmsDb:F2} dB, target {_toneGainDb:F2} dB, applied gain {appliedGainDb:F2} dB");
            }
        }
        else
        {
            toneGainLinear = 1.0;
            appliedGainDb  = 0.0;
            if (_verbose)
            {
                Log.Info($"[Roomtone] Seed RMS {roomtoneSeedStats.MeanRmsDb:F2} dB, adaptive gain disabled (unity gain)");
            }
        }

        // Build timeline on the original audio first
        var analyzer0 = new AudioAnalysisService(inputAudio);
        var entries0  = SentenceTimelineBuilder.Build(transcript.Sentences, analyzer0, asr);
        if (_verbose)
        {
            Log.Info($"[Roomtone] Timeline built on original audio: {entries0.Count} sentences.");
        }

        // Enforce exact structure (insert OR trim) and shift entries accordingly
        var norm = RoomtoneRenderer.NormalizeStructureExact(
            input: inputAudio,
            roomtoneSeed: roomtoneSeed,
            entries: entries0,
            targetSampleRate: _targetSampleRate,
            toneGainLinear: toneGainLinear,
            preRollSec: RoomtoneRenderer.DefaultPreRollSec,
            postChapterPauseSec: RoomtoneRenderer.DefaultPostChapterPauseSec,
            tailSec: RoomtoneRenderer.DefaultTailSec,
            overlapMs: (int)Math.Round(_fadeMs),
            debugDirectory: _emitDiagnostics ? manifest.ResolveStageDirectory("roomtone") : null);  // diagnostics optional  :contentReference[oaicite:4]{index=4}

        inputAudio = norm.Audio;
        var timelineEntries = norm.Entries;

        // Now plan/fill against the normalized audio
        var analyzer = new AudioAnalysisService(inputAudio);
        double audioDurationSec = inputAudio.SampleRate > 0 ? inputAudio.Length / (double)inputAudio.SampleRate : 0.0;

        Dictionary<(int? Prev, int? Next), List<TextGridGapHint>>? textGridHints = null;
        List<TextGridGapHint> textGridHintList = new();

        if (!string.IsNullOrWhiteSpace(manifest.TextGridPath))
        {
            if (File.Exists(manifest.TextGridPath))
            {
                try
                {
                    textGridHints = BuildTextGridHintMap(manifest.TextGridPath, timelineEntries, audioDurationSec, textGridHintList);
                    textGridHintList.Sort((a, b) => a.StartSec.CompareTo(b.StartSec));
                    if (_verbose)
                    {
                        Log.Info("[Roomtone] Loaded {Count} TextGrid gap hints from {Path}", textGridHintList.Count, manifest.TextGridPath);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("[Roomtone] Failed to parse TextGrid {Path}: {Message}", manifest.TextGridPath, ex.Message);
                }
            }
            else if (_verbose)
            {
                Log.Warn("[Roomtone] TextGrid not found at {Path}", manifest.TextGridPath);
            }
        }

        var gaps = BuildGaps(timelineEntries, analyzer, audioDurationSec, _verbose, out var gapSummary, _gapLeftThresholdDb, _gapRightThresholdDb, _gapStepSec, _gapBackoffSec, textGridHints);
        Log.Info($"[Roomtone] Gap candidates: {gapSummary.Candidates}, retained: {gapSummary.Retained}, collapsed: {gapSummary.Collapsed}");

        // Update transcript timings from the (shifted) entries
        var entryMap = timelineEntries.ToDictionary(e => e.SentenceId);
        var updatedSentences = transcript.Sentences
            .Select(s => entryMap.TryGetValue(s.Id, out var e) ? s with { Timing = e.Timing } : s)
            .ToList();

        HydratedTranscript? updatedHydrate = null;
        if (hydrate is not null)
        {
            var hydrateSentences = hydrate.Sentences
                .Select(s => entryMap.TryGetValue(s.Id, out var e)
                    ? s with { Timing = new TimingRange(e.Timing.StartSec, e.Timing.EndSec) }
                    : s)
                .ToList();

            updatedHydrate = hydrate with { Sentences = hydrateSentences };
        }

        // Plan/Render/Write
        var stageDir   = manifest.ResolveStageDirectory("roomtone");
        EnsureDirectory(stageDir);

        var planPath     = Path.Combine(stageDir, "plan.json");
        var timelinePath = Path.Combine(stageDir, "timeline.json");
        var metaPath     = Path.Combine(stageDir, "meta.json");
        var paramsPath   = Path.Combine(stageDir, "params.snapshot.json");

        var plan = BuildPlan(manifest, inputAudio, roomtonePath, timelineEntries, gaps,
                             roomtoneSeedStats.MeanRmsDb, appliedGainDb, _targetSampleRate, _toneGainDb, _fadeMs); // :contentReference[oaicite:6]{index=6}

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
                fadeMs: _fadeMs,
                debugDirectory: _emitDiagnostics ? stageDir : null);                                  // :contentReference[oaicite:7]{index=7}

            wavPath = Path.Combine(stageDir, "roomtone.wav");
            WavIo.WriteFloat32(wavPath, rendered);
        }

        var audioTargetPath = wavPath ?? manifest.AudioPath;
        if (!string.IsNullOrWhiteSpace(audioTargetPath))
        {
            audioTargetPath = Path.GetFullPath(audioTargetPath);
        }

        var updatedTranscript = transcript with
        {
            AudioPath = string.IsNullOrWhiteSpace(audioTargetPath) ? transcript.AudioPath : audioTargetPath,
            Sentences = updatedSentences
        };

        if (updatedHydrate is not null && !string.IsNullOrWhiteSpace(audioTargetPath))
        {
            updatedHydrate = updatedHydrate with { AudioPath = audioTargetPath };
        }

        await WriteTimelineAsync(timelineEntries, manifest, timelinePath, ct);
        await WriteUpdatedTimingsAsync(manifest.TranscriptIndexPath, updatedTranscript, updatedHydrate, jsonOptions, ct);
        await WritePlanAsync(plan, planPath, ct);
        await WriteMetaAsync(manifest, wavPath, roomtonePath, planPath, metaPath, ct);
        await WriteParamsAsync(paramsPath, _toneGainDb, roomtoneSeedStats.MeanRmsDb, appliedGainDb, _fadeMs, _useAdaptiveGain, _emitDiagnostics, _gapLeftThresholdDb, _gapRightThresholdDb, _gapStepSec, _gapBackoffSec, ct);

        string? textGridGapsPath = null;
        if (textGridHintList.Count > 0)
        {
            textGridGapsPath = Path.Combine(stageDir, "textgrid.gaps.json");
            var hintJson = JsonSerializer.Serialize(textGridHintList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(textGridGapsPath, hintJson, ct);
        }

        var outputs = new Dictionary<string, string>
        {
            ["plan"] = planPath,
            ["timeline"] = timelinePath,
            ["meta"] = metaPath,
            ["params"] = paramsPath
        };
        if (wavPath is not null) outputs["roomtoneWav"] = wavPath;
        outputs["transcript"] = manifest.TranscriptIndexPath;
        var hydratePath = DeriveHydratePath(manifest.TranscriptIndexPath);
        if (hydratePath is not null && File.Exists(hydratePath))
        {
            outputs["hydrate"] = hydratePath;
        }
        if (!string.IsNullOrWhiteSpace(textGridGapsPath)) outputs["textgridGaps"] = textGridGapsPath;
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

    private static string? DeriveHydratePath(string transcriptPath)
    {
        if (string.IsNullOrWhiteSpace(transcriptPath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(transcriptPath);
        var fileName = Path.GetFileName(transcriptPath);
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        string hydrateName;
        const string TxSuffix = ".tx.json";
        if (fileName.EndsWith(TxSuffix, StringComparison.OrdinalIgnoreCase))
        {
            hydrateName = fileName[..^TxSuffix.Length] + ".hydrate.json";
        }
        else
        {
            hydrateName = Path.ChangeExtension(fileName, ".hydrate.json") ?? fileName + ".hydrate.json";
        }

        return directory is null ? hydrateName : Path.Combine(directory, hydrateName);
    }

    private static async Task<HydratedTranscript?> TryLoadHydrateAsync(string transcriptPath, JsonSerializerOptions options, CancellationToken ct)
    {
        var hydratePath = DeriveHydratePath(transcriptPath);
        if (hydratePath is null || !File.Exists(hydratePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(hydratePath, ct);
        return JsonSerializer.Deserialize<HydratedTranscript>(json, options);
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

    private static async Task WriteUpdatedTimingsAsync(
        string transcriptPath,
        TranscriptIndex updatedTranscript,
        HydratedTranscript? updatedHydrate,
        JsonSerializerOptions options,
        CancellationToken ct)
    {
        var writeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = options.PropertyNamingPolicy,
            WriteIndented = true
        };

        Directory.CreateDirectory(Path.GetDirectoryName(transcriptPath) ?? Environment.CurrentDirectory);
        var transcriptJson = JsonSerializer.Serialize(updatedTranscript, writeOptions);
        await File.WriteAllTextAsync(transcriptPath, transcriptJson, ct);

        if (updatedHydrate is null)
        {
            return;
        }

        var hydratePath = DeriveHydratePath(transcriptPath);
        if (hydratePath is null)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(hydratePath) ?? Environment.CurrentDirectory);
        var hydrateJson = JsonSerializer.Serialize(updatedHydrate, writeOptions);
        await File.WriteAllTextAsync(hydratePath, hydrateJson, ct);
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

    private static Dictionary<(int? Prev, int? Next), List<TextGridGapHint>> BuildTextGridHintMap(
        string textGridPath,
        IReadOnlyList<SentenceTimelineEntry> sentences,
        double audioDurationSec,
        List<TextGridGapHint> collected)
    {
        var map = new Dictionary<(int? Prev, int? Next), List<TextGridGapHint>>();

        if (!File.Exists(textGridPath))
        {
            return map;
        }

        IReadOnlyList<TextGridInterval> intervals;
        try
        {
            intervals = TextGridParser.ParseWordIntervals(textGridPath);
        }
        catch
        {
            throw;
        }

        if (intervals.Count == 0 || sentences.Count == 0)
        {
            return map;
        }

        foreach (var interval in intervals)
        {
            var label = (interval.Text ?? string.Empty).Trim();
            if (!IsSilenceLabel(label))
            {
                continue;
            }

            double start = Math.Clamp(interval.Start, 0.0, audioDurationSec);
            double end = Math.Clamp(interval.End, 0.0, audioDurationSec);
            if (end - start <= TextGridMinGapLengthSec)
            {
                continue;
            }

            var containing = FindContainingSentence(sentences, start, end);
            if (containing is not null)
            {
                continue;
            }

            var previous = FindPreviousSentence(sentences, start);
            var next = FindNextSentence(sentences, end);

            if (previous is null && next is null)
            {
                continue;
            }

            if (previous?.SentenceId == next?.SentenceId)
            {
                continue;
            }

            var hint = new TextGridGapHint(start, end, previous?.SentenceId, next?.SentenceId, label);
            collected.Add(hint);

            var key = (hint.PreviousSentenceId, hint.NextSentenceId);
            if (!map.TryGetValue(key, out var list))
            {
                list = new List<TextGridGapHint>();
                map[key] = list;
            }

            list.Add(hint);
        }

        return map;
    }

    private static bool IsSilenceLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return true;
        }

        label = label.Trim();
        if (label.Length <= 3)
        {
            return label.Equals("sp", StringComparison.OrdinalIgnoreCase)
                || label.Equals("sil", StringComparison.OrdinalIgnoreCase);
        }

        return label.Equals("<sil>", StringComparison.OrdinalIgnoreCase)
            || label.Equals("<s>", StringComparison.OrdinalIgnoreCase)
            || label.Equals("</s>", StringComparison.OrdinalIgnoreCase)
            || label.Equals("silence", StringComparison.OrdinalIgnoreCase);
    }

    private static SentenceTimelineEntry? FindPreviousSentence(IReadOnlyList<SentenceTimelineEntry> sentences, double start)
    {
        for (int i = sentences.Count - 1; i >= 0; i--)
        {
            var entry = sentences[i];
            if (entry.Timing.EndSec <= start + TextGridGapToleranceSec)
            {
                return entry;
            }
        }

        return null;
    }

    private static SentenceTimelineEntry? FindNextSentence(IReadOnlyList<SentenceTimelineEntry> sentences, double end)
    {
        for (int i = 0; i < sentences.Count; i++)
        {
            var entry = sentences[i];
            if (entry.Timing.StartSec >= end - TextGridGapToleranceSec)
            {
                return entry;
            }
        }

        return null;
    }

    private static SentenceTimelineEntry? FindContainingSentence(IReadOnlyList<SentenceTimelineEntry> sentences, double start, double end)
    {
        foreach (var entry in sentences)
        {
            if (start >= entry.Timing.StartSec - TextGridGapToleranceSec &&
                end   <= entry.Timing.EndSec   + TextGridGapToleranceSec)
            {
                return entry;
            }
        }

        return null;
    }

    private static IReadOnlyList<RoomtonePlanGap> BuildGaps(
        IReadOnlyList<SentenceTimelineEntry> entries,
        AudioAnalysisService analyzer,
        double audioDurationSec,
        bool verbose,
        out GapSummary summary,
        double leftThresholdDb,
        double rightThresholdDb,
        double stepSec,
        double backoffSec,
        Dictionary<(int? Prev, int? Next), List<TextGridGapHint>>? textGridHints = null)
    {
        const double epsilon = 1e-6;

        var gaps = new List<RoomtonePlanGap>();
        int candidates = 0;
        int collapsed = 0;

        if (audioDurationSec <= epsilon)
        {
            Log.Warn("[Roomtone] Audio too short for gap analysis.");
            summary = new GapSummary(0, 0, 0);
            return gaps;
        }

        var orderedEntries = entries
            .OrderBy(e => e.Timing.StartSec)
            .ThenBy(e => e.Timing.EndSec)
            .ToList();

        if (orderedEntries.Count == 0)
        {
            Log.Warn("[Roomtone] No sentence timings; treating entire chapter as a single gap.");
            var stats = analyzer.AnalyzeGap(0.0, audioDurationSec);
            gaps.Add(CreateGap(0.0, audioDurationSec, null, null, stats));
            summary = new GapSummary(1, gaps.Count, 0);
            return gaps;
        }

        SentenceTimelineEntry? previous = null;
        foreach (var current in orderedEntries)
        {
            candidates++;
            var produced = ProcessGap(previous, current);
            if (produced.Count == 0)
            {
                collapsed++;
            }
            else
            {
                gaps.AddRange(produced);
            }
            previous = current;
        }

        candidates++;
        var tailProduced = ProcessGap(previous, null);
        if (tailProduced.Count == 0)
        {
            collapsed++;
        }
        else
        {
            gaps.AddRange(tailProduced);
        }

        summary = new GapSummary(candidates, gaps.Count, collapsed);
        return gaps;

        List<RoomtonePlanGap> ProcessGap(SentenceTimelineEntry? leftEntry, SentenceTimelineEntry? rightEntry)
        {
            var produced = new List<RoomtonePlanGap>();

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
                return produced;
            }

            double leftClamp = leftEntry?.Timing.EndSec ?? initialStart;
            double rightClamp = rightEntry?.Timing.StartSec ?? initialEnd;

            initialStart = Math.Max(initialStart, Math.Min(leftClamp, rightClamp));
            initialEnd = Math.Min(initialEnd, rightClamp);
            if (initialEnd - initialStart <= epsilon)
            {
                return produced;
            }

            var hintKey = (leftEntry?.SentenceId, rightEntry?.SentenceId);
            if (textGridHints is not null && textGridHints.TryGetValue(hintKey, out var hintRanges) && hintRanges.Count > 0)
            {
                foreach (var hint in hintRanges)
                {
                    AddHintRange(hint);
                }
                textGridHints.Remove(hintKey);
                return produced;
            }

            if (verbose)
            {
                string label = $"prev={(leftEntry?.SentenceId.ToString() ?? "null")} next={(rightEntry?.SentenceId.ToString() ?? "null")}";
                Log.Info($"[Roomtone] Gap candidate {label} base=({initialStart:F3},{initialEnd:F3}) dur={(initialEnd - initialStart):F3}");
            }

            double midpoint = (initialStart + initialEnd) / 2.0;
            midpoint = Math.Clamp(midpoint, initialStart + epsilon, initialEnd - epsilon);

            if (midpoint <= initialStart + epsilon || initialEnd <= midpoint + epsilon)
            {
                if (verbose)
                {
                    Log.Info("[Roomtone]   midpoint collapsed; skipping gap.");
                }
                return produced;
            }

            bool leftAccepted = TryCalibrateLeft(initialStart, midpoint, leftThresholdDb, out double leftStart);
            bool rightAccepted = TryCalibrateRight(midpoint, initialEnd, rightThresholdDb, out double rightEnd);

            RoomtonePlanGap? leftGap = null;
            RoomtonePlanGap? rightGap = null;

            if (leftAccepted && midpoint - leftStart > epsilon)
            {
                double safeLeftStart = Math.Clamp(Math.Max(leftStart, leftClamp), initialStart, rightClamp);

                if (midpoint - safeLeftStart > epsilon)
                {
                    var stats = analyzer.AnalyzeGap(safeLeftStart, midpoint);
                    if (stats.MaxRmsDb > leftThresholdDb)
                    {
                        if (verbose)
                        {
                            Log.Info($"[Roomtone]   left rejected [{safeLeftStart:F3},{midpoint:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {leftThresholdDb:F2} dB");
                        }
                    }
                    else
                    {
                        leftGap = CreateGap(safeLeftStart, midpoint, leftEntry?.SentenceId, rightEntry?.SentenceId, stats);
                        if (verbose)
                        {
                            Log.Info($"[Roomtone]   left accepted [{safeLeftStart:F3},{midpoint:F3}] meanRms={stats.MeanRmsDb:F2} dB");
                        }
                    }
                }
                else if (verbose)
                {
                    Log.Info("[Roomtone]   left collapsed after safety clamp.");
                }
            }
            else if (verbose)
            {
                Log.Info("[Roomtone]   left rejected or collapsed.");
            }

            if (rightAccepted && rightEnd - midpoint > epsilon)
            {
                double safeRightEnd = Math.Clamp(Math.Min(rightEnd, rightClamp), initialStart, rightClamp);

                if (safeRightEnd - midpoint > epsilon)
                {
                    var stats = analyzer.AnalyzeGap(midpoint, safeRightEnd);
                    if (stats.MaxRmsDb > rightThresholdDb)
                    {
                        if (verbose)
                        {
                        Log.Info($"[Roomtone]   right rejected [{midpoint:F3},{safeRightEnd:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {rightThresholdDb:F2} dB");
                        }
                    }
                    else
                    {
                        Log.Info($"[Roomtone]   right accepted [{midpoint:F3},{safeRightEnd:F3}] meanRms={stats.MeanRmsDb:F2} dB");
                        rightGap = CreateGap(midpoint, safeRightEnd, leftEntry?.SentenceId, rightEntry?.SentenceId, stats);
                    }
                }
                else if (verbose)
                {
                    Log.Info("[Roomtone]   right collapsed after safety clamp.");
                }
            }
            else if (verbose)
            {
                Log.Info("[Roomtone]   right rejected or collapsed.");
            }

            if (leftGap is not null && rightGap is not null)
            {
                double finalStart = Math.Max(leftGap.StartSec, leftClamp);
                double finalEnd = Math.Min(rightGap.EndSec, rightClamp);
                if (finalEnd - finalStart <= epsilon)
                {
                    if (verbose)
                    {
                        Log.Info("[Roomtone]   final collapsed after clamps.");
                    }
                    return produced;
                }

                var stats = analyzer.AnalyzeGap(finalStart, finalEnd);
                double combinedThreshold = Math.Min(leftThresholdDb, rightThresholdDb);
                if (stats.MaxRmsDb > combinedThreshold)
                {
                    if (verbose)
                    {
                        Log.Info($"[Roomtone]   final rejected [{finalStart:F3},{finalEnd:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {combinedThreshold:F2} dB");
                    }
                }
                else
                {
                    if (verbose)
                    {
                        Log.Info($"[Roomtone]   final gap [{finalStart:F3},{finalEnd:F3}] meanRms={stats.MeanRmsDb:F2} dB");
                    }
                    produced.Add(CreateGap(finalStart, finalEnd, leftEntry?.SentenceId, rightEntry?.SentenceId, stats));
                }
            }
            else
            {
                if (leftGap is not null)
                {
                    double start = Math.Max(leftGap.StartSec, leftClamp);
                    double end = Math.Min(leftGap.EndSec, rightClamp);
                    if (end - start > epsilon)
                    {
                        var stats = analyzer.AnalyzeGap(start, end);
                        if (stats.MaxRmsDb > leftThresholdDb)
                        {
                            if (verbose)
                            {
                                Log.Info($"[Roomtone]   left-only rejected [{start:F3},{end:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {leftThresholdDb:F2} dB");
                            }
                        }
                        else
                        {
                        produced.Add(CreateGap(start, end, leftEntry?.SentenceId, rightEntry?.SentenceId, stats));
                        }
                    }
                }
                if (rightGap is not null)
                {
                    double start = Math.Max(rightGap.StartSec, leftClamp);
                    double end = Math.Min(rightGap.EndSec, rightClamp);
                    if (end - start > epsilon)
                    {
                        var stats = analyzer.AnalyzeGap(start, end);
                        if (stats.MaxRmsDb > rightThresholdDb)
                        {
                            if (verbose)
                            {
                                Log.Info($"[Roomtone]   right-only rejected [{start:F3},{end:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {rightThresholdDb:F2} dB");
                            }
                        }
                        else
                        {
                            produced.Add(CreateGap(start, end, leftEntry?.SentenceId, rightEntry?.SentenceId, stats));
                        }
                    }
                }
            }

            return produced;

            void AddHintRange(TextGridGapHint hint)
            {
                double start = Math.Clamp(hint.StartSec, initialStart, initialEnd);
                double end = Math.Clamp(hint.EndSec, initialStart, initialEnd);
                if (leftEntry is not null)
                {
                    start = Math.Max(start, leftEntry.Timing.EndSec);
                }
                if (rightEntry is not null)
                {
                    end = Math.Min(end, rightEntry.Timing.StartSec);
                }
                start = Math.Max(start, leftClamp);
                end = Math.Min(end, rightClamp);
                if (end - start <= epsilon)
                {
                    if (verbose)
                    {
                        Log.Info("[Roomtone]   hint collapsed after clamping; skipping.");
                    }
                    return;
                }

                var stats = analyzer.AnalyzeGap(start, end);
                double hintThreshold = Math.Min(leftThresholdDb, rightThresholdDb);
                if (stats.MaxRmsDb > hintThreshold)
                {
                    if (verbose)
                    {
                        Log.Info($"[Roomtone]   hint rejected [{start:F3},{end:F3}] maxRms={stats.MaxRmsDb:F2} dB > threshold {hintThreshold:F2} dB");
                    }
                    return;
                }

                var prevId = hint.PreviousSentenceId ?? leftEntry?.SentenceId;
                var nextId = hint.NextSentenceId ?? rightEntry?.SentenceId;
                produced.Add(CreateGap(start, end, prevId, nextId, stats));
            }
        }

        bool TryCalibrateLeft(double initialStart, double end, double thresholdDb, out double result)
        {
            double boundary = initialStart;
            result = double.NaN;
            while (boundary + epsilon < end)
            {
                double rms = analyzer.MeasureRms(boundary, end);
                if (verbose)
                {
                    Log.Info($"[Roomtone]     test left [{boundary:F3},{end:F3}] rms={rms:F2} dB");
                }
                if (rms <= thresholdDb)
                {
                    double candidate = boundary;
                    double backoff = Math.Max(initialStart, candidate - backoffSec);
                    if (backoff < candidate)
                    {
                        double backoffRms = analyzer.MeasureRms(backoff, end);
                        if (backoffRms <= thresholdDb)
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

        bool TryCalibrateRight(double start, double initialEnd, double thresholdDb, out double result)
        {
            double boundary = initialEnd;
            result = double.NaN;
            while (boundary - epsilon > start)
            {
                double rms = analyzer.MeasureRms(start, boundary);
                if (verbose)
                {
                    Log.Info($"[Roomtone]     test right [{start:F3},{boundary:F3}] rms={rms:F2} dB");
                }
                if (rms <= thresholdDb)
                {
                    double candidate = boundary;
                    double backoff = Math.Min(initialEnd, candidate + backoffSec);
                    if (backoff > candidate)
                    {
                        double backoffRms = analyzer.MeasureRms(start, backoff);
                        if (backoffRms <= thresholdDb)
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
            SanitizeDb(stats.MinRmsDb),
            SanitizeDb(stats.MaxRmsDb),
            SanitizeDb(stats.MeanRmsDb),
            SanitizeFraction(stats.SilenceFraction));
    }

    private static double SanitizeDb(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return -120.0;
        }
        return value;
    }

    private static double SanitizeFraction(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 1.0;
        }
        return Math.Clamp(value, 0.0, 1.0);
    }

    private readonly struct GapSummary
    {
        public GapSummary(int candidates, int retained, int collapsed)
        {
            Candidates = candidates;
            Retained = retained;
            Collapsed = collapsed;
        }

        public int Candidates { get; }
        public int Retained { get; }
        public int Collapsed { get; }
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

    private static async Task WriteParamsAsync(
        string path,
        double targetRmsDb,
        double roomtoneSeedRmsDb,
        double appliedGainDb,
        double fadeMs,
        bool adaptiveGainEnabled,
        bool diagnosticsEnabled,
        double gapLeftThresholdDb,
        double gapRightThresholdDb,
        double gapStepSec,
        double gapBackoffSec,
        CancellationToken ct)
    {
        var snapshot = new
        {
            parameters = new
            {
                targetRmsDb,
                roomtoneSeedRmsDb,
                appliedGainDb,
                fadeMs,
                adaptiveGainEnabled,
                diagnosticsEnabled,
                gapLeftThresholdDb,
                gapRightThresholdDb,
                gapStepMs = gapStepSec * 1000.0,
                gapBackoffMs = gapBackoffSec * 1000.0,
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




