using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Ams.Core;

/// <summary>
/// Canonical indexer: preserves exact token text, builds sentence/paragraph ranges only.
/// No normalization, no timing.
/// </summary>
public class BookIndexer : IBookIndexer
{
    private static readonly Regex _blankLineSplit = new("(\r?\n){2,}", RegexOptions.Compiled);

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
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
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

        // Determine paragraphs: prefer structured from parser, else split from text
        var paragraphTexts = (parseResult.Paragraphs != null && parseResult.Paragraphs.Count > 0)
            ? parseResult.Paragraphs.Select(p => (Text: p.Text, Style: p.Style ?? "Unknown", Kind: p.Kind ?? "Body")).ToList()
            : _blankLineSplit.Split(parseResult.Text)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(t => (Text: t.TrimEnd('\r', '\n'), Style: "Unknown", Kind: "Body"))
                .ToList();

        var words = new List<BookWord>();
        var sentences = new List<SentenceRange>();
        var paragraphs = new List<ParagraphRange>();
        var sections = new List<SectionRange>();

        int globalWord = 0;
        int sentenceIndex = 0;
        int sectionId = 0;
        SectionOpen? currentSection = null;

        for (int pIndex = 0; pIndex < paragraphTexts.Count; pIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (pText, style, kind) = paragraphTexts[pIndex];
            if (string.IsNullOrEmpty(pText))
            {
                paragraphs.Add(new ParagraphRange(pIndex, globalWord, globalWord - 1, kind, style));
                continue;
            }

            int paragraphStartWord = globalWord;
            int sentenceStartWord = globalWord;

            // On a Heading paragraph, consider starting a new section before consuming tokens
            int headingLevel = GetHeadingLevel(style);
            // Some documents use Heading 2/3 for chapters; treat any Heading level >= 1 as a section start
            // but only when the text looks like a real section title (e.g., "Chapter 3", "Prologue").
            if (string.Equals(kind, "Heading", StringComparison.OrdinalIgnoreCase) && headingLevel >= 1 && LooksLikeSectionHeading(pText))
            {
                // Close previous open section
                if (currentSection != null)
                {
                    // End at the last word of the previous paragraph (globalWord - 1)
                    int endWord = Math.Max(currentSection.StartWord - 1, globalWord - 1);
                    int endParagraph = Math.Max(currentSection.StartParagraph, pIndex - 1);
                    sections.Add(new SectionRange(
                        Id: currentSection.Id,
                        Title: currentSection.Title,
                        Level: 1,
                        Kind: currentSection.Kind,
                        StartWord: currentSection.StartWord,
                        EndWord: endWord,
                        StartParagraph: currentSection.StartParagraph,
                        EndParagraph: endParagraph
                    ));
                }

                currentSection = new SectionOpen(
                    Id: sectionId++,
                    Title: pText.Trim(),
                    Kind: ClassifySectionKind(pText),
                    StartWord: globalWord,
                    StartParagraph: pIndex
                );
            }

            foreach (var token in TokenizeByWhitespace(pText))
            {
                var w = new BookWord(
                    Text: token,
                    WordIndex: globalWord,
                    SentenceIndex: sentenceIndex,
                    ParagraphIndex: pIndex,
                    SectionIndex: currentSection?.Id ?? -1
                );
                words.Add(w);
                globalWord++;

                if (IsSentenceTerminal(token))
                {
                    sentences.Add(new SentenceRange(Index: sentenceIndex, Start: sentenceStartWord, End: globalWord - 1));
                    sentenceIndex++;
                    sentenceStartWord = globalWord;
                }
            }

            // If paragraph ends without terminal punctuation but has words, close sentence
            if (globalWord > sentenceStartWord)
            {
                sentences.Add(new SentenceRange(Index: sentenceIndex, Start: sentenceStartWord, End: globalWord - 1));
                sentenceIndex++;
            }

            paragraphs.Add(new ParagraphRange(Index: pIndex, Start: paragraphStartWord, End: globalWord - 1, Kind: kind, Style: style));
        }

        var totals = new BookTotals(
            Words: words.Count,
            Sentences: sentences.Count,
            Paragraphs: paragraphs.Count,
            EstimatedDurationSec: words.Count / options.AverageWpm * 60.0
        );

        // Close last open section if any
        if (currentSection != null)
        {
            int endWord = Math.Max(currentSection.StartWord - 1, globalWord - 1);
            int endParagraph = Math.Max(currentSection.StartParagraph, paragraphTexts.Count - 1);
            sections.Add(new SectionRange(
                Id: currentSection.Id,
                Title: currentSection.Title,
                Level: 1,
                Kind: currentSection.Kind,
                StartWord: currentSection.StartWord,
                EndWord: endWord,
                StartParagraph: currentSection.StartParagraph,
                EndParagraph: endParagraph
            ));
        }

        var warnings = Array.Empty<string>();

        return new BookIndex(
            SourceFile: sourceFile,
            SourceFileHash: sourceFileHash,
            IndexedAt: DateTime.UtcNow,
            Title: parseResult.Title,
            Author: parseResult.Author,
            Totals: totals,
            Words: words.ToArray(),
            Sentences: sentences.ToArray(),
            Paragraphs: paragraphs.ToArray(),
            Sections: sections.ToArray(),
            BuildWarnings: warnings
        );
    }

    private static int GetHeadingLevel(string style)
    {
        if (string.IsNullOrEmpty(style)) return 0;
        // Common forms: "Heading 1", "Heading1"
        var s = style.Trim();
        var idx = s.IndexOf("Heading", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 0;
        // Extract trailing digits
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
        return 1; // treat generic Heading as level 1 if unspecified
    }

    private static string ClassifySectionKind(string headingText)
    {
        if (string.IsNullOrWhiteSpace(headingText)) return "chapter";
        var t = headingText.Trim().ToLowerInvariant();
        // Typical section kinds
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
        if (SectionTitleRegex.IsMatch(t)) return true;
        return false;
    }

    private record SectionOpen(int Id, string Title, string Kind, int StartWord, int StartParagraph);

    private static IEnumerable<string> TokenizeByWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;

        int i = 0;
        int n = text.Length;
        while (i < n)
        {
            // Skip whitespace
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
        // Strip common closing punctuation to inspect terminal char
        int i = token.Length - 1;
        while (i >= 0 && ")]}\'\"»”’".IndexOf(token[i]) >= 0) i--;
        if (i < 0) return false;
        char c = token[i];
        return c == '.' || c == '!' || c == '?' || c == '…';
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
