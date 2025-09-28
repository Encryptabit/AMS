using System.CommandLine;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ams.Core.Artifacts;
using Ams.Core.Common;

namespace Ams.Cli.Commands;

public static class ValidateCommand
{
    public static Command Create()
    {
        var validate = new Command("validate", "Validation utilities");
        validate.AddCommand(CreateReportCommand());
        return validate;
    }

    private static Command CreateReportCommand()
    {
        var cmd = new Command("report", "Render a human-friendly view of transcript validation metrics");

        var txOption = new Option<FileInfo?>(
            name: "--tx",
            description: "Path to TranscriptIndex JSON (e.g., *.align.tx.json)");
        txOption.AddAlias("-t");

        var hydrateOption = new Option<FileInfo?>(
            name: "--hydrate",
            description: "Path to hydrated transcript JSON (e.g., *.align.hydrate.json)");
        hydrateOption.AddAlias("-h");

        var outOption = new Option<FileInfo?>("--out", () => null, "Optional output file. If omitted, prints to console.");
        outOption.AddAlias("-o");

        var allErrorsOption = new Option<bool>("--show-all", () => true, "Flag to determine whether to display all errors or not");
        var topSentencesOption = new Option<int>("--top-sentences", () => 10, "Number of highest-WER sentences to display (0 to disable).");
        var topParagraphsOption = new Option<int>("--top-paragraphs", () => 5, "Number of highest-WER paragraphs to display (0 to disable).");
        var includeWordsOption = new Option<bool>("--include-words", () => false, "Include word-level alignment tallies when TranscriptIndex is provided.");

        cmd.AddOption(txOption);
        cmd.AddOption(hydrateOption);
        cmd.AddOption(outOption);
        cmd.AddOption(allErrorsOption);
        cmd.AddOption(topSentencesOption);
        cmd.AddOption(topParagraphsOption);
        cmd.AddOption(includeWordsOption);

        cmd.SetHandler(async context =>
        {
            var txFile = context.ParseResult.GetValueForOption(txOption);
            var hydrateFile = context.ParseResult.GetValueForOption(hydrateOption);
            var outputFile = context.ParseResult.GetValueForOption(outOption);
            var allErrors = context.ParseResult.GetValueForOption(allErrorsOption);
            var topSentences = context.ParseResult.GetValueForOption(topSentencesOption);
            var topParagraphs = context.ParseResult.GetValueForOption(topParagraphsOption);
            var includeWords = context.ParseResult.GetValueForOption(includeWordsOption);

            if (txFile is null && hydrateFile is null)
            {
                Log.Error("validate report requires --tx, --hydrate, or both");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var report = await GenerateReportAsync(txFile, hydrateFile, allErrors, topSentences, topParagraphs, includeWords);

                if (outputFile is not null)
                {
                    Directory.CreateDirectory(outputFile.DirectoryName ?? Directory.GetCurrentDirectory());
                    await File.WriteAllTextAsync(outputFile.FullName, report, Encoding.UTF8);
                    Log.Info("Validation report written to {Output}", outputFile.FullName);
                }
                else
                {
                    Console.WriteLine(report);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "validate report failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static async Task<string> GenerateReportAsync(FileInfo? txFile,
        FileInfo? hydrateFile,
        bool allErrors,
        int topSentences,
        int topParagraphs,
        bool includeWordTallies)
    {
        TranscriptIndex? tx = null;
        HydratedTranscript? hydrated = null;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (txFile is not null)
        {
            if (!txFile.Exists)
            {
                throw new FileNotFoundException($"TranscriptIndex file not found: {txFile.FullName}");
            }

            var txJson = await File.ReadAllTextAsync(txFile.FullName);
            tx = JsonSerializer.Deserialize<TranscriptIndex>(txJson, jsonOptions)
                 ?? throw new InvalidOperationException("Failed to deserialize TranscriptIndex JSON");
        }

        if (hydrateFile is not null)
        {
            if (!hydrateFile.Exists)
            {
                throw new FileNotFoundException($"Hydrated transcript file not found: {hydrateFile.FullName}");
            }

            var hydrateJson = await File.ReadAllTextAsync(hydrateFile.FullName);
            hydrated = JsonSerializer.Deserialize<HydratedTranscript>(hydrateJson, jsonOptions)
                        ?? throw new InvalidOperationException("Failed to deserialize hydrated transcript JSON");
        }

        if (tx is null && hydrated is null)
        {
            throw new InvalidOperationException("No transcript artifacts could be loaded");
        }

        var info = ExtractSourceInfo(tx, hydrated);
        var sentenceViews = BuildSentenceViews(tx, hydrated);
        var paragraphViews = BuildParagraphViews(tx, hydrated);
        var wordTallies = includeWordTallies ? BuildWordTallies(tx) : null;

        return BuildTextReport(info, sentenceViews, paragraphViews, wordTallies,allErrors, topSentences, topParagraphs);
    }

    private static SourceInfo ExtractSourceInfo(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        return tx is not null
            ? new SourceInfo(tx.AudioPath, tx.ScriptPath, tx.BookIndexPath, tx.CreatedAtUtc)
            : new SourceInfo(
                hydrated!.AudioPath,
                hydrated.ScriptPath,
                hydrated.BookIndexPath,
                hydrated.CreatedAtUtc);
    }

    private static IReadOnlyList<SentenceView> BuildSentenceViews(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        if (tx is null && hydrated is null)
        {
            return Array.Empty<SentenceView>();
        }

        var sentences = hydrated?.Sentences ?? tx!.Sentences.Select(s => new HydratedSentence(
            s.Id,
            new HydratedRange(s.BookRange.Start, s.BookRange.End),
            s.ScriptRange is null ? null : new HydratedScriptRange(s.ScriptRange.Start, s.ScriptRange.End),
            BookText: string.Empty,
            ScriptText: string.Empty,
            s.Metrics,
            s.Status)).ToList();

        return sentences!
            .Select(s => new SentenceView(
                s.Id,
                (s.BookRange.Start, s.BookRange.End),
                s.ScriptRange is null ? null : (s.ScriptRange.Start, s.ScriptRange.End),
                s.Metrics,
                s.Status,
                string.IsNullOrWhiteSpace(s.BookText) ? null : s.BookText,
                string.IsNullOrWhiteSpace(s.ScriptText) ? null : s.ScriptText))
            .OrderBy(s => s.Id)
            .ToList();
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
            p.Status)).ToList();

        return paragraphs!
            .Select(p => new ParagraphView(
                p.Id,
                (p.BookRange.Start, p.BookRange.End),
                p.Metrics,
                p.Status,
                string.IsNullOrWhiteSpace(p.BookText) ? null : p.BookText))
            .OrderBy(p => p.Id)
            .ToList();
    }

    private static WordTallies? BuildWordTallies(TranscriptIndex? tx)
    {
        if (tx is null)
        {
            return null;
        }

        int match = 0, substitution = 0, insertion = 0, deletion = 0;

        foreach (var word in tx.Words)
        {
            switch (word.Op)
            {
                case AlignOp.Match:
                    match++;
                    break;
                case AlignOp.Sub:
                    substitution++;
                    break;
                case AlignOp.Ins:
                    insertion++;
                    break;
                case AlignOp.Del:
                    deletion++;
                    break;
            }
        }

        return new WordTallies(match, substitution, insertion, deletion, tx.Words.Count);
    }

    private static string BuildTextReport(
        SourceInfo info,
        IReadOnlyList<SentenceView> sentences,
        IReadOnlyList<ParagraphView> paragraphs,
        WordTallies? wordTallies,
        bool allErrors,
        int topSentences,
        int topParagraphs)
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
            var avgWer = sentences.Average(s => s.Metrics.Wer);
            var maxWer = sentences.Max(s => s.Metrics.Wer);
            var flagged = sentences.Count(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase));

            builder.AppendLine($"Sentences : {sentences.Count} (Avg WER {avgWer:P2}, Max WER {maxWer:P2}, Flagged {flagged})");
        }
        else
        {
            builder.AppendLine("Sentences : 0");
        }

