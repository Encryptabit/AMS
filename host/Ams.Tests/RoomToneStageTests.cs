using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Processors;
using Ams.Core.Runtime.Documents;
using Ams.Core.Pipeline;
using Xunit;

namespace Ams.Tests.Audio;

public class RoomToneStageTests
{
    private static bool FiltersUnavailable()
    {
        try
        {
            return !Ams.Core.Services.Integrations.FFmpeg.FfSession.FiltersAvailable;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            Console.WriteLine($"Skipping roomtone FFmpeg-dependent test: {ex.Message}");
            return true;
        }
    }

    private const int FirstSentenceId = 91;
    private const int SecondSentenceId = 92;

    [Fact]
    public async Task RunAsync_ThrowsWhenRoomtoneMissing()
    {
        if (FiltersUnavailable()) return;
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: false);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: -60, fadeMs: 5, emitDiagnostics: false, useAdaptiveGain: true);

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => stage.RunAsync(context.Manifest, CancellationToken.None));
        Assert.Contains("roomtone.wav", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_WithRoomtone_GeneratesNormalizedOutputs()
    {
        if (FiltersUnavailable()) return;
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: true);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: -60, fadeMs: 5, emitDiagnostics: false, useAdaptiveGain: true);

        var outputs = await stage.RunAsync(context.Manifest, CancellationToken.None);

        Assert.True(outputs.TryGetValue("roomtoneWav", out var roomtonePath));
        Assert.True(File.Exists(roomtonePath));

        Assert.True(outputs.TryGetValue("plan", out var planPath));
        Assert.True(File.Exists(planPath));

        Assert.True(outputs.TryGetValue("timeline", out var timelinePath));
        Assert.True(File.Exists(timelinePath));

        var rendered = AudioProcessor.Decode(roomtonePath);
        Assert.Equal(8000, rendered.SampleRate);
        Assert.Equal(1, rendered.Channels);

        using var planDoc = JsonDocument.Parse(await File.ReadAllTextAsync(planPath));
        var planRoot = planDoc.RootElement;
        Assert.Equal(RoomtonePlanVersion.Current, planRoot.GetProperty("version").GetString());
        Assert.Equal(context.Manifest.AudioPath, planRoot.GetProperty("audioPath").GetString());
        Assert.Equal(context.RoomtonePath, planRoot.GetProperty("roomtoneSeedPath").GetString());

        var gaps = planRoot.GetProperty("gaps");
        Assert.True(gaps.GetArrayLength() > 0);

        double preGapSec = AggregateGapDuration(gaps, previousSentenceId: null, nextSentenceId: FirstSentenceId);
        Assert.InRange(preGapSec, 0.90, 1.00);

        double interGapSec = AggregateGapDuration(gaps, previousSentenceId: FirstSentenceId, nextSentenceId: SecondSentenceId);
        Assert.InRange(interGapSec, 1.65, 1.80);

        double tailGapSec = AggregateGapDuration(gaps, previousSentenceId: SecondSentenceId, nextSentenceId: null);
        Assert.InRange(tailGapSec, 3.80, 4.10);

        double durationSec = planRoot.GetProperty("audioDurationSec").GetDouble();

        using var timelineDoc = JsonDocument.Parse(await File.ReadAllTextAsync(timelinePath));
        var sentencesJson = timelineDoc.RootElement.GetProperty("sentences");
        Assert.Equal(2, sentencesJson.GetArrayLength());

        var first = sentencesJson[0];
        var second = sentencesJson[1];

        double firstStart = first.GetProperty("startSec").GetDouble();
        double firstEnd = first.GetProperty("endSec").GetDouble();
        double secondStart = second.GetProperty("startSec").GetDouble();
        double secondEnd = second.GetProperty("endSec").GetDouble();

        Assert.InRange(firstStart, 0.90, 1.00);
        Assert.True(firstEnd > firstStart);

        Assert.InRange(secondStart - firstEnd, 1.30, 1.80);
        Assert.InRange(durationSec - secondEnd, 2.90, 3.10);

        Assert.True(outputs.TryGetValue("meta", out var metaPath));
        using var metaDoc = JsonDocument.Parse(await File.ReadAllTextAsync(metaPath));
        var outputsJson = metaDoc.RootElement.GetProperty("outputs");
        Assert.Equal(roomtonePath, outputsJson.GetProperty("roomtone").GetString());
        Assert.Equal(planPath, outputsJson.GetProperty("plan").GetString());
    }

    [Fact]
    public void RenderWithSentenceMasks_FillsPlanGapsAndPreservesSpeech()
    {
        if (FiltersUnavailable()) return;
        const int sampleRate = 1000;
        var input = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: sampleRate);
        FillRange(input, 0.2, 0.4, 0.6f);

        var tone = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: 16);
        FillRange(tone, 0.0, tone.Length / (double)sampleRate, 0.1f);

        var sentences = new[]
        {
            new SentenceAlign(1, new IntRange(0, 2), new ScriptRange(0, 1), new TimingRange(0.2, 0.4), new SentenceMetrics(0, 0, 0, 0, 0), "ok")
        };

        var gaps = new List<RoomtonePlanGap>
        {
            new RoomtonePlanGap(0.0, 0.18, 0.18, null, 1, -70, -60, -65, 1.0, Array.Empty<RoomtoneBreathRegion>()),
            new RoomtonePlanGap(0.42, 0.80, 0.38, 1, null, -70, -60, -65, 1.0, Array.Empty<RoomtoneBreathRegion>())
        };

        var output = RoomtoneRenderer.RenderWithSentenceMasks(
            input,
            tone,
            gaps,
            sentences,
            targetSampleRate: sampleRate,
            toneGainLinear: 1.0,
            fadeMs: 5.0,
            debugDirectory: null);

        Assert.Equal(input.SampleRate, output.SampleRate);
        Assert.Equal(input.Length, output.Length);

        int gapSample = (int)(0.05 * sampleRate);
        Assert.NotEqual(0f, output.Planar[0][gapSample]);

        int speechSample = (int)(0.3 * sampleRate);
        Assert.Equal(input.Planar[0][speechSample], output.Planar[0][speechSample]);
    }

    [Fact]
    public async Task RunAsync_DisabledRendering_WritesPlanAndMetadataOnly()
    {
        if (FiltersUnavailable()) return;
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: true);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: -60, fadeMs: 5, emitDiagnostics: false, useAdaptiveGain: true);

        var outputs = await stage.RunAsync(context.Manifest, CancellationToken.None, renderAudio: false);

        Assert.False(outputs.ContainsKey("roomtoneWav"));
        Assert.True(outputs.TryGetValue("plan", out var planPath));
        Assert.True(File.Exists(planPath));
        Assert.True(outputs.TryGetValue("timeline", out var timelinePath));
        Assert.True(File.Exists(timelinePath));

        using var paramsDoc = JsonDocument.Parse(await File.ReadAllTextAsync(outputs["params"]));
        var parameters = paramsDoc.RootElement.GetProperty("parameters");
        Assert.False(parameters.GetProperty("diagnosticsEnabled").GetBoolean());
        Assert.True(parameters.GetProperty("adaptiveGainEnabled").GetBoolean());
    }

    private static async Task<TestContext> ArrangeAsync(TempDir temp, bool createExistingRoomtone)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        var workDir = temp.Path;
        var audioDir = Path.Combine(workDir, "audio");
        Directory.CreateDirectory(audioDir);

        var audioPath = Path.Combine(audioDir, "chapter.wav");
        var asrPath = Path.Combine(audioDir, "chapter.asr.json");

        const int sampleRate = 8000;
        var audio = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: sampleRate * 8);
        FillRange(audio, 1.0, 1.6, 0.6f);
        FillRange(audio, 3.4, 4.0, 0.55f);
        AudioProcessor.EncodeWav(audioPath, audio, new AudioEncodeOptions { TargetBitDepth = 32 });

        var tokens = new[]
        {
            new AsrToken(1.00, 0.30, "hello"),
            new AsrToken(1.35, 0.25, "world"),
            new AsrToken(3.40, 0.30, "second"),
            new AsrToken(3.75, 0.25, "passage")
        };
        var asr = new AsrResponse("test-model", tokens);
        await File.WriteAllTextAsync(asrPath, JsonSerializer.Serialize(asr, options));

        var manuscriptDir = Path.Combine(workDir, "manuscript");
        Directory.CreateDirectory(manuscriptDir);
        var docPath = Path.Combine(manuscriptDir, "BATTLESPACE NOMAD.docx");
        await File.WriteAllTextAsync(docPath, "dummy");

        var bookIndexPath = Path.Combine(workDir, "book-index.json");
        var bookIndex = new BookIndex(
            SourceFile: docPath,
            SourceFileHash: "hash",
            IndexedAt: DateTime.UtcNow,
            Title: null,
            Author: null,
            Totals: new BookTotals(0, 0, 0, 0),
            Words: Array.Empty<BookWord>(),
            Sentences: Array.Empty<SentenceRange>(),
            Paragraphs: Array.Empty<ParagraphRange>(),
            Sections: Array.Empty<SectionRange>(),
            BuildWarnings: null);
        await File.WriteAllTextAsync(bookIndexPath, JsonSerializer.Serialize(bookIndex, options));

        var sentences = new[]
        {
            new SentenceAlign(FirstSentenceId, new IntRange(0, 10), new ScriptRange(0, 1), new TimingRange(1.0, 1.6), new SentenceMetrics(0, 0, 0, 0, 0), "ok"),
            new SentenceAlign(SecondSentenceId, new IntRange(11, 20), new ScriptRange(2, 3), new TimingRange(3.4, 4.0), new SentenceMetrics(0, 0, 0, 0, 0), "ok")
        };
        var transcript = new TranscriptIndex(
            AudioPath: audioPath,
            ScriptPath: asrPath,
            BookIndexPath: bookIndexPath,
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "test",
            Words: Array.Empty<WordAlign>(),
            Sentences: sentences,
            Paragraphs: Array.Empty<ParagraphAlign>());

        var transcriptPath = Path.Combine(workDir, "chapter.tx.json");
        await File.WriteAllTextAsync(transcriptPath, JsonSerializer.Serialize(transcript, options));

        string? roomtonePath = null;
        if (createExistingRoomtone)
        {
            roomtonePath = Path.Combine(audioDir, "roomtone.wav");
            var roomtone = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: sampleRate / 4);
            FillRange(roomtone, 0.0, roomtone.Length / (double)sampleRate, 0.02f);
            AudioProcessor.EncodeWav(roomtonePath, roomtone, new AudioEncodeOptions { TargetBitDepth = 32 });
        }

        var manifest = new ManifestV2("chapter", workDir, audioPath, transcriptPath);
        return new TestContext(manifest, roomtonePath);
    }

    private static double AggregateGapDuration(JsonElement gaps, int? previousSentenceId, int? nextSentenceId)
    {
        double total = 0;
        foreach (var gap in gaps.EnumerateArray())
        {
            var prevProp = gap.GetProperty("previousSentenceId");
            var nextProp = gap.GetProperty("nextSentenceId");

            bool prevMatches = previousSentenceId is null
                ? prevProp.ValueKind == JsonValueKind.Null
                : prevProp.ValueKind == JsonValueKind.Number && prevProp.GetInt32() == previousSentenceId.Value;

            bool nextMatches = nextSentenceId is null
                ? nextProp.ValueKind == JsonValueKind.Null
                : nextProp.ValueKind == JsonValueKind.Number && nextProp.GetInt32() == nextSentenceId.Value;

            if (prevMatches && nextMatches)
            {
                total += gap.GetProperty("durationSec").GetDouble();
            }
        }

        return total;
    }

    private static void FillRange(AudioBuffer buffer, double startSec, double endSec, float value)
    {
        int start = (int)Math.Round(startSec * buffer.SampleRate);
        int end = (int)Math.Round(endSec * buffer.SampleRate);

        start = Math.Clamp(start, 0, buffer.Length);
        end = Math.Clamp(end, 0, buffer.Length);

        if (end <= start)
        {
            return;
        }

        var channel = buffer.Planar[0];
        for (int i = start; i < end; i++)
        {
            channel[i] = value;
        }
    }

    private sealed record TestContext(ManifestV2 Manifest, string? RoomtonePath);

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ams-tests-" + Guid.NewGuid().ToString("N"));

        public TempDir()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
