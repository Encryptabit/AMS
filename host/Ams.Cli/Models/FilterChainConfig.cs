using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ams.Cli.Models;

public sealed record FilterChainConfig
{
    public List<FilterConfig> Filters { get; set; } = new();

    public static async Task<FilterChainConfig> LoadAsync(FileInfo path, CancellationToken cancellationToken)
    {
        if (!path.Exists)
        {
            throw new FileNotFoundException("Filter-chain config not found.", path.FullName);
        }

        await using var stream = path.OpenRead();
        var config = await JsonSerializer.DeserializeAsync<FilterChainConfig>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false)
                     ?? new FilterChainConfig();
        config.Filters ??= new List<FilterConfig>();
        return config;
    }

    public async Task SaveAsync(FileInfo path, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(path.DirectoryName ?? Directory.GetCurrentDirectory());
        await using var stream = path.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, this, SerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    internal static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

public sealed record FilterConfig
{
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; init; } = true;
    public JsonElement Parameters { get; init; }
}
