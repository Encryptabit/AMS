using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ams.Core.Application.Mfa;

internal sealed class PronunciationLexiconCache
{
    private const int SchemaVersion = 1;
    private const string CacheFileEnvVar = "AMS_PHONEME_CACHE_FILE";
    private const string CacheDirEnvVar = "AMS_PHONEME_CACHE_DIR";
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly string _g2pModel;
    private readonly string _cacheFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public PronunciationLexiconCache(string g2pModel)
    {
        _g2pModel = string.IsNullOrWhiteSpace(g2pModel) ? MfaService.DefaultG2pModel : g2pModel.Trim();
        _cacheFilePath = ResolveCacheFilePath(_g2pModel);
    }

    public async Task<IReadOnlyDictionary<string, string[]>> GetManyAsync(
        IReadOnlyCollection<string> lexemes,
        CancellationToken cancellationToken = default)
    {
        if (lexemes.Count == 0)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        var document = await ReadAsync(cancellationToken).ConfigureAwait(false);
        if (document.Entries.Count == 0)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        var hits = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var lexeme in lexemes)
        {
            if (document.Entries.TryGetValue(lexeme, out var variants) && variants is { Length: > 0 })
            {
                hits[lexeme] = variants;
            }
        }

        return hits;
    }

    public async Task<int> MergeAsync(
        IReadOnlyDictionary<string, string[]> updates,
        CancellationToken cancellationToken = default)
    {
        if (updates.Count == 0)
        {
            return 0;
        }

        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var existing = await ReadCoreAsync(cancellationToken).ConfigureAwait(false);
            var merged = new Dictionary<string, string[]>(existing.Entries, StringComparer.OrdinalIgnoreCase);

            int changed = 0;
            foreach (var (lexeme, variants) in updates)
            {
                if (string.IsNullOrWhiteSpace(lexeme))
                {
                    continue;
                }

                var normalizedVariants = NormalizeVariants(variants);
                if (normalizedVariants.Length == 0)
                {
                    continue;
                }

                if (!merged.TryGetValue(lexeme, out var current))
                {
                    merged[lexeme] = normalizedVariants;
                    changed++;
                    continue;
                }

                var combined = MergeVariants(current, normalizedVariants);
                if (!AreSame(current, combined))
                {
                    merged[lexeme] = combined;
                    changed++;
                }
            }

            if (changed == 0)
            {
                return 0;
            }

            var next = new PronunciationLexiconCacheDocument(
                SchemaVersion,
                _g2pModel,
                DateTime.UtcNow,
                merged);

            await WriteCoreAsync(next, cancellationToken).ConfigureAwait(false);
            return changed;
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<PronunciationLexiconCacheDocument> ReadAsync(CancellationToken cancellationToken)
    {
        await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ReadCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<PronunciationLexiconCacheDocument> ReadCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_cacheFilePath))
        {
            return EmptyDocument();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_cacheFilePath, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return EmptyDocument();
            }

            var document = JsonSerializer.Deserialize<PronunciationLexiconCacheDocument>(json, _jsonOptions);
            if (document is null || document.SchemaVersion != SchemaVersion ||
                !string.Equals(document.G2pModel, _g2pModel, StringComparison.OrdinalIgnoreCase))
            {
                return EmptyDocument();
            }

            var entries = document.Entries ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            return new PronunciationLexiconCacheDocument(
                document.SchemaVersion,
                document.G2pModel,
                document.UpdatedAtUtc,
                NormalizeEntries(entries));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Debug("Failed to read pronunciation cache {Path}: {Message}", _cacheFilePath, ex.Message);
            return EmptyDocument();
        }
    }

    private async Task WriteCoreAsync(PronunciationLexiconCacheDocument document, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_cacheFilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Invalid pronunciation cache path: {_cacheFilePath}");
        }

        Directory.CreateDirectory(directory);

        var tempFile = _cacheFilePath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            var json = JsonSerializer.Serialize(document, _jsonOptions);
            await File.WriteAllTextAsync(tempFile, json, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            File.Move(tempFile, _cacheFilePath, overwrite: true);
        }
        finally
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            catch
            {
                // best effort cleanup
            }
        }
    }

    private PronunciationLexiconCacheDocument EmptyDocument()
    {
        return new PronunciationLexiconCacheDocument(
            SchemaVersion,
            _g2pModel,
            DateTime.UtcNow,
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));
    }

    private static Dictionary<string, string[]> NormalizeEntries(IReadOnlyDictionary<string, string[]> entries)
    {
        var normalized = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var (lexeme, variants) in entries)
        {
            if (string.IsNullOrWhiteSpace(lexeme))
            {
                continue;
            }

            var clean = NormalizeVariants(variants);
            if (clean.Length > 0)
            {
                normalized[lexeme] = clean;
            }
        }

        return normalized;
    }

    private static string[] MergeVariants(IEnumerable<string>? current, IEnumerable<string>? incoming)
    {
        var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AppendVariants(merged, current);
        AppendVariants(merged, incoming);
        return merged.ToArray();
    }

    private static string[] NormalizeVariants(IEnumerable<string>? variants)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AppendVariants(set, variants);
        return set.ToArray();
    }

    private static void AppendVariants(HashSet<string> set, IEnumerable<string>? variants)
    {
        if (variants is null)
        {
            return;
        }

        foreach (var variant in variants)
        {
            if (string.IsNullOrWhiteSpace(variant))
            {
                continue;
            }

            var normalized = string.Join(' ', variant.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (normalized.Length > 0)
            {
                set.Add(normalized);
            }
        }
    }

    private static bool AreSame(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        var set = new HashSet<string>(left, StringComparer.OrdinalIgnoreCase);
        return right.All(set.Contains);
    }

    private static string ResolveCacheFilePath(string g2pModel)
    {
        var explicitFile = Environment.GetEnvironmentVariable(CacheFileEnvVar);
        if (!string.IsNullOrWhiteSpace(explicitFile))
        {
            return Path.GetFullPath(explicitFile.Trim());
        }

        var baseDirectory = Environment.GetEnvironmentVariable(CacheDirEnvVar);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            baseDirectory = Path.Combine(localAppData, "AMS", "PronunciationCache");
        }

        var safeModel = SanitizeFileName(g2pModel);
        return Path.Combine(baseDirectory, $"{safeModel}.json");
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "default";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' ? ch : '_');
        }

        var safe = builder.ToString().Trim('_');
        return safe.Length == 0 ? "default" : safe;
    }

    private sealed record PronunciationLexiconCacheDocument(
        int SchemaVersion,
        string G2pModel,
        DateTime UpdatedAtUtc,
        Dictionary<string, string[]> Entries);
}
