using Ams.Core.Align.Tx;

namespace Ams.Core.Pipeline;

/// <summary>
/// Service for transforming BookIndex to TranscriptIndex for sentence refinement pipeline
/// </summary>
public static class BookIndexToTranscriptTransformer
{
    /// <summary>
    /// Transforms a BookIndex to TranscriptIndex by creating sentence alignments
    /// between book sentences and ASR token ranges
    /// </summary>
    public static TranscriptIndex Transform(
        BookIndex bookIndex,
        AsrResponse asr,
        string audioPath,
        string bookIndexPath)
    {
        var wordAligns = new List<WordAlign>();
        var sentenceAligns = new List<SentenceAlign>();
        var paragraphAligns = new List<ParagraphAlign>();

        // Create sentence alignments from book sentences
        foreach (var bookSentence in bookIndex.Sentences)
        {
            // Map book word range to ASR token range
            // For now, assume 1:1 mapping between book words and ASR tokens
            // This is a simplification - in reality, alignment would be more complex
            var scriptRange = new ScriptRange(
                Start: bookSentence.Start,
                End: bookSentence.End
            );

            var bookRange = new IntRange(
                Start: bookSentence.Start,
                End: bookSentence.End
            );

            var sentenceMetrics = new SentenceMetrics(
                Wer: 0.0, // Default - would be computed from actual alignment
                Cer: 0.0, // Default - would be computed from actual alignment
                SpanWer: 0.0, // Default - would be computed from actual alignment
                MissingRuns: 0, // Default - would be computed from actual alignment
                ExtraRuns: 0 // Default - would be computed from actual alignment
            );

            var sentenceAlign = new SentenceAlign(
                Id: bookSentence.Index,
                BookRange: bookRange,
                ScriptRange: scriptRange,
                Metrics: sentenceMetrics,
                Status: "aligned"
            );

            sentenceAligns.Add(sentenceAlign);
        }

        // Create paragraph alignments from book paragraphs
        foreach (var bookParagraph in bookIndex.Paragraphs)
        {
            var bookRange = new IntRange(
                Start: bookParagraph.Start,
                End: bookParagraph.End
            );

            // Find all sentence IDs that belong to this paragraph
            var sentenceIds = sentenceAligns
                .Where(s => s.BookRange.Start >= bookRange.Start && s.BookRange.End <= bookRange.End)
                .Select(s => s.Id)
                .ToList();

            var paragraphMetrics = new ParagraphMetrics(
                Wer: 0.0, // Default - would be computed from actual alignment
                Cer: 0.0, // Default - would be computed from actual alignment
                Coverage: 1.0 // Default - assume full coverage
            );

            var paragraphAlign = new ParagraphAlign(
                Id: bookParagraph.Index,
                BookRange: bookRange,
                SentenceIds: sentenceIds,
                Metrics: paragraphMetrics,
                Status: "aligned"
            );

            paragraphAligns.Add(paragraphAlign);
        }

        return new TranscriptIndex(
            AudioPath: audioPath,
            ScriptPath: "", // Not used in current pipeline
            BookIndexPath: bookIndexPath,
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1.0",
            Words: wordAligns,
            Sentences: sentenceAligns,
            Paragraphs: paragraphAligns
        );
    }
}
