using Ams.Core.Runtime.Book;
using System.Text;

namespace Ams.Core.Processors.Alignment.Anchors;

/// <summary>
/// Detects the most likely book section from the first few ASR tokens
/// by matching against normalized section titles (e.g., "chapter fourteen", "prologue").
/// </summary>
public static class SectionLocator
{
    private static readonly HashSet<string> HeadingKeywords = new(StringComparer.Ordinal)
    {
        "chapter", "prologue", "epilogue", "preface", "introduction", "foreword", "prelude", "contents"
    };

    private static readonly HashSet<string> LeadingChapterKeywords = new(StringComparer.Ordinal)
    {
        "chapter", "ch"
    };

    private static readonly Dictionary<string, int> SpelledUnits = new(StringComparer.Ordinal)
    {
        { "zero", 0 },
        { "one", 1 },
        { "two", 2 },
        { "three", 3 },
        { "four", 4 },
        { "five", 5 },
        { "six", 6 },
        { "seven", 7 },
        { "eight", 8 },
        { "nine", 9 },
        { "ten", 10 },
        { "eleven", 11 },
        { "twelve", 12 },
        { "thirteen", 13 },
        { "fourteen", 14 },
        { "fifteen", 15 },
        { "sixteen", 16 },
        { "seventeen", 17 },
        { "eighteen", 18 },
        { "nineteen", 19 }
    };

    private static readonly Dictionary<string, int> SpelledOrdinals = new(StringComparer.Ordinal)
    {
        { "first", 1 },
        { "second", 2 },
        { "third", 3 },
        { "fourth", 4 },
        { "fifth", 5 },
        { "sixth", 6 },
        { "seventh", 7 },
        { "eighth", 8 },
        { "ninth", 9 },
        { "tenth", 10 },
        { "eleventh", 11 },
        { "twelfth", 12 },
        { "thirteenth", 13 },
        { "fourteenth", 14 },
        { "fifteenth", 15 },
        { "sixteenth", 16 },
        { "seventeenth", 17 },
        { "eighteenth", 18 },
        { "nineteenth", 19 }
    };

    private static readonly Dictionary<string, int> SpelledTens = new(StringComparer.Ordinal)
    {
        { "twenty", 20 },
        { "thirty", 30 },
        { "forty", 40 },
        { "fifty", 50 },
        { "sixty", 60 },
        { "seventy", 70 },
        { "eighty", 80 },
        { "ninety", 90 }
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
    public static (int startWord, int endWord)? DetectSectionWindow(BookIndex book, IReadOnlyList<string> asrTokens,
        int prefixTokenCount = 8)
    {
        var sec = DetectSection(book, asrTokens, prefixTokenCount);
        return sec == null ? null : (sec.StartWord, sec.EndWord);
    }

    /// <summary>
    /// Resolve a section directly from an audio filename stem or arbitrary chapter label.
    /// Applies aggressive normalization so titles like
    /// "11- Aboard the Bounty" and "Chapter Eleven – Aboard the Bounty" map to the same section.
    /// </summary>
    public static SectionRange? ResolveSectionByTitle(BookIndex book, string? chapterLabel)
    {
        if (book.Sections is null || book.Sections.Length == 0) return null;
        if (string.IsNullOrWhiteSpace(chapterLabel)) return null;

        var audioTokensRaw = NormalizeTokens(chapterLabel);
        if (audioTokensRaw.Count == 0) return null;

        var audioTokens = CollapseNumberTokens(audioTokensRaw);
        var lookup = BuildSectionLookup(book.Sections);

        var audioNumber = ExtractLeadingNumber(audioTokens);
        if (audioNumber.HasValue && lookup.ByNumber.TryGetValue(audioNumber.Value, out var numberMatches))
        {
            if (numberMatches.Count == 1)
            {
                return numberMatches[0].Section;
            }
        }

        var audioVariants = BuildNormalizedVariants(audioTokens);
        foreach (var variant in audioVariants)
        {
            if (!lookup.ByNormalized.TryGetValue(variant, out var matches) || matches.Count == 0)
            {
                continue;
            }

            if (matches.Count == 1)
            {
                return matches[0].Section;
            }

            var exact = matches.FirstOrDefault(m =>
                string.Equals(m.NormalizedOriginal, variant, StringComparison.Ordinal));
            if (exact != null)
            {
                return exact.Section;
            }
        }

        return null;
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

    private static List<string> NormalizeTokens(string text)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return tokens;

        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }
        }

        if (sb.Length > 0)
        {
            tokens.Add(sb.ToString());
        }

