using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ams.Core;
using Ams.Core.Validation;

namespace Ams.Core.Alignment.Anchors;

/// <summary>
/// Detects likely sections by combining explicit heading heuristics with fuzzy matching of normalized script text.
/// </summary>

public static class SectionLocator
{
    private static readonly HashSet<string> HeadingKeywords = new(StringComparer.Ordinal)
    {
        "chapter", "prologue", "epilogue", "preface", "introduction", "foreword",
        "prelude", "contents", "appendix", "part", "book", "section", "afterword"
    };

    // Handles "28a" <-> "28 a" and "a28" <-> "a 28" for robust tokenization
    private static readonly Regex NumberLetterSplit = new(@"(?i)(\d+)([a-z])", RegexOptions.Compiled);
    private static readonly Regex LetterNumberSplit = new(@"(?i)([a-z])(\d+)", RegexOptions.Compiled);

    // --- NEW: explicit tag detection (fast-path) ---
    // Examples matched (case-insensitive, punctuation tolerated when normalized):
    // "chapter 1", "chapter 1.", "chapter 28a", "chapter 28a:", "prologue", "epilogue", "afterword"
    private static readonly Regex ChapterTag = new(@"(?i)\bchapter\s+(\d+)\s*([a-z])?\b", RegexOptions.Compiled);
    private static readonly Regex SingleWordHeadTag = new(@"(?i)\b(prologue|epilogue|preface|introduction|foreword|prelude|afterword|appendix|contents)\b", RegexOptions.Compiled);

    private sealed record HeadingCandidate(SectionRange Range, string[] Tokens, string Normalized, bool IsStructured);

    /// <summary>
    /// </summary>
    private static readonly Regex ChapterWordTag = new(@"(?i)\bchapter\s+([a-z][a-z\s-]*?)(?:\s+([a-z]))?\b", RegexOptions.Compiled);

    private static readonly HashSet<string> LeadingNoiseTokens = new(StringComparer.Ordinal)
    {
        "model", "version", "nvidia", "parakeet", "tdt", "total", "words", "computing", "anchors",
        "anchor", "rendering", "timeline", "built", "original", "audio", "hydrated", "hydrating",
        "transcript", "index", "generating", "roomtone", "written", "fallback", "restricting",
        "diagnostics", "emitting", "section", "detection", "gap", "candidate", "final", "meanrms",
        "prefix", "tokens"
    };

    private static readonly Dictionary<string, int> CardinalNumbers = new(StringComparer.Ordinal)
    {
        ["zero"] = 0,
        ["one"] = 1,
        ["two"] = 2,
        ["three"] = 3,
        ["four"] = 4,
        ["five"] = 5,
        ["six"] = 6,
        ["seven"] = 7,
        ["eight"] = 8,
        ["nine"] = 9,
        ["ten"] = 10,
        ["eleven"] = 11,
        ["twelve"] = 12,
        ["thirteen"] = 13,
        ["fourteen"] = 14,
        ["fifteen"] = 15,
        ["sixteen"] = 16,
        ["seventeen"] = 17,
        ["eighteen"] = 18,
        ["nineteen"] = 19
    };

    private static readonly Dictionary<string, int> TensNumbers = new(StringComparer.Ordinal)
    {
        ["twenty"] = 20,
        ["thirty"] = 30,
        ["forty"] = 40,
        ["fifty"] = 50,
        ["sixty"] = 60,
        ["seventy"] = 70,
        ["eighty"] = 80,
        ["ninety"] = 90
    };

    private static readonly Dictionary<string, int> OrdinalNumbers = new(StringComparer.Ordinal)
    {
        ["first"] = 1,
        ["second"] = 2,
        ["third"] = 3,
        ["fourth"] = 4,
        ["fifth"] = 5,
        ["sixth"] = 6,
        ["seventh"] = 7,
        ["eighth"] = 8,
        ["ninth"] = 9,
        ["tenth"] = 10,
        ["eleventh"] = 11,
        ["twelfth"] = 12,
        ["thirteenth"] = 13,
        ["fourteenth"] = 14,
        ["fifteenth"] = 15,
        ["sixteenth"] = 16,
        ["seventeenth"] = 17,
        ["eighteenth"] = 18,
        ["nineteenth"] = 19,
        ["twentieth"] = 20,
        ["thirtieth"] = 30,
        ["fortieth"] = 40,
        ["fiftieth"] = 50,
        ["sixtieth"] = 60,
        ["seventieth"] = 70,
        ["eightieth"] = 80,
        ["ninetieth"] = 90,
        ["hundredth"] = 100
    };

