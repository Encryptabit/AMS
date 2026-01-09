using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts.Hydrate;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Service for building detailed proof reports from HydratedTranscript.
/// </summary>
public class ProofReportService
{
    /// <summary>
    /// Build a complete chapter report from HydratedTranscript.
    /// </summary>
    /// <param name="chapterName">Display name for the chapter.</param>
    /// <param name="hydrate">The hydrated transcript to build report from.</param>
    /// <returns>Complete chapter report with sentences, paragraphs, and statistics.</returns>
    public ChapterReport BuildReport(string chapterName, HydratedTranscript hydrate)
    {
        ArgumentNullException.ThrowIfNull(hydrate);

        // Build sentence-to-paragraph mapping
        var sentenceToParagraph = new Dictionary<int, int>();
        foreach (var para in hydrate.Paragraphs)
        {
            foreach (var sentId in para.SentenceIds)
            {
                sentenceToParagraph[sentId] = para.Id;
            }
        }

        // Build sentence reports
        var sentenceReports = hydrate.Sentences
            .Select(s => BuildSentenceReport(s, sentenceToParagraph))
            .ToList();

        // Build paragraph reports
        var flaggedSentenceIds = hydrate.Sentences
            .Where(IsSentenceFlagged)
            .Select(s => s.Id)
            .ToHashSet();

        var paragraphReports = hydrate.Paragraphs
            .Select(p => BuildParagraphReport(p, hydrate.Sentences, flaggedSentenceIds))
            .ToList();

        // Compute stats
        var stats = ComputeStats(hydrate);

        return new ChapterReport(
            ChapterName: chapterName,
            AudioPath: hydrate.AudioPath,
            ScriptPath: hydrate.ScriptPath,
            Created: hydrate.CreatedAtUtc,
            Stats: stats,
            Sentences: sentenceReports,
            Paragraphs: paragraphReports);
    }

    private SentenceReport BuildSentenceReport(HydratedSentence sentence, Dictionary<int, int> sentenceToParagraph)
    {
        var timing = FormatTiming(
            sentence.Timing?.StartSec ?? 0,
            sentence.Timing?.EndSec ?? 0);

        var bookRange = $"{sentence.BookRange.Start}-{sentence.BookRange.End}";
        var scriptRange = sentence.ScriptRange is { Start: not null, End: not null }
            ? $"{sentence.ScriptRange.Start}-{sentence.ScriptRange.End}"
            : "-";

        var excerpt = sentence.BookText.Length > 100
            ? sentence.BookText[..100] + "..."
            : sentence.BookText;

        DiffReport? diffReport = null;
        if (sentence.Diff != null)
        {
            var ops = sentence.Diff.Ops
                .Select(op => new DiffOpReport(op.Operation, op.Tokens))
                .ToList();
            diffReport = new DiffReport(ops);
        }

        return new SentenceReport(
            Id: sentence.Id,
            Wer: FormatPercentage(sentence.Metrics.Wer),
            Cer: FormatPercentage(sentence.Metrics.Cer),
            Status: sentence.Status,
            BookRange: bookRange,
            ScriptRange: scriptRange,
            Timing: timing,
            BookText: sentence.BookText,
            ScriptText: sentence.ScriptText,
            Excerpt: excerpt,
            Diff: diffReport,
            StartTime: sentence.Timing?.StartSec ?? 0,
            EndTime: sentence.Timing?.EndSec ?? 0,
            ParagraphId: sentenceToParagraph.GetValueOrDefault(sentence.Id));
    }

    private ParagraphReport BuildParagraphReport(
        HydratedParagraph paragraph,
        IReadOnlyList<HydratedSentence> allSentences,
        HashSet<int> flaggedSentenceIds)
    {
        // Get timing from first and last sentences in the paragraph
        var parasentences = allSentences
            .Where(s => paragraph.SentenceIds.Contains(s.Id))
            .OrderBy(s => s.Id)
            .ToList();

        double startTime = parasentences.FirstOrDefault()?.Timing?.StartSec ?? 0;
        double endTime = parasentences.LastOrDefault()?.Timing?.EndSec ?? 0;

        var timing = FormatTiming(startTime, endTime);
        var bookRange = $"{paragraph.BookRange.Start}-{paragraph.BookRange.End}";

        var flaggedInParagraph = paragraph.SentenceIds
            .Where(id => flaggedSentenceIds.Contains(id))
            .ToList();

        return new ParagraphReport(
            Id: paragraph.Id,
            Wer: FormatPercentage(paragraph.Metrics.Wer),
            Coverage: FormatPercentage(paragraph.Metrics.Coverage),
            Status: paragraph.Status,
            BookRange: bookRange,
            Timing: timing,
            BookText: paragraph.BookText,
            StartTime: startTime,
            EndTime: endTime,
            SentenceIds: paragraph.SentenceIds.ToList(),
            FlaggedSentenceIds: flaggedInParagraph);
    }

    private ChapterStats ComputeStats(HydratedTranscript hydrate)
    {
        var sentences = hydrate.Sentences;
        var paragraphs = hydrate.Paragraphs;

        int sentenceCount = sentences.Count;
        int flaggedCount = sentences.Count(IsSentenceFlagged);

        double avgWer = sentenceCount > 0
            ? sentences.Average(s => s.Metrics.Wer)
            : 0;

        double maxWer = sentenceCount > 0
            ? sentences.Max(s => s.Metrics.Wer)
            : 0;

        int paragraphCount = paragraphs.Count;
        double paragraphAvgWer = paragraphCount > 0
            ? paragraphs.Average(p => p.Metrics.Wer)
            : 0;

        double avgCoverage = paragraphCount > 0
            ? paragraphs.Average(p => p.Metrics.Coverage)
            : 0;

        return new ChapterStats(
            SentenceCount: sentenceCount,
            FlaggedCount: flaggedCount,
            AvgWer: FormatPercentage(avgWer),
            MaxWer: FormatPercentage(maxWer),
            ParagraphCount: paragraphCount,
            ParagraphAvgWer: FormatPercentage(paragraphAvgWer),
            AvgCoverage: FormatPercentage(avgCoverage));
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
    /// Format timing as "870.530s -> 871.050s (delta 0.520s)".
    /// </summary>
    private static string FormatTiming(double start, double end)
    {
        var delta = end - start;
        return $"{start:F3}s -> {end:F3}s (delta {delta:F3}s)";
    }

    /// <summary>
    /// Format a decimal WER value as percentage string (e.g., "2.48%").
    /// </summary>
    private static string FormatPercentage(double value)
    {
        return $"{value * 100:F2}%";
    }
}
