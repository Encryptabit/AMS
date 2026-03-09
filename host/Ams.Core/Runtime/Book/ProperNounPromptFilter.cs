using System.Globalization;
using System.Text;
using Ams.Core.Common;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// Canonical filtering for section proper nouns that are persisted into BookIndex
/// and reused as ASR prompt terms.
/// </summary>
public static class ProperNounPromptFilter
{
    private const int MaxWordsPerEntry = 4;

    private static readonly HashSet<string> NoisySingletons = new(StringComparer.OrdinalIgnoreCase)
    {
        "ah", "eh", "ha", "haha", "hehe", "hm", "hmm", "huh", "mm", "oh", "uh", "um"
    };

    /// <summary>
    /// Filters and sanitizes proper noun candidates into ASR-safe prompt terms.
    /// </summary>
    public static string[] Filter(IEnumerable<string>? candidates)
    {
        if (candidates is null)
        {
            return Array.Empty<string>();
        }

        var filtered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in candidates)
        {
            if (!TryNormalizeCandidate(candidate, out var normalized))
            {
                continue;
            }

            if (!IsAllowed(normalized))
            {
                continue;
            }

            filtered.Add(normalized);
        }

        if (filtered.Count == 0)
        {
            return Array.Empty<string>();
        }

        return [.. filtered.Order(StringComparer.OrdinalIgnoreCase)];
    }

    private static bool TryNormalizeCandidate(string? candidate, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var text = TextNormalizer.NormalizeTypography(candidate);
        text = ReplaceFormatCharactersWithSpaces(text);
        text = text.Trim();
        text = TrimEnclosingBrackets(text);
        text = CollapseWhitespace(text);
        if (text.Length == 0)
        {
            return false;
        }

        normalized = text;
        return true;
    }

    private static bool IsAllowed(string value)
    {
        if (!ContainsAnyLetter(value))
        {
            return false;
        }

        if (value.Any(char.IsDigit))
        {
            return false;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0 || words.Length > MaxWordsPerEntry)
        {
            return false;
        }

        if (words.Any(static w => !ContainsAnyLetter(w)))
        {
            return false;
        }

        // Single-word entries are high-risk for decoder bias, so enforce stronger gates.
        if (words.Length != 1)
        {
            return true;
        }

        var lookup = ExtractLookupForm(words[0]);
        if (lookup.Length <= 1)
        {
            return false;
        }

        if (NoisySingletons.Contains(lookup))
        {
            return false;
        }

        if (lookup.Contains('-'))
        {
            var parts = lookup.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 &&
                   parts.All(static p => p.Length > 1 && EnglishFrequencyDictionary.IsRareOrUnknown(p));
        }

        return EnglishFrequencyDictionary.IsRareOrUnknown(lookup);
    }

    private static string ExtractLookupForm(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return string.Empty;
        }

        var span = token.AsSpan();

        var start = 0;
        while (start < span.Length && char.IsPunctuation(span[start]) && span[start] != '-')
        {
            start++;
        }

        var end = span.Length - 1;
        while (end >= start && char.IsPunctuation(span[end]) && span[end] != '-')
        {
            end--;
        }

        if (start > end)
        {
            return string.Empty;
        }

        var trimmed = span[start..(end + 1)];
        var trimmedStr = trimmed.ToString();

        if (trimmedStr.EndsWith("'s", StringComparison.OrdinalIgnoreCase) ||
            trimmedStr.EndsWith("\u2019s", StringComparison.OrdinalIgnoreCase))
        {
            trimmedStr = trimmedStr[..^2];
        }

        return trimmedStr.ToLowerInvariant();
    }

    private static string TrimEnclosingBrackets(string value)
    {
        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']')
        {
            return value[1..^1].Trim();
        }

        if (value.Length >= 2 && value[0] == '<' && value[^1] == '>')
        {
            return value[1..^1].Trim();
        }

        return value;
    }

    private static string ReplaceFormatCharactersWithSpaces(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var hasFormat = false;
        foreach (var ch in value)
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.Format)
            {
                hasFormat = true;
                break;
            }
        }

        if (!hasFormat)
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.Format)
            {
                builder.Append(' ');
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    private static string CollapseWhitespace(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(ch);
        }

        return builder.ToString().Trim();
    }

    private static bool ContainsAnyLetter(string value)
    {
        foreach (var ch in value)
        {
            if (char.IsLetter(ch))
            {
                return true;
            }
        }

        return false;
    }
}
