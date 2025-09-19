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

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: -50, fadeMs: 5);
        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => stage.RunAsync(context.Manifest, CancellationToken.None));

        Assert.Contains("roomtone.wav", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_UsesExistingRoomtoneWhenPresent()
    {
        using var temp = new TempDir();
        var context = await ArrangeAsync(temp, createExistingRoomtone: true);

        var stage = new RoomToneInsertionStage(targetSampleRate: 8000, toneGainDb: -50, fadeMs: 5);
        var outputs = await stage.RunAsync(context.Manifest, CancellationToken.None);

        Assert.True(File.Exists(outputs["roomtoneWav"]));

        var producedBytes = await File.ReadAllBytesAsync(outputs["roomtoneWav"]);
        var expectedBytes = await File.ReadAllBytesAsync(context.ExistingRoomtonePath!);
        Assert.True(producedBytes.SequenceEqual(expectedBytes));

        var metaJson = await File.ReadAllTextAsync(outputs["meta"]);
        using var meta = JsonDocument.Parse(metaJson);
        Assert.Equal(context.ExistingRoomtonePath!, meta.RootElement.GetProperty("sourceRoomtone").GetString());

        var paramsJson = await File.ReadAllTextAsync(outputs["params"]);
        using var parameters = JsonDocument.Parse(paramsJson);
        Assert.True(parameters.RootElement.GetProperty("parameters").GetProperty("usedExistingRoomtone").GetBoolean());
    }

    private static async Task<TestContext> ArrangeAsync(TempDir temp, bool createExistingRoomtone)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        var audioDir = Path.Combine(temp.Path, "audio");
        Directory.CreateDirectory(audioDir);
        var audioPath = Path.Combine(audioDir, "chapter.wav");
        var asrPath = Path.Combine(audioDir, "chapter.asr.json");

        var buffer = new AudioBuffer(channels: 1, sampleRate: 8000, length: 8000);
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer.Planar[0][i] = i < 4000 ? 0.4f : 0.0f;
        }
        WavIo.WriteInt16Pcm(audioPath, buffer);

        var asr = new AsrResponse("test-model", new[]
        {
            new AsrToken(0.0, 0.5, "hello"),
            new AsrToken(0.5, 0.5, "world")
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
            new SentenceAlign(1, new IntRange(0, 0), new ScriptRange(0, 0), new TimingRange(0.0, 0.5), new SentenceMetrics(0, 0, 0, 0, 0), "ok"),
            new SentenceAlign(2, new IntRange(1, 1), new ScriptRange(1, 1), new TimingRange(0.5, 1.0), new SentenceMetrics(0, 0, 0, 0, 0), "ok")
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

        string? existingRoomtonePath = null;
        if (createExistingRoomtone)
        {
            existingRoomtonePath = Path.Combine(audioDir, "RoomTone.WaV");
            await File.WriteAllBytesAsync(existingRoomtonePath, new byte[] { 0, 1, 2, 3, 4 });
        }

        var manifest = new ManifestV2("chapter", temp.Path, audioPath, txPath);
        return new TestContext(manifest, existingRoomtonePath);
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
