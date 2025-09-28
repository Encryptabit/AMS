using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ams.Cli.Models;

internal sealed class DspConfig
{
    [JsonPropertyName("pluginDirectories")]
    public List<string> PluginDirectories { get; set; } = new();

    [JsonPropertyName("plugins")]
    public Dictionary<string, DspPluginMetadata> Plugins { get; set; } = new();
}

internal sealed record DspPluginMetadata(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("name")] string? PluginName,
    [property: JsonPropertyName("rawParameters")] string RawParameters,
    [property: JsonPropertyName("parameters")] IReadOnlyList<DspPluginParameter>? Parameters,
    [property: JsonPropertyName("scannedAtUtc")] DateTimeOffset ScannedAtUtc,
    [property: JsonPropertyName("pluginModifiedUtc")] DateTimeOffset PluginModifiedUtc
);

internal sealed record DspPluginParameter(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("values")] string? Values,
    [property: JsonPropertyName("default")] string? Default,
    [property: JsonPropertyName("supportsText")] bool? SupportsTextValues
);

