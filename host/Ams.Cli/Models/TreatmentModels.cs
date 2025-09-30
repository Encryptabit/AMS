using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ams.Cli.Models;

internal sealed record TreatmentChain(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("sampleRate")] int? SampleRate,
    [property: JsonPropertyName("blockSize")] int? BlockSize,
    [property: JsonPropertyName("outChannels")] int? OutChannels,
    [property: JsonPropertyName("bitDepth")] int? BitDepth,
    [property: JsonPropertyName("nodes")] IReadOnlyList<TreatmentNode> Nodes)
{
    public TreatmentChain() : this(null, null, null, null, null, null, Array.Empty<TreatmentNode>())
    {
    }
}

internal sealed record TreatmentNode
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("plugin")] public string Plugin { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("params")] public IReadOnlyList<string>? Parameters { get; init; }
    [JsonPropertyName("paramFile")] public string? ParameterFile { get; init; }
    [JsonPropertyName("preset")] public string? Preset { get; init; }
    [JsonPropertyName("sampleRate")] public int? SampleRate { get; init; }
    [JsonPropertyName("blockSize")] public int? BlockSize { get; init; }
    [JsonPropertyName("outChannels")] public int? OutChannels { get; init; }
    [JsonPropertyName("bitDepth")] public int? BitDepth { get; init; }
    [JsonPropertyName("inputs")] public IReadOnlyList<string>? Inputs { get; init; }
    [JsonPropertyName("midiInput")] public string? MidiInput { get; init; }
    [JsonPropertyName("additionalArgs")] public IReadOnlyList<string>? AdditionalArguments { get; init; }
    [JsonPropertyName("outputFile")] public string? OutputFile { get; init; }

    [JsonConstructor]
    public TreatmentNode(string? name, string plugin, string? description, IReadOnlyList<string>? parameters,
        string? parameterFile, string? preset, int? sampleRate, int? blockSize, int? outChannels,
        int? bitDepth, IReadOnlyList<string>? inputs, string? midiInput, IReadOnlyList<string>? additionalArguments,
        string? outputFile)
    {
        if (string.IsNullOrWhiteSpace(plugin))
        {
            throw new ArgumentException("Plugin path is required", nameof(plugin));
        }

        Name = name;
        Plugin = plugin;
        Description = description;
        Parameters = parameters;
        ParameterFile = parameterFile;
        Preset = preset;
        SampleRate = sampleRate;
        BlockSize = blockSize;
        OutChannels = outChannels;
        BitDepth = bitDepth;
        Inputs = inputs;
        MidiInput = midiInput;
        AdditionalArguments = additionalArguments;
        OutputFile = outputFile;
    }
}
