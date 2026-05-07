using System.Globalization;
using System.Text.RegularExpressions;
using Humanizer;

namespace Ams.Core.Common;

public static partial class TextNormalizer
{
    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");
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
        { "i'm", "i am" },
        { "you're", "you are" },
        { "he's", "he is" },
        { "she's", "she is" },
        { "it's", "it is" },
        { "we're", "we are" },
        { "they're", "they are" },
        { "i've", "i have" },
        { "you've", "you have" },
        { "we've", "we have" },
        { "they've", "they have" },
        { "i'll", "i will" },
        { "you'll", "you will" },
        { "he'll", "he will" },
        { "she'll", "she will" },
        { "it'll", "it will" },
        { "we'll", "we will" },
        { "they'll", "they will" },
        { "i'd", "i would" },
        { "you'd", "you would" },
        { "he'd", "he would" },
        { "she'd", "she would" },
        { "we'd", "we would" },
        { "they'd", "they would" }
    };

    private static readonly Regex ContractionsRegex = BuildContractionsRegex();
    private static readonly char[] TypographyChars =
    [
        '\u2018', '\u2019', '\u201A', '\u2032', '\u2035',
        '\u201C', '\u201D', '\u201E', '\u2033', '\u00AB', '\u00BB',
        '\u2013', '\u2014', '\u2212'
    ];


    /// <summary>
    /// Normalizes text for scoring and matching.
    /// Numeric conversion applies to integer tokens in range 0..999 when enabled; other numeric forms are preserved unless removed.
    /// </summary>
    public static string Normalize(string text, bool expandContractions = false, bool removeNumbers = false) =>
        Normalize(text, new TextNormalizationOptions(expandContractions, removeNumbers));

    public static string Normalize(string text, TextNormalizationOptions options)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Trim();
        normalized = NormalizeTypography(normalized);
        normalized = normalized.ToLowerInvariant();

        if (options.ExpandContractions)
        {
            normalized = ContractionsRegex.Replace(
                normalized,
                static match => CommonContractions.TryGetValue(match.Value, out var expansion) ? expansion : match.Value);
        }

        // Keep grouped numerals intact (e.g. "4,224" -> "4224") before punctuation stripping.
        normalized = DigitGroupingSeparatorsRegex().Replace(normalized, string.Empty);

        // Preserve apostrophes so contractions like "he'd" remain a single token when not expanded.
        normalized = PunctuationRegex().Replace(normalized, " ");

        // Strip apostrophes used as quotation marks (not mid-word contractions like don't).
        // A leading apostrophe (not preceded by a word char) or trailing apostrophe
        // (not followed by a word char) is a surviving quote mark, not a contraction.
        normalized = OrphanApostropheRegex().Replace(normalized, "");

        if (options.RemoveNumbers)
        {
            normalized = NumbersRegex().Replace(normalized, " ");
        }
        else if (options.ConvertNumbersToWords)
        {
            normalized = NumbersRegex().Replace(normalized, static match =>
            {
                if (int.TryParse(match.Value, out int number) && number >= 0 && number <= 999)
                {
                    return HumanizeNumberWords(number);
                }

                return match.Value;
            });
        }

        normalized = WhitespaceRegex().Replace(normalized, " ").Trim();
        return normalized;
    }

    public static string NormalizeTypography(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.IndexOfAny(TypographyChars) < 0)
        {
            return text;
        }

        var normalized = text.ToCharArray();
        for (int i = 0; i < normalized.Length; i++)
        {
            if (TryMapTypographyChar(normalized[i], out var mapped))
            {
                normalized[i] = mapped;
            }
        }

        return new string(normalized);
    }

    private static string HumanizeNumberWords(int number)
    {
        if (number < 0 || number > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be in range 0..999.");
        }

        return string.Join(
            " ",
            ((long)number).ToWords(EnglishCulture, addAnd: false)
                .Replace('-', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.ToLowerInvariant()));
    }

    public static string[] TokenizeWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var tokens = new List<string>();
        TokenizeWords(text.AsSpan(), tokens);
        return tokens.Count == 0 ? Array.Empty<string>() : [.. tokens];
    }

    public static void TokenizeWords(string text, List<string> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        TokenizeWords(text.AsSpan(), destination);
    }

    public static void TokenizeWords(ReadOnlySpan<char> text, List<string> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        int index = 0;
        while (index < text.Length)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            var start = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            if (index > start)
            {
                destination.Add(new string(text[start..index]));
            }
        }
    }

    public static double CalculateSimilarity(string text1, string text2) =>
        CalculateSimilarity(text1, text2, TextNormalizationOptions.Default);

    public static double CalculateSimilarity(
        string text1,
        string text2,
        TextNormalizationOptions options,
        int? maxDistance = null)
    {
        var normalized1 = Normalize(text1, options);
        var normalized2 = Normalize(text2, options);
        return CalculateSimilarityNormalized(normalized1, normalized2, maxDistance);
    }

    /// <summary>
    /// Calculates similarity for pre-normalized inputs.
    /// </summary>
    public static double CalculateSimilarityNormalized(
        string normalized1,
        string normalized2,
        int? maxDistance = null)
    {
        ArgumentNullException.ThrowIfNull(normalized1);
        ArgumentNullException.ThrowIfNull(normalized2);

        if (normalized1 == normalized2)
        {
            return 1.0;
        }

        if (normalized1.Length == 0 || normalized2.Length == 0)
        {
            return 0.0;
        }

        var maxLength = Math.Max(normalized1.Length, normalized2.Length);
        if (maxDistance.HasValue)
        {
            var threshold = maxDistance.Value;
            var distance = LevenshteinMetrics.Distance(normalized1.AsSpan(), normalized2.AsSpan(), threshold);
            if (distance > threshold)
            {
                return 0.0;
            }

            return 1.0 - (double)distance / maxLength;
        }

        return LevenshteinMetrics.Similarity(normalized1, normalized2);
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^\w\s']", RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"(?<!\w)'|'(?!\w)", RegexOptions.CultureInvariant)]
    private static partial Regex OrphanApostropheRegex();

    [GeneratedRegex(@"\b\d+\b", RegexOptions.CultureInvariant)]
    private static partial Regex NumbersRegex();

    [GeneratedRegex(@"(?<=\d),(?=\d)", RegexOptions.CultureInvariant)]
    private static partial Regex DigitGroupingSeparatorsRegex();

    private static Regex BuildContractionsRegex()
    {
        var escaped = CommonContractions.Keys
            .OrderByDescending(static key => key.Length)
            .Select(Regex.Escape);

        var pattern = $@"\b(?:{string.Join("|", escaped)})\b";
        return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool TryMapTypographyChar(char value, out char mapped)
    {
        mapped = value switch
        {
            '\u2018' or '\u2019' or '\u201A' or '\u2032' or '\u2035' => '\'',
            '\u201C' or '\u201D' or '\u201E' or '\u2033' or '\u00AB' or '\u00BB' => '"',
            '\u2013' or '\u2014' or '\u2212' => '-',
            _ => value
        };

        return mapped != value;
    }
}

public readonly record struct TextNormalizationOptions(
    bool ExpandContractions = true,
    bool RemoveNumbers = false,
    bool ConvertNumbersToWords = false)
{
    public static TextNormalizationOptions Default => new();
}
