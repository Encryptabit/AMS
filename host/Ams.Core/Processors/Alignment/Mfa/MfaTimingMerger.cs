using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ams.Core.Common;

namespace Ams.Core.Processors.Alignment.Mfa;

public static class MfaTimingMerger
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static void MergeTimings(FileInfo hydrateFile, FileInfo textGridFile, IReadOnlyDictionary<int, (string? ScriptText, string? BookText)>? fallbackTexts = null)
    {
        if (!hydrateFile.Exists)
        {
            Log.Debug("Hydrate file not found; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        if (!textGridFile.Exists)
        {
            Log.Debug("TextGrid file not found; skipping MFA timing merge ({File})", textGridFile.FullName);
            return;
        }

        var textGridIntervals = TextGridParser.ParseWordIntervals(textGridFile.FullName)
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .ToList();

        if (textGridIntervals.Count == 0)
        {
            Log.Debug("TextGrid contained no word intervals ({File})", textGridFile.FullName);
            return;
        }

        var intervalTokens = BuildIntervalTokens(textGridIntervals);
        if (intervalTokens.Count == 0)
        {
            Log.Debug("TextGrid contained no usable tokens ({File})", textGridFile.FullName);
            return;
        }
        var intervalCursor = 0;
        double previousEndSec = 0.0;

        var hydrateNode = JsonNode.Parse(File.ReadAllText(hydrateFile.FullName))?.AsObject();
        if (hydrateNode is null)
        {
            Log.Debug("Failed to parse hydrate JSON; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        if (hydrateNode["sentences"] is not JsonArray sentencesArray)
        {
            Log.Debug("Hydrate JSON missing sentences array; skipping MFA timing merge ({File})", hydrateFile.FullName);
            return;
        }

        var updated = 0;
        foreach (var sentenceNode in sentencesArray.OfType<JsonObject>())
        {
            if (!TryFindSentenceTiming(sentenceNode, intervalTokens, ref intervalCursor, previousEndSec, fallbackTexts, out var start, out var end))
            {
                continue;
            }

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
            previousEndSec = end;
        }

        if (updated == 0)
        {
            Log.Debug("No MFA timings applied for {File}", hydrateFile.FullName);
            return;
        }

        File.WriteAllText(hydrateFile.FullName, hydrateNode.ToJsonString(SerializerOptions));
        Log.Debug("Updated {Count} sentences with MFA timings ({File})", updated, hydrateFile.Name);
    }

    private static List<IntervalToken> BuildIntervalTokens(IReadOnlyList<TextGridInterval> intervals)
    {
        var tokens = new List<IntervalToken>(intervals.Count);
        foreach (var interval in intervals)
        {
            var normalized = TextNormalizer.NormalizeTypography(interval.Text);
            var token = Sanitize(normalized);
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            tokens.Add(new IntervalToken(interval.Start, interval.End, token));
        }

        return tokens;
    }

    private const double MinimumStartTolerance = 0.05;

    private static bool TryFindSentenceTiming(
        JsonObject sentenceNode,
        IReadOnlyList<IntervalToken> intervalTokens,
        ref int cursor,
        double minStartSec,
        IReadOnlyDictionary<int, (string? ScriptText, string? BookText)>? fallbackTexts,
        out double start,
        out double end)
    {
        start = 0;
        end = 0;

        if (intervalTokens.Count == 0)
        {
            return false;
        }

        var tokenCandidates = ExtractSentenceTokenCandidates(sentenceNode, fallbackTexts);
        foreach (var sentenceTokens in tokenCandidates)
        {
            if (sentenceTokens.Count == 0)
            {
                continue;
            }

            var startIndex = Math.Max(0, cursor);
            for (var i = startIndex; i <= intervalTokens.Count - sentenceTokens.Count; i++)
            {
                if (intervalTokens[i].Start + MinimumStartTolerance < minStartSec)
                {
                    continue;
                }

                if (!string.Equals(intervalTokens[i].Token, sentenceTokens[0], StringComparison.Ordinal))
                {
                    continue;
                }

                var matched = true;
                var localEnd = i;

                for (var k = 1; k < sentenceTokens.Count; k++)
                {
                    var nextIndex = i + k;
                    if (nextIndex >= intervalTokens.Count || !string.Equals(intervalTokens[nextIndex].Token, sentenceTokens[k], StringComparison.Ordinal))
                    {
                        matched = false;
                        break;
                    }

                    localEnd = nextIndex;
                }

                if (!matched)
                {
                    continue;
                }

                start = intervalTokens[i].Start;
                end = intervalTokens[localEnd].End;
                cursor = localEnd + 1;
                return end > start;
            }
        }

        return false;
    }

    private static List<List<string>> ExtractSentenceTokenCandidates(JsonObject sentenceNode, IReadOnlyDictionary<int, (string? ScriptText, string? BookText)>? fallbackTexts)
    {
        sentenceNode.TryGetPropertyValue("scriptText", out var scriptNode);
        sentenceNode.TryGetPropertyValue("bookText", out var bookNode);

        var scriptText = scriptNode?.GetValue<string>();
        var bookText = bookNode?.GetValue<string>();

        var candidates = new List<List<string>>(2);
        var prioritized = new List<string?>();
        prioritized.Add(bookText);
        prioritized.Add(scriptText);

        if (fallbackTexts is not null && sentenceNode.TryGetPropertyValue("id", out var idNode) && idNode is not null)
        {
            var id = idNode.GetValue<int>();
            if (fallbackTexts.TryGetValue(id, out var texts))
            {
                prioritized.Add(texts.BookText);
                prioritized.Add(texts.ScriptText);
            }
        }

        foreach (var candidate in prioritized)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var normalized = TextNormalizer.NormalizeTypography(candidate);
            normalized = normalized.Replace('-', ' ');
            var tokens = new List<string>();
            foreach (var raw in normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var token = Sanitize(raw);
                if (!string.IsNullOrEmpty(token))
                {
                    tokens.Add(token);
                }
            }

            if (tokens.Count > 0)
            {
                // avoid duplicates
                if (!candidates.Any(existing => existing.SequenceEqual(tokens, StringComparer.Ordinal)))
                {
                    candidates.Add(tokens);
                }
            }
        }

        return candidates;
    }

    private sealed record IntervalToken(double Start, double End, string Token);

    public static IReadOnlyDictionary<int, (string? ScriptText, string? BookText)> BuildFallbackTextMap(FileInfo hydrateFile)
    {
        var map = new Dictionary<int, (string? ScriptText, string? BookText)>();

        if (!hydrateFile.Exists)
        {
            return map;
        }

        JsonNode? rootNode;
        try
        {
            rootNode = JsonNode.Parse(File.ReadAllText(hydrateFile.FullName));
        }
        catch
        {
            return map;
        }

        if (rootNode is not JsonObject root || root["sentences"] is not JsonArray sentencesArray)
        {
            return map;
        }

        foreach (var sentence in sentencesArray.OfType<JsonObject>())
        {
            if (!sentence.TryGetPropertyValue("id", out var idNode) || idNode is null)
            {
                continue;
            }

            var id = idNode.GetValue<int>();
            sentence.TryGetPropertyValue("scriptText", out var scriptNode);
            sentence.TryGetPropertyValue("bookText", out var bookNode);

            map[id] = (
                scriptNode?.GetValue<string>(),
                bookNode?.GetValue<string>());
        }

        return map;
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

}
