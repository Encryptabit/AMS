using System.Text.Json;
using System.Text.Json.Nodes;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Chapter;

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

    public static void MergeTimings(
        ChapterContext chapter,
        FileInfo textGridFile,
        bool updateHydratedTranscript = true,
        bool updateTranscriptIndex = true)
    {
        if (!updateHydratedTranscript && !updateTranscriptIndex)
        {
            return;
        }

        var textGridDocument = chapter.Documents.TextGrid;
        if (textGridDocument is null || textGridDocument.Intervals.Count == 0)
        {
            Log.Debug("TextGrid document not loaded; skipping MFA timing merge for {Chapter}", chapter.Descriptor.ChapterId);
            return;
        }

        var bookIndex = chapter.Book.Documents.BookIndex ?? throw new InvalidOperationException("BookIndex is not loaded.");
        if (bookIndex.Words.Length == 0)
        {
            Log.Debug("BookIndex contains no words; skipping MFA timing merge");
            return;
        }

        var nonEmptyIntervalCount = textGridDocument.Intervals.Count(static interval => !string.IsNullOrWhiteSpace(interval.Text));
        if (nonEmptyIntervalCount == 0)
        {
            Log.Debug("TextGrid contained no word intervals ({File})", textGridFile.FullName);
            return;
        }

        var (chapterStartWord, chapterEndWord) = ResolveChapterWordWindow(chapter, bookIndex);
        var chapterWordCount = Math.Max(0, chapterEndWord - chapterStartWord + 1);
        if (chapterWordCount == 0)
        {
            Log.Debug("Chapter {Chapter} has an empty word window; skipping MFA timing merge", chapter.Descriptor.ChapterId);
            return;
        }

        if (nonEmptyIntervalCount != chapterWordCount)
        {
            Log.Debug(
                "TextGrid word count {GridCount} differs from chapter word window ({ChapterCount}); mapping sequentially",
                nonEmptyIntervalCount,
                chapterWordCount);
        }

        var wordTimings = BuildWordTimingIndex(chapterStartWord, chapterEndWord, textGridDocument.Intervals);

        HydrateUpdateResult hydrateResult = new(null, 0, 0);
        if (updateHydratedTranscript && chapter.Documents.HydratedTranscript is { } hydratedTranscript)
        {
            hydrateResult = ApplyHydratedTranscriptTimings(hydratedTranscript, wordTimings);
            if (hydrateResult.Transcript is not null)
            {
                chapter.Documents.HydratedTranscript = hydrateResult.Transcript;
            }
        }

        TranscriptUpdateResult transcriptResult = new(null, 0);
        if (updateTranscriptIndex && chapter.Documents.Transcript is { } transcript)
        {
            transcriptResult = ApplyTranscriptIndexTimings(transcript, wordTimings);
            if (transcriptResult.Transcript is not null)
            {
                chapter.Documents.Transcript = transcriptResult.Transcript;
            }
        }

        if (hydrateResult.Transcript is null && transcriptResult.Transcript is null)
        {
            Log.Debug("No MFA timings applied for chapter {Chapter}", chapter.Descriptor.ChapterId);
            return;
        }

        chapter.Documents.SaveChanges();

        if (hydrateResult.Transcript is not null)
        {
            Log.Debug(
                "Updated {SentenceCount} hydrated sentences and {WordCount} hydrated words with MFA timings for ({Chapter})",
                hydrateResult.SentencesUpdated,
                hydrateResult.WordsUpdated,
                chapter.Descriptor.ChapterId);
        }

        if (transcriptResult.Transcript is not null)
        {
            Log.Debug(
                "Updated {SentenceCount} transcript sentences with MFA timings for ({Chapter})",
                transcriptResult.SentencesUpdated,
                chapter.Descriptor.ChapterId);
        }
    }

    private static List<IntervalToken> BuildIntervalTokens(IReadOnlyList<TextGridInterval> intervals)
    {
        var tokens = new List<IntervalToken>(intervals.Count);
        tokens.AddRange(from interval in intervals
            let normalized = TextNormalizer.NormalizeTypography(interval.Text)
            let token = Sanitize(normalized)
            where !string.IsNullOrEmpty(token)
            select new IntervalToken(interval.Start, interval.End, token));

        return tokens;
    }

    private static ChapterWordTimings BuildWordTimingIndex(int startWord, int endWord, IReadOnlyList<TextGridInterval> intervals)
    {
        var length = Math.Max(0, endWord - startWord + 1);
        var map = new TimingRange?[length];
        if (length == 0 || intervals.Count == 0)
        {
            return new ChapterWordTimings(startWord, endWord, map);
        }

        var intervalIndex = 0;
        for (var chapterIdx = 0; chapterIdx < length && intervalIndex < intervals.Count;)
        {
            var interval = intervals[intervalIndex++];
            if (string.IsNullOrWhiteSpace(interval.Text))
            {
                continue;
            }

            if (interval.End <= interval.Start)
            {
                continue;
            }

            map[chapterIdx++] = new TimingRange(interval.Start, interval.End);
        }

        return new ChapterWordTimings(startWord, endWord, map);
    }

    private static HydrateUpdateResult ApplyHydratedTranscriptTimings(HydratedTranscript hydrate, ChapterWordTimings timingWindow)
    {
        if (hydrate.Sentences.Count == 0 && hydrate.Words.Count == 0)
        {
            return new HydrateUpdateResult(null, 0, 0);
        }

        List<HydratedSentence>? updatedSentences = null;
        var sentencesUpdated = 0;
        for (var i = 0; i < hydrate.Sentences.Count; i++)
        {
            var sentence = hydrate.Sentences[i];
            var relativeStart = sentence.BookRange.Start - timingWindow.StartWord;
            var relativeEnd = sentence.BookRange.End - timingWindow.StartWord;
            if (!TryComputeTiming(timingWindow.Timings, relativeStart, relativeEnd, out var timing))
            {
                continue;
            }

            if (sentence.Timing is { } existingTiming && existingTiming.Equals(timing))
            {
                continue;
            }

            updatedSentences ??= hydrate.Sentences.ToList();
            updatedSentences[i] = sentence with { Timing = timing };
            sentencesUpdated++;
        }

        List<HydratedWord>? updatedWords = null;
        var wordsUpdated = 0;
        for (var i = 0; i < hydrate.Words.Count; i++)
        {
            var word = hydrate.Words[i];
            if (word.BookIdx is not int bookIdx)
            {
                continue;
            }

            var relativeIdx = bookIdx - timingWindow.StartWord;
            if (!TryGetWordTiming(timingWindow.Timings, relativeIdx, out var timing))
            {
                continue;
            }

            var startSec = timing.StartSec;
            var endSec = timing.EndSec;
            var durationSec = Math.Max(0, endSec - startSec);
            if (AreClose(word.StartSec, startSec) &&
                AreClose(word.EndSec, endSec) &&
                AreClose(word.DurationSec, durationSec))
            {
                continue;
            }

            updatedWords ??= hydrate.Words.ToList();
            updatedWords[i] = word with
            {
                StartSec = startSec,
                EndSec = endSec,
                DurationSec = durationSec
            };
            wordsUpdated++;
        }

        if (sentencesUpdated == 0 && wordsUpdated == 0)
        {
            return new HydrateUpdateResult(null, 0, 0);
        }

        var updated = hydrate with
        {
            Sentences = updatedSentences ?? hydrate.Sentences,
            Words = updatedWords ?? hydrate.Words
        };

        return new HydrateUpdateResult(updated, sentencesUpdated, wordsUpdated);
    }

    private static TranscriptUpdateResult ApplyTranscriptIndexTimings(TranscriptIndex transcript, ChapterWordTimings timingWindow)
    {
        if (transcript.Sentences.Count == 0)
        {
            return new TranscriptUpdateResult(null, 0);
        }

        List<SentenceAlign>? updatedSentences = null;
        var updatedCount = 0;
        for (var i = 0; i < transcript.Sentences.Count; i++)
        {
            var sentence = transcript.Sentences[i];
            var relativeStart = sentence.BookRange.Start - timingWindow.StartWord;
            var relativeEnd = sentence.BookRange.End - timingWindow.StartWord;
            if (!TryComputeTiming(timingWindow.Timings, relativeStart, relativeEnd, out var timing))
            {
                continue;
            }

            if (sentence.Timing is { } existingTiming && existingTiming.Equals(timing))
            {
                continue;
            }

            updatedSentences ??= transcript.Sentences.ToList();
            updatedSentences[i] = sentence with { Timing = timing };
            updatedCount++;
        }

        if (updatedCount == 0)
        {
            return new TranscriptUpdateResult(null, 0);
        }

        var updated = transcript with { Sentences = updatedSentences! };
        return new TranscriptUpdateResult(updated, updatedCount);
    }

    private static bool TryComputeTiming(TimingRange?[] wordTimings, int startIndex, int endIndex, out TimingRange timing)
    {
        timing = TimingRange.Empty;
        if (wordTimings.Length == 0)
        {
            return false;
        }

        var minIndex = Math.Min(startIndex, endIndex);
        var maxIndex = Math.Max(startIndex, endIndex);
        if (maxIndex < 0 || minIndex >= wordTimings.Length)
        {
            return false;
        }

        var lo = Math.Max(0, minIndex);
        var hi = Math.Min(wordTimings.Length - 1, maxIndex);

        double? start = null;
        double? end = null;

        for (var i = lo; i <= hi; i++)
        {
            if (wordTimings[i] is { } range)
            {
                start = range.StartSec;
                break;
            }
        }

        for (var i = hi; i >= lo; i--)
        {
            if (wordTimings[i] is { } range)
            {
                end = range.EndSec;
                break;
            }
        }

        if (!start.HasValue || !end.HasValue || end.Value <= start.Value)
        {
            return false;
        }

        timing = new TimingRange(start.Value, end.Value);
        return true;
    }

    private static bool TryGetWordTiming(TimingRange?[] wordTimings, int index, out TimingRange timing)
    {
        timing = TimingRange.Empty;
        if (index < 0 || index >= wordTimings.Length)
        {
            return false;
        }

        if (wordTimings[index] is not { } range)
        {
            return false;
        }

        timing = range;
        return true;
    }

    private static bool AreClose(double? left, double? right)
    {
        if (!left.HasValue && !right.HasValue)
        {
            return true;
        }

        if (!left.HasValue || !right.HasValue)
        {
            return false;
        }

        return Math.Abs(left.Value - right.Value) < 1e-6;
    }

    private static (int startWord, int endWord) ResolveChapterWordWindow(ChapterContext chapter, BookIndex bookIndex)
    {
        var start = chapter.Descriptor.BookStartWord;
        var end = chapter.Descriptor.BookEndWord;

        if (!start.HasValue || !end.HasValue)
        {
            foreach (var label in EnumerateChapterLabels(chapter))
            {
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                var section = SectionLocator.ResolveSectionByTitle(bookIndex, label);
                if (section is not null)
                {
                    start = section.StartWord;
                    end = section.EndWord;
                    break;
                }
            }
        }

        var maxIndex = Math.Max(0, bookIndex.Words.Length - 1);
        if (!start.HasValue || !end.HasValue)
        {
            return (0, maxIndex);
        }

        var normalizedStart = Math.Clamp(start.Value, 0, maxIndex);
        var normalizedEnd = Math.Clamp(end.Value, normalizedStart, maxIndex);
        return (normalizedStart, normalizedEnd);
    }

    private static IEnumerable<string> EnumerateChapterLabels(ChapterContext chapter)
    {
        if (!string.IsNullOrWhiteSpace(chapter.Descriptor.ChapterId))
        {
            yield return chapter.Descriptor.ChapterId;
        }

        foreach (var alias in chapter.Descriptor.Aliases)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                yield return alias;
            }
        }

        if (!string.IsNullOrWhiteSpace(chapter.Descriptor.RootPath))
        {
            var label = Path.GetFileName(chapter.Descriptor.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(label))
            {
                yield return label;
            }
        }
    }

    private sealed record ChapterWordTimings(int StartWord, int EndWord, TimingRange?[] Timings);

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
        sentenceNode.TryGetPropertyValue("bookText", out var bookNode);

        var bookText = bookNode?.GetValue<string>();

        var candidates = new List<List<string>>(2);
        var prioritized = new List<string?>
        {
            bookText
        };

        if (fallbackTexts is not null && sentenceNode.TryGetPropertyValue("id", out var idNode) && idNode is not null)
        {
            var id = idNode.GetValue<int>();
            if (fallbackTexts.TryGetValue(id, out var texts))
            {
                prioritized.Add(texts.BookText);
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

    private sealed record HydrateUpdateResult(HydratedTranscript? Transcript, int SentencesUpdated, int WordsUpdated);

    private sealed record TranscriptUpdateResult(TranscriptIndex? Transcript, int SentencesUpdated);

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