    /// <summary>
    /// Attempts to detect the most likely book section from the first few ASR tokens.
    /// Returns the best matching SectionRange or null if no confident match is found.
    /// </summary>

    public static SectionRange? DetectSection(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 12)
    {
        if (book is null) return null;
        if (asrTokens is null || asrTokens.Count == 0) return null;

        // Extract a short prefix from ASR and normalize to a heading-friendly form
        var asrHeadingTokens = ExtractAsrHeadingTokens(asrTokens, prefixTokenCount);
        if (asrHeadingTokens.Length == 0)
        {
            Console.WriteLine($"[SectionDetect] prefix (requested {prefixTokenCount}) produced no normalized heading tokens.");
            return null;
        }

        Console.WriteLine($"[SectionDetect] prefix (requested {prefixTokenCount}) -> {asrHeadingTokens.Length} tokens: {string.Join(' ', asrHeadingTokens.Take(16))}");

        // ---- NEW: explicit tag fast-path (handles "Chapter 1", "Chapter 28A", "Prologue", etc.) ----
        if (TryResolveByExplicitTag(book, asrHeadingTokens, out var explicitMatch))
        {
            Console.WriteLine($"[SectionDetect] explicit match -> {explicitMatch.Title} [{explicitMatch.StartWord}, {explicitMatch.EndWord}]");
            return explicitMatch;
        }
        Console.WriteLine("[SectionDetect] explicit match not found; falling back to fuzzy heading match.");

        // ---- Original approach: fuzzy heading match as a fallback ----
        var candidates = BuildHeadingCandidates(book);
        if (candidates.Count == 0) return null;

        var asrHeadingText = string.Join(' ', asrHeadingTokens);

        HeadingCandidate? best = null;
        double bestScore = 0.0;
        int bestLcp = 0;

        foreach (var candidate in candidates)
        {
            var lcp = LongestCommonPrefix(asrHeadingTokens, candidate.Tokens);
            if (lcp == 0) continue;

            var candidateText = candidate.Normalized;
            double similarity = TextNormalizer.CalculateSimilarity(asrHeadingText, candidateText);
            double coverage = (double)lcp / candidate.Tokens.Length;
            double score = similarity + 0.1 * coverage + (candidate.IsStructured ? 0.05 : 0.0);

            if (score > bestScore + 1e-6 ||
                (Math.Abs(score - bestScore) <= 1e-6 &&
                 (lcp > bestLcp || (lcp == bestLcp && candidate.Tokens.Length > (best?.Tokens.Length ?? 0)))))
            {
                best = candidate;
                bestScore = score;
                bestLcp = lcp;
            }
        }

        if (best is null)
        {
            Console.WriteLine("[SectionDetect] fuzzy fallback found no candidate.");
            return null;
        }

        double bestSimilarity = TextNormalizer.CalculateSimilarity(asrHeadingText, best.Normalized);
        double bestCoverage = (double)bestLcp / best.Tokens.Length;

        // Keep the permissive original gate: accept if either similarity is decent OR we cover most of the heading.
        if (bestSimilarity < 0.55 && bestCoverage < 0.6)
        {
            Console.WriteLine($"[SectionDetect] fuzzy candidate rejected: similarity={bestSimilarity:F2}, coverage={bestCoverage:F2}.");
            return null;
        }

        Console.WriteLine($"[SectionDetect] fuzzy candidate -> {best.Range.Title} [{best.Range.StartWord}, {best.Range.EndWord}] (similarity={bestSimilarity:F2}, coverage={bestCoverage:F2}).");
        return best.Range;
    }

    /// <summary>
    /// Returns the word-index window [start,end] of the detected section, or null.
    /// </summary>
    public static (int startWord, int endWord)? DetectSectionWindow(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 12)
    {
        var section = DetectSection(book, asrTokens, prefixTokenCount);
        return section == null ? null : (section.StartWord, section.EndWord);
    }

    // ---------- Fast-path explicit tag detection ----------

