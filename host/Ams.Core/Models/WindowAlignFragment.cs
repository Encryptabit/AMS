using System.Text.Json.Serialization;

namespace Ams.Core.Models;

public sealed record WindowAlignFragment(
    [property: JsonPropertyName("begin")] double Begin,
    [property: JsonPropertyName("end")] double End,
    [property: JsonPropertyName("sentenceIndex")] int SentenceIndex = -1,
    [property: JsonPropertyName("wordStartIndex")] int WordStartIndex = -1,
    [property: JsonPropertyName("wordEndIndex")] int WordEndIndex = -1
);
