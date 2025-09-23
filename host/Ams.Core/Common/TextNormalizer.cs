using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Ams.Core.Common;

public static class TextNormalizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex NumbersRegex = new(@"\b\d+\b", RegexOptions.Compiled);
    private static readonly Regex DigitLetterBoundary = new(@"(?<=\d)(?=[A-Za-z])", RegexOptions.Compiled);
    private static readonly Regex LetterDigitBoundary = new(@"(?<=[A-Za-z])(?=\d)", RegexOptions.Compiled);
    
    private static readonly Dictionary<string, string> CommonContractions = new(StringComparer.OrdinalIgnoreCase)
    {
        { "don't", "do not" },
        { "won't", "will not" },
        { "can't", "cannot" },
        { "isn't", "is not" },
        { "aren't", "are not" },
        { "wasn't", "was not" },
        { "weren't", "were not" },
        { "haven't", "have not" },
        { "hasn't", "has not" },
        { "hadn't", "had not" },
        { "doesn't", "does not" },
        { "didn't", "did not" },
        { "shouldn't", "should not" },
        { "wouldn't", "would not" },
        { "couldn't", "could not" },
        { "mustn't", "must not" },
        { "needn't", "need not" },
        { "I'm", "I am" },
        { "you're", "you are" },
        { "he's", "he is" },
        { "she's", "she is" },
        { "it's", "it is" },
        { "we're", "we are" },
        { "they're", "they are" },
        { "I've", "I have" },
        { "you've", "you have" },
        { "we've", "we have" },
        { "they've", "they have" },
        { "I'll", "I will" },
        { "you'll", "you will" },
        { "he'll", "he will" },
        { "she'll", "she will" },
        { "it'll", "it will" },
        { "we'll", "we will" },
        { "they'll", "they will" },
        { "I'd", "I would" },
        { "you'd", "you would" },
        { "he'd", "he would" },
        { "she'd", "she would" },
        { "we'd", "we would" },
        { "they'd", "they would" }
    };

    public static string Normalize(string text, bool expandContractions = true, bool removeNumbers = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Trim();

        // Insert explicit boundaries between digits and letters so numbers like "28A" can be processed.
        normalized = DigitLetterBoundary.Replace(normalized, " ");
        normalized = LetterDigitBoundary.Replace(normalized, " ");

        // Convert to lowercase
        normalized = normalized.ToLowerInvariant();

        // Expand contractions
        if (expandContractions)
        {
            foreach (var (contraction, expansion) in CommonContractions)
            {
                normalized = Regex.Replace(normalized, @"\b" + Regex.Escape(contraction) + @"\b", 
                    expansion, RegexOptions.IgnoreCase);
            }
        }

        // Remove or normalize punctuation
        normalized = PunctuationRegex.Replace(normalized, " ");

        // Handle numbers
        if (removeNumbers)
        {
            normalized = NumbersRegex.Replace(normalized, " ");
        }
        else
        {
            // Convert numbers to words (basic implementation)
            normalized = NumbersRegex.Replace(normalized, match =>
            {
                if (int.TryParse(match.Value, out int number) && number >= 0 && number <= 999)
                {
                    return NumberToWords(number);
                }
                return match.Value;
            });
        }

        // Normalize whitespace
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();

        return normalized;
    }

    private static string NumberToWords(int number)
    {
        if (number == 0) return "zero";
        
        var ones = new[] { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        var teens = new[] { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        var tens = new[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        var hundreds = new[] { "", "one hundred", "two hundred", "three hundred", "four hundred", "five hundred", 
                              "six hundred", "seven hundred", "eight hundred", "nine hundred" };

        var result = new StringBuilder();

        if (number >= 100)
        {
            result.Append(hundreds[number / 100]);
            number %= 100;
            if (number > 0) result.Append(" ");
        }

        if (number >= 20)
        {
            result.Append(tens[number / 10]);
            number %= 10;
            if (number > 0) result.Append(" ");
        }
        else if (number >= 10)
        {
            result.Append(teens[number - 10]);
            number = 0;
        }

        if (number > 0)
        {
            result.Append(ones[number]);
        }

        return result.ToString().Trim();
    }

    public static string[] TokenizeWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public static double CalculateSimilarity(string text1, string text2)
    {
        var normalized1 = Normalize(text1);
        var normalized2 = Normalize(text2);
        
        if (normalized1 == normalized2) return 1.0;
        if (string.IsNullOrEmpty(normalized1) || string.IsNullOrEmpty(normalized2)) return 0.0;
        
        // Use Levenshtein distance for similarity
        var distance = LevenshteinDistance(normalized1, normalized2);
        var maxLength = Math.Max(normalized1.Length, normalized2.Length);
        
        return 1.0 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        if (s1.Length == 0) return s2.Length;
        if (s2.Length == 0) return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}

