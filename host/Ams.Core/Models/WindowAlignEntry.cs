using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ams.Core;

namespace Ams.Core.Models;

public sealed record WindowAlignEntry(
    [property: JsonPropertyName("windowId")] string WindowId,
    [property: JsonPropertyName("offsetSec")] double OffsetSec,
    [property: JsonPropertyName("textDigest")] string TextDigest,
    [property: JsonPropertyName("fragments")] IReadOnlyList<WindowAlignFragment> Fragments,
    [property: JsonPropertyName("tool")] IReadOnlyDictionary<string, string> ToolVersions,
    [property: JsonPropertyName("generatedAt")] DateTime GeneratedAt
);

public sealed record WindowAlignIndex(
    [property: JsonPropertyName("windowIds")] IReadOnlyList<string> WindowIds,
    [property: JsonPropertyName("map")] IReadOnlyDictionary<string, string> WindowToJsonMap,
    [property: JsonPropertyName("params")] WindowAlignParams Params,
    [property: JsonPropertyName("tool")] IReadOnlyDictionary<string, string> ToolVersions
);
