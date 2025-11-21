using Ams.Core.Artifacts.Hydrate;
using Ams.Web.Shared.ValidationViewer;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

internal sealed class ValidationViewerService : IValidationViewerService
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly ILogger<ValidationViewerService> _logger;
    private readonly WorkspaceResolver _resolver;

    public ValidationViewerService(ValidationViewerWorkspaceState state, WorkspaceResolver resolver, ILogger<ValidationViewerService> logger)
    {
        _state = state;
        _resolver = resolver;
        _logger = logger;
    }

    public IReadOnlyList<ValidationChapterSummary> GetChapters(string bookId)
    {
        var book = _resolver.ResolveBook(bookId);
        var summaries = new List<ValidationChapterSummary>();

        foreach (var descriptor in book.Chapters.Descriptors)
        {
            var chapter = book.Chapters.Load(descriptor.ChapterId);
            var hydrate = chapter.Documents.HydratedTranscript;
            if (hydrate is null) continue;

            var metrics = BuildMetrics(hydrate);
            summaries.Add(new ValidationChapterSummary(
                descriptor.ChapterId,
                descriptor.RootPath,
                metrics));
        }

        return summaries
            .OrderBy(c => NaturalSortKey(c.Name))
            .ToList();
    }

    public ValidationOverviewResponse GetOverview(string bookId)
    {
        var chapters = GetChapters(bookId);
        var totalSentences = chapters.Sum(c => c.Metrics.SentenceCount);
        var totalFlaggedSentences = chapters.Sum(c => c.Metrics.SentenceFlagged);
        var totalParagraphs = chapters.Sum(c => c.Metrics.ParagraphCount);
        var totalFlaggedParagraphs = chapters.Sum(c => c.Metrics.ParagraphFlagged);

        var avgSentenceWer = totalSentences > 0
            ? chapters.Where(c => c.Metrics.SentenceCount > 0)
                .Sum(c => c.Metrics.SentenceCount * double.Parse(c.Metrics.SentenceAvgWer.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture)) / totalSentences
            : 0;

        var avgParagraphWer = totalParagraphs > 0
            ? chapters.Where(c => c.Metrics.ParagraphCount > 0)
                .Sum(c => c.Metrics.ParagraphCount * double.Parse(c.Metrics.ParagraphAvgWer.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture)) / totalParagraphs
            : 0;

        return new ValidationOverviewResponse(
            _resolver.ResolveBookRoot(bookId).Name,
            chapters.Count,
            totalSentences,
            totalFlaggedSentences,
            $"{avgSentenceWer:F2}%",
            totalParagraphs,
            totalFlaggedParagraphs,
            $"{avgParagraphWer:F2}%",
            chapters);
    }

    public ValidationReportResponse? GetReport(string bookId, string chapterId)
    {
        using var handle = _resolver.OpenChapter(bookId, chapterId);
        if (handle is null)
        {
            return null;
        }

        var hydrate = handle.Chapter.Documents.HydratedTranscript;
        if (hydrate is null) return null;

        return BuildReport(handle.Chapter.Descriptor.ChapterId, hydrate);
    }

    #region helpers

    private ValidationChapterMetrics BuildMetrics(HydratedTranscript hydrate)
    {
        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var paragraphs = hydrate.Paragraphs ?? Array.Empty<HydratedParagraph>();

        var flaggedSentences = sentences.Count(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase));
        var avgWer = sentences.Count > 0 ? sentences.Average(s => s.Metrics.Wer) * 100 : 0;

        var flaggedParagraphs = paragraphs.Count(p => !string.Equals(p.Status, "ok", StringComparison.OrdinalIgnoreCase));
        var avgParagraphWer = paragraphs.Count > 0 ? paragraphs.Average(p => p.Metrics.Wer) * 100 : 0;

        return new ValidationChapterMetrics(
            sentences.Count,
            flaggedSentences,
            $"{avgWer:F2}%",
            paragraphs.Count,
            flaggedParagraphs,
            $"{avgParagraphWer:F2}%");
    }

    private ValidationReportResponse BuildReport(string chapterName, HydratedTranscript hydrate)
    {
        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var paragraphs = hydrate.Paragraphs ?? Array.Empty<HydratedParagraph>();

        var flagged = sentences.Where(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase)).ToList();
        var avgWer = sentences.Count > 0 ? sentences.Average(s => s.Metrics.Wer) * 100 : 0;
        var maxWer = sentences.Count > 0 ? sentences.Max(s => s.Metrics.Wer) * 100 : 0;

        var sentenceToParagraph = new Dictionary<int, int>();
        foreach (var para in paragraphs)
        {
            foreach (var sid in para.SentenceIds)
            {
                sentenceToParagraph[sid] = para.Id;
            }
        }

        var wordOpsBySentence = BuildWordOpsBySentence(hydrate);

        var sentenceDtos = sentences.Select(s =>
        {
            var timing = s.Timing;
            var timingText = timing is null
                ? string.Empty
                : $"{timing.StartSec:0.000}s → {timing.EndSec:0.000}s (Δ {timing.Duration:0.000}s)";

            var bookRange = s.BookRange;
            var scriptRange = s.ScriptRange;

            return new ValidationSentenceResponse(
                s.Id,
                $"{s.Metrics.Wer * 100:0.1}%",
                $"{s.Metrics.Cer * 100:0.1}%",
                s.Status,
                $"{bookRange.Start}-{bookRange.End}",
                scriptRange is null ? string.Empty : $"{scriptRange.Start?.ToString() ?? "0"}-{scriptRange.End?.ToString() ?? "0"}",
                timingText,
                s.BookText,
                s.ScriptText,
                s.BookText.Length > 100 ? s.BookText[..100] : s.BookText,
                s.Diff,
                timing?.StartSec,
                timing?.EndSec,
                bookRange.Start,
                bookRange.End,
                sentenceToParagraph.TryGetValue(s.Id, out var pid) ? pid : null,
                wordOpsBySentence.TryGetValue(s.Id, out var ops) ? ops : null
            );
        }).ToList();

        var stats = new ValidationReportStats(
            sentences.Count.ToString(),
            $"{avgWer:0.2}%",
            $"{maxWer:0.2}%",
            flagged.Count.ToString(),
            paragraphs.Count.ToString(),
            paragraphs.Count > 0 ? $"{paragraphs.Average(p => p.Metrics.Wer) * 100:0.2}%" : "0.00%",
            paragraphs.Count > 0 ? $"{paragraphs.Average(p => p.Metrics.Coverage) * 100:0.2}%" : "0.00%"
        );

        return new ValidationReportResponse(
            chapterName,
            hydrate.AudioPath,
            hydrate.ScriptPath,
            hydrate.BookIndexPath,
            DateTime.UtcNow.ToString("o"),
            stats,
            sentenceDtos,
            new List<ValidationParagraphResponse>());
    }

    private static Dictionary<int, IReadOnlyList<Dictionary<string, string?>>> BuildWordOpsBySentence(HydratedTranscript hydrate)
    {
        var result = new Dictionary<int, IReadOnlyList<Dictionary<string, string?>>>();
        if (hydrate.Words is null || hydrate.Words.Count == 0)
        {
            return result;
        }

        var sentences = hydrate.Sentences ?? Array.Empty<HydratedSentence>();
        var sentenceByBookIdx = new Dictionary<int, int>();
        foreach (var s in sentences)
        {
            var start = Math.Min(s.BookRange.Start, s.BookRange.End);
            var end = Math.Max(s.BookRange.Start, s.BookRange.End);
            for (var i = start; i <= end; i++)
            {
                sentenceByBookIdx[i] = s.Id;
            }
        }

        int? lastSentence = null;
        foreach (var word in hydrate.Words)
        {
            var sentenceId = word.BookIdx.HasValue && sentenceByBookIdx.TryGetValue(word.BookIdx.Value, out var sid)
                ? sid
                : lastSentence;

            if (sentenceId is null)
            {
                continue;
            }

            var entry = new Dictionary<string, string?>
            {
                ["op"] = word.Op,
                ["reason"] = word.Reason,
                ["bookWord"] = word.BookWord?.Trim(),
                ["asrWord"] = word.AsrWord?.Trim()
            };

            if (!result.TryGetValue(sentenceId.Value, out var list))
            {
                result[sentenceId.Value] = new List<Dictionary<string, string?>> { entry };
            }
            else if (list is List<Dictionary<string, string?>> mutable)
            {
                mutable.Add(entry);
            }
            else
            {
                result[sentenceId.Value] = new List<Dictionary<string, string?>>(list) { entry };
            }

            lastSentence = sentenceId;
        }

        return result;
    }

    private static (int Primary, string Value) NaturalSortKey(string name)
    {
        var nums = System.Text.RegularExpressions.Regex.Matches(name, "\\d+")
            .Select(m => int.Parse(m.Value))
            .ToList();
        return nums.Count > 0 ? (nums[0], name.ToLowerInvariant()) : (int.MaxValue, name.ToLowerInvariant());
    }

    #endregion
}
