using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Processors.Alignment.Tx;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for building transcript index documents from book text and ASR output.
/// </summary>
public sealed class TranscriptIndexService : ITranscriptIndexService
{
    private readonly IPronunciationProvider _pronunciationProvider;
    private static readonly ILogger Logger = Log.For<TranscriptIndexService>();

    public TranscriptIndexService(IPronunciationProvider? pronunciationProvider = null)
    {
        _pronunciationProvider = pronunciationProvider ?? NullPronunciationProvider.Instance;
    }

    /// <inheritdoc />
    public async Task<TranscriptIndex> BuildTranscriptIndexAsync(
        ChapterContext context,
        TranscriptBuildOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var (book, asr) = RequireBookAndAsr(context);
        var opts = options ?? new TranscriptBuildOptions();
        var anchorOpts = opts.AnchorOptions ?? new AnchorComputationOptions();

        var bookView = AnchorPreprocessor.BuildBookView(book);
        var asrView = AnchorPreprocessor.BuildAsrView(asr);
        var policy = BuildPolicy(anchorOpts);
        var sectionOverride = context.GetOrResolveSection(book, anchorOpts, stage: "transcript", Logger);
        var sectionOptions = new SectionDetectOptions(
            Detect: anchorOpts.DetectSection && sectionOverride is null,
            AsrPrefixTokens: anchorOpts.AsrPrefixTokens);

        var pipeline = AnchorPipeline.ComputeAnchors(
            book,
            asr,
            policy,
            sectionOptions,
            includeWindows: true,
            overrideSection: sectionOverride);

        if (sectionOverride is not null)
        {
            Logger.LogInformation(
                "Section override applied for {ChapterId}: {Title} (Id={Id}, Words={StartWord}-{EndWord})",
                context.Descriptor.ChapterId,
                sectionOverride.Title,
                sectionOverride.Id,
                sectionOverride.StartWord,
                sectionOverride.EndWord);
            context.SetDetectedSection(sectionOverride);
        }
        else if (pipeline.SectionDetected && pipeline.Section is not null)
        {
            context.SetDetectedSection(pipeline.Section);
            Logger.LogInformation(
                "Section auto-detected for {ChapterId}: {Title} (Id={Id}, Words={StartWord}-{EndWord})",
                context.Descriptor.ChapterId,
                pipeline.Section.Title,
                pipeline.Section.Id,
                pipeline.Section.StartWord,
                pipeline.Section.EndWord);
        }
        else
        {
            Logger.LogInformation(
                "No section selected for {ChapterId}; aligning against full book",
                context.Descriptor.ChapterId);
        }

        var windows = pipeline.Windows;
        if (windows is null || windows.Count == 0)
        {
            windows = BuildFallbackWindows(pipeline, asrView.Tokens.Count, policy);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var bookPhonemes = BuildBookPhonemeView(book, pipeline.BookFilteredToOriginalWord, bookView.Tokens.Count);
        var asrPhonemes = await BuildAsrPhonemeViewAsync(asr, asrView, cancellationToken).ConfigureAwait(false);

        var (wordOps, anchorOps) =
            BuildWordOperations(pipeline, policy, book, asrView, windows, bookPhonemes, asrPhonemes);
        var (sentences, paragraphs) = BuildRollups(book, asr, pipeline, wordOps, anchorOps);
        var timedSentences = sentences
            .Select(s => s with { Timing = ComputeTiming(s.ScriptRange, asr) })
            .ToList();

        var audioPath = opts.AudioPath ?? ResolveDefaultAudioPath(context);
        var scriptPath = opts.ScriptPath ?? audioPath;
        var bookIndexPath = opts.BookIndexPath ?? ResolveDefaultBookIndexPath(context.Book);

        var transcript = new TranscriptIndex(
            AudioPath: audioPath,
            ScriptPath: scriptPath,
            BookIndexPath: bookIndexPath,
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "v1",
            Words: wordOps,
            Sentences: timedSentences,
            Paragraphs: paragraphs);

        context.Documents.Transcript = transcript;
        context.Documents.Anchors = BuildAnchorDocument(pipeline, anchorOpts);
        return transcript;
    }

    private static (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(ChapterContext context)
    {
        var book = context.Book.Documents.BookIndex ?? throw new InvalidOperationException("BookIndex is not loaded.");
        var asr = context.Documents.Asr ?? throw new InvalidOperationException("ASR document is not loaded.");
        return (book, asr);
    }

    private static AnchorPolicy BuildPolicy(AnchorComputationOptions options)
    {
        var stopwords = options.UseDomainStopwords
            ? StopwordSets.EnglishPlusDomain
            : new HashSet<string>(StringComparer.Ordinal);

        return new AnchorPolicy(
            NGram: options.NGram,
            TargetPerTokens: options.TargetPerTokens,
            AllowDuplicates: false,
            MinSeparation: options.MinSeparation,
            Stopwords: stopwords,
            DisallowBoundaryCross: !options.AllowBoundaryCross);
    }

    private static AnchorDocument BuildAnchorDocument(AnchorPipelineResult pipeline, AnchorComputationOptions options)
    {
        var anchors = pipeline.Anchors.Select(a => new AnchorDocumentAnchor(
            BookPosition: a.Bp,
            BookWordIndex: a.Bp >= 0 && a.Bp < pipeline.BookFilteredToOriginalWord.Count
                ? pipeline.BookFilteredToOriginalWord[a.Bp]
                : -1,
            AsrPosition: a.Ap)).ToList();

        var windows = pipeline.Windows?.Select(w => new AnchorDocumentWindowSegment(w.bLo, w.bHi, w.aLo, w.aHi))
            .ToList();

        var document = new AnchorDocument(
            SectionDetected: pipeline.SectionDetected,
            Section: pipeline.Section is null
                ? null
                : new AnchorDocumentSection(
                    pipeline.Section.Id,
                    pipeline.Section.Title,
                    pipeline.Section.Level,
                    pipeline.Section.Kind,
                    pipeline.Section.StartWord,
                    pipeline.Section.EndWord),
            Policy: new AnchorDocumentPolicy(
                NGram: options.NGram,
                TargetPerTokens: options.TargetPerTokens,
                MinSeparation: options.MinSeparation,
                DisallowBoundaryCross: !options.AllowBoundaryCross,
                Stopwords: options.UseDomainStopwords ? "domain" : "none"),
            Tokens: new AnchorDocumentTokenStats(
                pipeline.BookTokenCount,
                pipeline.BookFilteredCount,
                pipeline.AsrTokenCount,
                pipeline.AsrFilteredCount),
            Window: new AnchorDocumentWindow(
                pipeline.BookWindowFiltered.bStart,
                pipeline.BookWindowFiltered.bEnd),
            Anchors: anchors,
            Windows: windows);

        return document;
    }

    private static (List<WordAlign> WordOps, List<WordAlign> AnchorOps) BuildWordOperations(
        AnchorPipelineResult pipeline,
        AnchorPolicy policy,
        BookIndex book,
        AsrAnchorView asrView,
        IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows,
        string[][] bookPhonemes,
        string[][] asrPhonemes)
    {
        var bookView = AnchorPreprocessor.BuildBookView(book);
        var equiv = new Dictionary<string, string>(StringComparer.Ordinal);
        var fillers = new HashSet<string>(new[] { "uh", "um", "erm", "uhh", "hmm", "mm", "huh", "like" },
            StringComparer.Ordinal);

        var opsNm = TranscriptAligner.AlignWindows(
            bookView.Tokens,
            asrView.Tokens,
            windows,
            equiv,
            fillers,
            bookPhonemes,
            asrPhonemes);

        var anchorOps = new List<WordAlign>(pipeline.Anchors.Count * policy.NGram);
        var anchorSeen = new HashSet<(int? BookIdx, int? AsrIdx, AlignOp Op)>();
        foreach (var anchor in pipeline.Anchors)
        {
            for (int k = 0; k < policy.NGram; k++)
            {
                int bookFiltered = anchor.Bp + k;
                if (bookFiltered < 0 || bookFiltered >= pipeline.BookFilteredToOriginalWord.Count)
                {
                    continue;
                }

                int bookIdx = pipeline.BookFilteredToOriginalWord[bookFiltered];
                int? asrIdx = null;
                int asrFiltered = anchor.Ap + k;
                if (asrFiltered >= 0 && asrFiltered < asrView.FilteredToOriginalToken.Count)
                {
                    asrIdx = asrView.FilteredToOriginalToken[asrFiltered];
                }

                var key = ((int?)bookIdx, asrIdx, AlignOp.Match);
                if (anchorSeen.Add(key))
                {
                    anchorOps.Add(new WordAlign(bookIdx, asrIdx, AlignOp.Match, "anchor", 0.0));
                }
            }
        }

        var dpOps = new List<WordAlign>(opsNm.Count);
        foreach (var (bi, aj, op, reason, score) in opsNm)
        {
            int? bookIdx = bi.HasValue ? pipeline.BookFilteredToOriginalWord[bi.Value] : (int?)null;
            int? asrIdx = aj.HasValue ? asrView.FilteredToOriginalToken[aj.Value] : (int?)null;
            dpOps.Add(new WordAlign(bookIdx, asrIdx, op, reason, score));
        }

        var combinedOps = anchorOps
            .OrderBy(op => op.BookIdx)
            .ThenBy(op => op.AsrIdx ?? int.MaxValue)
            .Concat(dpOps)
            .ToList();

        var seenOps = new HashSet<(int? BookIdx, int? AsrIdx, AlignOp Op)>();
        var wordOps = new List<WordAlign>(combinedOps.Count);
        foreach (var op in combinedOps)
        {
            if (seenOps.Add((op.BookIdx, op.AsrIdx, op.Op)))
            {
                wordOps.Add(op);
            }
        }

        return (wordOps, anchorOps);
    }

    private static (IReadOnlyList<SentenceAlign> Sentences, IReadOnlyList<ParagraphAlign> Paragraphs) BuildRollups(
        BookIndex book,
        AsrResponse asr,
        AnchorPipelineResult pipeline,
        IReadOnlyList<WordAlign> wordOps,
        IReadOnlyList<WordAlign> anchorOps)
    {
        int secStartWord = 0;
        int secEndWord = book.Words.Length - 1;
        if (pipeline.Section != null)
        {
            secStartWord = Math.Max(0, pipeline.Section.StartWord);
            secEndWord = Math.Min(book.Words.Length - 1, pipeline.Section.EndWord);
        }
        else
        {
            static bool IsAlignedWord(WordAlign op) =>
                op.BookIdx.HasValue && op.AsrIdx.HasValue && op.Op != AlignOp.Del;

            var matchedBookIdx = new List<int>();
            matchedBookIdx.AddRange(wordOps.Where(IsAlignedWord).Select(o => o.BookIdx!.Value));
            matchedBookIdx.AddRange(anchorOps.Where(a => a.BookIdx.HasValue).Select(a => a.BookIdx!.Value));
            matchedBookIdx.Sort();

            if (matchedBookIdx.Count > 0)
            {
                secStartWord = matchedBookIdx.First();
                secEndWord = matchedBookIdx.Last();

                var firstSentence =
                    book.Sentences.FirstOrDefault(s => s.Start <= secStartWord && s.End >= secStartWord);
                if (firstSentence != null)
                {
                    secStartWord = firstSentence.Start;
                }

                var lastSentence = book.Sentences.LastOrDefault(s => s.Start <= secEndWord && s.End >= secEndWord);
                if (lastSentence != null)
                {
                    secEndWord = lastSentence.End;
                }

                secStartWord = Math.Max(0, secStartWord);
                secEndWord = Math.Min(book.Words.Length - 1, Math.Max(secStartWord, secEndWord));
            }
        }

        var sentTuples = book.Sentences
            .Where(s => s.Start <= secEndWord && s.End >= secStartWord)
            .Select(s => (s.Index, Math.Max(secStartWord, s.Start), Math.Min(secEndWord, s.End)))
            .ToList();
        var paraTuples = book.Paragraphs
            .Where(p => p.Start <= secEndWord && p.End >= secStartWord)
            .Select(p => (p.Index, Math.Max(secStartWord, p.Start), Math.Min(secEndWord, p.End)))
            .ToList();

        return TranscriptAligner.Rollup(
            wordOps,
            sentTuples.Select(t => (t.Index, t.Item2, t.Item3)).ToList(),
            paraTuples.Select(t => (t.Index, t.Item2, t.Item3)).ToList(),
            book,
            asr);
    }

    private static string[][] BuildBookPhonemeView(BookIndex book, IReadOnlyList<int> filteredToOriginal,
        int filteredCount)
    {
        var result = new string[filteredCount][];
        for (int i = 0; i < filteredCount; i++)
        {
            var originalIndex = filteredToOriginal[i];
            if (originalIndex >= 0 && originalIndex < book.Words.Length)
            {
                result[i] = book.Words[originalIndex].Phonemes ?? Array.Empty<string>();
            }
            else
            {
                result[i] = Array.Empty<string>();
            }
        }

        return result;
    }

    private async Task<string[][]> BuildAsrPhonemeViewAsync(
        AsrResponse asr,
        AsrAnchorView asrView,
        CancellationToken cancellationToken)
    {
        var pronunciations = await _pronunciationProvider.GetPronunciationsAsync(
            asr.Words,
            cancellationToken).ConfigureAwait(false);

        var result = new string[asrView.Tokens.Count][];
        for (int i = 0; i < asrView.Tokens.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var originalIndex = asrView.FilteredToOriginalToken[i];
            if (originalIndex < 0 || originalIndex >= asr.WordCount)
            {
                result[i] = Array.Empty<string>();
                continue;
            }

            var word = asr.GetWord(originalIndex);
            var lexeme = PronunciationHelper.NormalizeForLookup(word ?? string.Empty);
            if (!string.IsNullOrEmpty(lexeme) && pronunciations.TryGetValue(lexeme, out var variants) &&
                variants.Length > 0)
            {
                result[i] = variants;
            }
            else
            {
                result[i] = Array.Empty<string>();
            }
        }

        return result;
    }

    private static IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> BuildFallbackWindows(
        AnchorPipelineResult pipeline,
        int asrTokenCount,
        AnchorPolicy policy)
    {
        if (pipeline.Anchors.Count == 0)
        {
            return new List<(int, int, int, int)>
            {
                (pipeline.BookWindowFiltered.bStart, pipeline.BookWindowFiltered.bEnd + 1, 0, asrTokenCount)
            };
        }

        int minBp = pipeline.Anchors.Min(a => a.Bp);
        int maxBp = pipeline.Anchors.Max(a => a.Bp + policy.NGram - 1);
        int bookSpan = Math.Max(0, maxBp - minBp + 1);
        int bookPad = Math.Max(64, Math.Min(8192, Math.Max(policy.NGram * 2, bookSpan / 5)));
        int bookStart = Math.Max(pipeline.BookWindowFiltered.bStart, minBp - bookPad);
        int bookEndExclusive = Math.Min(pipeline.BookWindowFiltered.bEnd + 1, maxBp + bookPad + 1);

        int minAp = pipeline.Anchors.Min(a => a.Ap);
        int maxAp = pipeline.Anchors.Max(a => a.Ap);
        int asrSpan = Math.Max(0, maxAp - minAp + 1);
        int asrPad = Math.Max(32, Math.Min(4096, Math.Max(policy.NGram * 2, asrSpan / 5)));
        int asrStart = Math.Max(0, minAp - asrPad);
        int asrEndExclusive = Math.Min(asrTokenCount, maxAp + asrPad + 1);

        if (bookEndExclusive <= bookStart)
        {
            bookEndExclusive = Math.Min(pipeline.BookWindowFiltered.bEnd + 1,
                bookStart + Math.Max(1, bookSpan + bookPad));
        }

        if (asrEndExclusive <= asrStart)
        {
            asrEndExclusive = Math.Min(asrTokenCount, asrStart + Math.Max(1, asrSpan + asrPad));
        }

        return new List<(int, int, int, int)>
        {
            (bookStart, bookEndExclusive, asrStart, asrEndExclusive)
        };
    }

    private static TimingRange ComputeTiming(ScriptRange? scriptRange, AsrResponse asr)
    {
        if (!asr.HasWordTimings || scriptRange?.Start is not int start || scriptRange.End is not int end)
        {
            return TimingRange.Empty;
        }

        start = Math.Clamp(start, 0, asr.Tokens.Length - 1);
        end = Math.Clamp(end, start, asr.Tokens.Length - 1);

        var startToken = asr.Tokens[start];
        var endToken = asr.Tokens[end];
        var startSec = startToken.StartTime;
        var endSec = endToken.StartTime + endToken.Duration;

        return new TimingRange(startSec, endSec);
    }

    private static string ResolveDefaultAudioPath(ChapterContext context)
    {
        var descriptor = context.Descriptor.AudioBuffers.FirstOrDefault();
        if (descriptor is not null && !string.IsNullOrWhiteSpace(descriptor.Path))
        {
            return descriptor.Path;
        }

        return Path.Combine(context.Descriptor.RootPath, $"{context.Descriptor.ChapterId}.wav");
    }

    private static string ResolveDefaultBookIndexPath(BookContext bookContext)
        => Path.Combine(bookContext.Descriptor.RootPath, "book-index.json");
}
