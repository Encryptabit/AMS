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

        int globalWord = 0;
        int sentenceIndex = 0;

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

            foreach (var token in TokenizeByWhitespace(pText))
            {
                var w = new BookWord(
                    Text: token,
                    WordIndex: globalWord,
                    SentenceIndex: sentenceIndex,
                    ParagraphIndex: pIndex
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
            BuildWarnings: warnings
        );
    }

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