        return tokens;
    }

    private static List<string> CollapseNumberTokens(IReadOnlyList<string> tokens)
    {
        var result = new List<string>(tokens.Count);
        int i = 0;
        while (i < tokens.Count)
        {
            if (TryParseCombinedNumber(tokens, i, out var value, out var consumed))
            {
                result.Add(value.ToString());
                i += consumed;
            }
            else
            {
                result.Add(tokens[i]);
                i++;
            }
        }

        return result;
    }

    private static bool TryParseCombinedNumber(IReadOnlyList<string> tokens, int index, out int value, out int consumed)
    {
        consumed = 0;
        value = 0;

        if (index >= tokens.Count) return false;
        var token = tokens[index];

        if (TryParseIntToken(token, out value))
        {
            consumed = 1;
            return true;
        }

        if (SpelledOrdinals.TryGetValue(token, out value))
        {
            consumed = 1;
            return true;
        }

        if (TryParseRoman(token, out value))
        {
            consumed = 1;
            return true;
        }

        if (SpelledUnits.TryGetValue(token, out value))
        {
            consumed = 1;
            return true;
        }

        if (SpelledTens.TryGetValue(token, out var tens))
        {
            consumed = 1;
            value = tens;
            if (index + 1 < tokens.Count && SpelledUnits.TryGetValue(tokens[index + 1], out var unit) && unit < 10)
            {
                value += unit;
                consumed = 2;
            }

            return true;
        }

        return false;
    }

    private static bool TryParseIntToken(string token, out int value)
    {
        if (int.TryParse(token, out value)) return true;

        if (token.Length > 2)
        {
            var suffix = token[^2..];
            if (suffix is "st" or "nd" or "rd" or "th")
            {
                var span = token.AsSpan(0, token.Length - 2);
                if (int.TryParse(span, out value)) return true;
            }
        }

        value = 0;
        return false;
    }

    private static bool TryParseRoman(string token, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(token)) return false;

        int result = 0;
        int prev = 0;
        foreach (var ch in token.ToUpperInvariant())
        {
            if (!RomanMap.TryGetValue(ch, out var current))
            {
                value = 0;
                return false;
            }

            if (current > prev)
            {
                result += current - (2 * prev);
            }
            else
            {
                result += current;
            }

            prev = current;
        }

        value = result;
        return result > 0;
    }

    private static readonly Dictionary<char, int> RomanMap = new()
    {
        { 'I', 1 },
        { 'V', 5 },
        { 'X', 10 },
        { 'L', 50 },
        { 'C', 100 },
        { 'D', 500 },
        { 'M', 1000 }
    };

    private static int? ExtractLeadingNumber(IReadOnlyList<string> tokens)
    {
        if (tokens == null || tokens.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (LeadingChapterKeywords.Contains(token))
            {
                if (i + 1 < tokens.Count && TryParseFullNumber(tokens, i + 1, out var keywordNumber, out _))
                {
                    return keywordNumber;
                }

                continue;
            }

            if (TryParseEmbeddedChapterNumber(token, out var embeddedValue))
            {
                return embeddedValue;
            }
        }

        if (TryParseFullNumber(tokens, 0, out var fallbackValue, out _))
        {
            return fallbackValue;
        }

        return null;
    }

    private static bool TryParseEmbeddedChapterNumber(string token, out int value)
    {
        foreach (var keyword in LeadingChapterKeywords)
        {
            if (token.StartsWith(keyword, StringComparison.Ordinal) && token.Length > keyword.Length)
            {
                var suffix = token[keyword.Length..];
                if (TryParseIntToken(suffix, out value))
                {
                    return true;
                }
            }
        }

        value = 0;
        return false;
    }

    private static bool TryParseFullNumber(IReadOnlyList<string> tokens, int index, out int value, out int consumed)
    {
        consumed = 1;
        value = 0;
        if (index >= tokens.Count)
        {
            return false;
        }

        var token = tokens[index];
        if (TryParseIntToken(token, out value)) return true;
        if (TryParseRoman(token, out value)) return true;
        if (SpelledOrdinals.TryGetValue(token, out value)) return true;
        if (SpelledUnits.TryGetValue(token, out value)) return true;

        if (SpelledTens.TryGetValue(token, out var tensVal))
        {
            value = tensVal;
            if (index + 1 < tokens.Count && SpelledUnits.TryGetValue(tokens[index + 1], out var unit) && unit < 10)
            {
                value += unit;
                consumed = 2;
            }

            return true;
        }

        return false;
    }

    private static SectionLookup BuildSectionLookup(IReadOnlyList<SectionRange> sections)
    {
        var byNumber = new Dictionary<int, List<SectionCandidate>>();
        var byNormalized = new Dictionary<string, List<SectionCandidate>>(StringComparer.Ordinal);

        foreach (var section in sections)
        {
            var rawTokens = NormalizeTokens(section.Title ?? string.Empty);
            var collapsed = CollapseNumberTokens(rawTokens);
            var variants = BuildNormalizedVariants(collapsed);

            var normalizedOriginal = string.Join(" ", collapsed);
            var candidate = new SectionCandidate(section, normalizedOriginal);

            var number = ExtractLeadingNumber(collapsed);
            if (number.HasValue)
            {
                if (!byNumber.TryGetValue(number.Value, out var list))
                {
                    list = new List<SectionCandidate>();
                    byNumber[number.Value] = list;
                }

                list.Add(candidate);
            }

            foreach (var variant in variants)
            {
                if (!byNormalized.TryGetValue(variant, out var list))
                {
                    list = new List<SectionCandidate>();
                    byNormalized[variant] = list;
                }

                list.Add(candidate);
            }
        }

        return new SectionLookup(byNumber, byNormalized);
    }

    private static HashSet<string> BuildNormalizedVariants(IReadOnlyList<string> tokens)
    {
        var variants = new HashSet<string>(StringComparer.Ordinal);

        void AddVariant(IReadOnlyList<string> source)
        {
            if (source.Count == 0) return;
            var str = string.Join(" ", source);
            if (!string.IsNullOrEmpty(str)) variants.Add(str);
        }

        AddVariant(tokens);

        var trimmed = TrimLeadingKeywords(tokens);
        AddVariant(trimmed);

        if (trimmed.Count > 0 && TryParseIntToken(trimmed[0], out _))
        {
            AddVariant(trimmed.Skip(1).ToList());
        }

        return variants;
    }

    private static List<string> TrimLeadingKeywords(IReadOnlyList<string> tokens)
    {
        int idx = 0;
        while (idx < tokens.Count && LeadingChapterKeywords.Contains(tokens[idx]))
        {
            idx++;
        }

        return tokens.Skip(idx).ToList();
    }

    private sealed record SectionCandidate(SectionRange Section, string NormalizedOriginal);

    private sealed record SectionLookup(
        Dictionary<int, List<SectionCandidate>> ByNumber,
        Dictionary<string, List<SectionCandidate>> ByNormalized);
}