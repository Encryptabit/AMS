using System.Text;
using System.Text.RegularExpressions;

namespace Ams.Core.Asr;

public static class AsrTranscriptBuilder
{
    private static readonly Regex SentenceBoundaryRegex = new(
        @"(?<=[.!?])\s+(?=(?:[""'\)\]\u2019\u201D]+)?[A-Z0-9])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string BuildCorpusText(AsrResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var sentences = BuildSentences(response);
        if (sentences.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, sentences);
    }

    public static IReadOnlyList<string> BuildSentences(AsrResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var aggregate = BuildAggregateText(response);
        if (string.IsNullOrWhiteSpace(aggregate))
        {
            return Array.Empty<string>();
        }

        var normalized = Regex.Replace(aggregate, "\\s+", " ", RegexOptions.CultureInvariant).Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return Array.Empty<string>();
        }

        var pieces = SentenceBoundaryRegex.Split(normalized);
        var sentences = new List<string>(pieces.Length);
        foreach (var piece in pieces)
        {
            var trimmed = piece.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                sentences.Add(trimmed);
            }
        }

        if (sentences.Count == 0)
        {
            sentences.Add(normalized);
        }

        return sentences;
    }

    private static string BuildAggregateText(AsrResponse response)
    {
        if (response.Segments.Length == 0 && response.WordCount == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        if (response.Segments.Length > 0)
        {
            foreach (var segment in response.Segments)
            {
                if (string.IsNullOrWhiteSpace(segment.Text))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(segment.Text.Trim());
            }

            return builder.ToString();
        }

        for (var i = 0; i < response.WordCount; i++)
        {
            var word = response.GetWord(i);
            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(word);
        }

        return builder.ToString();
    }
}