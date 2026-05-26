using System.Security.Cryptography;
using System.Text;

namespace Ams.Core.Runtime.Artifacts;

public sealed class BookCacheArtifactResolver
{
    public BookCacheArtifactResolver(string? cacheRoot = null)
    {
        CacheRoot = Path.GetFullPath(cacheRoot ?? GetDefaultCacheRoot());
        Directory.CreateDirectory(CacheRoot);
    }

    public string CacheRoot { get; }

    public bool CacheRootExists => Directory.Exists(CacheRoot);

    public FileArtifact<BookCacheArtifactAddress> Resolve(string sourceFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);

        var sourceFileHash = ComputeCacheKeyHash(sourceFile);
        var sourceFileName = Path.GetFileNameWithoutExtension(sourceFile);
        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        var safeFileName = string.Concat(sourceFileName.Where(c => !invalidFileNameChars.Contains(c)));

        if (safeFileName.Length > 50)
        {
            safeFileName = safeFileName[..50];
        }

        var cacheFileName = $"{safeFileName}_{sourceFileHash[..8]}.json";
        var address = new BookCacheArtifactAddress(CacheRoot, cacheFileName);
        return new FileArtifact<BookCacheArtifactAddress>(address);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(CacheRoot))
        {
            return;
        }

        await Task.Run(() =>
        {
            var cacheFiles = Directory.GetFiles(CacheRoot, "*.json", SearchOption.AllDirectories);
            foreach (var file in cacheFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Delete(file);
            }
        }, cancellationToken);
    }

    public IReadOnlyList<FileArtifact<BookCacheArtifactAddress>> List()
    {
        if (!Directory.Exists(CacheRoot))
        {
            return [];
        }

        return Directory.GetFiles(CacheRoot, "*.json", SearchOption.AllDirectories)
            .Select(path => new FileArtifact<BookCacheArtifactAddress>(
                new BookCacheArtifactAddress(
                    Path.GetDirectoryName(path) ?? CacheRoot,
                    Path.GetFileName(path))))
            .ToArray();
    }

    private static string ComputeCacheKeyHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private static string GetDefaultCacheRoot()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, "AMS", "BookCache");
    }
}
