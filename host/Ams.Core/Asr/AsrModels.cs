using System.Text.Json.Serialization;

namespace Ams.Core.Asr;

public sealed record AsrSegment(
    [property: JsonPropertyName("startSec")] double StartSec,
    [property: JsonPropertyName("endSec")] double EndSec,
    [property: JsonPropertyName("text")] string Text
);

public sealed record AsrToken(
    [property: JsonPropertyName("t")] double StartTime,
    [property: JsonPropertyName("d")] double Duration,
    [property: JsonPropertyName("w")] string Word
);

public sealed record AsrResponse
{
    [JsonConstructor]
    public AsrResponse(
        string modelVersion,
        AsrToken[]? tokens = null,
        AsrSegment[]? segments = null)
    {
        ModelVersion = modelVersion ?? throw new ArgumentNullException(nameof(modelVersion));
        Tokens = tokens ?? Array.Empty<AsrToken>();
        Segments = segments ?? Array.Empty<AsrSegment>();
    }

    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; init; }

    [JsonPropertyName("tokens")]
    public AsrToken[] Tokens { get; init; }

    [JsonPropertyName("segments")]
    public AsrSegment[] Segments { get; init; }

    [JsonIgnore]
    public bool HasWordTimings => Tokens.Any(t => t.Duration > 0.0001);

    [JsonIgnore]
    public IReadOnlyList<string> Words => _wordCache ??= BuildWords();

    [JsonIgnore]
    public int WordCount => Words.Count;

    [JsonIgnore]
    public bool HasWords => WordCount > 0;

    public string? GetWord(int index) => index >= 0 && index < WordCount ? Words[index] : null;

    private IReadOnlyList<string>? _wordCache;

    private IReadOnlyList<string> BuildWords()
    {
        if (Tokens.Length > 0)
        {
            return Tokens.Select(t => t.Word).ToArray();
        }

        if (Segments.Length == 0)
        {
            return Array.Empty<string>();
        }

        var list = new List<string>();
        foreach (var segment in Segments)
        {
            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                continue;
            }

            var words = segment.Text
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                continue;
            }

            foreach (var word in words)
            {
                list.Add(word);
            }
        }

        return list.Count == 0 ? Array.Empty<string>() : list.ToArray();
    }
}

public record AsrRequest(
    [property: JsonPropertyName("audio_path")] string AudioPath,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("language")] string Language = "en"
);

