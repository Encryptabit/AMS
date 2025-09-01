using System.Text.Json.Serialization;

namespace Ams.Core;

public record AsrToken(
    [property: JsonPropertyName("t")] double StartTime,
    [property: JsonPropertyName("d")] double Duration,
    [property: JsonPropertyName("w")] string Word
);

public record AsrResponse(
    [property: JsonPropertyName("modelVersion")] string ModelVersion,
    [property: JsonPropertyName("tokens")] AsrToken[] Tokens
);

public record AsrRequest(
    [property: JsonPropertyName("audio_path")] string AudioPath,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("language")] string Language = "en"
);
