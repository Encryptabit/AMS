using System.Globalization;

namespace Ams.Core.Processors.Alignment.Mfa;

public sealed record TextGridInterval(double Start, double End, string Text);

public static class TextGridParser
{
    public static IReadOnlyList<TextGridInterval> ParseWordIntervals(string textGridPath)
        => ParseIntervals(textGridPath, static name =>
            string.Equals(name, "words", StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<TextGridInterval> ParsePhoneIntervals(string textGridPath)
        => ParseIntervals(textGridPath, static name =>
            string.Equals(name, "phones", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "phonemes", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "segments", StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<TextGridInterval> ParseIntervals(
        string textGridPath,
        Func<string?, bool> tierPredicate)
    {
        if (!File.Exists(textGridPath))
        {
            throw new FileNotFoundException("TextGrid file not found", textGridPath);
        }

        var intervals = new List<TextGridInterval>();
        bool capture = false;
        double? xmin = null;
        double? xmax = null;
        string? text = null;

        foreach (var rawLine in File.ReadLines(textGridPath))
        {
            var line = rawLine.Trim();

            if (line.StartsWith("item [", StringComparison.Ordinal))
            {
                capture = false;
                xmin = null;
                xmax = null;
                text = null;
                continue;
            }

            if (line.StartsWith("name =", StringComparison.Ordinal))
            {
                var name = ExtractQuotedValue(line);
                capture = tierPredicate(name);
                xmin = null;
                xmax = null;
                text = null;
                continue;
            }

            if (!capture)
            {
                continue;
            }

            if (line.StartsWith("xmin =", StringComparison.Ordinal))
            {
                xmin = ParseDouble(line);
                continue;
            }

            if (line.StartsWith("xmax =", StringComparison.Ordinal))
            {
                xmax = ParseDouble(line);
                continue;
            }

            if (!line.StartsWith("text =", StringComparison.Ordinal))
            {
                continue;
            }

            text = ExtractQuotedValue(line) ?? string.Empty;

            if (xmin.HasValue && xmax.HasValue)
            {
                intervals.Add(new TextGridInterval(xmin.Value, xmax.Value, text));
            }

            xmin = null;
            xmax = null;
            text = null;
        }

        return intervals;
    }

    private static string? ExtractQuotedValue(string line)
    {
        var firstQuote = line.IndexOf('"');
        if (firstQuote < 0)
        {
            return null;
        }

        var secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0)
        {
            return null;
        }

        return line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }

    private static double ParseDouble(string line)
    {
        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return 0;
        }

        return double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }
}