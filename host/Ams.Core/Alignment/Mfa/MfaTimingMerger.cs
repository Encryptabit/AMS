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
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private const double TimingExtensionTolerance = 1e-3;

    public static void MergeTimings(FileInfo hydrateFile, FileInfo asrFile, FileInfo textGridFile, IReadOnlyDictionary<int, (string? ScriptText, string? BookText)>? fallbackTexts = null)
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

        var asrTokens = asr.Tokens;
        var timingMap = BuildTimingMap(asrTokens, textGridIntervals);
        if (timingMap.Count == 0)
        {
            Log.Warn("Unable to map any MFA timings to ASR tokens for {File}", hydrateFile.Name);
            return;
        }

        var intervalTokens = BuildIntervalTokens(textGridIntervals);
        var intervalCursor = 0;
        double previousEndSec = 0.0;

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

            var useBookForTiming = IsUnreliable(sentenceNode);
            var timings = new List<(double start, double end)>();
            int totalTokenCount = 0;
            int mfaTokenCount = 0;
            int asrTokenCount = 0;

            if (!useBookForTiming && startIdx >= 0 && endIdx < asrTokens.Length && startIdx <= endIdx)
            {
                totalTokenCount = endIdx - startIdx + 1;
                for (int idx = startIdx; idx <= endIdx; idx++)
                {
                    if (timingMap.TryGetValue(idx, out var mapped))
                    {
                        timings.Add(mapped);
                        mfaTokenCount++;
                    }
                    else
                    {
                        var token = asrTokens[idx];
                        double startTime = token.StartTime;
                        double endTime = token.StartTime + token.Duration;
                        timings.Add((startTime, endTime));
                        asrTokenCount++;
                    }
                }
            }

            int expectedTokenCount = endIdx >= startIdx ? (endIdx - startIdx + 1) : 0;
            int coveredTokenCount = mfaTokenCount;
            bool needsFallback = useBookForTiming || timings.Count == 0 || (expectedTokenCount > 0 && coveredTokenCount < expectedTokenCount);

            double fallbackStart = 0;
            double fallbackEnd = 0;
            bool fallbackApplied = false;
            if (intervalTokens.Count > 0)
            {
                var fallbackCursor = intervalCursor;
                if (TryFindSentenceTiming(sentenceNode, intervalTokens, ref fallbackCursor, previousEndSec, fallbackTexts, out fallbackStart, out fallbackEnd))
                {
                    double minTiming = timings.Count > 0 ? timings.Min(t => t.start) : double.PositiveInfinity;
                    double maxTiming = timings.Count > 0 ? timings.Max(t => t.end) : double.NegativeInfinity;
                    bool extendsExisting = timings.Count == 0
                        || fallbackStart < minTiming - TimingExtensionTolerance
                        || fallbackEnd > maxTiming + TimingExtensionTolerance;

                    if (needsFallback || extendsExisting)
                    {
                        timings.Add((fallbackStart, fallbackEnd));
                        intervalCursor = fallbackCursor;
                        fallbackApplied = true;
                        Log.Info("MFA fallback timing applied for sentence {SentenceId}: {Start:F3}-{End:F3}{Reason}",
                            sentenceNode["id"]?.GetValue<int?>(),
                            fallbackStart,
                            fallbackEnd,
                            needsFallback ? string.Empty : " (extended coverage)");
                    }
                }
            }

            if (needsFallback && !fallbackApplied && useBookForTiming)
            {
                Log.Info("MFA fallback timing failed for unreliable sentence {SentenceId}", sentenceNode["id"]?.GetValue<int?>());
            }

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

            if (IsUnreliable(sentenceNode) && sentenceNode.ContainsKey("bookText") && sentenceNode.ContainsKey("scriptText"))
            {
                var bookText = sentenceNode["bookText"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(bookText))
                {
                    sentenceNode["scriptText"] = JsonValue.Create(TextNormalizer.NormalizeTypography(bookText));
                }
            }

            updated++;
            previousEndSec = end;

            if (!useBookForTiming && totalTokenCount > 0)
            {
                Log.Info("MFA merge sentence {SentenceId}: tokens={Total} mfa={Mfa} asr={Asr}{Fallback}",
                    sentenceNode["id"]?.GetValue<int?>(),
                    totalTokenCount,
                    mfaTokenCount,
                    asrTokenCount,
                    fallbackApplied ? " fallbackApplied" : string.Empty);
            }
            else if (useBookForTiming && fallbackApplied)
            {
                Log.Info("MFA merge sentence {SentenceId}: fallback applied to unreliable sentence", sentenceNode["id"]?.GetValue<int?>());
            }
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
        Dictionary<int, (double start, double end)> map,
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

    private static bool IsUnreliable(JsonObject sentenceNode)
    {
        if (!sentenceNode.TryGetPropertyValue("status", out var statusNode) || statusNode is null)
        {
            return false;
        }

        var status = statusNode.GetValue<string>();
        return string.Equals(status, "unreliable", StringComparison.OrdinalIgnoreCase);
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
        string? status = null;
        if (sentenceNode.TryGetPropertyValue("status", out var statusNode) && statusNode is not null)
        {
            status = statusNode.GetValue<string>();
        }

        sentenceNode.TryGetPropertyValue("scriptText", out var scriptNode);
        sentenceNode.TryGetPropertyValue("bookText", out var bookNode);

        var scriptText = scriptNode?.GetValue<string>();
        var bookText = bookNode?.GetValue<string>();

        var candidates = new List<List<string>>(2);

        var prioritized = new List<string?>();
        if (string.Equals(status, "unreliable", StringComparison.OrdinalIgnoreCase))
        {
            prioritized.Add(bookText);
            prioritized.Add(scriptText);
        }
        else
        {
            prioritized.Add(scriptText);
            prioritized.Add(bookText);
        }

        if (fallbackTexts is not null && sentenceNode.TryGetPropertyValue("id", out var idNode) && idNode is not null)
        {
            var id = idNode.GetValue<int>();
            if (fallbackTexts.TryGetValue(id, out var texts))
            {
                var fallbackFirst = string.Equals(status, "unreliable", StringComparison.OrdinalIgnoreCase)
                    ? texts.BookText
                    : texts.ScriptText;
                var fallbackSecond = string.Equals(status, "unreliable", StringComparison.OrdinalIgnoreCase)
                    ? texts.ScriptText
                    : texts.BookText;

                prioritized.Add(fallbackFirst);
                prioritized.Add(fallbackSecond);
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
