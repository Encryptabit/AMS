using System.Text.Json;
using Ams.Core.Processors.DocumentProcessor;

namespace Ams.Tests;

public class BookParsingTests
{
    [Fact]
    public void BookParser_CanParse_Extensions()
    {
        Assert.True(DocumentProcessor.CanParseBook("test.docx"));
        Assert.True(DocumentProcessor.CanParseBook("test.txt"));
        Assert.True(DocumentProcessor.CanParseBook("test.md"));
        Assert.True(DocumentProcessor.CanParseBook("test.rtf"));
        Assert.False(DocumentProcessor.CanParseBook("test.pdf"));
    }

    [Fact]
    public async Task Parser_Text_NoNormalization()
    {
        var tmp = Path.GetTempFileName() + ".txt";
        var content = "Title Line\r\n\r\n“Odin’” can’ end—of—line test.\r\nNext—para with ‘quotes’ and hyphen‑minus - and ellipsis…”";
        try
        {
            await File.WriteAllTextAsync(tmp, content);
            var result = await DocumentProcessor.ParseBookAsync(tmp);
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
        var tmp = Path.GetTempFileName() + ".xyz";
        try
        {
            await File.WriteAllTextAsync(tmp, "x");
            await Assert.ThrowsAsync<InvalidOperationException>(() => DocumentProcessor.ParseBookAsync(tmp));
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
        var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = DocumentProcessor.CreateBookCache(cacheDir);
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "A title\n\nHello “Odin’” can’ world. Next line.");
            var parsed = await DocumentProcessor.ParseBookAsync(source);
            var idx1 = await DocumentProcessor.BuildBookIndexAsync(parsed, source, new BookIndexOptions { AverageWpm = 240 });
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
        var source = Path.GetTempFileName() + ".txt";
        var line = "“Odin’” can’ end—of—line test.”";
        try
        {
            await File.WriteAllTextAsync(source, line);
            var parsed = await DocumentProcessor.ParseBookAsync(source);
            var idx = await DocumentProcessor.BuildBookIndexAsync(parsed, source);

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
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world.");
            var parsed = await DocumentProcessor.ParseBookAsync(source);
            var idx = await DocumentProcessor.BuildBookIndexAsync(parsed, source);

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
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world. This is a test.\n\nNew para here.");
            var parsed = await DocumentProcessor.ParseBookAsync(source);
            var idx = await DocumentProcessor.BuildBookIndexAsync(parsed, source);
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
        var cacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = DocumentProcessor.CreateBookCache(cacheDir);
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world");
            var parsed = await DocumentProcessor.ParseBookAsync(source);
            var idx = await DocumentProcessor.BuildBookIndexAsync(parsed, source);
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
        Assert.Equal(-1, w.SectionIndex);
        Assert.Null(w.Phonemes);
    }

    [Fact]
    public async Task BookPhonemePopulator_PopulatesPhonemes()
    {
        var source = Path.GetTempFileName() + ".txt";
        try
        {
            await File.WriteAllTextAsync(source, "Hello world.\n");
            var parsed = await DocumentProcessor.ParseBookAsync(source);

            var pronunciations = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["hello"] = new[] { "HH AH0 L OW1" },
                ["world"] = new[] { "W ER1 L D", "W ER1 L D" }
            };

            var index = await DocumentProcessor.BuildBookIndexAsync(parsed, source);
            var enriched = await DocumentProcessor.PopulateMissingPhonemesAsync(index, new StubPronunciationProvider(pronunciations));

            Assert.Contains(enriched.Words, w => w.Text == "Hello" && w.Phonemes is { Length: >0 } phon && phon.Contains("HH AH0 L OW1"));
            Assert.Contains(enriched.Words, w => w.Text == "world." && w.Phonemes is { Length: 1 or >1 } phon && phon.Contains("W ER1 L D"));
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }
        }
    }

    [Fact]
    public void PronunciationHelper_NormalizesNumbersToWords()
    {
        var lexeme = PronunciationHelper.NormalizeForLookup("123");
        Assert.Equal("one hundred twenty three", lexeme);

        var parts = PronunciationHelper.ExtractPronunciationParts("1,024");
        Assert.Equal(new[] { "one", "thousand", "twenty", "four" }, parts);
    }

    [Fact]
    public void BookIndexOptions_Defaults()
    {
        var opt = new BookIndexOptions();
        Assert.Equal(200.0, opt.AverageWpm);
    }

    private sealed class StubPronunciationProvider : IPronunciationProvider
    {
        private readonly IReadOnlyDictionary<string, string[]> _map;

        public StubPronunciationProvider(IReadOnlyDictionary<string, string[]> map)
        {
            _map = map;
        }

        public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
        {
            return Task.FromResult(_map);
        }
    }
}
