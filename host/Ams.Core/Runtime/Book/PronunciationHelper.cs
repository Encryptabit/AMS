using System.Globalization;
using System.Text;
using Humanizer;

namespace Ams.Core.Runtime.Book;

public static class PronunciationHelper
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

    public static string? NormalizeForLookup(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var parts = ExtractPronunciationParts(token);
        if (parts.Count == 0)
        {
            return null;
        }

        return string.Join(" ", parts);
    }

    public static IReadOnlyList<string> ExtractPronunciationParts(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Array.Empty<string>();
        }

        token = TextNormalizer.NormalizeTypography(token);
        var segments = new List<string>();
        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length > 0)
            {
                segments.Add(current.ToString());
                current.Clear();
            }
        }

        for (int i = 0; i < token.Length; i++)
        {
            var ch = token[i];
            if (char.IsLetter(ch))
            {
                current.Append(char.ToLowerInvariant(ch));
            }
            else if (char.IsDigit(ch))
            {
                Flush();
                int start = i;
                while (i < token.Length && (char.IsDigit(token[i]) || token[i] == ',' || token[i] == '_'))
                {
                    i++;
                }

                var digits = token.Substring(start, i - start);
                var suffixLength = GetOrdinalSuffixLength(token, i, digits);
                string words;
                if (suffixLength > 0)
                {
                    words = ConvertOrdinalNumberToWordsSafe(digits);
                    i += suffixLength;
                }
                else
                {
                    words = ConvertNumberToWordsSafe(digits);
                }

                i--;

                if (!string.IsNullOrEmpty(words))
                {
                    segments.AddRange(words.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    segments.Add(digits);
                }
            }
            else if (ch == '\'' || ch == '’')
            {
                current.Append('\'');
            }
            else if (ch == '-')
            {
                Flush();
            }
            else
            {
                Flush();
            }
        }

        Flush();

        return segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }

    public static IReadOnlyList<string> SplitLexemeIntoWords(string lexeme)
    {
        if (string.IsNullOrWhiteSpace(lexeme))
        {
            return Array.Empty<string>();
        }

        return lexeme
            .Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            .Where(word => word.Length > 0)
            .ToArray();
    }

    private static readonly char[] SplitSeparators = { ' ', '\t', '-' };

    private static string SpellOutDigits(string raw)
    {
        return string.Join(" ", raw.Select(ch => ch switch
        {
            '0' => "zero",
            '1' => "one",
            '2' => "two",
            '3' => "three",
            '4' => "four",
            '5' => "five",
            '6' => "six",
            '7' => "seven",
            '8' => "eight",
            '9' => "nine",
            _ => string.Empty
        }).Where(s => s.Length > 0));
    }

    private static string ConvertNumberToWordsSafe(string digits)
    {
        var canonical = digits.Replace("_", string.Empty).Replace(",", string.Empty);
        if (!long.TryParse(canonical, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return SpellOutDigits(digits);
        }

        return NumberToWords(value);
    }

    private static string ConvertOrdinalNumberToWordsSafe(string digits)
    {
        var canonical = digits.Replace("_", string.Empty).Replace(",", string.Empty);
        if (!int.TryParse(canonical, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return SpellOutDigits(digits);
        }

        return NormalizeSpelledNumberWords(value.ToOrdinalWords(EnglishCulture));
    }

    private static int GetOrdinalSuffixLength(string token, int start, string digits)
    {
        const int SuffixLength = 2;
        if (start + SuffixLength > token.Length)
        {
            return 0;
        }

        var suffix = token.Substring(start, SuffixLength);
        if (!IsOrdinalSuffixForDigits(digits, suffix))
        {
            return 0;
        }

        var afterSuffix = start + SuffixLength;
        return afterSuffix >= token.Length || !char.IsLetterOrDigit(token[afterSuffix])
            ? SuffixLength
            : 0;
    }

    private static bool IsOrdinalSuffixForDigits(string digits, string suffix)
    {
        var canonical = digits.Replace("_", string.Empty).Replace(",", string.Empty);
        if (!long.TryParse(canonical, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        var expected = "th";
        var lastTwo = Math.Abs(value % 100);
        if (lastTwo is < 11 or > 13)
        {
            expected = Math.Abs(value % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        return string.Equals(suffix, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string NumberToWords(long number)
        => NormalizeSpelledNumberWords(number.ToWords(EnglishCulture, addAnd: false));

    private static string NormalizeSpelledNumberWords(string value)
        => string.Join(
            " ",
            value.Replace('-', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.ToLowerInvariant()));
}
