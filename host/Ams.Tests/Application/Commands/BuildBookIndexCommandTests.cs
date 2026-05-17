using Ams.Core.Application.Commands;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Runtime.Documents;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Documents;
using Ams.Core.Services.Interfaces;

namespace Ams.Tests.Application.Commands;

public class BuildBookIndexCommandTests
{
    [Fact]
    public async Task ExecuteAsync_CacheMiss_BuildsIndex_WritesOutput_AndCachesResult()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello world.");
            var outputFile = new FileInfo(Path.Combine(root, "artifacts", "book-index.json"));
            var cache = new RecordingBookCache();
            var command = CreateCommand(cache);

            var result = await command.ExecuteAsync(
                new BuildBookIndexRequest(
                    bookFile,
                    outputFile,
                    new BookIndexOptions { AverageWpm = 180 },
                    BookIndexCacheMode.PreferCache));

            Assert.Equal(BookIndexCacheDisposition.CacheMiss, result.CacheDisposition);
            Assert.True(result.WasRebuilt);
            Assert.False(result.PhonemesBackfilled);
            Assert.Equal(ModuleIds.BuildBookIndex.Value, result.ModuleId.Value);
            Assert.Equal(RunState.Completed, result.State);
            Assert.True(outputFile.Exists);
            Assert.Single(result.Artifacts);
            Assert.Equal(outputFile.FullName, result.Artifacts[0].Path);
            Assert.True(result.Artifacts[0].Exists);
            Assert.Equal(1, cache.GetCalls);
            Assert.Equal(1, cache.SetCalls);
            Assert.NotNull(cache.LastSet);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExecuteAsync_CacheHit_ReusesCachedIndex_AndStillWritesOutput()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello cached world.");
            var cachedIndex = await BuildIndexAsync(bookFile.FullName);
            var outputFile = new FileInfo(Path.Combine(root, "nested", "book-index.json"));
            var cache = new RecordingBookCache(cachedIndex);
            var command = CreateCommand(cache);

            var result = await command.ExecuteAsync(
                new BuildBookIndexRequest(bookFile, outputFile, cacheMode: BookIndexCacheMode.PreferCache));

            Assert.Equal(BookIndexCacheDisposition.CacheHit, result.CacheDisposition);
            Assert.False(result.WasRebuilt);
            Assert.True(outputFile.Exists);
            Assert.Equal(1, cache.GetCalls);
            Assert.Equal(0, cache.SetCalls);
            Assert.Equal(cachedIndex.SourceFileHash, result.Index.SourceFileHash);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExecuteAsync_RebuildMode_BypassesCacheRead_AndRefreshesCache()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = await WriteBookAsync(root, "book.txt", "Title\n\nHello rebuilt world.");
            var cachedIndex = await BuildIndexAsync(bookFile.FullName);
            var outputFile = new FileInfo(Path.Combine(root, "book-index.json"));
            var cache = new RecordingBookCache(cachedIndex);
            var command = CreateCommand(cache);

            var result = await command.ExecuteAsync(
                new BuildBookIndexRequest(bookFile, outputFile, cacheMode: BookIndexCacheMode.Rebuild));

