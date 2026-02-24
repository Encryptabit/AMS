using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Core.Processors.Diffing;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for hydrating transcript index into a fully enriched transcript with diffs and metrics.
/// </summary>
public sealed class TranscriptHydrationService : ITranscriptHydrationService
{
    private readonly IPronunciationProvider _pronunciationProvider;
    private static readonly TokenPhonemeView EmptyTokenPhonemeView = new(
        Array.Empty<string>(),
        Array.Empty<string[]?>());

    public TranscriptHydrationService(IPronunciationProvider? pronunciationProvider = null)
    {
        _pronunciationProvider = pronunciationProvider ?? NullPronunciationProvider.Instance;
    }

    public async Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var transcript = context.Documents.Transcript ??
                         throw new InvalidOperationException("TranscriptIndex is not loaded for this chapter.");
        var hydrate = await BuildHydratedTranscriptAsync(context, transcript, cancellationToken).ConfigureAwait(false);
        context.Documents.HydratedTranscript = hydrate;
        return hydrate;
    }

    private async Task<HydratedTranscript> BuildHydratedTranscriptAsync(
        ChapterContext context,
        TranscriptIndex transcript,
        CancellationToken cancellationToken)
    {
        var book = context.Book.Documents.BookIndex ?? throw new InvalidOperationException("BookIndex is not loaded.");
        var asr = context.Documents.Asr ?? throw new InvalidOperationException("ASR document is not loaded.");

        var fallbackPronunciations =
            await BuildPronunciationFallbackAsync(book, asr, transcript, cancellationToken).ConfigureAwait(false);

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

        var sentences = new List<HydratedSentence>(transcript.Sentences.Count);
        var sentenceScoringMap = new Dictionary<int, TokenPhonemeView>(transcript.Sentences.Count);
        foreach (var sentence in transcript.Sentences)
        {
            var bookText = JoinBook(book, sentence.BookRange.Start, sentence.BookRange.End);
            var scriptRange = sentence.ScriptRange is null
                ? null
                : new HydratedScriptRange(sentence.ScriptRange.Start, sentence.ScriptRange.End);
            var scriptText = sentence.ScriptRange is null
                ? string.Empty
                : JoinAsr(asr, sentence.ScriptRange.Start, sentence.ScriptRange.End);

            var bookScoringView = BuildBookScoringView(
                book,
                sentence.BookRange.Start,
                sentence.BookRange.End,
                fallbackPronunciations);
            var scriptScoringView = sentence.ScriptRange is null
                ? EmptyTokenPhonemeView
                : BuildAsrScoringView(
                    asr,
                    sentence.ScriptRange.Start,
                    sentence.ScriptRange.End,
                    fallbackPronunciations);

            sentenceScoringMap[sentence.Id] = scriptScoringView;

            var diffResult = TextDiffAnalyzer.Analyze(
                bookText,
                scriptText,
                BuildPhonemeAwareScoringOptions(bookScoringView, scriptScoringView));
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
            var paragraphScriptScoring = BuildParagraphScoringView(paragraph.SentenceIds, sentenceScoringMap);
            var paragraphBookScoring = BuildBookScoringView(
                book,
                paragraph.BookRange.Start,
                paragraph.BookRange.End,
                fallbackPronunciations);

            var diffResult = TextDiffAnalyzer.Analyze(
                bookText,
                paragraphScript,
                BuildPhonemeAwareScoringOptions(paragraphBookScoring, paragraphScriptScoring));
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

    private static TextDiffScoringOptions BuildPhonemeAwareScoringOptions(
        TokenPhonemeView referenceView,
        TokenPhonemeView hypothesisView)
    {
        return new TextDiffScoringOptions(
            ReferenceTokens: referenceView.Tokens,
            HypothesisTokens: hypothesisView.Tokens,
            ReferencePhonemeVariants: referenceView.Phonemes,
            HypothesisPhonemeVariants: hypothesisView.Phonemes,
            UseExactPhonemeEquivalence: true);
    }

    private async Task<IReadOnlyDictionary<string, string[]>> BuildPronunciationFallbackAsync(
        BookIndex book,
        AsrResponse asr,
        TranscriptIndex transcript,
        CancellationToken cancellationToken)
    {
        var lookups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var word in asr.Words)
        {
            var lexeme = PronunciationHelper.NormalizeForLookup(word);
            if (!string.IsNullOrEmpty(lexeme))
            {
                lookups.Add(lexeme);
            }
        }

        foreach (var sentence in transcript.Sentences)
        {
            var start = Math.Max(0, sentence.BookRange.Start);
            var end = Math.Min(book.Words.Length - 1, sentence.BookRange.End);
            if (end < start)
            {
                continue;
            }

            for (int i = start; i <= end; i++)
            {
                var word = book.Words[i];
                if (word.Phonemes is { Length: > 0 })
                {
                    continue;
                }

                var lexeme = PronunciationHelper.NormalizeForLookup(word.Text);
                if (!string.IsNullOrEmpty(lexeme))
                {
                    lookups.Add(lexeme);
                }
            }
        }

        if (lookups.Count == 0)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            return await _pronunciationProvider.GetPronunciationsAsync(lookups, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Debug("Phoneme fallback lookup failed during hydrate scoring; continuing text-only ({Message})",
                ex.Message);
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static TokenPhonemeView BuildBookScoringView(
        BookIndex book,
        int start,
        int end,
        IReadOnlyDictionary<string, string[]> fallbackPronunciations)
    {
        if (start < 0 || end < start || start >= book.Words.Length)
        {
            return EmptyTokenPhonemeView;
        }

        end = Math.Min(end, book.Words.Length - 1);
        var tokens = new List<string>(Math.Max(0, end - start + 1));
        var phonemes = new List<string[]?>(Math.Max(0, end - start + 1));

        for (int i = start; i <= end; i++)
        {
            var word = book.Words[i];
            var normalized = TextNormalizer.Normalize(word.Text, expandContractions: true, removeNumbers: false);
            var wordTokens = TextNormalizer.TokenizeWords(normalized);
            if (wordTokens.Length == 0)
            {
                continue;
            }

            string[]? variants = null;
            if (wordTokens.Length == 1)
            {
                variants = ResolveBookWordPhonemes(word, fallbackPronunciations);
            }

            foreach (var token in wordTokens)
            {
                tokens.Add(token);
                phonemes.Add(variants);
            }
        }

        return new TokenPhonemeView(tokens, phonemes);
    }

    private static TokenPhonemeView BuildAsrScoringView(
        AsrResponse asr,
        int? start,
        int? end,
        IReadOnlyDictionary<string, string[]> pronunciations)
    {
        if (!start.HasValue || !end.HasValue)
        {
            return EmptyTokenPhonemeView;
        }

        int s = Math.Max(0, start.Value);
        int e = Math.Min(asr.WordCount - 1, end.Value);
        if (e < s)
        {
            return EmptyTokenPhonemeView;
        }

        var tokens = new List<string>(Math.Max(0, e - s + 1));
        var phonemes = new List<string[]?>(Math.Max(0, e - s + 1));

        for (int i = s; i <= e; i++)
        {
            var word = asr.GetWord(i);
            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            var normalized = TextNormalizer.Normalize(word, expandContractions: true, removeNumbers: false);
            var wordTokens = TextNormalizer.TokenizeWords(normalized);
            if (wordTokens.Length == 0)
            {
                continue;
            }

            string[]? variants = null;
            if (wordTokens.Length == 1)
            {
                var lexeme = PronunciationHelper.NormalizeForLookup(word);
                if (!string.IsNullOrEmpty(lexeme) && pronunciations.TryGetValue(lexeme, out var mapped) &&
                    mapped.Length > 0)
                {
                    variants = mapped;
                }
            }

            foreach (var token in wordTokens)
            {
                tokens.Add(token);
                phonemes.Add(variants);
            }
        }

        return new TokenPhonemeView(tokens, phonemes);
    }

    private static string[]? ResolveBookWordPhonemes(
        BookWord word,
        IReadOnlyDictionary<string, string[]> fallbackPronunciations)
    {
        if (word.Phonemes is { Length: > 0 })
        {
            return word.Phonemes;
        }

        var lexeme = PronunciationHelper.NormalizeForLookup(word.Text);
        if (string.IsNullOrEmpty(lexeme))
        {
            return null;
        }

        return fallbackPronunciations.TryGetValue(lexeme, out var mapped) && mapped.Length > 0
            ? mapped
            : null;
    }

    private static TokenPhonemeView BuildParagraphScoringView(
        IReadOnlyList<int> sentenceIds,
        IReadOnlyDictionary<int, TokenPhonemeView> sentenceScoringViews)
    {
        if (sentenceIds.Count == 0)
        {
            return EmptyTokenPhonemeView;
        }

        var tokenCountEstimate = 0;
        foreach (var id in sentenceIds)
        {
            if (sentenceScoringViews.TryGetValue(id, out var view))
            {
                tokenCountEstimate += view.Tokens.Count;
            }
        }

        var tokens = new List<string>(tokenCountEstimate);
        var phonemes = new List<string[]?>(tokenCountEstimate);

        foreach (var id in sentenceIds)
        {
            if (!sentenceScoringViews.TryGetValue(id, out var view) || view.Tokens.Count == 0)
            {
                continue;
            }

            tokens.AddRange(view.Tokens);
            phonemes.AddRange(view.Phonemes);
        }

        return new TokenPhonemeView(tokens, phonemes);
    }

    private static string NormalizeSurface(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = TextNormalizer.NormalizeTypography(text);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized.Trim();
    }

    private static string JoinBook(BookIndex book, int start, int end)
    {
        if (start < 0 || end >= book.Words.Length || end < start)
        {
            return string.Empty;
        }

        var raw = string.Join(" ", book.Words.Skip(start).Take(end - start + 1).Select(x => x.Text));
        return NormalizeSurface(raw);
    }

    private static string JoinAsr(AsrResponse asr, int? start, int? end)
    {
        if (!start.HasValue || !end.HasValue)
        {
            return string.Empty;
        }

        int s = start.Value;
        int e = end.Value;
        if (s < 0 || e >= asr.WordCount || e < s)
        {
            return string.Empty;
        }

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

    private static string ResolveSentenceStatus(SentenceMetrics metrics)
        => metrics.Wer <= 0.10 && metrics.MissingRuns < 3
            ? "ok"
            : (metrics.Wer <= 0.25 ? "attention" : "unreliable");

    private static string ResolveParagraphStatus(double wer)
        => wer <= 0.10 ? "ok" : (wer <= 0.25 ? "attention" : "unreliable");

    private static string BuildParagraphScript(
        IReadOnlyList<int> sentenceIds,
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

    private sealed record TokenPhonemeView(
        IReadOnlyList<string> Tokens,
        IReadOnlyList<string[]?> Phonemes);
}