    private static bool TryResolveByExplicitTag(BookIndex book, string[] asrHeadingTokens, out SectionRange? range)
    {
        // Normalize the first tokens to a single line and split digits/letters to match "28A" robustly.
        var asrLine = string.Join(' ', asrHeadingTokens);
        var normalized = NormalizeHeadingLine(asrLine);

        if (TryExtractChapterNumber(normalized, out var number, out var suffix))
        {
            Console.WriteLine($"Normalized: {normalized}");
            var section = FindByChapterIndex(book, number, suffix);
            if (section is not null)
            {
                range = section;
                return true;
            }
        }

        // (b) Single-word headings like "Prologue", "Epilogue", etc.
        var m2 = SingleWordHeadTag.Match(normalized);
        if (m2.Success)
        {
            var key = m2.Groups[1].Value; // already lowercase due to NormalizeHeadingLine
            var sec = FindSingleWordHeading(book, key);
            if (sec is not null)
            {
                range = sec;
                return true;
            }
        }

        range = null;
        return false;
    }

    private static string[] TrimHeadingNoiseTokens(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return Array.Empty<string>();
        }

        int start = 0;
        if (tokens.Count >= 3 && tokens[0] == "section" && tokens[1] == "detection" && tokens[2] == "fallback")
        {
            start = 3;
        }

        if (tokens.Count >= 2 && tokens[0] == "timeline" && tokens[1] == "built")
        {
            start = Math.Max(start, 2);
        }

        while (start < tokens.Count && ShouldDropLeadingToken(tokens[start]))
        {
            start++;
        }

        if (start >= tokens.Count)
        {
            return Array.Empty<string>();
        }

        if (start == 0 && tokens is string[] direct)
        {
            return direct;
        }

        var result = new string[tokens.Count - start];
        for (int i = start; i < tokens.Count; i++)
        {
            result[i - start] = tokens[i];
        }

