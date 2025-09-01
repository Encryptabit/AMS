using System.Text.Json;
using Ams.Core;

namespace Ams.Tests;

public class BookParsingTests
{
    [Fact]
    public void BookParser_CanParse_Extensions()
    {
        var parser = new BookParser();
        Assert.True(parser.CanParse("test.docx"));
        Assert.True(parser.CanParse("test.txt"));
        Assert.True(parser.CanParse("test.md"));
        Assert.True(parser.CanParse("test.rtf"));
        Assert.False(parser.CanParse("test.pdf"));
    }

    [Fact]
    public async Task Parser_Text_NoNormalization()
    {
        var parser = new BookParser();
        var tmp = Path.GetTempFileName() + ".txt";
        var content = "Title Line\r\n\r\n“Odin’” can’ end—of—line test.\r\nNext—para with ‘quotes’ and hyphen‑minus - and ellipsis…";
        try
        {
            await File.WriteAllTextAsync(tmp, content);
            var result = await parser.ParseAsync(tmp);
            Assert.Equal("Title Line", result.Title);
            Assert.Contains("“Odin’” can’ end—of—line test.", result.Text);
            Assert.NotNull(result.Paragraphs);
            Assert.True(result.Paragraphs!.Count >= 2);
        }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }

    [Fact]
    public async Task Parser_Unsupported_Throws()
    {
        var parser = new BookParser();
        var tmp = Path.GetTempFileName() + ".xyz";
        try
        {
            await File.WriteAllTextAsync(tmp, "x");
            await Assert.ThrowsAsync<InvalidOperationException>(() => parser.ParseAsync(tmp));
        }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
}

public class BookIndexAcceptanceTests
{
    private static readonly JsonSerializerOptions deterministic = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public async Task Canonical_RoundTrip_DeterministicBytes_WithCache()
    {
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(cacheDir);
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "A title\n\nHello “Odin’” can’ world. Next line.");
            var parsed = await parser.ParseAsync(source);
            var idx1 = await indexer.CreateIndexAsync(parsed, source, new BookIndexOptions { AverageWpm = 240 });
            await cache.SetAsync(idx1);

            var cached = await cache.GetAsync(source);
            Assert.NotNull(cached);

            var json1 = JsonSerializer.Serialize(idx1, deterministic);
            var json2 = JsonSerializer.Serialize(cached!, deterministic);
            Assert.Equal(json1, json2);
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (Directory.Exists(cacheDir)) Directory.Delete(cacheDir, true);
        }
    }

    [Fact]
    public async Task NoNormalization_WordsPreserveExactText()
    {
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var source = Path.GetTempFileName() + ".txt";
        var line = "“Odin’” can’ end—of—line test.”";
        try
        {
            await File.WriteAllTextAsync(source, line);
            var parsed = await parser.ParseAsync(source);
            var idx = await indexer.CreateIndexAsync(parsed, source);

            var texts = idx.Words.Select(w => w.Text).ToArray();
            Assert.Contains("“Odin’”", texts);
            Assert.Contains("can’", texts);
            Assert.Contains("end—of—line", texts);
            Assert.Contains("test.”", texts);
        }
        finally { if (File.Exists(source)) File.Delete(source); }
    }

    [Fact]
    public async Task Slimness_WordsContainOnlyCanonicalFields()
    {
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world.");
            var parsed = await parser.ParseAsync(source);
            var idx = await indexer.CreateIndexAsync(parsed, source);

            var json = JsonSerializer.Serialize(idx, deterministic);
            using var doc = JsonDocument.Parse(json);
            var firstWord = doc.RootElement.GetProperty("words")[0];
            var names = firstWord.EnumerateObject().Select(p => p.Name).OrderBy(n => n).ToArray();
            Assert.DoesNotContain("startTime", names);
            Assert.DoesNotContain("endTime", names);
            Assert.DoesNotContain("confidence", names);
        }
        finally { if (File.Exists(source)) File.Delete(source); }
    }

    [Fact]
    public async Task StructureRanges_CoverAllWords_NoGaps()
    {
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world. This is a test.\n\nNew para here.");
            var parsed = await parser.ParseAsync(source);
            var idx = await indexer.CreateIndexAsync(parsed, source);
            var total = idx.Words.Length;

            var orderedSents = idx.Sentences.OrderBy(s => s.Start).ToArray();
            Assert.True(orderedSents.First().Start == 0);
            Assert.True(orderedSents.Last().End == total - 1);
            for (int i = 1; i < orderedSents.Length; i++)
                Assert.Equal(orderedSents[i - 1].End + 1, orderedSents[i].Start);

            var orderedParas = idx.Paragraphs.OrderBy(p => p.Start).ToArray();
            Assert.True(orderedParas.First().Start == 0);
            Assert.True(orderedParas.Last().End == total - 1);
            for (int i = 1; i < orderedParas.Length; i++)
                Assert.Equal(orderedParas[i - 1].End + 1, orderedParas[i].Start);
        }
        finally { if (File.Exists(source)) File.Delete(source); }
    }

    [Fact]
    public async Task CacheReuse_InvalidatedOnSourceChange()
    {
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(cacheDir);
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world");
            var parsed = await parser.ParseAsync(source);
            var idx = await indexer.CreateIndexAsync(parsed, source);
            await cache.SetAsync(idx);

            var found = await cache.GetAsync(source);
            Assert.NotNull(found);

            await File.WriteAllTextAsync(source, "Hello brave new world");
            var after = await cache.GetAsync(source);
            Assert.Null(after);
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (Directory.Exists(cacheDir)) Directory.Delete(cacheDir, true);
        }
    }
}

public class BookModelsTests
{
    [Fact]
    public void BookWord_CanonicalShape()
    {
        var w = new BookWord("Hello", 3, 1, 0);
        Assert.Equal("Hello", w.Text);
        Assert.Equal(3, w.WordIndex);
        Assert.Equal(1, w.SentenceIndex);
        Assert.Equal(0, w.ParagraphIndex);
    }

    [Fact]
    public void BookIndexOptions_Defaults()
    {
        var opt = new BookIndexOptions();
        Assert.Equal(200.0, opt.AverageWpm);
    }
}
