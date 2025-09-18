using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Ams.Core;

/// <summary>
/// Canonical indexer: emits data that matches CORRECT_RESULTS expectations.
/// </summary>
public class BookIndexer : IBookIndexer
{
    private static readonly Regex _blankLineSplit = new("(\\r?\\n){2,}", RegexOptions.Compiled);

    public async Task<BookIndex> CreateIndexAsync(
        BookParseResult parseResult,
        string sourceFile,
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (parseResult == null)
            throw new ArgumentNullException(nameof(parseResult));
        if (string.IsNullOrWhiteSpace(sourceFile))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));

        options ??= new BookIndexOptions();

        try
        {
            return await Task.Run(() => Process(parseResult, sourceFile, options, cancellationToken), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ArgumentException)
        {
            throw new BookIndexException($"Failed to create book index for '{sourceFile}': {ex.Message}", ex);
        }
    }
    

    private BookIndex Process(
        BookParseResult parseResult,
        string sourceFile,
        BookIndexOptions options,
        CancellationToken cancellationToken)
    {
        var sourceFileHash = ComputeFileHash(sourceFile);

        var paragraphTexts = (parseResult.Paragraphs != null && parseResult.Paragraphs.Count > 0)
            ? parseResult.Paragraphs.Select(p => (Text: p.Text, Style: p.Style ?? "Unknown", Kind: p.Kind ?? "Body")).ToList()
            : _blankLineSplit.Split(parseResult.Text)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(t => (Text: t.TrimEnd('\r', '\n'), Style: "Unknown", Kind: "Body"))
                .ToList();

        var words = new List<BookWord>();
        var sentenceRanges = new List<(int Index, int Start, int End)>();
        var paragraphRanges = new List<(int Index, int Start, int End, string Kind, string Style)>();
        var sections = new List<SectionRange>();
        var warnings = new List<string>();

        int globalWord = 0;
        int sentenceIndex = 0;
        int sectionId = 0;
        SectionOpen? currentSection = null;

        for (int pIndex = 0; pIndex < paragraphTexts.Count; pIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (pText, style, kind) = paragraphTexts[pIndex];
            if (string.IsNullOrWhiteSpace(pText))
            {
                paragraphRanges.Add((pIndex, globalWord, globalWord - 1, kind, style));
                continue;
            }

            int paragraphStartWord = globalWord;
            int sentenceStartWord = globalWord;

            int headingLevel = GetHeadingLevel(style);
            if (string.Equals(kind, "Heading", StringComparison.OrdinalIgnoreCase) && headingLevel >= 1 && LooksLikeSectionHeading(pText))
            {
                CloseOpenSection(sections, currentSection, globalWord, pIndex - 1);
                currentSection = new SectionOpen(sectionId++, pText.Trim(), ClassifySectionKind(pText), globalWord, pIndex);
            }

            foreach (var token in TokenizeByWhitespace(pText))
            {
                int sectionIndex = currentSection?.Id ?? -1;

                words.Add(new BookWord(
                    Text: token,
                    WordIndex: globalWord,
                    SentenceIndex: sentenceIndex,
                    ParagraphIndex: pIndex,
                    SectionIndex: sectionIndex
                ));
                globalWord++;

                if (IsSentenceTerminal(token))
                {
                    sentenceRanges.Add((sentenceIndex, sentenceStartWord, globalWord - 1));
                    sentenceIndex++;
                    sentenceStartWord = globalWord;
                }
            }

            if (globalWord > sentenceStartWord)
            {
                sentenceRanges.Add((sentenceIndex, sentenceStartWord, globalWord - 1));
                sentenceIndex++;
            }

            paragraphRanges.Add((pIndex, paragraphStartWord, globalWord - 1, kind, style));
        }

        CloseOpenSection(sections, currentSection, globalWord, paragraphTexts.Count - 1);

        int totalWords = words.Count;
        int totalSentences = sentenceRanges.Count;
        int totalParagraphs = paragraphRanges.Count;
        double estimatedDuration = totalWords / options.AverageWpm * 60.0;

        var sentences = sentenceRanges
            .Where(r => r.Start >= 0 && r.End >= r.Start)
            .Select(r => new BookSentence(r.Index, r.Start, r.End))
            .ToArray();

        var paragraphs = paragraphRanges
            .Where(r => r.Start >= 0 && r.End >= r.Start)
            .Select(r => new BookParagraph(r.Index, r.Start, r.End, r.Kind, r.Style))
            .ToArray();

        var totals = new BookTotals(
            Words: totalWords,
            Sentences: totalSentences,
            Paragraphs: totalParagraphs,
            EstimatedDurationSec: estimatedDuration
        );

        return new BookIndex(
            SourceFile: sourceFile,
            SourceFileHash: sourceFileHash,
            IndexedAt: DateTime.UtcNow,
            Title: parseResult.Title,
            Author: parseResult.Author,
            Totals: totals,
            Words: words.ToArray(),
            Sentences: sentences,
            Paragraphs: paragraphs,
            Sections: sections.Count == 0 ? Array.Empty<SectionRange>() : sections.ToArray(),
            BuildWarnings: warnings.Count == 0 ? Array.Empty<string>() : warnings.ToArray()
        );
    }

    private static void CloseOpenSection(List<SectionRange> sections, SectionOpen? current, int currentWord, int lastParagraphIndex)
    {
        if (current is null)
        {
            return;
        }

        int endWord = Math.Max(current.StartWord, currentWord) - 1;
        if (endWord < current.StartWord)
        {
            return;
        }

        int endParagraph = Math.Max(current.StartParagraph, lastParagraphIndex);
        sections.Add(new SectionRange(
            Id: current.Id,
            Title: current.Title,
            Level: 1,
            Kind: current.Kind,
            StartWord: current.StartWord,
            EndWord: endWord,
            StartParagraph: current.StartParagraph,
            EndParagraph: endParagraph
        ));
    }

    private static int GetHeadingLevel(string style)
    {
        if (string.IsNullOrEmpty(style)) return 0;
        var s = style.Trim();
        var idx = s.IndexOf("Heading", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 0;
        for (int i = idx + 7; i < s.Length; i++)
        {
            if (char.IsDigit(s[i]))
            {
                int j = i;
                while (j < s.Length && char.IsDigit(s[j])) j++;
                if (int.TryParse(s[i..j], out int level)) return level;
                break;
            }
        }
        return 1;
    }

    private static string ClassifySectionKind(string headingText)
    {
        if (string.IsNullOrWhiteSpace(headingText)) return "chapter";
        var t = headingText.Trim().ToLowerInvariant();
        if (t.Contains("prologue")) return "prologue";
        if (t.Contains("epilogue")) return "epilogue";
        if (t.Contains("prelude")) return "prelude";
        if (t.Contains("foreword")) return "foreword";
        if (t.Contains("introduction")) return "introduction";
        if (t.Contains("afterword")) return "afterword";
        if (t.Contains("acknowledg")) return "acknowledgments";
        if (t.Contains("appendix")) return "appendix";
        if (t.Contains("chapter")) return "chapter";
        return "chapter";
    }

    private static readonly Regex SectionTitleRegex = new(
        @"^\s*(chapter\b|prologue\b|epilogue\b|prelude\b|foreword\b|introduction\b|afterword\b|appendix\b|part\b|book\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static bool LooksLikeSectionHeading(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        return SectionTitleRegex.IsMatch(t);
    }

    private record SectionOpen(int Id, string Title, string Kind, int StartWord, int StartParagraph);

    private static IEnumerable<string> TokenizeByWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;

        int i = 0;
        int n = text.Length;
        while (i < n)
        {
            while (i < n && char.IsWhiteSpace(text[i])) i++;
            if (i >= n) yield break;

            int start = i;
            while (i < n && !char.IsWhiteSpace(text[i])) i++;
            yield return text[start..i];
        }
    }

    private static bool IsSentenceTerminal(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        int i = token.Length - 1;
        const string trailing = ")]}'\"���";
        while (i >= 0 && trailing.IndexOf(token[i]) >= 0) i--;
        if (i < 0) return false;
        char c = token[i];
        return c == '.' || c == '!' || c == '?' || c == '\u2026';
    }

    private static string ComputeFileHash(string filePath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            throw new BookIndexException($"Failed to compute hash for file '{filePath}': {ex.Message}", ex);
        }
    }
}
