using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors.Diffing;
using Ams.Core.Common;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;

namespace Ams.Core.Services.Alignment;

public sealed class AlignmentService : IAlignmentService
{
    private readonly IAnchorComputeService _anchorService;
    private readonly ITranscriptIndexService _transcriptService;

    public AlignmentService(
        IPronunciationProvider? pronunciationProvider = null,
        IAnchorComputeService? anchorService = null,
        ITranscriptIndexService? transcriptService = null)
    {
        var provider = pronunciationProvider ?? NullPronunciationProvider.Instance;
        _anchorService = anchorService ?? new AnchorComputeService();
        _transcriptService = transcriptService ?? new TranscriptIndexService(provider);
    }

    public Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _anchorService.ComputeAnchorsAsync(context, options, cancellationToken);
    }

    public Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _transcriptService.BuildTranscriptIndexAsync(context, options, cancellationToken);
    }

    public Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var transcript = context.Documents.Transcript ??
                         throw new InvalidOperationException("TranscriptIndex is not loaded for this chapter.");
        var hydrate = BuildHydratedTranscript(context, transcript);
        context.Documents.HydratedTranscript = hydrate;
        return Task.FromResult(hydrate);
    }

    private static HydratedTranscript BuildHydratedTranscript(ChapterContext context, TranscriptIndex transcript)
    {
        var book = context.Book.Documents.BookIndex ?? throw new InvalidOperationException("BookIndex is not loaded.");
        var asr = context.Documents.Asr ?? throw new InvalidOperationException("ASR document is not loaded.");

        var words = transcript.Words.Select(w => new HydratedWord(
            w.BookIdx,
            w.AsrIdx,
            w.BookIdx.HasValue && w.BookIdx.Value >= 0 && w.BookIdx.Value < book.Words.Length
                ? book.Words[w.BookIdx.Value].Text
                : null,
            w.AsrIdx.HasValue && w.AsrIdx.Value >= 0 && w.AsrIdx.Value < asr.WordCount
                ? asr.GetWord(w.AsrIdx.Value)
                : null,
            w.Op.ToString(),
            w.Reason,
            w.Score)).ToList();

        static string NormalizeSurface(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = TextNormalizer.NormalizeTypography(text);
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized.Trim();
        }

        static string JoinBook(BookIndex book, int start, int end)
        {
            if (start < 0 || end >= book.Words.Length || end < start) return string.Empty;
            var raw = string.Join(" ", book.Words.Skip(start).Take(end - start + 1).Select(x => x.Text));
            return NormalizeSurface(raw);
        }

        static string JoinAsr(AsrResponse asr, int? start, int? end)
        {
            if (!start.HasValue || !end.HasValue) return string.Empty;
            int s = start.Value, e = end.Value;
            if (s < 0 || e >= asr.WordCount || e < s) return string.Empty;
            var collected = new List<string>(Math.Max(0, e - s + 1));
            for (int i = s; i <= e; i++)
            {
                var word = asr.GetWord(i);
                if (!string.IsNullOrWhiteSpace(word))
                {
                    collected.Add(word!);
                }
            }

            var raw = collected.Count == 0 ? string.Empty : string.Join(" ", collected);
            return NormalizeSurface(raw);
        }

        static string ResolveSentenceStatus(SentenceMetrics metrics)
            => metrics.Wer <= 0.10 && metrics.MissingRuns < 3
                ? "ok"
                : (metrics.Wer <= 0.25 ? "attention" : "unreliable");

        static string ResolveParagraphStatus(double wer)
            => wer <= 0.10 ? "ok" : (wer <= 0.25 ? "attention" : "unreliable");

        var sentences = new List<HydratedSentence>(transcript.Sentences.Count);
        foreach (var sentence in transcript.Sentences)
        {
            var bookText = JoinBook(book, sentence.BookRange.Start, sentence.BookRange.End);
            var scriptRange = sentence.ScriptRange is null
                ? null
                : new HydratedScriptRange(sentence.ScriptRange.Start, sentence.ScriptRange.End);
            var scriptText = sentence.ScriptRange is null
                ? string.Empty
                : JoinAsr(asr, sentence.ScriptRange.Start, sentence.ScriptRange.End);

            var diffResult = TextDiffAnalyzer.Analyze(bookText, scriptText);
            var status = ResolveSentenceStatus(diffResult.Metrics);

            sentences.Add(new HydratedSentence(
                sentence.Id,
                new HydratedRange(sentence.BookRange.Start, sentence.BookRange.End),
                scriptRange,
                bookText,
                scriptText,
                diffResult.Metrics,
                status,
                sentence.Timing,
                diffResult.Diff));
        }

        var sentenceMap = sentences.ToDictionary(s => s.Id);
        var paragraphs = new List<HydratedParagraph>(transcript.Paragraphs.Count);

        foreach (var paragraph in transcript.Paragraphs)
        {
            var bookText = JoinBook(book, paragraph.BookRange.Start, paragraph.BookRange.End);
            var paragraphScript = BuildParagraphScript(paragraph.SentenceIds, sentenceMap);
            var diffResult = TextDiffAnalyzer.Analyze(bookText, paragraphScript);
            var status = ResolveParagraphStatus(diffResult.Metrics.Wer);
            var metrics = new ParagraphMetrics(diffResult.Metrics.Wer, diffResult.Metrics.Cer, diffResult.Coverage);

            paragraphs.Add(new HydratedParagraph(
                paragraph.Id,
                new HydratedRange(paragraph.BookRange.Start, paragraph.BookRange.End),
                paragraph.SentenceIds,
                bookText,
                metrics,
                status,
                diffResult.Diff));
        }

        return new HydratedTranscript(
            transcript.AudioPath,
            transcript.ScriptPath,
            transcript.BookIndexPath,
            transcript.CreatedAtUtc,
            transcript.NormalizationVersion,
            words,
            sentences,
            paragraphs);
    }

    private static string BuildParagraphScript(IReadOnlyList<int> sentenceIds,
        IReadOnlyDictionary<int, HydratedSentence> sentenceMap)
    {
        if (sentenceIds.Count == 0)
        {
            return string.Empty;
        }

        var parts = new List<string>(sentenceIds.Count);
        foreach (var id in sentenceIds)
        {
            if (sentenceMap.TryGetValue(id, out var sentence) && !string.IsNullOrWhiteSpace(sentence.ScriptText))
            {
                parts.Add(sentence.ScriptText);
            }
        }

        return parts.Count == 0 ? string.Empty : string.Join(" ", parts);
    }
}
