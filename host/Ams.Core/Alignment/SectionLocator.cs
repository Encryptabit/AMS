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
/// matching against normalized section headings (either precomputed sections
/// or heading-style paragraphs).
/// </summary>
public static class SectionLocator
{
    private static readonly HashSet<string> HeadingKeywords = new(StringComparer.Ordinal)
    {
        "chapter", "prologue", "epilogue", "preface", "introduction", "foreword",
        "prelude", "contents", "appendix", "part", "book", "section", "afterword"
    };

    private static readonly Regex NumberLetterSplit = new(@"(?i)(\d+)([a-z])", RegexOptions.Compiled);
    private static readonly Regex LetterNumberSplit = new(@"(?i)([a-z])(\d+)", RegexOptions.Compiled);

    private sealed record HeadingCandidate(SectionRange Range, string[] Tokens, string Normalized, bool IsStructured);

    /// <summary>
    /// Attempts to detect a section using the first few ASR tokens.
    /// Returns the best matching SectionRange or null if no confident match.
    /// </summary>
    public static SectionRange? DetectSection(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 12)
    {
        if (book is null) return null;
        if (asrTokens is null || asrTokens.Count == 0) return null;

        var candidates = BuildHeadingCandidates(book);
        if (candidates.Count == 0) return null;

        var asrHeadingTokens = ExtractAsrHeadingTokens(asrTokens, prefixTokenCount);
        if (asrHeadingTokens.Length == 0) return null;
        var asrHeadingText = string.Join(' ', asrHeadingTokens);

        HeadingCandidate? best = null;
        double bestScore = 0.0;
        int bestLcp = 0;

        foreach (var candidate in candidates)
        {
            var lcp = LongestCommonPrefix(asrHeadingTokens, candidate.Tokens);
            if (lcp == 0)
            {
                continue;
            }

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
            return null;
        }

        double bestSimilarity = TextNormalizer.CalculateSimilarity(asrHeadingText, best.Normalized);
        double bestCoverage = (double)bestLcp / best.Tokens.Length;
        if (bestSimilarity < 0.55 && bestCoverage < 0.6)
        {
            return null;
        }

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

    private static List<HeadingCandidate> BuildHeadingCandidates(BookIndex book)
    {
        var candidates = new List<HeadingCandidate>();

        if (book.Sections is { Length: > 0 })
        {
            foreach (var sec in book.Sections)
            {
                var normalized = NormalizeHeadingLine(sec.Title);
                var tokens = TextNormalizer.TokenizeWords(normalized);
                if (tokens.Length == 0)
                {
                    continue;
                }

                candidates.Add(new HeadingCandidate(sec, tokens, normalized, true));
            }
        }

        foreach (var candidate in BuildParagraphHeadingCandidates(book))
        {
            if (!candidates.Any(c => c.Range.StartWord == candidate.Range.StartWord))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private static IEnumerable<HeadingCandidate> BuildParagraphHeadingCandidates(BookIndex book)
    {
        if (book.Paragraphs is null || book.Paragraphs.Length == 0)
        {
            yield break;
        }

        var headings = new List<(ParagraphRange Paragraph, string Raw, string Normalized, string[] Tokens)>();
        foreach (var paragraph in book.Paragraphs)
        {
            var raw = ExtractParagraphText(book, paragraph);
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var normalized = NormalizeHeadingLine(raw);
            var tokens = TextNormalizer.TokenizeWords(normalized);
            if (!IsHeadingParagraph(paragraph, tokens))
            {
                continue;
            }

            headings.Add((paragraph, raw.Trim(), normalized, tokens));
        }

        if (headings.Count == 0)
        {
            yield break;
        }

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
            if (i > 0)
            {
                sb.Append(' ');
            }
            sb.Append(asrTokens[i]);
        }

        var normalized = NormalizeHeadingLine(sb.ToString());
        return TextNormalizer.TokenizeWords(normalized);
    }

    private static string NormalizeHeadingLine(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var separated = NumberLetterSplit.Replace(text, "$1 $2");
        separated = LetterNumberSplit.Replace(separated, "$1 $2");
        return TextNormalizer.Normalize(separated, expandContractions: true, removeNumbers: false);
    }

    private static string ExtractParagraphText(BookIndex book, ParagraphRange paragraph)
    {
        int start = Math.Clamp(paragraph.Start, 0, book.Words.Length - 1);
        int end = Math.Clamp(paragraph.End, start, book.Words.Length - 1);
        if (start > end)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (int i = start; i <= end; i++)
        {
            var token = book.Words[i].Text;
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append(token);
        }
        return sb.ToString();
    }

    private static bool IsHeadingParagraph(ParagraphRange paragraph, string[] tokens)
    {
        if (tokens.Length == 0)
        {
            return false;
        }

        bool ifHeadingMeta = (!string.IsNullOrEmpty(paragraph.Kind) && paragraph.Kind.Contains("heading", StringComparison.OrdinalIgnoreCase))
                             || (!string.IsNullOrEmpty(paragraph.Style) && paragraph.Style.Contains("heading", StringComparison.OrdinalIgnoreCase));
        if (ifHeadingMeta)
        {
            return true;
        }

        if (tokens.Length <= 10 && HeadingKeywords.Contains(tokens[0]))
        {
            return true;
        }

        return false;
    }

    private static int DeduceHeadingLevel(ParagraphRange paragraph)
    {
        string? source = paragraph.Style ?? paragraph.Kind;
        if (!string.IsNullOrEmpty(source))
        {
            var digits = new string(source.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var level) && level > 0)
            {
                return level;
            }
        }
        return 1;
    }

    private static int LongestCommonPrefix(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        int n = Math.Min(a.Count, b.Count);
        int k = 0;
        for (; k < n; k++)
        {
            if (!string.Equals(a[k], b[k], StringComparison.Ordinal))
            {
                break;
            }
        }
        return k;
    }
}
