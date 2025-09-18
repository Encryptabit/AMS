using System.Text.Json;
using Ams.Core.Align.Tx;

namespace Ams.Core.Services;

/// <summary>
/// Service for generating hydrated TX format with human-readable fields.
/// Extracted from AlignCommand.CreateHydrateTx() to enable reuse in pipeline.
/// </summary>
public sealed class HydratedTxService
{
    /// <summary>
    /// Generates hydrated TranscriptIndex with bookWord/asrWord/bookText/scriptText fields.
    /// Converts AlignOp enum values to readable strings and maintains all original TX structure.
    /// </summary>
    /// <param name="tx">TranscriptIndex containing word/sentence alignment data</param>
    /// <param name="book">BookIndex providing source text for word indices</param>
    /// <param name="asr">ASR response providing script text for token indices</param>
    /// <returns>Hydrated object matching ./CORRECT_RESULTS/Chapter*.hydrated-tx.json format</returns>
    public object GenerateHydratedTx(TranscriptIndex tx, BookIndex book, AsrResponse asr)
    {
        if (tx == null) throw new ArgumentNullException(nameof(tx));
        if (book == null) throw new ArgumentNullException(nameof(book));
        if (asr == null) throw new ArgumentNullException(nameof(asr));

        // Hydrate words with values
        var hydratedWords = HydrateWords(tx.Words, book, asr);

        // Hydrate sentences with text content
        var hydratedSentences = HydrateSentences(tx.Sentences, book, asr);

        // Hydrate paragraphs with text content  
        var hydratedParagraphs = HydrateParagraphs(tx.Paragraphs, book);

        return new
        {
            audioPath = tx.AudioPath,
            scriptPath = tx.ScriptPath,
            bookIndexPath = tx.BookIndexPath,
            createdAtUtc = tx.CreatedAtUtc,
            normalizationVersion = tx.NormalizationVersion,
            words = hydratedWords,
            sentences = hydratedSentences,
            paragraphs = hydratedParagraphs
        };
    }

    /// <summary>
    /// Hydrates word alignments with actual book and ASR word text.
    /// Converts AlignOp enum to readable string representation.
    /// </summary>
    private object[] HydrateWords(IReadOnlyList<WordAlign> words, BookIndex book, AsrResponse asr)
    {
        return words.Select(w => new
        {
            bookIdx = w.BookIdx,
            asrIdx = w.AsrIdx,
            bookWord = GetBookWord(w.BookIdx, book),
            asrWord = GetAsrWord(w.AsrIdx, asr),
            op = ConvertAlignOpToString(w.Op),
            reason = w.Reason,
            score = w.Score
        }).ToArray();
    }

    /// <summary>
    /// Hydrates sentence alignments with full book and script text content.
    /// Joins words within sentence ranges to create readable text.
    /// </summary>
    private object[] HydrateSentences(IReadOnlyList<SentenceAlign> sentences, BookIndex book, AsrResponse asr)
    {
        return sentences.Select(s => new
        {
            id = s.Id,
            bookRange = new { start = s.BookRange.Start, end = s.BookRange.End },
            scriptRange = s.ScriptRange != null ? 
                new { start = s.ScriptRange.Start, end = s.ScriptRange.End } : null,
            bookText = JoinBookWords(book, s.BookRange.Start, s.BookRange.End),
            scriptText = s.ScriptRange != null ? 
                JoinAsrWords(asr, s.ScriptRange.Start, s.ScriptRange.End) : string.Empty,
            metrics = s.Metrics,
            status = s.Status
        }).ToArray();
    }

    /// <summary>
    /// Hydrates paragraph alignments with full book text content.
    /// </summary>
    private object[] HydrateParagraphs(IReadOnlyList<ParagraphAlign> paragraphs, BookIndex book)
    {
        return paragraphs.Select(p => new
        {
            id = p.Id,
            bookRange = new { start = p.BookRange.Start, end = p.BookRange.End },
            sentenceIds = p.SentenceIds,
            bookText = JoinBookWords(book, p.BookRange.Start, p.BookRange.End),
            metrics = p.Metrics,
            status = p.Status
        }).ToArray();
    }

    /// <summary>
    /// Converts AlignOp enum to string representation for human readability.
    /// Matches the format used in ./CORRECT_RESULTS/ reference files.
    /// </summary>
    private string ConvertAlignOpToString(AlignOp op)
    {
        return op switch
        {
            AlignOp.Match => "Match",
            AlignOp.Sub => "Sub", 
            AlignOp.Ins => "Ins",
            AlignOp.Del => "Del",
            _ => op.ToString()
        };
    }

    /// <summary>
    /// Gets book word text by index, with bounds checking.
    /// </summary>
    private string? GetBookWord(int? bookIdx, BookIndex book)
    {
        if (!bookIdx.HasValue || bookIdx.Value < 0 || bookIdx.Value >= book.Words.Length)
            return null;
        return book.Words[bookIdx.Value].Text;
    }

    /// <summary>
    /// Gets ASR word text by index, with bounds checking.
    /// </summary>
    private string? GetAsrWord(int? asrIdx, AsrResponse asr)
    {
        if (!asrIdx.HasValue || asrIdx.Value < 0 || asrIdx.Value >= asr.Tokens.Length)
            return null;
        return asr.Tokens[asrIdx.Value].Word;
    }

    /// <summary>
    /// Joins book words within a range to create readable text.
    /// </summary>
    private string JoinBookWords(BookIndex book, int start, int end)
    {
        if (start < 0 || end >= book.Words.Length || end < start)
            return string.Empty;
        return string.Join(" ", book.Words.Skip(start).Take(end - start + 1).Select(w => w.Text));
    }

    /// <summary>
    /// Joins ASR tokens within a range to create readable text.
    /// </summary>
    private string JoinAsrWords(AsrResponse asr, int? start, int? end)
    {
        if (!start.HasValue || !end.HasValue)
            return string.Empty;
        int s = start.Value, e = end.Value;
        if (s < 0 || e >= asr.Tokens.Length || e < s)
            return string.Empty;
        return string.Join(" ", asr.Tokens.Skip(s).Take(e - s + 1).Select(t => t.Word));
    }
}