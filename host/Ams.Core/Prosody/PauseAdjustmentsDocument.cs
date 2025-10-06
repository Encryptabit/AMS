using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ams.Core.Prosody;

public sealed record PauseAdjustmentsDocument(
    [property: JsonPropertyName("sourceTranscript")] string SourceTranscript,
    [property: JsonPropertyName("generatedAtUtc")] DateTime GeneratedAtUtc,
    [property: JsonPropertyName("policy")] PausePolicySnapshot Policy,
    [property: JsonPropertyName("adjustments")] IReadOnlyList<PauseAdjust> Adjustments)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static PauseAdjustmentsDocument Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Pause adjustments JSON not found", path);
        }

        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<PauseAdjustmentsDocument>(json, JsonOptions);
        return document ?? throw new InvalidOperationException($"Failed to deserialize pause adjustments from {path}");
    }

    public static PauseAdjustmentsDocument Create(
        string sourceTranscript,
        DateTime generatedAtUtc,
        PausePolicy policy,
        IEnumerable<PauseAdjust> adjustments)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (adjustments is null) throw new ArgumentNullException(nameof(adjustments));

        var list = adjustments
            .Where(adjust => adjust is not null)
            .ToList();

        return new PauseAdjustmentsDocument(
            sourceTranscript,
            generatedAtUtc,
            PausePolicySnapshot.FromPolicy(policy),
            list);
    }

    public void Save(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must be provided", nameof(path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var ordered = Adjustments
            .OrderBy(adjust => adjust.StartSec)
            .ThenBy(adjust => adjust.LeftSentenceId)
            .ThenBy(adjust => adjust.RightSentenceId)
            .ToList();

        var document = this with { Adjustments = ordered };
        var json = JsonSerializer.Serialize(document, JsonOptions);
        File.WriteAllText(path, json);
    }
}

public sealed record PausePolicySnapshot(
    [property: JsonPropertyName("comma")] PauseWindowSnapshot Comma,
    [property: JsonPropertyName("sentence")] PauseWindowSnapshot Sentence,
    [property: JsonPropertyName("paragraph")] PauseWindowSnapshot Paragraph,
    [property: JsonPropertyName("headOfChapter")] double HeadOfChapter,
    [property: JsonPropertyName("postChapterRead")] double PostChapterRead,
    [property: JsonPropertyName("tail")] double Tail,
    [property: JsonPropertyName("kneeWidth")] double KneeWidth,
    [property: JsonPropertyName("ratioInside")] double RatioInside,
    [property: JsonPropertyName("ratioOutside")] double RatioOutside,
    [property: JsonPropertyName("preserveTopQuantile")] double PreserveTopQuantile)
{
    public static PausePolicySnapshot FromPolicy(PausePolicy policy)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        return new PausePolicySnapshot(
            PauseWindowSnapshot.FromWindow(policy.Comma),
            PauseWindowSnapshot.FromWindow(policy.Sentence),
            PauseWindowSnapshot.FromWindow(policy.Paragraph),
            policy.HeadOfChapter,
            policy.PostChapterRead,
            policy.Tail,
            policy.KneeWidth,
            policy.RatioInside,
            policy.RatioOutside,
            policy.PreserveTopQuantile);
    }
}

public sealed record PauseWindowSnapshot(
    [property: JsonPropertyName("min")] double Min,
    [property: JsonPropertyName("max")] double Max)
{
    public static PauseWindowSnapshot FromWindow(PauseWindow window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        return new PauseWindowSnapshot(window.Min, window.Max);
    }
}
