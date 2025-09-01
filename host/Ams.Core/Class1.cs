using System.Text.Json.Serialization;

namespace Ams.Core;

public record AsrToken(
    [property: JsonPropertyName("t")] double StartTime,
    [property: JsonPropertyName("d")] double Duration,
    [property: JsonPropertyName("w")] string Word,
    [property: JsonPropertyName("c")] double Confidence
);

public record AsrSegment(
    [property: JsonPropertyName("start")] double Start,
    [property: JsonPropertyName("end")] double End,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("conf")] double Confidence,
    [property: JsonPropertyName("tokens")] AsrToken[] Tokens
);

public record AsrResponse(
    [property: JsonPropertyName("modelVersion")] string ModelVersion,
    [property: JsonPropertyName("segments")] AsrSegment[] Segments
);

public record AsrRequest(
    [property: JsonPropertyName("audio_path")] string AudioPath,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("language")] string Language = "en"
);
