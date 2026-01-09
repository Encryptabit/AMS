using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts.Hydrate;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Metrics for a single chapter computed from HydratedTranscript.
/// </summary>
public record ChapterMetrics(
    int SentenceCount,
    int SentenceFlagged,
    string SentenceAvgWer,
    int ParagraphCount,
    int ParagraphFlagged,
    string ParagraphAvgWer);

/// <summary>
/// Chapter info with name and metrics for overview listing.
/// </summary>
public record ProofChapterInfo(string Name, ChapterMetrics Metrics);

/// <summary>
/// Book-wide aggregated metrics across all chapters.
/// </summary>
public record BookOverview(
    string BookName,
    int ChapterCount,
    int TotalSentences,
    int TotalFlaggedSentences,
    string AvgSentenceWer,
    int TotalParagraphs,
    int TotalFlaggedParagraphs,
    string AvgParagraphWer,
    IReadOnlyList<ProofChapterInfo> Chapters);

/// <summary>
/// Service for computing validation metrics from HydratedTranscript.
/// </summary>
public class ValidationMetricsService
{
    /// <summary>
    /// Compute chapter metrics from HydratedTranscript.
    /// </summary>
    /// <param name="hydrate">The hydrated transcript to analyze.</param>
    /// <returns>Chapter metrics with sentence/paragraph counts and WER averages.</returns>
    public ChapterMetrics ComputeChapterMetrics(HydratedTranscript hydrate)
    {
        ArgumentNullException.ThrowIfNull(hydrate);

        var sentences = hydrate.Sentences;
        var paragraphs = hydrate.Paragraphs;

        // Sentence metrics
        int sentenceCount = sentences.Count;
        int sentenceFlagged = sentences.Count(s => IsSentenceFlagged(s));
        double sentenceAvgWer = sentenceCount > 0
            ? sentences.Average(s => s.Metrics.Wer)
            : 0;

        // Paragraph metrics - flagged if any contained sentence is flagged
        int paragraphCount = paragraphs.Count;
        var sentenceIdToFlagged = sentences.ToDictionary(s => s.Id, s => IsSentenceFlagged(s));
        int paragraphFlagged = paragraphs.Count(p =>
            p.SentenceIds.Any(id => sentenceIdToFlagged.GetValueOrDefault(id, false)));
        double paragraphAvgWer = paragraphCount > 0
            ? paragraphs.Average(p => p.Metrics.Wer)
            : 0;

        return new ChapterMetrics(
            SentenceCount: sentenceCount,
            SentenceFlagged: sentenceFlagged,
            SentenceAvgWer: FormatPercentage(sentenceAvgWer),
            ParagraphCount: paragraphCount,
            ParagraphFlagged: paragraphFlagged,
            ParagraphAvgWer: FormatPercentage(paragraphAvgWer));
    }

    /// <summary>
    /// Compute book-wide aggregated metrics from all chapters.
    /// </summary>
    /// <param name="chapters">List of chapter names with their metrics.</param>
    /// <returns>Book overview with aggregated statistics.</returns>
    public BookOverview ComputeBookOverview(IEnumerable<(string ChapterName, ChapterMetrics Metrics)> chapters)
    {
        ArgumentNullException.ThrowIfNull(chapters);

        var chapterList = chapters.ToList();

        if (chapterList.Count == 0)
        {
            return new BookOverview(
                BookName: "",
                ChapterCount: 0,
                TotalSentences: 0,
                TotalFlaggedSentences: 0,
                AvgSentenceWer: "0.00%",
                TotalParagraphs: 0,
                TotalFlaggedParagraphs: 0,
                AvgParagraphWer: "0.00%",
                Chapters: Array.Empty<ProofChapterInfo>());
        }

        int totalSentences = chapterList.Sum(c => c.Metrics.SentenceCount);
        int totalFlaggedSentences = chapterList.Sum(c => c.Metrics.SentenceFlagged);
        int totalParagraphs = chapterList.Sum(c => c.Metrics.ParagraphCount);
        int totalFlaggedParagraphs = chapterList.Sum(c => c.Metrics.ParagraphFlagged);

        // Weighted average WER by sentence/paragraph count
        double avgSentenceWer = totalSentences > 0
            ? chapterList.Sum(c => ParsePercentage(c.Metrics.SentenceAvgWer) * c.Metrics.SentenceCount) / totalSentences
            : 0;
        double avgParagraphWer = totalParagraphs > 0
            ? chapterList.Sum(c => ParsePercentage(c.Metrics.ParagraphAvgWer) * c.Metrics.ParagraphCount) / totalParagraphs
            : 0;

        var chapterInfos = chapterList
            .Select(c => new ProofChapterInfo(c.ChapterName, c.Metrics))
            .ToList();

        return new BookOverview(
            BookName: "", // Set by caller if needed
            ChapterCount: chapterList.Count,
            TotalSentences: totalSentences,
            TotalFlaggedSentences: totalFlaggedSentences,
            AvgSentenceWer: FormatPercentage(avgSentenceWer),
            TotalParagraphs: totalParagraphs,
            TotalFlaggedParagraphs: totalFlaggedParagraphs,
            AvgParagraphWer: FormatPercentage(avgParagraphWer),
            Chapters: chapterInfos);
    }

    /// <summary>
    /// Determines if a sentence is flagged (needs review).
    /// </summary>
    private static bool IsSentenceFlagged(HydratedSentence sentence)
    {
        return !string.Equals(sentence.Status, "ok", StringComparison.OrdinalIgnoreCase)
               || sentence.Diff != null;
    }

    /// <summary>
    /// Format a decimal WER value as percentage string (e.g., "2.48%").
    /// </summary>
    private static string FormatPercentage(double value)
    {
        return $"{value * 100:F2}%";
    }

    /// <summary>
    /// Parse a percentage string back to decimal value.
    /// </summary>
    private static double ParsePercentage(string percentage)
    {
        if (string.IsNullOrEmpty(percentage))
            return 0;

        var trimmed = percentage.TrimEnd('%');
        return double.TryParse(trimmed, out var value) ? value / 100 : 0;
    }
}
