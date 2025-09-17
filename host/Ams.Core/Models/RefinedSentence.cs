using System.Text.Json.Serialization;

namespace Ams.Core.Models;

public sealed record RefinedSentence(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("start")] double Start,
    [property: JsonPropertyName("end")] double End,
    [property: JsonPropertyName("startWordIdx")] int? StartWordIdx,
    [property: JsonPropertyName("endWordIdx")] int? EndWordIdx,
    [property: JsonPropertyName("source")] string Source = "aeneas+silence.start"
);
