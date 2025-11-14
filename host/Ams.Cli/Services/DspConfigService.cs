using System.Text.Json;

namespace Ams.Cli.Services;

internal static class DspConfigService
{
    private const string ConfigDirectoryEnv = "AMS_DSP_CONFIG_DIR";
    private const string DefaultFolderName = "AMS";
    private const string ConfigFileName = "dsp-config.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    internal static string GetConfigFilePath()
    {
        var configured = Environment.GetEnvironmentVariable(ConfigDirectoryEnv);
        string baseDirectory;

        if (!string.IsNullOrWhiteSpace(configured))
        {
            baseDirectory = Path.GetFullPath(configured);
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appData))
            {
                appData = Environment.CurrentDirectory;
            }

            baseDirectory = Path.Combine(appData, DefaultFolderName);
        }

        return Path.Combine(baseDirectory, ConfigFileName);
    }

    internal static async Task<DspConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = GetConfigFilePath();
        if (!File.Exists(path))
        {
            return new DspConfig();
        }

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var config = await JsonSerializer.DeserializeAsync<DspConfig>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false);
        config ??= new DspConfig();

        NormalizeConfig(config);
        return config;
    }

    internal static async Task SaveAsync(DspConfig config, CancellationToken cancellationToken = default)
    {
        NormalizeConfig(config);

        var path = GetConfigFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, config, SerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    private static void NormalizeConfig(DspConfig config)
    {
        config.PluginDirectories = config.PluginDirectories
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        config.Plugins ??= new Dictionary<string, DspPluginMetadata>();

        var normalized = new Dictionary<string, DspPluginMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in config.Plugins)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value is null)
            {
                continue;
            }

            var fullPath = Path.GetFullPath(kvp.Key);
            normalized[fullPath] = kvp.Value with { Path = fullPath };
        }

        config.Plugins = normalized;
    }
}

