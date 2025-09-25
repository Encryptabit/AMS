using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ams.Core;
using Ams.Core.Validation;

namespace Ams.Core.Alignment.Anchors;

/// <summary>
/// Detects the most likely book section from the first few ASR tokens by
/// (1) fast path: recognizing explicit section heading tags (e.g., "Chapter 1", "Chapter 28A", "Prologue")
/// (2) fallback: matching against normalized section headings (either precomputed sections
///     or heading-style paragraphs) using similarity + coverage
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
    /// Attempts to detect a section using the first few ASR tokens.
    /// Returns the best matching SectionRange or null if no confident match.
    /// </summary>
    public static SectionRange? DetectSection(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 12)
    {
        if (book is null) return null;
        if (asrTokens is null || asrTokens.Count == 0) return null;

        // Extract a short prefix from ASR and normalize to a heading-friendly form
        var asrHeadingTokens = ExtractAsrHeadingTokens(asrTokens, prefixTokenCount);
        if (asrHeadingTokens.Length == 0) return null;

        // ---- NEW: explicit tag fast-path (handles "Chapter 1", "Chapter 28A", "Prologue", etc.) ----
        if (TryResolveByExplicitTag(book, asrHeadingTokens, out var explicitMatch))
        {
            return explicitMatch;
        }

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

        if (best is null) return null;

        double bestSimilarity = TextNormalizer.CalculateSimilarity(asrHeadingText, best.Normalized);
        double bestCoverage = (double)bestLcp / best.Tokens.Length;

        // Keep the permissive original gate: accept if either similarity is decent OR we cover most of the heading.
        if (bestSimilarity < 0.55 && bestCoverage < 0.6) return null;

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

        // (a) Chapter N [A|B]?
        var m = ChapterTag.Match(normalized);
        if (m.Success)
        {
            int num = int.Parse(m.Groups[1].Value);
            char? suffix = m.Groups[2].Success ? char.ToUpperInvariant(m.Groups[2].Value[0]) : (char?)null;

            var sec = FindByChapterIndex(book, num, suffix);
            if (sec is not null)
            {
                range = sec;
                return true;
            }

            // If exact suffix didn't match, try without the suffix (some docs may omit it in titles)
            sec = FindByChapterIndex(book, num, letter: null);
            if (sec is not null)
            {
                range = sec;
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

    private static SectionRange? FindByChapterIndex(BookIndex book, int chapterNumber, char? letter)
    {
        if (book.Sections is not { Length: > 0 }) return null;

        // Canonical "CHAPTER <N>[A|B]" formats in source sections
        foreach (var sec in book.Sections)
        {
            if (string.IsNullOrWhiteSpace(sec.Title)) continue;
            var titleNorm = NormalizeHeadingLine(sec.Title);

            var match = ChapterTag.Match(titleNorm);
            if (!match.Success) continue;

            int n = int.Parse(match.Groups[1].Value);
            char? ltr = match.Groups[2].Success ? char.ToUpperInvariant(match.Groups[2].Value[0]) : (char?)null;

            if (n != chapterNumber) continue;

            // If caller supplied a suffix, require it; otherwise prefer exact suffix but allow bare-number match.
            if (letter.HasValue)
            {
                if (ltr.HasValue && ltr.Value == letter.Value) return sec;
            }
            else
            {
                // Prefer exact bare-number match, but accept lettered if that's what we have in the index.
                return sec;
            }
        }

        return null;
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
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(asrTokens[i]);
        }

        var normalized = NormalizeHeadingLine(sb.ToString());
        return TextNormalizer.TokenizeWords(normalized);
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
