using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Core.Artifacts;
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

    public async Task<IDictionary<string, string>> RunAsync(ManifestV2 manifest, CancellationToken ct)
    {
        if (manifest is null) throw new ArgumentNullException(nameof(manifest));
        ct.ThrowIfCancellationRequested();

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var transcript = await LoadTranscriptAsync(manifest.TranscriptIndexPath, jsonOptions, ct);
        var asr = await LoadAsrAsync(transcript.ScriptPath, jsonOptions, ct);
        var bookIndex = await LoadBookIndexAsync(transcript.BookIndexPath, jsonOptions, ct);

        if (!File.Exists(manifest.AudioPath))
            throw new FileNotFoundException("Audio file not found", manifest.AudioPath);

        var inputAudio = WavIo.ReadPcmOrFloat(manifest.AudioPath);
        var analyzer = new AudioAnalysisService(inputAudio);
        var timelineEntries = SentenceTimelineBuilder.Build(transcript.Sentences, analyzer);

        var entryMap = timelineEntries.ToDictionary(e => e.SentenceId);
        var updatedSentences = transcript.Sentences
            .Select(s => entryMap.TryGetValue(s.Id, out var entry) ? s with { Timing = entry.Timing } : s)
            .ToList();

        var stageDir = manifest.ResolveStageDirectory("roomtone");
        var wavPath = Path.Combine(stageDir, "roomtone.wav");
        var timelinePath = Path.Combine(stageDir, "timeline.json");
        var metaPath = Path.Combine(stageDir, "meta.json");
        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");

        var existingRoomtone = TryLocateRoomtone(manifest.AudioPath, bookIndex.SourceFile);
        bool usedExisting = existingRoomtone is not null;

        if (usedExisting)
        {
            EnsureDirectory(stageDir);
            File.Copy(existingRoomtone!, wavPath, overwrite: true);
        }
        else
        {
            var rendered = RoomtoneRenderer.RenderWithSentenceMasks(
                input: inputAudio,
                asr: asr,
                sentences: updatedSentences,
                targetSampleRate: _targetSampleRate,
                toneGainDb: _toneGainDb,
                fadeMs: _fadeMs);

            WavIo.WriteInt16Pcm(wavPath, rendered);
        }

        await WriteTimelineAsync(timelineEntries, manifest, timelinePath, ct);
        await WriteMetaAsync(manifest, wavPath, usedExisting ? existingRoomtone : null, metaPath, ct);
        await WriteParamsAsync(paramsPath, _toneGainDb, _fadeMs, usedExisting, ct);

        return new Dictionary<string, string>
        {
            ["roomtoneWav"] = wavPath,
            ["timeline"] = timelinePath,
            ["meta"] = metaPath,
            ["params"] = paramsPath
        };
    }

    private static string? TryLocateRoomtone(string audioPath, string? docPath)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidateDirectory(Path.GetDirectoryName(audioPath));
        AddCandidateDirectory(Path.GetDirectoryName(docPath));

        foreach (var dir in candidates)
        {
            if (!Directory.Exists(dir)) continue;
            var match = Directory.EnumerateFiles(dir, "roomtone.wav", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), "roomtone.wav", StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
        }

        return null;

        void AddCandidateDirectory(string? directory)
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                candidates.Add(Path.GetFullPath(directory));
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
                fragmentBacked = e.Timing.FragmentBacked,
                hasTiming = e.HasTiming,
                confidence = e.Timing.Confidence
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static async Task WriteMetaAsync(ManifestV2 manifest, string wavPath, string? sourceRoomtone, string path, CancellationToken ct)
    {
        var meta = new
        {
            stage = "roomtone",
            chapter = manifest.ChapterId,
            generatedAtUtc = DateTime.UtcNow,
            outputs = new { roomtone = wavPath },
            sourceRoomtone
        };

        var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }

    private static async Task WriteParamsAsync(string path, double toneGainDb, double fadeMs, bool usedExisting, CancellationToken ct)
    {
        var snapshot = new
        {
            parameters = new { toneGainDb, fadeMs, usedExistingRoomtone = usedExisting }
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
