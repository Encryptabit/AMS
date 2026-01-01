using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Microsoft.Extensions.Logging;

namespace Ams.Core.Services.Alignment;

/// <summary>
/// Service for computing anchor points between book text and ASR output.
/// </summary>
public sealed class AnchorComputeService : IAnchorComputeService
{
    private static readonly ILogger Logger = Log.For<AnchorComputeService>();

    /// <inheritdoc />
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
        var sectionOverride = context.GetOrResolveSection(book, opts, stage: "anchors", Logger);
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
        if (pipeline.SectionDetected && pipeline.Section is not null)
        {
            context.SetDetectedSection(pipeline.Section);
        }

        var document = BuildAnchorDocument(pipeline, opts);
        context.Documents.Anchors = document;
        return Task.FromResult(document);
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
}
