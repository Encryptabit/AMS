using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Processors.Alignment.Tx;
using Ams.Core.Processors.Diffing;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services.Alignment;

public sealed class AlignmentService : IAlignmentService
{
    private readonly IPronunciationProvider _pronunciationProvider;

    public AlignmentService(IPronunciationProvider? pronunciationProvider = null)
    {
        _pronunciationProvider = pronunciationProvider ?? NullPronunciationProvider.Instance;
    }

    public Task<AnchorDocument> ComputeAnchorsAsync(
        ChapterContext context,
        AnchorComputationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var (book, asr) = RequireBookAndAsr(context);
        var opts = options ?? new AnchorComputationOptions();
        var bookView = AnchorPreprocessor.BuildBookView(book);
        var asrView = AnchorPreprocessor.BuildAsrView(asr);
        var policy = BuildPolicy(opts);
        var sectionOverride = ResolveSectionOverride(context, book, opts);
        var sectionOptions = new SectionDetectOptions(
            Detect: opts.DetectSection && sectionOverride is null,
            AsrPrefixTokens: opts.AsrPrefixTokens);
        var pipeline = AnchorPipeline.ComputeAnchors(
            book,
            asr,
            policy,
            sectionOptions,
            includeWindows: opts.EmitWindows,
            overrideSection: sectionOverride);

        var document = BuildAnchorDocument(pipeline, opts);
        context.Documents.Anchors = document;
        return Task.FromResult(document);
    }

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
        var sectionOverride = ResolveSectionOverride(context, book, anchorOpts);
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

        var windows = pipeline.Windows;
        if (windows is null || windows.Count == 0)
        {
            windows = BuildFallbackWindows(pipeline, asrView.Tokens.Count, policy);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var bookPhonemes = BuildBookPhonemeView(book, pipeline.BookFilteredToOriginalWord, bookView.Tokens.Count);
        var asrPhonemes = await BuildAsrPhonemeViewAsync(asr, asrView, cancellationToken).ConfigureAwait(false);

        var (wordOps, anchorOps) = BuildWordOperations(pipeline, policy, book, asrView, windows, bookPhonemes, asrPhonemes);
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

    public Task<HydratedTranscript> HydrateTranscriptAsync(
        ChapterContext context,
        HydrationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var transcript = context.Documents.Transcript ?? throw new InvalidOperationException("TranscriptIndex is not loaded for this chapter.");
        var hydrate = BuildHydratedTranscript(context, transcript);
        context.Documents.HydratedTranscript = hydrate;
        return Task.FromResult(hydrate);
    }

    private (BookIndex Book, AsrResponse Asr) RequireBookAndAsr(ChapterContext context)
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

    private AnchorDocument BuildAnchorDocument(AnchorPipelineResult pipeline, AnchorComputationOptions options)
    {
        var anchors = pipeline.Anchors.Select(a => new AnchorDocumentAnchor(
            BookPosition: a.Bp,
            BookWordIndex: a.Bp >= 0 && a.Bp < pipeline.BookFilteredToOriginalWord.Count
                ? pipeline.BookFilteredToOriginalWord[a.Bp]
                : -1,
            AsrPosition: a.Ap)).ToList();

        var windows = pipeline.Windows?.Select(w => new AnchorDocumentWindowSegment(w.bLo, w.bHi, w.aLo, w.aHi)).ToList();

        var document = new AnchorDocument(
            SectionDetected: pipeline.SectionDetected,
            Section: pipeline.Section is null ? null : new AnchorDocumentSection(
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

    private SectionRange? ResolveSectionOverride(
        ChapterContext context,
        BookIndex book,
        AnchorComputationOptions options)
    {
        if (options.SectionOverride is not null)
        {
            return options.SectionOverride;
        }

        if (!options.TryResolveSectionFromLabels)
        {
            return null;
        }

        foreach (var label in EnumerateLabelCandidates(context))
        {
            var section = SectionLocator.ResolveSectionByTitle(book, label);
            if (section is not null)
            {
                return section;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateLabelCandidates(ChapterContext context)
    {
        var descriptor = context.Descriptor;
        if (!string.IsNullOrWhiteSpace(descriptor.ChapterId))
        {
            yield return descriptor.ChapterId;
        }

        if (!string.IsNullOrWhiteSpace(descriptor.RootPath))
        {
            var rootName = Path.GetFileName(descriptor.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(rootName))
            {
                yield return rootName;
            }
        }
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
        var fillers = new HashSet<string>(new[] { "uh", "um", "erm", "uhh", "hmm", "mm", "huh", "like" }, StringComparer.Ordinal);

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
            static bool IsAlignedWord(WordAlign op) => op.BookIdx.HasValue && op.AsrIdx.HasValue && op.Op != AlignOp.Del;

            var matchedBookIdx = new List<int>();
            matchedBookIdx.AddRange(wordOps.Where(IsAlignedWord).Select(o => o.BookIdx!.Value));
            matchedBookIdx.AddRange(anchorOps.Where(a => a.BookIdx.HasValue).Select(a => a.BookIdx!.Value));
            matchedBookIdx.Sort();

            if (matchedBookIdx.Count > 0)
            {
                secStartWord = matchedBookIdx.First();
                secEndWord = matchedBookIdx.Last();

                var firstSentence = book.Sentences.FirstOrDefault(s => s.Start <= secStartWord && s.End >= secStartWord);
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

    private static string[][] BuildBookPhonemeView(BookIndex book, IReadOnlyList<int> filteredToOriginal, int filteredCount)
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
            if (!string.IsNullOrEmpty(lexeme) && pronunciations.TryGetValue(lexeme, out var variants) && variants.Length > 0)
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
            bookEndExclusive = Math.Min(pipeline.BookWindowFiltered.bEnd + 1, bookStart + Math.Max(1, bookSpan + bookPad));
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

    private HydratedTranscript BuildHydratedTranscript(ChapterContext context, TranscriptIndex transcript)
    {
        var book = context.Book.Documents.BookIndex ?? throw new InvalidOperationException("BookIndex is not loaded.");
        var asr = context.Documents.Asr ?? throw new InvalidOperationException("ASR document is not loaded.");

        var words = transcript.Words.Select(w => new HydratedWord(
            w.BookIdx,
            w.AsrIdx,
            w.BookIdx.HasValue && w.BookIdx.Value >= 0 && w.BookIdx.Value < book.Words.Length ? book.Words[w.BookIdx.Value].Text : null,
            w.AsrIdx.HasValue && w.AsrIdx.Value >= 0 && w.AsrIdx.Value < asr.WordCount ? asr.GetWord(w.AsrIdx.Value) : null,
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
            var scriptText = sentence.ScriptRange is null ? string.Empty : JoinAsr(asr, sentence.ScriptRange.Start, sentence.ScriptRange.End);

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

    private static string BuildParagraphScript(IReadOnlyList<int> sentenceIds, IReadOnlyDictionary<int, HydratedSentence> sentenceMap)
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
