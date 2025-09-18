using System.Text.Json;
using Ams.Core;
using Ams.Core.Pipeline;
using Xunit;

namespace Ams.Tests;

public class AnchorWindowsStageTests
{
    [Fact]
    public async Task AnchorsAndAnchorWindowsStages_ProduceDeterministicArtifacts()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "ams_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        try
        {
            // Create minimal manifest
            var input = new InputMetadata(Path.Combine(tmp, "chapter.wav"), "DEADBEEF", 0.0, 0, DateTime.UtcNow);
            var manifest = ManifestV2.CreateNew(input);
            await File.WriteAllTextAsync(Path.Combine(tmp, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

            // Create minimal book.index.json
            var words = new[]
            {
                new BookWord("hello", 0, 0, 0, 0),
                new BookWord("world", 1, 0, 0, 0)
            };
            var sentences = new[]
            {
                new BookSentence(0, 0, 1)
            };

            var paragraphs = new[]
            {
                new BookParagraph(0, 0, 1, "Body", "Normal")
            };

            var sections = Array.Empty<SectionRange>();

            var totals = new BookTotals(
                Words: words.Length,
                Sentences: sentences.Length,
                Paragraphs: paragraphs.Length,
                EstimatedDurationSec: 1.0
            );

            var book = new BookIndex(
                SourceFile: "test.txt",
                SourceFileHash: "B00K",
                IndexedAt: DateTime.UtcNow,
                Title: "Test",
                Author: "Tester",
                Totals: totals,
                Words: words,
                Sentences: sentences,
                Paragraphs: paragraphs,
                Sections: sections
            );
            var bookPath = Path.Combine(tmp, "book.index.json");
            await File.WriteAllTextAsync(bookPath, JsonSerializer.Serialize(book, new JsonSerializerOptions { WriteIndented = true }));

            // Create transcripts/merged.json with Words
            var tdir = Path.Combine(tmp, "transcripts");
            Directory.CreateDirectory(tdir);
            var merged = new
            {
                ChunkCount = 1,
                TotalWords = 2,
                Words = new[]
                {
                    new { Word = "hello", Start = 0.0, End = 0.5, Confidence = 1.0 },
                    new { Word = "world", Start = 0.5, End = 1.0, Confidence = 1.0 }
                },
                Params = new { },
                GeneratedAt = DateTime.UtcNow
            };
            await File.WriteAllTextAsync(Path.Combine(tdir, "merged.json"), JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true }));

            // Run anchors stage
            var anchorsStage = new AnchorsStage(tmp, bookPath, Path.Combine(tdir, "merged.json"), new AnchorsParams());
            Assert.True(await anchorsStage.RunAsync(manifest));

            // Check anchors artifact
            var anchorsJson = await File.ReadAllTextAsync(Path.Combine(tmp, "anchors", "anchors.json"));
            Assert.Contains("\"Selected\"", anchorsJson);

            // Run anchor windows stage
            var anchorWindowsStage = new AnchorWindowsStage(tmp, new AnchorWindowParams());
            Assert.True(await anchorWindowsStage.RunAsync(manifest));

            // Check windows artifact
            var windowsJson = await File.ReadAllTextAsync(Path.Combine(tmp, "anchor-windows", "anchor-windows.json"));
            Assert.Contains("\"Windows\"", windowsJson);

            // Determinism: rerun and ensure fingerprint matches (skip)
            var manifestAfter = JsonSerializer.Deserialize<ManifestV2>(await File.ReadAllTextAsync(Path.Combine(tmp, "manifest.json")))!;
            var beforeFp = manifestAfter.Stages["anchor-windows"].Fingerprint;
            Assert.True(await anchorWindowsStage.RunAsync(manifestAfter));
            var afterFp = JsonSerializer.Deserialize<ManifestV2>(await File.ReadAllTextAsync(Path.Combine(tmp, "manifest.json")))!.Stages["anchor-windows"].Fingerprint;
            Assert.Equal(beforeFp.InputHash, afterFp.InputHash);
            Assert.Equal(beforeFp.ParamsHash, afterFp.ParamsHash);
        }
        finally
        {
            try { Directory.Delete(tmp, recursive: true); } catch { /* ignore */ }
        }
    }
}



