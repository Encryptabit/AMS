using System.Security.Cryptography;

namespace Ams.Core.Runtime.Artifacts;

public interface IFileArtifactAddress
{
    string FullPath { get; }

    FileInfo ToFile();
}

public sealed class FileArtifact<TAddress>
    where TAddress : IFileArtifactAddress
{
    private readonly Lazy<string> _sha256Hash;

    public FileArtifact(TAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        Address = address;
        _sha256Hash = new Lazy<string>(ComputeSha256Hash, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public TAddress Address { get; }

    public string FullPath => Address.FullPath;

    public FileInfo File => Address.ToFile();

    public bool Exists
    {
        get
        {
            var file = File;
            file.Refresh();
            return file.Exists;
        }
    }

    public long Length
    {
        get
        {
            var file = File;
            file.Refresh();
            return file.Length;
        }
    }

    public DateTime LastWriteTimeUtc
    {
        get
        {
            var file = File;
            file.Refresh();
            return file.LastWriteTimeUtc;
        }
    }

    public string Sha256Hash => _sha256Hash.Value;

    public string ReadText() => System.IO.File.ReadAllText(FullPath);

    public Task<string> ReadTextAsync(CancellationToken cancellationToken = default)
        => System.IO.File.ReadAllTextAsync(FullPath, cancellationToken);

    public async Task WriteTextAsync(string contents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var directory = File.Directory;
        if (!string.IsNullOrWhiteSpace(directory?.FullName))
        {
            Directory.CreateDirectory(directory.FullName);
        }

        await System.IO.File.WriteAllTextAsync(FullPath, contents, cancellationToken);
    }

    public async Task<bool> DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (!Exists)
        {
            return false;
        }

        await Task.Run(() => System.IO.File.Delete(FullPath), cancellationToken);
        return true;
    }

    public override string ToString() => FullPath;

    private string ComputeSha256Hash()
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }
}

public static class FileArtifact
{
    public static FileArtifact<SourceArtifactAddress> FromPath(string path)
        => new(new SourceArtifactAddress(path));
}
