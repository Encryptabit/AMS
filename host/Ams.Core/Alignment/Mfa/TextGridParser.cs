using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ams.Core.Alignment.Mfa;

internal sealed record TextGridInterval(double Start, double End, string Text);

internal static class TextGridParser
{
    public static IReadOnlyList<TextGridInterval> ParseWordIntervals(string textGridPath)
    {
        if (!File.Exists(textGridPath))
        {
            throw new FileNotFoundException("TextGrid file not found", textGridPath);
        }

        var intervals = new List<TextGridInterval>();
        string? currentTier = null;
        double? xmin = null;
        double? xmax = null;
        string? text = null;

        foreach (var rawLine in File.ReadLines(textGridPath))
        {
            var line = rawLine.Trim();

            if (line.StartsWith("item [", StringComparison.Ordinal))
            {
                currentTier = null;
                continue;
            }

            if (line.StartsWith("name =", StringComparison.Ordinal))
            {
                var name = ExtractQuotedValue(line);
                currentTier = string.Equals(name, "words", StringComparison.OrdinalIgnoreCase) ? "words" : null;
                continue;
            }

            if (!string.Equals(currentTier, "words", StringComparison.Ordinal))
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

            if (line.StartsWith("text =", StringComparison.Ordinal))
            {
                text = ExtractQuotedValue(line) ?? string.Empty;

                if (xmin.HasValue && xmax.HasValue)
                {
                    intervals.Add(new TextGridInterval(xmin.Value, xmax.Value, text));
                }

                xmin = null;
                xmax = null;
                text = null;
            }
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

