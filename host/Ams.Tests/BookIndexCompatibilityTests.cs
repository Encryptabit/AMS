using Ams.Core.Runtime.Documents;
using Ams.Core.Runtime.Book;

namespace Ams.Tests;

public sealed class BookIndexCompatibilityTests : IDisposable
{
    private readonly List<string> _paths = [];

    [Fact]
    public async Task BuildBookIndexAsync_StampsCurrentSchemaVersion()
    {
        var source = await WriteBookAsync("Title\n\nHello current schema.");

        var parsed = await new BookParser().ParseAsync(source);
        var index = await new BookIndexer().CreateIndexAsync(parsed, source);

        Assert.Equal(BookIndex.CurrentSchemaVersion, index.SchemaVersion);
    }

    [Fact]
    public async Task BookCache_GetAsync_InvalidatesLegacyIndexWithoutSchemaVersion()
    {
        var cacheDir = CreateDirectory();
        var source = await WriteBookAsync("Title\n\nHello legacy cache.");
        var index = await BuildCurrentIndexAsync(source);
        var legacyIndex = index with { SchemaVersion = null };
        var cache = new BookCache(cacheDir);

        await cache.SetAsync(legacyIndex);

        var cached = await cache.GetAsync(source);
        var stats = await cache.GetStatsAsync();

        Assert.Null(cached);
        Assert.Equal(0, stats.TotalFiles);
    }

    [Fact]
    public async Task BookCache_GetAsync_InvalidatesUnsupportedSchemaVersion()
    {
        var cacheDir = CreateDirectory();
        var source = await WriteBookAsync("Title\n\nHello future cache.");
        var index = await BuildCurrentIndexAsync(source);
        var futureIndex = index with { SchemaVersion = BookIndex.CurrentSchemaVersion + 1 };
        var cache = new BookCache(cacheDir);

        await cache.SetAsync(futureIndex);

        var cached = await cache.GetAsync(source);

        Assert.Null(cached);
    }

    [Fact]
    public async Task BookCache_GetAsync_InvalidatesMismatchedTotals()
    {
        var cacheDir = CreateDirectory();
        var source = await WriteBookAsync("Title\n\nHello malformed totals.");
        var index = await BuildCurrentIndexAsync(source);
        var malformed = index with
        {
            Totals = index.Totals with
            {
                Words = index.Totals.Words + 1
            }
        };
        var cache = new BookCache(cacheDir);

        await cache.SetAsync(malformed);

        var cached = await cache.GetAsync(source);

        Assert.Null(cached);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task BuildBookIndexAsync_RejectsInvalidAverageWpm(double averageWpm)
    {
        var source = await WriteBookAsync("Title\n\nHello invalid options.");
        var parsed = await new BookParser().ParseAsync(source);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            new BookIndexer().CreateIndexAsync(
                parsed,
                source,
                new BookIndexOptions
                {
                    AverageWpm = averageWpm
                }));
    }

    public void Dispose()
    {
        foreach (var path in _paths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
    }

    private async Task<string> WriteBookAsync(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"ams-book-index-compat-{Guid.NewGuid():N}.txt");
        _paths.Add(path);
        await File.WriteAllTextAsync(path, content);
        return path;
    }

    private string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ams-book-index-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        _paths.Add(path);
        return path;
    }

    private static async Task<BookIndex> BuildCurrentIndexAsync(string source)
    {
        var parsed = await new BookParser().ParseAsync(source);
        return await new BookIndexer().CreateIndexAsync(parsed, source);
    }
}