            Assert.Equal(BookIndexCacheDisposition.ForceRefresh, result.CacheDisposition);
            Assert.True(result.WasRebuilt);
            Assert.True(outputFile.Exists);
            Assert.Equal(0, cache.GetCalls);
            Assert.Equal(1, cache.SetCalls);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExecuteAsync_MissingBookFile_ThrowsTypedValidationFailure()
    {
        var root = CreateTempDirectory();
        try
        {
            var missingBook = new FileInfo(Path.Combine(root, "missing.txt"));
            var outputFile = new FileInfo(Path.Combine(root, "book-index.json"));
            var command = CreateCommand(new RecordingBookCache());

            var exception = await Assert.ThrowsAsync<BuildBookIndexCommandException>(() =>
                command.ExecuteAsync(new BuildBookIndexRequest(missingBook, outputFile)));

            Assert.Equal(RunFailureKind.Validation, exception.Failure.Kind);
            Assert.Contains("Book file not found", exception.Failure.Message);
            Assert.Single(exception.Artifacts);
            Assert.Equal(outputFile.FullName, exception.Artifacts[0].Path);
            Assert.False(exception.Artifacts[0].Exists);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ExecuteAsync_UnsupportedExtension_ThrowsTypedValidationFailure()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = await WriteBookAsync(root, "book.xyz", "Unsupported content");
            var outputFile = new FileInfo(Path.Combine(root, "book-index.json"));
            var command = CreateCommand(new RecordingBookCache());

            var exception = await Assert.ThrowsAsync<BuildBookIndexCommandException>(() =>
                command.ExecuteAsync(new BuildBookIndexRequest(bookFile, outputFile)));

            Assert.Equal(RunFailureKind.Validation, exception.Failure.Kind);
            Assert.Contains("Unsupported file format", exception.Failure.Message);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void BuildBookIndexRequest_FromCliOptions_RejectsConflictingForceFlags()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = new FileInfo(Path.Combine(root, "book.txt"));
            var outputFile = new FileInfo(Path.Combine(root, "book-index.json"));

            var exception = Assert.Throws<ArgumentException>(() => BuildBookIndexRequest.FromCliOptions(
                bookFile,
                outputFile,
                forceRefresh: true,
                noCache: true,
                averageWordsPerMinute: 200));

            Assert.Contains("--force-refresh", exception.Message);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void BuildBookIndexRequest_RequiresOutputFile()
    {
        var root = CreateTempDirectory();
        try
        {
            var bookFile = new FileInfo(Path.Combine(root, "book.txt"));

            Assert.Throws<ArgumentNullException>(() => new BuildBookIndexRequest(bookFile, null!));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public void BuildBookIndexRequest_FromPipelineOptions_MapsForceToRebuildMode()
    {
        var root = CreateTempDirectory();
        try
        {
            var request = BuildBookIndexRequest.FromPipelineOptions(new PipelineRunOptions
            {
                BookFile = new FileInfo(Path.Combine(root, "book.md")),
                BookIndexFile = new FileInfo(Path.Combine(root, "book-index.json")),
                AudioFile = new FileInfo(Path.Combine(root, "chapter.wav")),
                ChapterId = "chapter-01",
                Force = true,
                ForceIndex = false,
                AverageWordsPerMinute = 155.0
            });

            Assert.Equal(BookIndexCacheMode.Rebuild, request.CacheMode);
            Assert.Equal(155.0, request.IndexOptions?.AverageWpm);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static BuildBookIndexCommand CreateCommand(RecordingBookCache cache)
    {
        var service = new DocumentService(pronunciationProvider: null, cache: cache);
        return new BuildBookIndexCommand(service);
    }

    private static async Task<BookIndex> BuildIndexAsync(string sourceFile)
    {
        var parseResult = await new BookParser().ParseAsync(sourceFile);
        return await new BookIndexer().CreateIndexAsync(parseResult, sourceFile);
    }

    private static async Task<FileInfo> WriteBookAsync(string root, string fileName, string content)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, fileName);
        await File.WriteAllTextAsync(path, content);
        return new FileInfo(path);
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "ams-build-book-index-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteDirectory(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class RecordingBookCache : IBookCache
    {
        public RecordingBookCache(BookIndex? cachedIndex = null)
        {
            CachedIndex = cachedIndex;
        }

        public BookIndex? CachedIndex { get; private set; }

        public BookIndex? LastSet { get; private set; }

        public int GetCalls { get; private set; }

        public int SetCalls { get; private set; }

        public int RemoveCalls { get; private set; }

        public Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            GetCalls++;
            return Task.FromResult(CachedIndex);
        }

        public Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
        {
            SetCalls++;
            CachedIndex = bookIndex;
            LastSet = bookIndex;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            RemoveCalls++;
            CachedIndex = null;
            return Task.FromResult(true);
        }

        public Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            CachedIndex = null;
            return Task.CompletedTask;
        }
    }
}
