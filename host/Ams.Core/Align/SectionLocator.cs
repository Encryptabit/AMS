using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core;
using Ams.Core.Validation;

namespace Ams.Align.Anchors;

/// <summary>
/// Detects the most likely book section from the first few ASR tokens
/// by matching against normalized section titles (e.g., "chapter fourteen", "prologue").
/// </summary>
public static class SectionLocator
{
    private static readonly HashSet<string> HeadingKeywords = new(StringComparer.Ordinal)
    {
        "chapter","prologue","epilogue","preface","introduction","foreword","prelude","contents"
    };

    /// <summary>
    /// Attempts to detect a section using the first few ASR tokens.
    /// Returns the best matching SectionRange or null if no confident match.
    /// </summary>
    public static SectionRange? DetectSection(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 8)
    {
        if (book.Sections == null || book.Sections.Length == 0) return null;
        if (asrTokens == null || asrTokens.Count == 0) return null;

        // Normalize a short ASR prefix (keeps numbers but converts digits to words)
        var asrPrefix = string.Join(" ", asrTokens.Take(Math.Max(1, prefixTokenCount)));
        var asrNorm = TextNormalizer.Normalize(asrPrefix, expandContractions: true, removeNumbers: false);
        var asrNormToks = TextNormalizer.TokenizeWords(asrNorm);
        if (asrNormToks.Length == 0) return null;

        // Compute a simple prefix-match score against each section title
        SectionRange? best = null;
        int bestScore = 0;
        foreach (var sec in book.Sections)
        {
            var title = sec.Title ?? string.Empty;
            var titleNorm = TextNormalizer.Normalize(title, expandContractions: true, removeNumbers: false);
            var titleToks = TextNormalizer.TokenizeWords(titleNorm);
            if (titleToks.Length == 0) continue;

            int lcp = LongestCommonPrefix(asrNormToks, titleToks);
            // Heuristic boost: if both start with a heading keyword (e.g., "chapter").
            if (HeadingKeywords.Contains(asrNormToks[0]) && titleToks[0] == asrNormToks[0])
            {
                lcp += 1; // small, stable boost
            }

            if (lcp > bestScore)
            {
                bestScore = lcp;
                best = sec;
            }
        }

        // Require a minimal confidence: at least 2 matching leading tokens,
        // or 1 if it is a heading keyword (e.g., "prologue").
        bool isHeadingKeyword = HeadingKeywords.Contains(asrNormToks[0]);
        int minRequired = isHeadingKeyword ? 1 : 2;
        return bestScore >= minRequired ? best : null;
    }

    /// <summary>
    /// Returns the word-index window [start,end] of the detected section, or null.
    /// </summary>
    public static (int startWord, int endWord)? DetectSectionWindow(BookIndex book, IReadOnlyList<string> asrTokens, int prefixTokenCount = 8)
    {
        var sec = DetectSection(book, asrTokens, prefixTokenCount);
        return sec == null ? null : (sec.StartWord, sec.EndWord);
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

