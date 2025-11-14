using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Artifacts.Validation;

namespace Ams.Core.Processors.Validation;

public static class ValidationReportBuilder
{
    public static ReportResult Build(
        TranscriptIndex? transcript,
        HydratedTranscript? hydrated,
        ValidationReportOptions options)
    {
        if (transcript is null && hydrated is null)
        {
            throw new InvalidOperationException("At least one of transcript or hydrated transcript must be provided.");
        }

        var info = ExtractSourceInfo(transcript, hydrated);
        var sentences = BuildSentenceViews(transcript, hydrated);
        var paragraphs = BuildParagraphViews(transcript, hydrated);
        var tallies = options.IncludeWordTallies ? BuildWordTallies(sentences) : null;

        var report = BuildTextReport(info, sentences, paragraphs, tallies, options, hydrated);
        return new ReportResult(report, sentences, paragraphs, tallies);
    }

    private static SourceInfo ExtractSourceInfo(TranscriptIndex? tx, HydratedTranscript? hydrated)
        => tx is not null
            ? new SourceInfo(tx.AudioPath, tx.ScriptPath, tx.BookIndexPath, tx.CreatedAtUtc)
            : new SourceInfo(hydrated!.AudioPath, hydrated.ScriptPath, hydrated.BookIndexPath, hydrated.CreatedAtUtc);

    private static IReadOnlyList<SentenceView> BuildSentenceViews(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        if (tx is null && hydrated is null)
        {
            return Array.Empty<SentenceView>();
        }

        var txMap = tx?.Sentences.ToDictionary(s => s.Id);
        var hydratedMap = hydrated?.Sentences.ToDictionary(s => s.Id);

        var ids = new SortedSet<int>();
        if (txMap is not null)
        {
            foreach (var id in txMap.Keys)
            {
                ids.Add(id);
            }
        }
        if (hydratedMap is not null)
        {
            foreach (var id in hydratedMap.Keys)
            {
                ids.Add(id);
            }
        }

        var views = new List<SentenceView>(ids.Count);
        foreach (var id in ids)
        {
            SentenceAlign? txSentence = null;
            HydratedSentence? hydSentence = null;
            txMap?.TryGetValue(id, out txSentence);
            hydratedMap?.TryGetValue(id, out hydSentence);

            var bookRange = hydSentence is not null
                ? (hydSentence.BookRange.Start, hydSentence.BookRange.End)
                : txSentence is not null
                    ? (txSentence.BookRange.Start, txSentence.BookRange.End)
                    : (0, 0);

            var scriptRange = hydSentence?.ScriptRange is not null
                ? (hydSentence.ScriptRange.Start, hydSentence.ScriptRange.End)
                : txSentence?.ScriptRange is not null
                    ? (txSentence.ScriptRange.Start, txSentence.ScriptRange.End)
                    : (null, null);

            var metrics = txSentence?.Metrics ?? hydSentence?.Metrics
                ?? new SentenceMetrics(0, 0, 0, 0, 0);
            var status = hydSentence?.Status ?? txSentence?.Status ?? "unknown";
            string? bookText = hydSentence?.BookText;
            string? scriptText = hydSentence?.ScriptText;
            var timing = hydSentence?.Timing ?? txSentence?.Timing;
            var diff = hydSentence?.Diff;

            views.Add(new SentenceView(
                id,
                bookRange,
                scriptRange,
                metrics,
                status,
                string.IsNullOrWhiteSpace(bookText) ? null : bookText,
                string.IsNullOrWhiteSpace(scriptText) ? null : scriptText,
                timing,
                diff));
        }

        return views.OrderBy(s => s.Id).ToList();
    }

    private static IReadOnlyList<ParagraphView> BuildParagraphViews(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        if (tx is null && hydrated is null)
        {
            return Array.Empty<ParagraphView>();
        }

        var paragraphs = hydrated?.Paragraphs ?? tx!.Paragraphs.Select(p => new HydratedParagraph(
            p.Id,
            new HydratedRange(p.BookRange.Start, p.BookRange.End),
            p.SentenceIds,
            BookText: string.Empty,
            p.Metrics,
            p.Status,
            Diff: null)).ToList();

        return paragraphs!
            .Select(p => new ParagraphView(
                p.Id,
                (p.BookRange.Start, p.BookRange.End),
                p.Metrics,
                p.Status,
                string.IsNullOrWhiteSpace(p.BookText) ? null : p.BookText,
                p.Diff))
            .OrderBy(p => p.Id)
            .ToList();
    }

    private static WordTallies? BuildWordTallies(IReadOnlyList<SentenceView> sentences)
    {
        if (sentences.Count == 0)
        {
            return null;
        }

        var totals = AggregateDiffStats(sentences.Select(s => s.Diff?.Stats));
        if (!totals.HasAny)
        {
            return null;
        }

        var substitution = Math.Min(totals.Insertions, totals.Deletions);
        var insertionsOnly = totals.Insertions - substitution;
        var deletionsOnly = totals.Deletions - substitution;

        return new WordTallies(
            Match: ClampToInt(totals.Matches),
            Substitution: ClampToInt(substitution),
            Insertion: ClampToInt(insertionsOnly),
            Deletion: ClampToInt(deletionsOnly),
            Total: ClampToInt(totals.ReferenceTokens));
    }

