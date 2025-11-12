using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ams.Core;

namespace Ams.Core.Processors.Alignment.Anchors;

public sealed record SectionDetectOptions(bool Detect = true, int AsrPrefixTokens = 8);

public sealed record AnchorPipelineResult(
    bool SectionDetected,
    SectionRange? Section,
    IReadOnlyList<Anchor> Anchors,
    (int bStart, int bEnd) BookWindowFiltered,
    int BookTokenCount,
    int BookFilteredCount,
    int AsrTokenCount,
    int AsrFilteredCount,
    IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)>? Windows,
    IReadOnlyList<int> BookFilteredToOriginalWord
);

public static class AnchorPipeline
{
    public static AnchorPipelineResult ComputeAnchors(
        BookIndex book,
        AsrResponse asr,
        AnchorPolicy policy,
        SectionDetectOptions? sectionOptions = null,
        bool includeWindows = false,
        SectionRange? overrideSection = null)
    {
        sectionOptions ??= new SectionDetectOptions();

        var bookView = AnchorPreprocessor.BuildBookView(book);
        var asrView = AnchorPreprocessor.BuildAsrView(asr);

        // Section detection on raw ASR words (not normalized) to allow number-to-words
        var asrRawTokens = asr.Tokens.Select(t => t.Word).ToList();
        SectionRange? section = null;
        (int bStart, int bEnd) bookWindowFiltered = (0, bookView.Tokens.Count - 1);

        if (overrideSection is not null)
        {
            section = overrideSection;
            if (AnchorPreprocessor.TryMapSectionWindow(bookView, (section.StartWord, section.EndWord), out var mapped))
            {
                bookWindowFiltered = mapped;
            }
        }
        else if (sectionOptions.Detect)
        {
            section = SectionLocator.DetectSection(book, asrRawTokens, sectionOptions.AsrPrefixTokens);
            if (section != null && AnchorPreprocessor.TryMapSectionWindow(bookView, (section.StartWord, section.EndWord), out var mapped))
            {
                bookWindowFiltered = mapped;
            }
        }

        var anchors = AnchorDiscovery.SelectAnchors(
            bookView.Tokens,
            bookView.SentenceIndex,
            asrView.Tokens,
            policy,
            bookWindowFiltered.bStart,
            bookWindowFiltered.bEnd
        ).ToList();

        if (anchors.Count > 0)
        {
            int minBp = anchors.Min(a => a.Bp);
            int maxBp = anchors.Max(a => a.Bp + policy.NGram - 1);
            int span = Math.Max(0, maxBp - minBp + 1);

            // Expand around anchor span to keep some context while avoiding full book fallback.
            int pad = Math.Max(64, Math.Min(8192, Math.Max(policy.NGram * 2, span / 5)));
            int refinedStart = Math.Max(0, minBp - pad);
            int refinedEnd = Math.Min(bookView.Tokens.Count - 1, maxBp + pad);

            // Only shrink when anchors provide a materially smaller window.
            if (refinedStart > bookWindowFiltered.bStart || refinedEnd < bookWindowFiltered.bEnd)
            {
                refinedStart = Math.Max(bookWindowFiltered.bStart, refinedStart);
                refinedEnd = Math.Min(bookWindowFiltered.bEnd, refinedEnd);
                bookWindowFiltered = (refinedStart, refinedEnd);
            }
        }

        IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)>? windows = null;
        if (includeWindows)
        {
            windows = AnchorDiscovery.BuildWindows(anchors, bookWindowFiltered.bStart, bookWindowFiltered.bEnd, 0, asrView.Tokens.Count - 1);
        }

        return new AnchorPipelineResult(
            SectionDetected: section != null,
            Section: section,
            Anchors: anchors,
            BookWindowFiltered: bookWindowFiltered,
            BookTokenCount: book.Words.Length,
            BookFilteredCount: bookView.Tokens.Count,
            AsrTokenCount: asr.Tokens.Length,
            AsrFilteredCount: asrView.Tokens.Count,
            Windows: windows,
            BookFilteredToOriginalWord: bookView.FilteredToOriginalWord
        );
    }
}



