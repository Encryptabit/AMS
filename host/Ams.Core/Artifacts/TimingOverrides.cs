using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts;

public sealed record SentenceTimingOverride(
    [property: JsonPropertyName("sentenceId")]
    int SentenceId,
    [property: JsonPropertyName("startSec")]
    double StartSec,
    [property: JsonPropertyName("endSec")] double EndSec);

public sealed record TimingOverridesDocument(
    [property: JsonPropertyName("sourceTranscript")]
    string SourceTranscript,
    [property: JsonPropertyName("generatedAtUtc")]
    DateTime GeneratedAtUtc,
    [property: JsonPropertyName("sentences")]
    IReadOnlyList<SentenceTimingOverride> Sentences)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static TimingOverridesDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Timing overrides file not found", path);
        }

        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<TimingOverridesDocument>(json, JsonOptions);
        return doc ?? throw new InvalidOperationException("Failed to deserialize timing overrides document.");
    }

    public void Save(string path)
    {
        if (Sentences.Count == 0)
        {
            throw new InvalidOperationException(
                "Timing overrides document must contain at least one sentence override.");
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var ordered = Sentences
            .OrderBy(s => s.SentenceId)
            .ToList();

        var doc = this with { Sentences = ordered };
        var json = JsonSerializer.Serialize(doc, JsonOptions);
        File.WriteAllText(path, json);
    }
}