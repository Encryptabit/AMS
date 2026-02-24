using Ams.Core.Application.Mfa;

namespace Ams.Tests.Common;

public sealed class PronunciationLexiconCacheTests
{
    private static readonly SemaphoreSlim EnvironmentGate = new(1, 1);

    [Fact]
    public async Task Cache_PersistsAndReturnsHitsAcrossInstances()
    {
        await EnvironmentGate.WaitAsync();
        try
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "ams-phoneme-cache-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(cacheDir);

            using var env = new EnvironmentVariableScope("AMS_PHONEME_CACHE_DIR", cacheDir);

            var cache = new PronunciationLexiconCache("english_us_mfa");
            var changed = await cache.MergeAsync(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["colour"] = new[] { "K AH1 L ER0" }
            });
            Assert.Equal(1, changed);

            var second = new PronunciationLexiconCache("english_us_mfa");
            var hits = await second.GetManyAsync(new[] { "colour", "unknown" });

            Assert.True(hits.TryGetValue("colour", out var variants));
            Assert.Equal(new[] { "K AH1 L ER0" }, variants);
            Assert.False(hits.ContainsKey("unknown"));
        }
        finally
        {
            EnvironmentGate.Release();
        }
    }

    [Fact]
    public async Task Cache_IsModelScoped()
    {
        await EnvironmentGate.WaitAsync();
        try
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "ams-phoneme-cache-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(cacheDir);

            using var env = new EnvironmentVariableScope("AMS_PHONEME_CACHE_DIR", cacheDir);

            var usCache = new PronunciationLexiconCache("english_us_mfa");
            await usCache.MergeAsync(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["color"] = new[] { "K AH1 L ER0" }
            });

            var ukCache = new PronunciationLexiconCache("english_uk_mfa");
            var ukHits = await ukCache.GetManyAsync(new[] { "color" });
            Assert.Empty(ukHits);
        }
        finally
        {
            EnvironmentGate.Release();
        }
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _previousValue;

        public EnvironmentVariableScope(string name, string? value)
        {
            _name = name;
            _previousValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_name, _previousValue);
        }
    }
}
