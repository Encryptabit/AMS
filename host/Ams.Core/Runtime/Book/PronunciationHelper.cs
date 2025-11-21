using System.Globalization;
using System.Text;

namespace Ams.Core.Runtime.Book;

public static class PronunciationHelper
{
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
                while (i < token.Length && (char.IsDigit(token[i]) || token[i] == ',' || token[i] == '_' ))
                {
                    i++;
                }

                var digits = token.Substring(start, i - start);
                i--;

                var words = ConvertNumberToWordsSafe(digits);
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
        static string SpellOutDigits(string raw)
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

        var canonical = digits.Replace("_", string.Empty).Replace(",", string.Empty);
        if (!long.TryParse(canonical, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return SpellOutDigits(digits);
        }

        return NumberToWords(value);
    }

    private static string NumberToWords(long number)
    {
        if (number == 0)
        {
            return "zero";
        }

        if (number < 0)
        {
            return "minus " + NumberToWords(Math.Abs(number));
        }

        var parts = new List<string>();
        var units = new[] { "", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion" };
        int unitIndex = 0;

        while (number > 0 && unitIndex < units.Length)
        {
            int chunk = (int)(number % 1000);
            if (chunk != 0)
            {
                var chunkWords = ChunkToWords(chunk);
                if (!string.IsNullOrEmpty(units[unitIndex]))
                {
                    chunkWords += " " + units[unitIndex];
                }
                parts.Insert(0, chunkWords);
            }

            number /= 1000;
            unitIndex++;
        }

        if (number > 0)
        {
            parts.Add(SpellOutDigits(number.ToString(CultureInfo.InvariantCulture)));
        }

        return string.Join(" ", parts);
    }

    private static string ChunkToWords(int number)
    {
        var ones = new[] { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        var teens = new[] { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        var tens = new[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        var words = new List<string>();

        if (number >= 100)
        {
            words.Add(ones[number / 100]);
            words.Add("hundred");
            number %= 100;
        }

        if (number >= 20)
        {
            words.Add(tens[number / 10]);
            number %= 10;
        }
        else if (number >= 10)
        {
            words.Add(teens[number - 10]);
            number = 0;
        }

        if (number > 0)
        {
            words.Add(ones[number]);
        }

        return string.Join(" ", words.Where(w => w.Length > 0));
    }
}
