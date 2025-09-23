using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ams.Core.Asr;

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
    [property: JsonPropertyName("beam_size")] int? BeamSize = null,
    [property: JsonPropertyName("nfa_model")] string? NfaModel = null,
    [property: JsonPropertyName("save_ass")] bool SaveAss = false
);

internal record AlignResponseDto(
    [property: JsonPropertyName("transcript")] string Transcript,
    [property: JsonPropertyName("asr_model")] string AsrModel,
    [property: JsonPropertyName("nfa_model")] string NfaModel,
    [property: JsonPropertyName("words")] AsrToken[] Words,
    [property: JsonPropertyName("segments")] AsrToken[] Segments,
    [property: JsonPropertyName("files")] Dictionary<string, string> Files
);
