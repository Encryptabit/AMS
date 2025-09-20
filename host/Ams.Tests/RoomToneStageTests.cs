using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Book;
using Ams.Core.Pipeline;
using Xunit;

namespace Ams.Tests.Audio;

public class RoomToneStageTests
{
    [Fact]
    public async Task RunAsync_ThrowsWhenRoomtoneMissing()
    {
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: false);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: 0, fadeMs: 0);
        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => stage.RunAsync(context.Manifest, CancellationToken.None));

        Assert.Contains("roomtone.wav", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_ReplacesGapsWithLoopedRoomtone()
    {
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: true);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: 0, fadeMs: 0);
        var outputs = await stage.RunAsync(context.Manifest, CancellationToken.None);

        Assert.True(outputs.TryGetValue("roomtoneWav", out var wavPath));
        Assert.True(File.Exists(wavPath));

        Assert.True(outputs.TryGetValue("plan", out var planPath));
        Assert.True(File.Exists(planPath));

        var rendered = WavIo.ReadPcmOrFloat(wavPath);
        Assert.Equal(8000, rendered.SampleRate);
        Assert.Equal(1, rendered.Channels);

        int speechSample = 1000; // within first sentence (0-0.25s)
        Assert.InRange(rendered.Planar[0][speechSample], 0.49f, 0.51f);

        var seedPath = context.ExistingRoomtonePath ?? throw new InvalidOperationException("Expected roomtone seed");
        var seedBuffer = WavIo.ReadPcmOrFloat(seedPath);
        var planJson = await File.ReadAllTextAsync(planPath);
        using var plan = JsonDocument.Parse(planJson);
        Assert.Equal(RoomtonePlanVersion.Current, plan.RootElement.GetProperty("version").GetString());
        Assert.Equal(context.Manifest.AudioPath, plan.RootElement.GetProperty("audioPath").GetString());

        var gaps = plan.RootElement.GetProperty("gaps");
        Assert.True(gaps.GetArrayLength() > 0);
        var firstGap = gaps[0];
        var gapStartSec = firstGap.GetProperty("startSec").GetDouble();
        var gapEndSec = firstGap.GetProperty("endSec").GetDouble();
        Assert.True(gapEndSec > gapStartSec);

        var gapStartSample = Math.Clamp((int)Math.Round(gapStartSec * rendered.SampleRate), 0, rendered.Planar[0].Length - 1);
        var gapNextSample = Math.Clamp(gapStartSample + 1, 0, rendered.Planar[0].Length - 1);

        var appliedGainDb = plan.RootElement.GetProperty("appliedGainDb").GetDouble();
        var targetRmsDb = plan.RootElement.GetProperty("targetRmsDb").GetDouble();
        Assert.Equal(0, targetRmsDb);

        float SeedScaled(float sample)
        {
            var scaled = sample * (float)Math.Pow(10.0, appliedGainDb / 20.0);
            return Math.Clamp(scaled, -1.0f, 1.0f);
        }

        float expected0 = SeedScaled(seedBuffer.Planar[0][0]);
        float expected1 = SeedScaled(seedBuffer.Planar[0][1]);
        Assert.InRange(rendered.Planar[0][gapStartSample], expected0 - 0.01f, expected0 + 0.01f);
        Assert.InRange(rendered.Planar[0][gapNextSample], expected1 - 0.01f, expected1 + 0.01f);

        var metaJson = await File.ReadAllTextAsync(outputs["meta"]);
        using var meta = JsonDocument.Parse(metaJson);
        Assert.Equal(seedPath, meta.RootElement.GetProperty("sourceRoomtone").GetString());
        Assert.Equal(planPath, meta.RootElement.GetProperty("outputs").GetProperty("plan").GetString());
        Assert.Equal(wavPath, meta.RootElement.GetProperty("outputs").GetProperty("roomtone").GetString());

        var paramsJson = await File.ReadAllTextAsync(outputs["params"]);
        using var parms = JsonDocument.Parse(paramsJson);
        var parameters = parms.RootElement.GetProperty("parameters");
        Assert.True(parameters.GetProperty("usedExistingRoomtone").GetBoolean());
        Assert.Equal(0, parameters.GetProperty("targetRmsDb").GetDouble());
        Assert.Equal(plan.RootElement.GetProperty("roomtoneSeedRmsDb").GetDouble(), parameters.GetProperty("roomtoneSeedRmsDb").GetDouble());
    }

    [Fact]
    public void RenderWithSentenceMasks_RepairsSilentRegions()
    {
        var input = new AudioBuffer(channels: 1, sampleRate: 1000, length: 1000);
        for (int i = 0; i < input.Length; i++)
        {
            input.Planar[0][i] = i < 400 ? 0.5f : -0.25f;
        }

        var tone = new AudioBuffer(channels: 1, sampleRate: 1000, length: 4);
        tone.Planar[0][0] = 0.1f;
        tone.Planar[0][1] = -0.1f;
        tone.Planar[0][2] = 0.05f;
        tone.Planar[0][3] = -0.05f;

        var sentences = new[]
        {
            new SentenceAlign(
                1,
                new IntRange(0, 0),
                new ScriptRange(0, 0),
                new TimingRange(0.0, 0.4),
                new SentenceMetrics(0, 0, 0, 0, 0),
                "ok")
        };

        var output = RoomtoneRenderer.RenderWithSentenceMasks(
            input,
            tone,
            Array.Empty<RoomtonePlanGap>(),
            sentences,
            targetSampleRate: 1000,
            toneGainLinear: 1.0,
            fadeMs: 0.0);

        Assert.Equal(input.SampleRate, output.SampleRate);
        Assert.Equal(input.Length, output.Length);

        int protectedSamples = (int)Math.Round(sentences[0].Timing.EndSec * output.SampleRate);

        for (int i = 0; i < input.Length; i++)
        {
            float sourceSample = input.Planar[0][i];
            if (i < protectedSamples && Math.Abs(sourceSample) > 1e-6)
            {
                float renderedSample = output.Planar[0][i];
                Assert.InRange(renderedSample, sourceSample - 1e-6f, sourceSample + 1e-6f);
            }
        }
    }

    [Fact]
    public async Task RunAsync_DisabledRendering_WritesPlanAndMetadataOnly()
    {
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: true);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: 0, fadeMs: 0);
        var outputs = await stage.RunAsync(context.Manifest, CancellationToken.None, renderAudio: false);

        Assert.False(outputs.ContainsKey("roomtoneWav"));
        Assert.True(outputs.TryGetValue("plan", out var planPath));
        Assert.True(File.Exists(planPath));
        Assert.True(outputs.TryGetValue("timeline", out var timelinePath));
        Assert.True(File.Exists(timelinePath));

        var planJson = await File.ReadAllTextAsync(planPath);
        using var plan = JsonDocument.Parse(planJson);
        Assert.Equal(RoomtonePlanVersion.Current, plan.RootElement.GetProperty("version").GetString());
        Assert.Equal(context.Manifest.AudioPath, plan.RootElement.GetProperty("audioPath").GetString());
        Assert.Equal(0, plan.RootElement.GetProperty("targetRmsDb").GetDouble());

        var metaJson = await File.ReadAllTextAsync(outputs["meta"]);
        using var meta = JsonDocument.Parse(metaJson);
        Assert.Equal(planPath, meta.RootElement.GetProperty("outputs").GetProperty("plan").GetString());
        Assert.False(meta.RootElement.GetProperty("outputs").TryGetProperty("roomtone", out _));

        var paramsJson = await File.ReadAllTextAsync(outputs["params"]);
        using var parms = JsonDocument.Parse(paramsJson);
        var parameters = parms.RootElement.GetProperty("parameters");
        Assert.True(parameters.GetProperty("usedExistingRoomtone").GetBoolean());
        Assert.Equal(plan.RootElement.GetProperty("targetRmsDb").GetDouble(), parameters.GetProperty("targetRmsDb").GetDouble());
        Assert.Equal(plan.RootElement.GetProperty("appliedGainDb").GetDouble(), parameters.GetProperty("appliedGainDb").GetDouble());
    }

    private static async Task<TestContext> ArrangeAsync(TempDir temp, bool createExistingRoomtone)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        var audioDir = Path.Combine(temp.Path, "audio");
        Directory.CreateDirectory(audioDir);
        var audioPath = Path.Combine(audioDir, "chapter.wav");
        var asrPath = Path.Combine(audioDir, "chapter.asr.json");

        var inputBuffer = new AudioBuffer(channels: 1, sampleRate: 8000, length: 8000);
        for (int i = 0; i < inputBuffer.Length; i++)
        {
            inputBuffer.Planar[0][i] = i < 2000 ? 0.5f : 0.0f;
        }
        WavIo.WriteFloat32(audioPath, inputBuffer);

        var asr = new AsrResponse("test-model", new[]
        {
            new AsrToken(0.0, 0.25, "hello"),
            new AsrToken(0.25, 0.05, "world")
        });
        await File.WriteAllTextAsync(asrPath, JsonSerializer.Serialize(asr, jsonOptions));

        var docDir = Path.Combine(temp.Path, "manuscript");
        Directory.CreateDirectory(docDir);
        var docPath = Path.Combine(docDir, "BATTLESPACE NOMAD.docx");
        await File.WriteAllTextAsync(docPath, "dummy");

        var bookIndexPath = Path.Combine(temp.Path, "book-index.json");
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
        await File.WriteAllTextAsync(bookIndexPath, JsonSerializer.Serialize(bookIndex, jsonOptions));

        var sentences = new[]
        {
            new SentenceAlign(1, new IntRange(0, 10), new ScriptRange(0, 1), new TimingRange(0.0, 0.25), new SentenceMetrics(0, 0, 0, 0, 0), "ok")
        };
        var tx = new TranscriptIndex(
            AudioPath: audioPath,
            ScriptPath: asrPath,
            BookIndexPath: bookIndexPath,
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "test",
            Words: Array.Empty<WordAlign>(),
            Sentences: sentences,
            Paragraphs: Array.Empty<ParagraphAlign>());

        var txPath = Path.Combine(temp.Path, "chapter.tx.json");
        await File.WriteAllTextAsync(txPath, JsonSerializer.Serialize(tx, jsonOptions));

        string? roomtonePath = null;
        if (createExistingRoomtone)
        {
            roomtonePath = Path.Combine(audioDir, "roomtone.wav");
            var toneBuffer = new AudioBuffer(1, 8000, 4);
            toneBuffer.Planar[0][0] = 0.1f;
            toneBuffer.Planar[0][1] = -0.1f;
            toneBuffer.Planar[0][2] = 0.05f;
            toneBuffer.Planar[0][3] = -0.05f;
            WavIo.WriteFloat32(roomtonePath, toneBuffer);
        }

        var manifest = new ManifestV2("chapter", temp.Path, audioPath, txPath);
        return new TestContext(manifest, roomtonePath);
    }

    private sealed record TestContext(ManifestV2 Manifest, string? ExistingRoomtonePath);

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
