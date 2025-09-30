using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using Ams.Core.Asr;
using Ams.Core.Common;

namespace Ams.Core.Alignment.Mfa;

public static class MfaTimingMerger
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static void MergeTimings(FileInfo hydrateFile, FileInfo asrFile, FileInfo textGridFile)
    {
        if (!hydrateFile.Exists)
        {
            Log.Warn("Hydrate file not found; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        if (!asrFile.Exists)
        {
            Log.Warn("ASR file not found; skipping MFA timing merge ({File})", asrFile.FullName);
            return;
        }

        if (!textGridFile.Exists)
        {
            Log.Warn("TextGrid file not found; skipping MFA timing merge ({File})", textGridFile.FullName);
            return;
        }

        var textGridIntervals = TextGridParser.ParseWordIntervals(textGridFile.FullName)
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .ToList();

        if (textGridIntervals.Count == 0)
        {
            Log.Warn("TextGrid contained no word intervals ({File})", textGridFile.FullName);
            return;
        }

        var asr = JsonSerializer.Deserialize<AsrResponse>(File.ReadAllText(asrFile.FullName), SerializerOptions);
        if (asr?.Tokens is null || asr.Tokens.Length == 0)
        {
            Log.Warn("ASR file missing tokens; skipping MFA timing merge ({File})", asrFile.FullName);
            return;
        }

        var timingMap = BuildTimingMap(asr.Tokens, textGridIntervals);
        if (timingMap.Count == 0)
        {
            Log.Warn("Unable to map any MFA timings to ASR tokens for {File}", hydrateFile.Name);
            return;
        }

        var hydrateNode = JsonNode.Parse(File.ReadAllText(hydrateFile.FullName))?.AsObject();
        if (hydrateNode is null)
        {
            Log.Warn("Failed to parse hydrate JSON; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        if (hydrateNode["sentences"] is not JsonArray sentencesArray)
        {
            Log.Warn("Hydrate JSON missing sentences array; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        var updated = 0;
        foreach (var sentenceNode in sentencesArray.OfType<JsonObject>())
        {
            if (sentenceNode["scriptRange"] is not JsonObject scriptRange)
            {
                continue;
            }

            if (!TryGetRange(scriptRange, out var startIdx, out var endIdx))
            {
                continue;
            }

            var timings = CollectTimings(timingMap, startIdx, endIdx);
            if (timings.Count == 0)
            {
                continue;
            }

            var start = timings.Min(t => t.start);
            var end = timings.Max(t => t.end);
            if (end <= start)
            {
                continue;
            }

            var duration = end - start;

            if (sentenceNode["timing"] is not JsonObject timingNode)
            {
                timingNode = new JsonObject();
                sentenceNode["timing"] = timingNode;
            }

            timingNode["startSec"] = start;
            timingNode["endSec"] = end;
            timingNode["duration"] = duration;
            updated++;
        }

        File.WriteAllText(hydrateFile.FullName, hydrateNode.ToJsonString(SerializerOptions));
        Log.Info("Updated {Count} sentences with MFA timings ({File})", updated, hydrateFile.Name);
    }

    private static Dictionary<int, (double start, double end)> BuildTimingMap(
        AsrToken[] asrTokens,
        IReadOnlyList<TextGridInterval> intervals)
    {
        var map = new Dictionary<int, (double, double)>();
        var j = 0;
        const double tolerance = 0.25; // seconds; tweak as needed

        for (var i = 0; i < asrTokens.Length && j < intervals.Count;)
        {
            var asrSanitized = Sanitize(asrTokens[i].Word);
            if (string.IsNullOrEmpty(asrSanitized))
            {
                i++;
                continue;
            }

            var interval = intervals[j];
            var gridSanitized = Sanitize(interval.Text);

            if (string.IsNullOrEmpty(gridSanitized))
            {
                j++;
                continue;
            }

            if (gridSanitized == "unk")
            {
                j++;
                continue;
            }

            if (asrSanitized == gridSanitized)
            {
                map[i] = (interval.Start, interval.End);
                i++;
                j++;
                continue;
            }

            var nextAsr = i + 1 < asrTokens.Length ? Sanitize(asrTokens[i + 1].Word) : null;
            if (gridSanitized == nextAsr)
            {
                i++;
                continue;
            }

            var nextGrid = j + 1 < intervals.Count ? Sanitize(intervals[j + 1].Text) : null;
            if (asrSanitized == nextGrid)
            {
                j++;
                continue;
            }

            // Compare timings to decide which pointer to advance
            var asrStart = asrTokens[i].StartTime;
            var asrEnd = asrStart + asrTokens[i].Duration;
            var gridStart = interval.Start;
            var gridEnd = interval.End;

            if (asrEnd + tolerance < gridStart)
            {
                i++;
                continue;
            }

            if (gridEnd + tolerance < asrStart)
            {
                j++;
                continue;
            }

            if (IsPunctuation(asrTokens[i].Word))
            {
                i++;
                continue;
            }

            // If we reach here, timings overlap but tokens differ; advance both to avoid stalling
            i++;
            j++;
        }

        return map;
    }

    private static List<(double start, double end)> CollectTimings(
        IReadOnlyDictionary<int, (double start, double end)> map,
        int startIdx,
        int endIdx)
    {
        var timings = new List<(double, double)>();
        for (var idx = startIdx; idx <= endIdx; idx++)
        {
            if (map.TryGetValue(idx, out var timing))
            {
                timings.Add(timing);
            }
        }

        return timings;
    }

    private static bool TryGetRange(JsonObject rangeNode, out int start, out int end)
    {
        start = 0;
        end = -1;

        if (rangeNode["start"] is not JsonNode startNode ||
            rangeNode["end"] is not JsonNode endNode)
        {
            return false;
        }

        try
        {
            start = startNode.GetValue<int>();
            end = endNode.GetValue<int>();
        }
        catch
        {
            return false;
        }

        if (end < start)
        {
            return false;
        }

        return true;
    }

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var count = 0;

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer[count++] = char.ToLowerInvariant(ch);
            }
        }

        return count == 0 ? string.Empty : new string(buffer[..count]);
    }

    private static bool IsPunctuation(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                return false;
            }
        }

        return true;
    }
}
