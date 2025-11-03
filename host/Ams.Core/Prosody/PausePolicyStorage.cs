using System;
using System.IO;
using System.Text.Json;

namespace Ams.Core.Prosody;

public static class PausePolicyStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static PausePolicy Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Pause policy JSON not found", path);
        }

        var json = File.ReadAllText(path);
        var snapshot = JsonSerializer.Deserialize<PausePolicySnapshot>(json, JsonOptions);
        if (snapshot is null)
        {
            throw new InvalidOperationException($"Failed to deserialize pause policy from {path}");
        }

        return snapshot.ToPolicy();
    }

    public static void Save(string path, PausePolicy policy)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided", nameof(path));
        }

        if (policy is null)
        {
            throw new ArgumentNullException(nameof(policy));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = PausePolicySnapshot.FromPolicy(policy);
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        File.WriteAllText(path, json);
    }
}
