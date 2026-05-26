using System.Text.Json;
using Ams.Core.Runtime.Artifacts;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// File-based cache implementation for book indexes with SHA256 validation.
/// Stores cached book indexes as JSON files with automatic invalidation
/// when source files change.
/// </summary>
public class BookCache : IBookCache
{
    private readonly BookCacheArtifactResolver _artifacts;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the BookCache class.
    /// </summary>
    /// <param name="cacheDirectory">Directory to store cache files. If null, uses default cache directory.</param>
    public BookCache(string? cacheDirectory = null)
        : this(new BookCacheArtifactResolver(cacheDirectory))
    {
    }

    internal BookCache(BookCacheArtifactResolver artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);

        _artifacts = artifacts;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false, // Compact JSON for cache files
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));

        try
        {
            var cacheArtifact = _artifacts.Resolve(sourceFile);
            if (!cacheArtifact.Exists)
            {
                return null;
            }

            var json = await cacheArtifact.ReadTextAsync(cancellationToken);
            var cachedIndex = JsonSerializer.Deserialize<BookIndex>(json, _jsonOptions);

            if (cachedIndex == null)
                return null;

            if (await IsValidAsync(cachedIndex, cancellationToken))
            {
                return cachedIndex;
            }

            await RemoveAsync(sourceFile, cancellationToken);
            return null;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookCacheException($"Failed to retrieve cached index for '{sourceFile}': {ex.Message}", ex);
        }
    }

    public async Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
    {
        if (bookIndex == null)
            throw new ArgumentNullException(nameof(bookIndex));

        if (string.IsNullOrWhiteSpace(bookIndex.SourceFile))
            throw new ArgumentException("BookIndex must have a valid source file path.", nameof(bookIndex));

        try
        {
            var json = JsonSerializer.Serialize(bookIndex, _jsonOptions);
            await _artifacts.Resolve(bookIndex.SourceFile).WriteTextAsync(json, cancellationToken);

            return true;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookCacheException($"Failed to cache index for '{bookIndex.SourceFile}': {ex.Message}", ex);
        }
    }

    public async Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));

        try
        {
            return await _artifacts.Resolve(sourceFile).DeleteAsync(cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookCacheException($"Failed to remove cached index for '{sourceFile}': {ex.Message}", ex);
        }
    }

    public async Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
    {
        if (bookIndex == null)
            throw new ArgumentNullException(nameof(bookIndex));

        try
        {
            var compatibility = BookIndexCompatibility.ValidateForCache(bookIndex);
            if (!compatibility.IsCompatible)
            {
                return false;
            }

            var sourceArtifact = FileArtifact.FromPath(bookIndex.SourceFile);

            // Check if source artifact still exists
            if (!sourceArtifact.Exists)
                return false;

            // Check if file hash matches
            cancellationToken.ThrowIfCancellationRequested();
            var currentHash = sourceArtifact.Sha256Hash;
            if (!string.Equals(currentHash, bookIndex.SourceFileHash, StringComparison.OrdinalIgnoreCase))
                return false;

            // Optional: Check file modification time as additional validation
            var sourceLastWriteTimeUtc = sourceArtifact.LastWriteTimeUtc;

            // If the file was modified after the cache was created, it's invalid
            if (sourceLastWriteTimeUtc > bookIndex.IndexedAt)
                return false;

            return true;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookCacheException($"Failed to validate cached index for '{bookIndex.SourceFile}': {ex.Message}",
                ex);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _artifacts.ClearAsync(cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookCacheException($"Failed to clear cache directory '{_artifacts.CacheRoot}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets cache statistics including total cache size and number of cached items.
    /// </summary>
    public async Task<BookCacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_artifacts.CacheRootExists)
            {
                return new BookCacheStats(0, 0, 0);
            }

            return await Task.Run(() =>
            {
                var cacheFiles = _artifacts.List();
                var totalSize = cacheFiles.Sum(static artifact => artifact.Length);
                var validCount = 0;

                foreach (var cacheFile in cacheFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var json = cacheFile.ReadText();
                        var bookIndex = JsonSerializer.Deserialize<BookIndex>(json, _jsonOptions);

                        if (bookIndex != null
                            && BookIndexCompatibility.ValidateForCache(bookIndex).IsCompatible
                            && FileArtifact.FromPath(bookIndex.SourceFile).Exists)
                        {
                            validCount++;
                        }
                    }
                    catch
                    {
                        // Ignore invalid cache files
                    }
                }

                return new BookCacheStats(cacheFiles.Count, validCount, totalSize);
            }, cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookCacheException($"Failed to get cache statistics: {ex.Message}", ex);
        }
    }

}

/// <summary>
/// Statistics about the book cache.
/// </summary>
/// <param name="TotalFiles">Total number of cache files</param>
/// <param name="ValidFiles">Number of valid cache files (source still exists)</param>
/// <param name="TotalSizeBytes">Total size of cache files in bytes</param>
public record BookCacheStats(
    int TotalFiles,
    int ValidFiles,
    long TotalSizeBytes
)
{
    /// <summary>Gets the total cache size in a human-readable format.</summary>
    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);

    private static string FormatBytes(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;

        return bytes switch
        {
            >= gb => $"{bytes / (double)gb:F2} GB",
            >= mb => $"{bytes / (double)mb:F2} MB",
            >= kb => $"{bytes / (double)kb:F2} KB",
            _ => $"{bytes} bytes"
        };
    }
}