        if (paragraphs.Count > 0)
        {
            var avgWer = paragraphs.Average(p => p.Metrics.Wer);
            var avgCoverage = paragraphs.Average(p => p.Metrics.Coverage);
            builder.AppendLine($"Paragraphs: {paragraphs.Count} (Avg WER {avgWer:P2}, Avg Coverage {avgCoverage:P2})");
        }
        else
        {
            builder.AppendLine("Paragraphs: 0");
        }

        if (wordTallies is not null)
        {
            builder.AppendLine($"Words     : {wordTallies.Total} (Match {wordTallies.Match}, Sub {wordTallies.Substitution}, Ins {wordTallies.Insertion}, Del {wordTallies.Deletion})");
        }

        builder.AppendLine();

        if ( topSentences > 0 && sentences.Count > 0)
        {
            if (allErrors) builder.AppendLine("All sentences by WER:");
            else builder.AppendLine($"Top {Math.Min(topSentences, sentences.Count)} sentences by WER:");

            var sentencesOrdered = sentences
                .OrderByDescending(s => s.Metrics.Wer)
                .ThenByDescending(s => s.Metrics.Cer);
            
            var sentenceBucket = allErrors ? sentencesOrdered.Where(s => !s.Status.Equals("ok", StringComparison.OrdinalIgnoreCase)) : sentencesOrdered.Take(topSentences);

            foreach (var sentence in sentenceBucket)
            {
                builder.AppendLine($"  #{sentence.Id} | WER {sentence.Metrics.Wer:P1} | CER {sentence.Metrics.Cer:P1} | Status {sentence.Status}");
                builder.AppendLine($"    Book range: {sentence.BookRange.Start}-{sentence.BookRange.End}");

                if (sentence.ScriptRange is not null)
                {
                    builder.AppendLine($"    Script range: {sentence.ScriptRange.Value.Start}-{sentence.ScriptRange.Value.End}");
                }

                if (!string.IsNullOrWhiteSpace(sentence.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(sentence.BookText)}");
                }

                if (!string.IsNullOrWhiteSpace(sentence.ScriptText))
                {
                    builder.AppendLine($"    Script : {TrimText(sentence.ScriptText)}");
                }

                builder.AppendLine();
            }
        }