    private static string BuildTextReport(
        SourceInfo info,
        IReadOnlyList<SentenceView> sentences,
        IReadOnlyList<ParagraphView> paragraphs,
        WordTallies? wordTallies,
        ValidationReportOptions options,
        HydratedTranscript? hydrated)
    {
        var builder = new StringBuilder();

        builder.AppendLine("=== Validation Report ===");
        builder.AppendLine($"Audio     : {info.AudioPath}");
        builder.AppendLine($"Script    : {info.ScriptPath}");
        builder.AppendLine($"Book Index: {info.BookIndexPath}");
        builder.AppendLine($"Created   : {info.CreatedAtUtc:O}");
        builder.AppendLine();

        if (sentences.Count > 0)
        {
            var totals = AggregateDiffStats(sentences.Select(s => s.Diff?.Stats));
            builder.AppendLine($"Sentences : {sentences.Count} {FormatDiffTotals(totals)}");
        }
        else
        {
            builder.AppendLine("Sentences : 0 (no diff data)");
        }

        if (paragraphs.Count > 0)
        {
            var totals = AggregateDiffStats(paragraphs.Select(p => p.Diff?.Stats));
            builder.AppendLine($"Paragraphs: {paragraphs.Count} {FormatDiffTotals(totals)}");
        }
        else
        {
            builder.AppendLine("Paragraphs: 0 (no diff data)");
        }

        if (wordTallies is not null)
        {
            builder.AppendLine($"Words     : {wordTallies.Total} (Match {wordTallies.Match}, Sub {wordTallies.Substitution}, Ins {wordTallies.Insertion}, Del {wordTallies.Deletion})");
        }

        builder.AppendLine();

        if (options.TopSentences > 0 && sentences.Count > 0)
        {
            builder.AppendLine(options.AllErrors
                ? "All sentences by diff mismatch:"
                : $"Top {Math.Min(options.TopSentences, sentences.Count)} sentences by diff mismatch:");

            var sentencesOrdered = sentences
                .OrderByDescending(ComputeSentenceDiffScore)
                .ThenByDescending(s => s.Diff?.Stats?.ReferenceTokens ?? 0)
                .ThenBy(s => s.Id);

            var sentenceBucket = options.AllErrors
                ? sentencesOrdered.Where(HasSentenceDiffIssues).ToList()
                : sentencesOrdered.Take(options.TopSentences).ToList();

            if (sentenceBucket.Count == 0)
            {
                builder.AppendLine("  (no diff issues detected)");
            }

            foreach (var sentence in sentenceBucket)
            {
                builder.AppendLine($"  #{sentence.Id} | {FormatDiffStats(sentence.Diff?.Stats)} | Status {sentence.Status}");
                if (!string.IsNullOrWhiteSpace(sentence.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(sentence.BookText)}");
                }

                if (!string.IsNullOrWhiteSpace(sentence.ScriptText))
                {
                    builder.AppendLine($"    Script : {TrimText(sentence.ScriptText)}");
                }

                AppendDiffOps(builder, sentence.Diff, "    ");
                builder.AppendLine();
            }
        }

        if (options.TopParagraphs > 0 && paragraphs.Count > 0)
        {
            builder.AppendLine(options.AllErrors
                ? "All paragraphs by diff mismatch:"
                : $"Top {Math.Min(options.TopParagraphs, paragraphs.Count)} paragraphs by diff mismatch:");

            var paragraphOrdered = paragraphs
                .OrderByDescending(ComputeParagraphDiffScore)
                .ThenByDescending(p => p.Diff?.Stats?.ReferenceTokens ?? 0)
                .ThenBy(p => p.Id);

            HashSet<int>? paragraphsWithFlaggedSentences = null;
            if (options.IncludeAllFlagged && hydrated?.Paragraphs is not null)
            {
                var flaggedSentenceIds = new HashSet<int>(
                    sentences.Where(HasSentenceDiffIssues).Select(s => s.Id));

                paragraphsWithFlaggedSentences = new HashSet<int>(
                    hydrated.Paragraphs
                        .Where(p => p.SentenceIds.Any(flaggedSentenceIds.Contains))
                        .Select(p => p.Id));
            }

            var paragraphBucket = options.AllErrors
                ? paragraphOrdered.Where(p =>
                    HasParagraphDiffIssues(p) ||
                    (paragraphsWithFlaggedSentences?.Contains(p.Id) ?? false)).ToList()
                : paragraphOrdered.Take(options.TopParagraphs).ToList();

            if (paragraphBucket.Count == 0)
            {
                builder.AppendLine("  (no diff issues detected)");
            }

            foreach (var paragraph in paragraphBucket)
            {
                builder.AppendLine($"  #{paragraph.Id} | {FormatDiffStats(paragraph.Diff?.Stats)} | Status {paragraph.Status}");
                if (!string.IsNullOrWhiteSpace(paragraph.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(paragraph.BookText)}");
                }

                AppendDiffOps(builder, paragraph.Diff, "    ");
                builder.AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }

    private sealed record DiffTotals(long ReferenceTokens, long HypothesisTokens, long Matches, long Insertions, long Deletions)
    {
        public bool HasAny => ReferenceTokens > 0 || HypothesisTokens > 0 || Insertions > 0 || Deletions > 0;
    }

    private static DiffTotals AggregateDiffStats(IEnumerable<HydratedDiffStats?> stats)
    {
        long refTotal = 0, hypTotal = 0, matches = 0, insertions = 0, deletions = 0;
        foreach (var stat in stats)
        {
            if (stat is null) continue;

            refTotal += stat.ReferenceTokens;
            hypTotal += stat.HypothesisTokens;
            matches += stat.Matches;
            insertions += stat.Insertions;
            deletions += stat.Deletions;
        }

        return new DiffTotals(refTotal, hypTotal, matches, insertions, deletions);
    }

    private static string FormatDiffTotals(DiffTotals totals)
    {
        if (!totals.HasAny)
        {
            return "(diff data unavailable)";
        }

        var matchPct = totals.ReferenceTokens > 0
            ? (double)totals.Matches / totals.ReferenceTokens
            : 1.0;

        return $"(ref {totals.ReferenceTokens}, hyp {totals.HypothesisTokens}, match {totals.Matches} ({matchPct:P1}), +{totals.Insertions}, -{totals.Deletions})";
    }

    private static string FormatDiffStats(HydratedDiffStats? stats)
    {
        if (stats is null)
        {
            return "diff unavailable";
        }

        var matchPct = stats.ReferenceTokens > 0
            ? (double)stats.Matches / stats.ReferenceTokens
            : 1.0;

        return $"ref {stats.ReferenceTokens}, hyp {stats.HypothesisTokens}, match {stats.Matches} ({matchPct:P1}), +{stats.Insertions}, -{stats.Deletions}";
    }

    private static double ComputeSentenceDiffScore(SentenceView sentence)
        => ComputeDiffScore(sentence.Diff?.Stats, sentence.Metrics.Wer);

    private static double ComputeParagraphDiffScore(ParagraphView paragraph)
        => ComputeDiffScore(paragraph.Diff?.Stats, paragraph.Metrics.Wer);

    private static double ComputeDiffScore(HydratedDiffStats? stats, double fallback)
    {
        if (stats is null)
        {
            return fallback;
        }

        var denominator = Math.Max(1, stats.ReferenceTokens);
        return (double)(stats.Insertions + stats.Deletions) / denominator;
    }

    private static bool HasSentenceDiffIssues(SentenceView sentence)
    {
        var stats = sentence.Diff?.Stats;
        if (stats is null)
        {
            return !string.Equals(sentence.Status, "ok", StringComparison.OrdinalIgnoreCase);
        }

        return stats.Insertions > 0 || stats.Deletions > 0;
    }

    private static bool HasParagraphDiffIssues(ParagraphView paragraph)
    {
        var stats = paragraph.Diff?.Stats;
        if (stats is null)
        {
            return !string.Equals(paragraph.Status, "ok", StringComparison.OrdinalIgnoreCase);
        }

        return stats.Insertions > 0 || stats.Deletions > 0;
    }

    private static void AppendDiffOps(StringBuilder builder, HydratedDiff? diff, string indent, int maxOps = 5)
    {
        if (diff?.Ops is not { Count: > 0 } ops)
        {
            builder.AppendLine($"{indent}Diff ops: (none)");
            return;
        }

        var interesting = ops
            .Where(op => !string.Equals(op.Operation, "equal", StringComparison.OrdinalIgnoreCase))
            .Take(maxOps)
            .ToList();

        if (interesting.Count == 0)
        {
            builder.AppendLine($"{indent}Diff ops: (only equal segments)");
            return;
        }

        foreach (var op in interesting)
        {
            builder.AppendLine($"{indent}{op.Operation.ToUpperInvariant(),-7} {FormatTokens(op.Tokens)}");
        }

        if (ops.Count > interesting.Count)
        {
            builder.AppendLine($"{indent}... ({ops.Count - interesting.Count} more op(s))");
        }
    }

    private static string FormatTokens(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return "(empty)";
        }

        var joined = string.Join(' ', tokens);
        return TrimText(joined, 80);
    }

    private static string TrimText(string text, int? maxLength = null)
    {
        var normalized = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        if (maxLength is null || normalized.Length <= maxLength.Value)
        {
            return normalized;
        }

        return normalized[..maxLength.Value].TrimEnd() + "…";
    }

    private static int ClampToInt(long value)
        => value > int.MaxValue ? int.MaxValue : (int)value;

}