        return result;
    }

    private static bool ShouldDropLeadingToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return true;
        }

        if (HeadingKeywords.Contains(token))
        {
            return false;
        }

        if (LeadingNoiseTokens.Contains(token))
        {
            return true;
        }

        if (token.All(ch => !char.IsLetterOrDigit(ch)))
        {
            return true;
        }

        if (token.All(char.IsDigit))
        {
            return true;
        }

        if (token.Any(char.IsDigit) && token.Length <= 4)
        {
            return true;
        }

        if (token.Length == 1)
        {
            return true;
        }

        return false;
    }

    private static bool IsNumberWordToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        if (CardinalNumbers.ContainsKey(token) || TensNumbers.ContainsKey(token) || OrdinalNumbers.ContainsKey(token))
        {
            return true;
        }

        if (token is "and" or "hundred" or "hundredth" or "thousand" or "thousandth")
        {
            return true;
        }

        if (token.Contains('-'))
        {
            var parts = token.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1 && parts.All(IsNumberWordToken))
            {
                return true;
            }

            var sanitized = token.Replace('-', ' ');
            if (TryParseNumberWords(sanitized, out var parsed) && parsed > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseNumberWords(string phrase, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return false;
        }

        var sanitized = phrase.Replace('-', ' ');
        var tokens = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return false;
        }

        int total = 0;
        int current = 0;
        bool seenAny = false;

        foreach (var token in tokens)
        {
            if (token == "and")
            {
                continue;
            }

            if (OrdinalNumbers.TryGetValue(token, out var ordinal))
            {
                current += ordinal;
                seenAny = true;
                break; // ordinals typically terminate the phrase
            }

            if (CardinalNumbers.TryGetValue(token, out var cardinal))
            {
                current += cardinal;
                seenAny = true;
                continue;
            }

            if (TensNumbers.TryGetValue(token, out var tens))
            {
                current += tens;
                seenAny = true;
                continue;
            }

            if (token == "hundred")
            {
                if (current == 0)
                {
                    current = 1;
                }
                current *= 100;
                seenAny = true;
                continue;
            }

            if (token == "thousand")
            {
                if (current == 0)
                {
                    current = 1;
                }
                total += current * 1000;
                current = 0;
                seenAny = true;
                continue;
            }

            if (seenAny)
            {
                break;
            }

            return false;
        }

        value = total + current;
        return seenAny && value > 0;
    }

    private static SectionRange? FindByChapterIndex(BookIndex book, int chapterNumber, char? letter)
{
    if (book.Sections is not { Length: > 0 }) return null;

    SectionRange? fallback = null;

    foreach (var sec in book.Sections)
    {
        if (string.IsNullOrWhiteSpace(sec.Title)) continue;

        var titleNorm = NormalizeHeadingLine(sec.Title);
        if (!TryExtractChapterNumber(titleNorm, out var number, out var suffix))
        {
            continue;
        }

        if (number != chapterNumber)
        {
            continue;
        }

        if (letter.HasValue)
        {
            if (suffix.HasValue && suffix.Value == letter.Value)
            {
                return sec;
            }

            if (fallback is null && !suffix.HasValue)
            {
                fallback = sec;
            }

            continue;
        }

        if (!suffix.HasValue)
        {
            return sec;
        }

        fallback ??= sec;
    }

    return fallback;
}

    private static bool TryExtractChapterNumber(string titleNorm, out int number, out char? suffix)
{
    number = 0;
    suffix = null;

    if (string.IsNullOrWhiteSpace(titleNorm))
    {
        return false;
    }

    var numericMatch = ChapterTag.Match(titleNorm);
    if (numericMatch.Success)
    {
        number = int.Parse(numericMatch.Groups[1].Value);
        suffix = numericMatch.Groups[2].Success
            ? char.ToUpperInvariant(numericMatch.Groups[2].Value[0])
            : (char?)null;
        return true;
    }

    var tokens = TextNormalizer.TokenizeWords(titleNorm);
    if (tokens.Length >= 2 && tokens[0] == "chapter")
    {
        int idx = 1;
        var numberTokens = new List<string>();
        for (; idx < tokens.Length; idx++)
        {
            var token = tokens[idx];
            if (IsNumberWordToken(token))
            {
                numberTokens.Add(token);
                continue;
            }

            break;
        }

        if (numberTokens.Count > 0)
        {
            var phrase = string.Join(' ', numberTokens);
            if (TryParseNumberWords(phrase, out var parsed) && parsed > 0)
            {
                number = parsed;
                if (idx < tokens.Length && tokens[idx].Length == 1 && char.IsLetter(tokens[idx][0]))
                {
                    suffix = char.ToUpperInvariant(tokens[idx][0]);
                }

                return true;
            }
        }
    }

    var wordsMatch = ChapterWordTag.Match(titleNorm);
    if (wordsMatch.Success)
    {
        var phrase = wordsMatch.Groups[1].Value.Trim();
        if (TryParseNumberWords(phrase, out var parsed) && parsed > 0)
        {
            number = parsed;
            suffix = wordsMatch.Groups[2].Success
                ? char.ToUpperInvariant(wordsMatch.Groups[2].Value[0])
                : (char?)null;
            return true;
        }
    }

    return false;
}

    private static SectionRange? FindSingleWordHeading(BookIndex book, string keyLower)
    {
        if (book.Sections is not { Length: > 0 }) return null;

        foreach (var sec in book.Sections)
        {
            var titleNorm = NormalizeHeadingLine(sec.Title);
            // Titles like "PROLOGUE" / "EPILOGUE" etc.
            if (string.Equals(titleNorm, keyLower, StringComparison.Ordinal)) return sec;

            // Also accept titles that start with key (e.g., "Appendix A", "Introduction: ...")
            if (titleNorm.StartsWith(keyLower + " ", StringComparison.Ordinal)) return sec;
        }

        return null;
    }

    // ---------- Original candidate builder & helpers ----------

    private static List<HeadingCandidate> BuildHeadingCandidates(BookIndex book)
    {
        var candidates = new List<HeadingCandidate>();

        if (book.Sections is { Length: > 0 })
        {
            foreach (var sec in book.Sections)
            {
                var normalized = NormalizeHeadingLine(sec.Title);
                var tokens = TextNormalizer.TokenizeWords(normalized);
                if (tokens.Length == 0) continue;

                candidates.Add(new HeadingCandidate(sec, tokens, normalized, true));
            }
        }

        foreach (var candidate in BuildParagraphHeadingCandidates(book))
        {
            if (!candidates.Any(c => c.Range.StartWord == candidate.Range.StartWord))
                candidates.Add(candidate);
        }

        return candidates;
    }

    private static IEnumerable<HeadingCandidate> BuildParagraphHeadingCandidates(BookIndex book)
    {
        if (book.Paragraphs is null || book.Paragraphs.Length == 0) yield break;

        var headings = new List<(ParagraphRange Paragraph, string Raw, string Normalized, string[] Tokens)>();
        foreach (var paragraph in book.Paragraphs)
        {
            var raw = ExtractParagraphText(book, paragraph);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var normalized = NormalizeHeadingLine(raw);
            var tokens = TextNormalizer.TokenizeWords(normalized);
            if (!IsHeadingParagraph(paragraph, tokens)) continue;

            headings.Add((paragraph, raw.Trim(), normalized, tokens));
        }

        if (headings.Count == 0) yield break;

        int maxWordIndex = Math.Max(0, book.Words.Length - 1);
        for (int i = 0; i < headings.Count; i++)
        {
            var (paragraph, raw, normalized, tokens) = headings[i];
            int startWord = Math.Clamp(paragraph.Start, 0, maxWordIndex);
            int nextStart = (i + 1 < headings.Count) ? headings[i + 1].Paragraph.Start : book.Words.Length;
            int endWord = Math.Max(startWord, Math.Min(maxWordIndex, nextStart - 1));
            int startParagraph = paragraph.Index;
            int endParagraph = (i + 1 < headings.Count)
                ? Math.Max(paragraph.Index, headings[i + 1].Paragraph.Index - 1)
                : book.Paragraphs[^1].Index;

            var range = new SectionRange(
                paragraph.Index,
                raw,
                DeduceHeadingLevel(paragraph),
                paragraph.Kind ?? paragraph.Style ?? "Heading",
                startWord,
                endWord,
                startParagraph,
                endParagraph
            );

            yield return new HeadingCandidate(range, tokens, normalized, false);
        }
    }

    private static string[] ExtractAsrHeadingTokens(IReadOnlyList<string> asrTokens, int prefixTokenCount)
    {
        int count = Math.Max(1, Math.Min(prefixTokenCount, asrTokens.Count));
        var builder = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0) builder.Append(' ');
            builder.Append(asrTokens[i]);
        }

        string normalized = NormalizeHeadingLine(builder.ToString());
        var tokens = TextNormalizer.TokenizeWords(normalized);
        var trimmed = TrimHeadingNoiseTokens(tokens);

        int next = count;
        int maxTokens = Math.Min(asrTokens.Count, count + 12);
        while (trimmed.Length == 0 && next < maxTokens)
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append(asrTokens[next]);
            next++;

            normalized = NormalizeHeadingLine(builder.ToString());
            tokens = TextNormalizer.TokenizeWords(normalized);
            trimmed = TrimHeadingNoiseTokens(tokens);
        }

        return trimmed;
    }

    private static string NormalizeHeadingLine(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Ensure digits/letters are separated so "28A" and "28 A" normalize consistently
        var separated = NumberLetterSplit.Replace(text, "$1 $2");
        separated = LetterNumberSplit.Replace(separated, "$1 $2");
        return TextNormalizer.Normalize(separated, expandContractions: true, removeNumbers: false);
    }

    private static string ExtractParagraphText(BookIndex book, ParagraphRange paragraph)
    {
        int start = Math.Clamp(paragraph.Start, 0, book.Words.Length - 1);
        int end = Math.Clamp(paragraph.End, start, book.Words.Length - 1);
        if (start > end) return string.Empty;

        var sb = new StringBuilder();
        for (int i = start; i <= end; i++)
        {
            var token = book.Words[i].Text;
            if (string.IsNullOrWhiteSpace(token)) continue;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(token);
        }
        return sb.ToString();
    }

    private static bool IsHeadingParagraph(ParagraphRange paragraph, string[] tokens)
    {
        if (tokens.Length == 0) return false;

        bool ifHeadingMeta = (!string.IsNullOrEmpty(paragraph.Kind) && paragraph.Kind.Contains("heading", StringComparison.OrdinalIgnoreCase))
                             || (!string.IsNullOrEmpty(paragraph.Style) && paragraph.Style.Contains("heading", StringComparison.OrdinalIgnoreCase));
        if (ifHeadingMeta) return true;

        if (tokens.Length <= 10 && HeadingKeywords.Contains(tokens[0])) return true;

        return false;
    }

    private static int DeduceHeadingLevel(ParagraphRange paragraph)
    {
        string? source = paragraph.Style ?? paragraph.Kind;
        if (!string.IsNullOrEmpty(source))
        {
            var digits = new string(source.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var level) && level > 0) return level;
        }
        return 1;
    }

    private static int LongestCommonPrefix(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        int n = Math.Min(a.Count, b.Count);
        int k = 0;
        for (; k < n; k++)
        {
            if (!string.Equals(a[k], b[k], StringComparison.Ordinal)) break;
        }
        return k;
    }
}
