using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    /// Compute full book overview by reading hydrate files directly from disk.
    /// Much faster than going through full chapter context loading.
    /// </summary>
    /// <param name="workspace">The workspace to get chapter info from.</param>
    /// <returns>Book overview with all chapter metrics, or null if workspace not initialized.</returns>
    public BookOverview? ComputeBookOverviewDirect(BlazorWorkspace workspace)
    {
        if (!workspace.IsInitialized || string.IsNullOrEmpty(workspace.WorkingDirectory))
            return null;

        var chapterMetrics = new List<(string ChapterName, ChapterMetrics Metrics)>();

        foreach (var chapterName in workspace.AvailableChapters)
        {
            var stem = workspace.GetStemForChapter(chapterName);
            if (string.IsNullOrEmpty(stem)) continue;

            var hydratePath = Path.Combine(workspace.WorkingDirectory, stem, $"{stem}.align.hydrate.json");
            if (!File.Exists(hydratePath)) continue;

            try
            {
                var json = File.ReadAllText(hydratePath);
                var hydrate = JsonSerializer.Deserialize<HydratedTranscript>(json);
                if (hydrate == null) continue;

                var metrics = ComputeChapterMetrics(hydrate);
                chapterMetrics.Add((chapterName, metrics));
            }
            catch
            {
                // Skip chapters that fail to load
            }
        }

        return ComputeBookOverview(chapterMetrics);
    }

    /// <summary>
    /// Determines if a sentence is flagged (needs review).
    /// A sentence is flagged if it has insertions or deletions in its diff,
    /// or if Status is not "ok" when no diff stats are available.
    /// </summary>
    private static bool IsSentenceFlagged(HydratedSentence sentence)
    {
        var stats = sentence.Diff?.Stats;
        if (stats is null)
        {
            return !string.Equals(sentence.Status, "ok", StringComparison.OrdinalIgnoreCase);
        }

        return stats.Insertions > 0 || stats.Deletions > 0;
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
