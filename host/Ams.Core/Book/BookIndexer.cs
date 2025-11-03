using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Ams.Core.Book;

/// <summary>
/// Canonical indexer: preserves exact token text, builds sentence/paragraph ranges only.
/// No normalization, no timing.
/// </summary>
public class BookIndexer : IBookIndexer
{
    private static readonly Regex _blankLineSplit = new("(\r?\n){2,}", RegexOptions.Compiled);
    private readonly IPronunciationProvider _pronunciationProvider;

    public BookIndexer(IPronunciationProvider? pronunciationProvider = null)
    {
        _pronunciationProvider = pronunciationProvider ?? NullPronunciationProvider.Instance;
    }

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

        var paragraphTexts = BuildParagraphTexts(parseResult);
        var lexicalTokens = CollectLexicalTokens(paragraphTexts);
        IReadOnlyDictionary<string, string[]> pronunciations;

        try
        {
            pronunciations = await _pronunciationProvider.GetPronunciationsAsync(lexicalTokens, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new BookIndexException($"Failed to generate pronunciations for '{sourceFile}': {ex.Message}", ex);
        }

        try
        {
            return await Task.Run(() => Process(parseResult, sourceFile, options, paragraphTexts, pronunciations, cancellationToken), cancellationToken);
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
        List<(string Text, string Style, string Kind)> paragraphTexts,
        IReadOnlyDictionary<string, string[]> pronunciations,
        CancellationToken cancellationToken)
    {
        var sourceFileHash = ComputeFileHash(sourceFile);

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

            var trimmedParagraph = pText.Trim();
            bool paragraphHasLexical = ContainsLexicalContent(trimmedParagraph);

            if (paragraphHasLexical && ShouldStartSection(trimmedParagraph, style, kind))
            {
                int sectionLevel = GetHeadingLevel(style);
                if (sectionLevel <= 0 && LooksLikeHeadingStyle(style, kind))
                {
                    sectionLevel = 1;
                }
                if (sectionLevel <= 0 && LooksLikeSectionHeading(trimmedParagraph))
                {
                    sectionLevel = 1;
                }
                if (sectionLevel <= 0)
                {
                    sectionLevel = 1;
                }

                if (currentSection != null)
                {
                    int endWord = Math.Max(currentSection.StartWord, globalWord) - 1;
                    int endParagraph = Math.Max(currentSection.StartParagraph, pIndex - 1);
                    sections.Add(new SectionRange(
                        Id: currentSection.Id,
                        Title: currentSection.Title,
                        Level: currentSection.Level,
                        Kind: currentSection.Kind,
                        StartWord: currentSection.StartWord,
                        EndWord: endWord,
                        StartParagraph: currentSection.StartParagraph,
                        EndParagraph: endParagraph
                    ));
                }

                currentSection = new SectionOpen(
                    Id: sectionId++,
                    Title: trimmedParagraph,
                    Kind: ClassifySectionKind(trimmedParagraph),
                    Level: sectionLevel,
                    StartWord: globalWord,
                    StartParagraph: pIndex
                );
            }

            int paragraphStartWord = globalWord;
            int sentenceStartWord = globalWord;

            foreach (var token in TokenizeByWhitespace(pText))
            {
                if (!ContainsLexicalContent(token))
                {
                    continue;
                }

                paragraphHasLexical = true;

                string? lexeme = PronunciationHelper.NormalizeForLookup(token);
                string[]? phonemes = null;
                if (!string.IsNullOrEmpty(lexeme) && pronunciations.TryGetValue(lexeme, out var mapped) && mapped.Length > 0)
                {
                    phonemes = mapped.ToArray();
                }

                var w = new BookWord(
                    Text: token,
                    WordIndex: globalWord,
                    SentenceIndex: sentenceIndex,
                    ParagraphIndex: pIndex,
                    SectionIndex: currentSection?.Id ?? -1,
                    Phonemes: phonemes
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

            if (!paragraphHasLexical)
            {
                paragraphs.Add(new ParagraphRange(Index: pIndex, Start: paragraphStartWord, End: paragraphStartWord - 1, Kind: "Pause", Style: style));
                continue;
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
            int endWord = Math.Max(currentSection.StartWord, globalWord) - 1;
            int endParagraph = Math.Max(currentSection.StartParagraph, paragraphTexts.Count - 1);
            sections.Add(new SectionRange(
                Id: currentSection.Id,
                Title: currentSection.Title,
                Level: currentSection.Level,
                Kind: currentSection.Kind,
                StartWord: currentSection.StartWord,
                EndWord: endWord,
                StartParagraph: currentSection.StartParagraph,
                EndParagraph: endParagraph
            ));
        }

        ApplyChapterDuplicateSuffixes(sections);

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

    private static bool ShouldStartSection(string text, string style, string kind)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (IsNonSectionParagraphStyle(style))
            return false;

        var trimmed = text.Trim();
        bool textHasKeyword = LooksLikeSectionHeading(trimmed);
        bool styleSuggestsHeading = LooksLikeHeadingStyle(style, kind);
        bool standaloneCandidate = LooksLikeStandaloneTitle(trimmed);

        if (textHasKeyword)
        {
            if (LooksLikeTableOfContentsEntry(trimmed))
                return false;

            return true;
        }

        if (styleSuggestsHeading && standaloneCandidate)
            return true;

        return false;
    }

    private static bool LooksLikeHeadingStyle(string? style, string? kind)
    {
        if (!string.IsNullOrEmpty(kind) && kind.Equals("Heading", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrWhiteSpace(style))
            return false;

        if (IsNonSectionParagraphStyle(style))
            return false;

        return style.Contains("heading", StringComparison.OrdinalIgnoreCase)
            || style.Contains("title", StringComparison.OrdinalIgnoreCase)
            || style.Contains("chapter", StringComparison.OrdinalIgnoreCase)
            || style.Contains("section", StringComparison.OrdinalIgnoreCase)
            || style.Contains("part", StringComparison.OrdinalIgnoreCase)
            || style.Contains("book", StringComparison.OrdinalIgnoreCase)
            || style.Contains("prologue", StringComparison.OrdinalIgnoreCase)
            || style.Contains("epilogue", StringComparison.OrdinalIgnoreCase)
            || style.Contains("foreword", StringComparison.OrdinalIgnoreCase)
            || style.Contains("afterword", StringComparison.OrdinalIgnoreCase)
            || style.Contains("preface", StringComparison.OrdinalIgnoreCase)
            || style.Contains("acknowledg", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonSectionParagraphStyle(string? style)
    {
        if (string.IsNullOrWhiteSpace(style))
            return false;

        return style.Contains("toc", StringComparison.OrdinalIgnoreCase)
            || style.Contains("tableofcontents", StringComparison.OrdinalIgnoreCase)
            || style.Contains("table of contents", StringComparison.OrdinalIgnoreCase)
            || style.Contains("caption", StringComparison.OrdinalIgnoreCase)
            || style.Contains("footer", StringComparison.OrdinalIgnoreCase)
            || style.Contains("header", StringComparison.OrdinalIgnoreCase)
            || style.Contains("page number", StringComparison.OrdinalIgnoreCase)
            || style.Contains("pagenumber", StringComparison.OrdinalIgnoreCase)
            || style.Contains("index", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeStandaloneTitle(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();
        if (trimmed.Length == 0 || trimmed.Length > 120)
            return false;

        if (LooksLikeTableOfContentsEntry(trimmed))
            return false;

        if (trimmed.IndexOfAny(new[] { '?', '!', ';' }) >= 0)
            return false;

        // Allow typical "1 - Title" or "III. Title" patterns often used for chapters
        if (NumberedHeadingRegex.IsMatch(trimmed))
            return true;

        int letterCount = 0;
        int upperCount = 0;
        foreach (var ch in trimmed)
        {
            if (char.IsLetter(ch))
            {
                letterCount++;
                if (char.IsUpper(ch))
                    upperCount++;
            }
        }

        if (letterCount > 0)
        {
            double upperRatio = (double)upperCount / letterCount;
            if (upperRatio >= 0.6)
                return true;
        }

        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0 && words.Length <= 8 && words.All(w => char.IsLetter(w[0]) && char.IsUpper(w[0])))
            return true;

        return false;
    }

    private static bool LooksLikeTableOfContentsEntry(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Contains("....", StringComparison.Ordinal))
            return true;

        if (Regex.IsMatch(text, @"\.{2,}\s*\d+$"))
            return true;

        if (Regex.IsMatch(text, @"[ \t]{2,}\d+$"))
            return true;

        if (text.Contains('	'))
        {
            var parts = text.Split('	', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var tail = parts[^1].Trim();
                if (tail.Length > 0 && tail.All(char.IsDigit))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static readonly Regex ChapterDuplicateRegex = new(@"^(?<prefix>\s*chapter)(?<ws>\s+)(?<number>\d+)(?<suffix>\s*[A-Za-z]*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SectionTitleRegex = new(
        @"^\s*(chapter\b|prologue\b|epilogue\b|prelude\b|foreword\b|introduction\b|afterword\b|appendix\b|part\b|book\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NumberedHeadingRegex = new(
        @"^\s*((\d+|[ivxlcdm]+)\s*[-–.:]\s*[a-zA-Z])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static bool LooksLikeSectionHeading(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        if (SectionTitleRegex.IsMatch(t)) return true;
        return false;
    }

    private static void ApplyChapterDuplicateSuffixes(List<SectionRange> sections)
    {
        if (sections == null || sections.Count == 0)
            return;

        var candidates = new List<(int Index, SectionRange Section, string Prefix, string Ws, string Number, string BaseKey)>();

        for (int i = 0; i < sections.Count; i++)
        {
            var section = sections[i];
            if (!string.Equals(section.Kind, "chapter", StringComparison.OrdinalIgnoreCase))
                continue;

            var title = section.Title ?? string.Empty;
            var match = ChapterDuplicateRegex.Match(title);
            if (!match.Success)
                continue;

            if (!string.IsNullOrWhiteSpace(match.Groups["suffix"].Value))
                continue;

            var prefix = match.Groups["prefix"].Value;
            var ws = match.Groups["ws"].Value;
            var number = match.Groups["number"].Value;
            var baseKey = (prefix + ws + number).Trim().ToUpperInvariant();

            candidates.Add((i, section, prefix, ws, number, baseKey));
        }

        foreach (var group in candidates.GroupBy(c => (c.BaseKey, c.Section.Level)))
        {
            if (group.Count() <= 1)
                continue;

            var distinctTitles = group
                .Select(c => (c.Section.Title ?? string.Empty).Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctTitles.Count > 1)
                continue;

            int offset = 0;
            foreach (var item in group.OrderBy(c => c.Section.StartWord))
            {
                var suffixLetter = (char)('A' + offset);
                var newTitle = string.Concat(item.Prefix, item.Ws, item.Number, suffixLetter);
                sections[item.Index] = item.Section with { Title = newTitle };
                offset++;
            }
        }
    }

    private record SectionOpen(int Id, string Title, string Kind, int Level, int StartWord, int StartParagraph);

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
        while (i >= 0 && ")]}'\"»”’".Contains(token[i])) i--;
        if (i < 0) return false;
        char c = token[i];
        return c is '.' or '!' or '?' or '…';
    }

    private static List<(string Text, string Style, string Kind)> BuildParagraphTexts(BookParseResult parseResult)
    {
        if (parseResult.Paragraphs != null && parseResult.Paragraphs.Count > 0)
        {
            return parseResult.Paragraphs
                .Select(p => (Text: p.Text, Style: p.Style ?? "Unknown", Kind: p.Kind ?? "Body"))
                .ToList();
        }

        return _blankLineSplit.Split(parseResult.Text)
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(t => (Text: t.TrimEnd('\r', '\n'), Style: "Unknown", Kind: "Body"))
            .ToList();
    }

    private static IEnumerable<string> CollectLexicalTokens(List<(string Text, string Style, string Kind)> paragraphs)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (text, _, _) in paragraphs)
        {
            foreach (var token in TokenizeByWhitespace(text))
            {
                if (!ContainsLexicalContent(token))
                {
                    continue;
                }

                var normalized = PronunciationHelper.NormalizeForLookup(token);
                if (!string.IsNullOrEmpty(normalized))
                {
                    set.Add(normalized);
                }
            }
        }

        return set;
    }

    private static bool ContainsLexicalContent(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                return true;
            }
        }

        return false;
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

