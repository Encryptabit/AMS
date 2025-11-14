using System.Security.Cryptography;
using System.Text.Json;

namespace Ams.Core.Runtime.Documents;

/// <summary>
/// File-based cache implementation for book indexes with SHA256 validation.
/// Stores cached book indexes as JSON files with automatic invalidation
/// when source files change.
/// </summary>
public class BookCache : IBookCache
{
    private readonly string _cacheDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the BookCache class.
    /// </summary>
    /// <param name="cacheDirectory">Directory to store cache files. If null, uses default cache directory.</param>
    public BookCache(string? cacheDirectory = null)
    {
        _cacheDirectory = cacheDirectory ?? GetDefaultCacheDirectory();
        
        // Ensure cache directory exists
        Directory.CreateDirectory(_cacheDirectory);
        
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
            var cacheFilePath = GetCacheFilePath(sourceFile);
            
            if (!File.Exists(cacheFilePath))
                return null;

            // Read cached index
            var json = await File.ReadAllTextAsync(cacheFilePath, cancellationToken);
            var cachedIndex = JsonSerializer.Deserialize<BookIndex>(json, _jsonOptions);
            
            if (cachedIndex == null)
                return null;

            // Validate cache integrity
            if (await IsValidAsync(cachedIndex, cancellationToken))
            {
                return cachedIndex;
            }
            else
            {
                // Cache is invalid, remove it
                await RemoveAsync(sourceFile, cancellationToken);
                return null;
            }
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
            var cacheFilePath = GetCacheFilePath(bookIndex.SourceFile);
            
            // Ensure cache directory exists
            var cacheDir = Path.GetDirectoryName(cacheFilePath);
            if (cacheDir != null)
                Directory.CreateDirectory(cacheDir);
            
            // Serialize and write to cache
            var json = JsonSerializer.Serialize(bookIndex, _jsonOptions);
            await File.WriteAllTextAsync(cacheFilePath, json, cancellationToken);
            
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
            var cacheFilePath = GetCacheFilePath(sourceFile);
            
            if (File.Exists(cacheFilePath))
            {
                await Task.Run(() => File.Delete(cacheFilePath), cancellationToken);
                return true;
            }
            
            return false;
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
            // Check if source file still exists
            if (!File.Exists(bookIndex.SourceFile))
                return false;

            // Check if file hash matches
            var currentHash = await ComputeFileHashAsync(bookIndex.SourceFile, cancellationToken);
            if (!string.Equals(currentHash, bookIndex.SourceFileHash, StringComparison.OrdinalIgnoreCase))
                return false;

            // Optional: Check file modification time as additional validation
            var sourceFileInfo = new FileInfo(bookIndex.SourceFile);
            var cacheAge = DateTime.UtcNow - bookIndex.IndexedAt;
            var fileAge = DateTime.UtcNow - sourceFileInfo.LastWriteTimeUtc;
            
            // If the file was modified after the cache was created, it's invalid
            if (sourceFileInfo.LastWriteTimeUtc > bookIndex.IndexedAt)
                return false;

            return true;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookCacheException($"Failed to validate cached index for '{bookIndex.SourceFile}': {ex.Message}", ex);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                await Task.Run(() =>
                {
                    var cacheFiles = Directory.GetFiles(_cacheDirectory, "*.json", SearchOption.AllDirectories);
                    foreach (var file in cacheFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        File.Delete(file);
                    }
                }, cancellationToken);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookCacheException($"Failed to clear cache directory '{_cacheDirectory}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets cache statistics including total cache size and number of cached items.
    /// </summary>
    public async Task<BookCacheStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(_cacheDirectory))
                return new BookCacheStats(0, 0, 0);

            return await Task.Run(() =>
            {
                var cacheFiles = Directory.GetFiles(_cacheDirectory, "*.json", SearchOption.AllDirectories);
                var totalSize = cacheFiles.Sum(f => new FileInfo(f).Length);
                var validCount = 0;
                
                foreach (var file in cacheFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    try
                    {
                        var json = File.ReadAllText(file);
                        var bookIndex = JsonSerializer.Deserialize<BookIndex>(json, _jsonOptions);
                        
                        if (bookIndex != null && File.Exists(bookIndex.SourceFile))
                        {
                            validCount++;
                        }
                    }
                    catch
                    {
                        // Ignore invalid cache files
                    }
                }
                
                return new BookCacheStats(cacheFiles.Length, validCount, totalSize);
            }, cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookCacheException($"Failed to get cache statistics: {ex.Message}", ex);
        }
    }

    private string GetCacheFilePath(string sourceFile)
    {
        // Create a safe filename from the source file path
        var sourceFileHash = ComputeStringHash(sourceFile);
        var sourceFileName = Path.GetFileNameWithoutExtension(sourceFile);
        
        // Remove invalid filename characters
        var safeFileName = string.Concat(sourceFileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        if (safeFileName.Length > 50) // Limit length
            safeFileName = safeFileName[..50];
        
        var cacheFileName = $"{safeFileName}_{sourceFileHash[..8]}.json";
        return Path.Combine(_cacheDirectory, cacheFileName);
    }

    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            return Convert.ToHexString(hashBytes);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookCacheException($"Failed to compute hash for file '{filePath}': {ex.Message}", ex);
        }
    }

    private static string ComputeStringHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private static string GetDefaultCacheDirectory()
    {
        // Use a subdirectory in the user's temp or app data folder
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, "AMS", "BookCache");
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