        var paragraphOrdered = paragraphs
            .OrderByDescending(p => p.Metrics.Wer)
            .ThenByDescending(p => p.Metrics.Coverage);
        
        var paragraphBucket = allErrors ? paragraphOrdered.Where(p => !p.Status.Equals("ok", StringComparison.OrdinalIgnoreCase)) : paragraphOrdered.Take(topParagraphs);

        if (topParagraphs > 0 && paragraphs.Count > 0)
        {
            if (allErrors) builder.AppendLine("All paragraphs by WER:");
            else builder.AppendLine($"Top {Math.Min(topParagraphs, paragraphs.Count)} paragraphs by WER:");

            foreach (var paragraph in paragraphBucket)
            {
                builder.AppendLine($"  #{paragraph.Id} | WER {paragraph.Metrics.Wer:P1} | Coverage {paragraph.Metrics.Coverage:P1} | Status {paragraph.Status}");
                builder.AppendLine($"    Book range: {paragraph.BookRange.Start}-{paragraph.BookRange.End}");

                if (!string.IsNullOrWhiteSpace(paragraph.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(paragraph.BookText)}");
                }

                builder.AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string TrimText(string text, int maxLength = 160)
    {
        var normalized = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength].TrimEnd() + "…";
    }

    private sealed record SourceInfo(string AudioPath, string ScriptPath, string BookIndexPath, DateTime CreatedAtUtc);

    private sealed record SentenceView(
        int Id,
        (int Start, int End) BookRange,
        (int? Start, int? End)? ScriptRange,
        SentenceMetrics Metrics,
        string Status,
        string? BookText,
        string? ScriptText);

    private sealed record ParagraphView(
        int Id,
        (int Start, int End) BookRange,
        ParagraphMetrics Metrics,
        string Status,
        string? BookText);

    private sealed record WordTallies(int Match, int Substitution, int Insertion, int Deletion, int Total);

    private sealed record HydratedTranscript(
        string AudioPath,
        string ScriptPath,
        string BookIndexPath,
        DateTime CreatedAtUtc,
        string? NormalizationVersion,
        IReadOnlyList<HydratedWord> Words,
        IReadOnlyList<HydratedSentence> Sentences,
        IReadOnlyList<HydratedParagraph> Paragraphs);

    private sealed record HydratedWord(int? BookIdx, int? AsrIdx, string? BookWord, string? AsrWord, string Op, string Reason, double Score);

    private sealed record HydratedSentence(
        int Id,
        HydratedRange BookRange,
        HydratedScriptRange? ScriptRange,
        string BookText,
        string ScriptText,
        SentenceMetrics Metrics,
        string Status);

    private sealed record HydratedParagraph(
        int Id,
        HydratedRange BookRange,
        IReadOnlyList<int> SentenceIds,
        string BookText,
        ParagraphMetrics Metrics,
        string Status);

    private sealed record HydratedRange(int Start, int End);

    private sealed record HydratedScriptRange(int? Start, int? End);
}
