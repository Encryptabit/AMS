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

        Assert.True(File.Exists(outputs["roomtoneWav"]));

        var rendered = WavIo.ReadPcmOrFloat(outputs["roomtoneWav"]);
        Assert.Equal(8000, rendered.SampleRate);
        Assert.Equal(1, rendered.Channels);

        int speechSample = 1000; // within first sentence (0-0.25s)
        Assert.InRange(rendered.Planar[0][speechSample], 0.49f, 0.51f);

        int gapStart = (int)(0.25 * rendered.SampleRate);
        float expected0 = 0.1f;
        float expected1 = -0.1f;
        Assert.InRange(rendered.Planar[0][gapStart], expected0 - 0.01f, expected0 + 0.01f);
        Assert.InRange(rendered.Planar[0][gapStart + 1], expected1 - 0.01f, expected1 + 0.01f);

        var metaJson = await File.ReadAllTextAsync(outputs["meta"]);
        using var meta = JsonDocument.Parse(metaJson);
        Assert.Equal(context.ExistingRoomtonePath, meta.RootElement.GetProperty("sourceRoomtone").GetString());

        var paramsJson = await File.ReadAllTextAsync(outputs["params"]);
        using var parms = JsonDocument.Parse(paramsJson);
        Assert.True(parms.RootElement.GetProperty("parameters").GetProperty("usedExistingRoomtone").GetBoolean());
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
        WavIo.WriteInt16Pcm(audioPath, inputBuffer);

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
            WavIo.WriteInt16Pcm(roomtonePath, toneBuffer);
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
