using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Audio;
using Ams.Core.Processors;

namespace Ams.Tests.Audio;

public sealed class AudioTreatmentServiceTests
{
    [Fact]
    public void FindSpeechBoundaries_UsesHydrateTitleAndNextSentence_WhenAvailable()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 460_000); // 460s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.28), TimeSpan.FromSeconds(0.28)),
            new SilenceInterval(TimeSpan.FromSeconds(92.003), TimeSpan.FromSeconds(93.031), TimeSpan.FromSeconds(1.028)),
            new SilenceInterval(TimeSpan.FromSeconds(458.617), TimeSpan.FromSeconds(460.0), TimeSpan.FromSeconds(1.383))
        };

        var hydratedSentences = new[]
        {
            BuildSentence(63, 0.29, 0.97, 0, 1, 0, 1, "Chapter 1", "Chapter 1."),
            BuildSentence(64, 1.35, 4.20, 2, 7, 2, 7, "The first line", "The first line."),
            BuildSentence(65, 4.25, 9.75, 8, 18, 8, 18, "More content", "More content.")
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydratedSentences);

        Assert.Equal(0.29, result.TitleStart, precision: 2);
        Assert.Equal(0.97, result.TitleEnd, precision: 2);
        Assert.Equal(1.35, result.ContentStart, precision: 2);
        Assert.Equal(458.617, result.ContentEnd, precision: 3);
    }

    [Fact]
    public void FindTreatmentLayout_FallsBackToSilenceBoundaries_WhenHydrateOmitsHeadingSentence()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 10_000); // 10s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.30), TimeSpan.FromSeconds(0.30)),
            new SilenceInterval(TimeSpan.FromSeconds(2.00), TimeSpan.FromSeconds(3.50), TimeSpan.FromSeconds(1.50)),
            new SilenceInterval(TimeSpan.FromSeconds(9.0), TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(1.0))
        };

        // Heading speech exists before the 2.0-3.5 gap, but hydrate starts at first body sentence.
        var hydrate = BuildTranscript(
            words:
            [
                BuildWord(0, "The", 3.50, 3.72),
                BuildWord(1, "first", 3.74, 4.05),
                BuildWord(2, "line", 4.06, 4.35),
                BuildWord(3, "More", 4.60, 4.90),
                BuildWord(4, "content", 4.92, 5.40)
            ],
            sentences:
            [
                BuildSentence(0, 3.50, 4.35, 0, 2, 0, 2, "The first line", "The first line."),
                BuildSentence(1, 4.60, 5.40, 3, 4, 3, 4, "More content", "More content.")
            ]);

        var result = AudioTreatmentService.FindTreatmentLayout(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydrate,
            sectionTitle: "Chapter 12: Squad Kill");

        Assert.Equal(0.30, result.TitleStart, precision: 2);
        Assert.Equal(2.00, result.TitleEnd, precision: 2);
        Assert.Equal(3.50, result.ContentStart, precision: 2);
        Assert.Equal(9.0, result.ContentEnd, precision: 2);
        Assert.Null(result.DecoratorEnd);
        Assert.Null(result.TitleResumeStart);
    }

    [Fact]
    public void FindSpeechBoundaries_FallsBackToNoTitle_WhenOnlyLateGapExists()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 460_000); // 460s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(92.003), TimeSpan.FromSeconds(93.031), TimeSpan.FromSeconds(1.028)),
            new SilenceInterval(TimeSpan.FromSeconds(458.617), TimeSpan.FromSeconds(460.0), TimeSpan.FromSeconds(1.383))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydratedSentences: null);

        Assert.Equal(-1.0, result.TitleStart, precision: 6);
        Assert.Equal(-1.0, result.TitleEnd, precision: 6);
        Assert.Equal(0.0, result.ContentStart, precision: 6);
        Assert.Equal(458.617, result.ContentEnd, precision: 3);
    }

    [Fact]
    public void FindTreatmentLayout_SplitsCombinedHeading_WhenDecoratorAndTitleShareSentence()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 10_000); // 10s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.28), TimeSpan.FromSeconds(0.28)),
            new SilenceInterval(TimeSpan.FromSeconds(9.0), TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(1.0))
        };
        var hydrate = BuildTranscript(
            words:
            [
                BuildWord(0, "Chapter", 0.29, 0.54),
                BuildWord(1, "1", 0.56, 0.80),
                BuildWord(2, "Back", 1.58, 2.11),
                BuildWord(3, "The", 3.63, 3.88),
                BuildWord(4, "first", 3.90, 4.18)
            ],
            sentences:
            [
                BuildSentence(0, 0.29, 2.11, 0, 2, 0, 2, "Chapter 1: Back", "Chapter 1: Back."),
                BuildSentence(1, 3.63, 4.18, 3, 4, 3, 4, "The first", "The first.")
            ]);

        var result = AudioTreatmentService.FindTreatmentLayout(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydrate,
            sectionTitle: "Chapter 1: Back");

        Assert.Equal(0.29, result.TitleStart, precision: 2);
        Assert.Equal(2.11, result.TitleEnd, precision: 2);
        Assert.Equal(3.63, result.ContentStart, precision: 2);
        Assert.Equal(9.0, result.ContentEnd, precision: 3);
        Assert.NotNull(result.DecoratorEnd);
        Assert.NotNull(result.TitleResumeStart);
        Assert.Equal(0.80, result.DecoratorEnd!.Value, precision: 2);
        Assert.Equal(1.58, result.TitleResumeStart!.Value, precision: 2);
    }

    [Fact]
    public void FindTreatmentLayout_SplitsCombinedHeading_WhenDecoratorAndTitleAreSeparateSentences()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 10_000); // 10s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.28), TimeSpan.FromSeconds(0.28)),
            new SilenceInterval(TimeSpan.FromSeconds(9.0), TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(1.0))
        };
        var hydrate = BuildTranscript(
            words:
            [
                BuildWord(0, "Chapter", 0.29, 0.53),
                BuildWord(1, "1", 0.55, 0.84),
                BuildWord(2, "Back", 1.61, 2.08),
                BuildWord(3, "The", 3.58, 3.84),
                BuildWord(4, "first", 3.87, 4.16)
            ],
            sentences:
            [
                BuildSentence(0, 0.29, 0.84, 0, 1, 0, 1, "Chapter 1", "Chapter 1."),
                BuildSentence(1, 1.61, 2.08, 2, 2, 2, 2, "Back", "Back."),
                BuildSentence(2, 3.58, 4.16, 3, 4, 3, 4, "The first", "The first.")
            ]);

        var result = AudioTreatmentService.FindTreatmentLayout(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydrate,
            sectionTitle: "Chapter 1: Back");

        Assert.Equal(0.29, result.TitleStart, precision: 2);
        Assert.Equal(2.08, result.TitleEnd, precision: 2);
        Assert.Equal(3.58, result.ContentStart, precision: 2);
        Assert.Equal(9.0, result.ContentEnd, precision: 3);
        Assert.NotNull(result.DecoratorEnd);
        Assert.NotNull(result.TitleResumeStart);
        Assert.Equal(0.84, result.DecoratorEnd!.Value, precision: 2);
        Assert.Equal(1.61, result.TitleResumeStart!.Value, precision: 2);
    }

    [Fact]
    public void FindTreatmentLayout_LeavesSingleHeadingUntouched_WhenSectionTitleHasNoDecoratorTitleSplit()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 10_000); // 10s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.28), TimeSpan.FromSeconds(0.28)),
            new SilenceInterval(TimeSpan.FromSeconds(1.05), TimeSpan.FromSeconds(2.55), TimeSpan.FromSeconds(1.5)),
            new SilenceInterval(TimeSpan.FromSeconds(9.0), TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(1.0))
        };
        var hydrate = BuildTranscript(
            words:
            [
                BuildWord(0, "Back", 0.29, 0.83),
                BuildWord(1, "The", 2.55, 2.83),
                BuildWord(2, "first", 2.86, 3.17)
            ],
            sentences:
            [
                BuildSentence(0, 0.29, 0.83, 0, 0, 0, 0, "Back", "Back."),
                BuildSentence(1, 2.55, 3.17, 1, 2, 1, 2, "The first", "The first.")
            ]);

        var result = AudioTreatmentService.FindTreatmentLayout(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydrate,
            sectionTitle: "Back");

        Assert.Equal(0.29, result.TitleStart, precision: 2);
        Assert.Equal(0.83, result.TitleEnd, precision: 2);
        Assert.Equal(2.55, result.ContentStart, precision: 2);
        Assert.Equal(9.0, result.ContentEnd, precision: 3);
        Assert.Null(result.DecoratorEnd);
        Assert.Null(result.TitleResumeStart);
    }

    [Fact]
    public void ApplyLayoutPadding_ExpandsTreatBoundaries_WhenPaddingFitsWithinGap()
    {
        var result = AudioTreatmentService.ApplyLayoutPadding(
            (TitleStart: 0.29, TitleEnd: 0.83, ContentStart: 2.55, ContentEnd: 9.0, DecoratorEnd: (double?)null, TitleResumeStart: (double?)null),
            audioDuration: 10.0,
            paddingSec: 0.05);

        Assert.Equal(0.24, result.TitleStart, precision: 2);
        Assert.Equal(0.88, result.TitleEnd, precision: 2);
        Assert.Equal(2.50, result.ContentStart, precision: 2);
        Assert.Equal(9.05, result.ContentEnd, precision: 2);
        Assert.Null(result.DecoratorEnd);
        Assert.Null(result.TitleResumeStart);
    }

    [Fact]
    public void ApplyLayoutPadding_SharesShortGapWithoutOverlap()
    {
        var result = AudioTreatmentService.ApplyLayoutPadding(
            (TitleStart: 0.29, TitleEnd: 0.83, ContentStart: 0.89, ContentEnd: 9.0, DecoratorEnd: (double?)null, TitleResumeStart: (double?)null),
            audioDuration: 10.0,
            paddingSec: 0.05);

        Assert.Equal(0.24, result.TitleStart, precision: 2);
        Assert.Equal(0.86, result.TitleEnd, precision: 2);
        Assert.Equal(0.86, result.ContentStart, precision: 2);
        Assert.Equal(9.05, result.ContentEnd, precision: 2);
    }

    [Fact]
    public void ApplyLayoutPadding_ExpandsDecoratorGapBoundaries_WhenHeadingIsSplit()
    {
        var result = AudioTreatmentService.ApplyLayoutPadding(
            (TitleStart: 0.29, TitleEnd: 2.08, ContentStart: 3.58, ContentEnd: 9.0, DecoratorEnd: 0.84, TitleResumeStart: 1.61),
            audioDuration: 10.0,
            paddingSec: 0.05);

        Assert.Equal(0.24, result.TitleStart, precision: 2);
        Assert.Equal(2.13, result.TitleEnd, precision: 2);
        Assert.Equal(3.53, result.ContentStart, precision: 2);
        Assert.Equal(9.05, result.ContentEnd, precision: 2);
        Assert.Equal(0.89, result.DecoratorEnd!.Value, precision: 2);
        Assert.Equal(1.56, result.TitleResumeStart!.Value, precision: 2);
    }

    [Fact]
    public void ApplyLayoutPadding_ExpandsContentOnly_WhenNoTitleExists()
    {
        var result = AudioTreatmentService.ApplyLayoutPadding(
            (TitleStart: -1.0, TitleEnd: -1.0, ContentStart: 0.30, ContentEnd: 9.0, DecoratorEnd: (double?)null, TitleResumeStart: (double?)null),
            audioDuration: 10.0,
            paddingSec: 0.05);

        Assert.Equal(-1.0, result.TitleStart, precision: 6);
        Assert.Equal(-1.0, result.TitleEnd, precision: 6);
        Assert.Equal(0.25, result.ContentStart, precision: 2);
        Assert.Equal(9.05, result.ContentEnd, precision: 2);
        Assert.Null(result.DecoratorEnd);
        Assert.Null(result.TitleResumeStart);
    }

    [Fact]
    public void ResolvePreferredBitDepth_Returns24_ForPcmS24Codec()
    {
        var metadata = AudioBufferMetadata.CreateDefault(sampleRate: 44_100, channels: 1) with
        {
            CodecName = "pcm_s24le"
        };
        var buffer = new AudioBuffer(channels: 1, sampleRate: 44_100, length: 1000, metadata);

        var bitDepth = AudioTreatmentService.ResolvePreferredBitDepth(buffer);

        Assert.Equal(24, bitDepth);
    }

    [Fact]
    public void ResolvePreferredBitDepth_DefaultsTo16_ForUnknownCodec()
    {
        var metadata = AudioBufferMetadata.CreateDefault(sampleRate: 44_100, channels: 1) with
        {
            CodecName = "mp3float"
        };
        var buffer = new AudioBuffer(channels: 1, sampleRate: 44_100, length: 1000, metadata);

        var bitDepth = AudioTreatmentService.ResolvePreferredBitDepth(buffer);

        Assert.Equal(16, bitDepth);
    }

    // Regression guard for the click-immunity refactor: the head/tail finders apply burst-merge
    // locally, but mid-content structural-gap detection must still see raw silences. Two ~0.65s
    // silences separated by a 100ms (sub-tolerance) "word" must not coalesce into a fake 1.4s
    // title-body gap that would shift contentStart from 10.65 (after first silence) to 11.40
    // (after merged region).
    [Fact]
    public void FindSpeechBoundaries_DoesNotMergeMidContentSilencesAcrossSubToleranceBlips()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 60_000); // 60s
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.30), TimeSpan.FromSeconds(0.30)),
            new SilenceInterval(TimeSpan.FromSeconds(10.00), TimeSpan.FromSeconds(10.65), TimeSpan.FromSeconds(0.65)),
            new SilenceInterval(TimeSpan.FromSeconds(10.75), TimeSpan.FromSeconds(11.40), TimeSpan.FromSeconds(0.65)),
            new SilenceInterval(TimeSpan.FromSeconds(59.5), TimeSpan.FromSeconds(60.0), TimeSpan.FromSeconds(0.5))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydratedSentences: null,
            sectionTitle: null,
            boundaryToleranceSec: 0.150);

        // Heuristic falls through to the first ≥0.3s silence after speechStart. With raw silences
        // (no global coalesce), that's the [10.00–10.65] silence so contentStart = 10.65, NOT
        // 11.40 which is what the legacy global coalesce produced by absorbing the 100ms blip.
        Assert.Equal(0.30, result.TitleStart, precision: 2);
        Assert.Equal(10.00, result.TitleEnd, precision: 2);
        Assert.Equal(10.65, result.ContentStart, precision: 2);
        Assert.Equal(59.5, result.ContentEnd, precision: 2);
    }

    [Fact]
    public void FindContentEnd_AbsorbsTrailingSilencesFragmentedByShortBlips()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 30_000); // 30s
        // Clear title-body gap at 5–6.5s suppresses FindSpeechBoundaries' fallback heuristic so
        // trailing silences aren't mis-classified as title boundary. Trailing region [26.0–30.0]
        // is fragmented by ~10-50ms blips; tail trim should walk back to 26.0.
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.30), TimeSpan.FromSeconds(0.30)),
            new SilenceInterval(TimeSpan.FromSeconds(5.00), TimeSpan.FromSeconds(6.50), TimeSpan.FromSeconds(1.50)),
            new SilenceInterval(TimeSpan.FromSeconds(26.00), TimeSpan.FromSeconds(27.50), TimeSpan.FromSeconds(1.50)),
            new SilenceInterval(TimeSpan.FromSeconds(27.55), TimeSpan.FromSeconds(28.70), TimeSpan.FromSeconds(1.15)),
            new SilenceInterval(TimeSpan.FromSeconds(28.71), TimeSpan.FromSeconds(30.00), TimeSpan.FromSeconds(1.29))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydratedSentences: null,
            sectionTitle: null,
            boundaryToleranceSec: 0.150);

        Assert.Equal(26.00, result.ContentEnd, precision: 2);
    }

    [Fact]
    public void FindContentEnd_TrimsWhenLastSilenceEndsWithinToleranceOfEof()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 30_000); // 30s
        // Last silence ends 120ms before EOF — past the legacy 100ms anchor but inside the new
        // 150ms click-immunity tolerance. Trailing 120ms burst is treated as part of the trailing
        // silence so the chapter is still trimmed at 28.83. Clear title-body gap suppresses the
        // fallback heuristic.
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.30), TimeSpan.FromSeconds(0.30)),
            new SilenceInterval(TimeSpan.FromSeconds(5.00), TimeSpan.FromSeconds(6.50), TimeSpan.FromSeconds(1.50)),
            new SilenceInterval(TimeSpan.FromSeconds(28.83), TimeSpan.FromSeconds(29.88), TimeSpan.FromSeconds(1.05))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 1.0,
            hydratedSentences: null,
            sectionTitle: null,
            boundaryToleranceSec: 0.150);

        Assert.Equal(28.83, result.ContentEnd, precision: 2);
    }

    [Fact]
    public void FindContentEnd_DoesNotTrimWhenLastSilenceFarFromEof()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 30_000); // 30s
        // Last silence ends at 25.0s — far beyond the click-immunity tolerance of EOF.
        // No tail trim; content runs to the end of the buffer.
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.20), TimeSpan.FromSeconds(0.20)),
            new SilenceInterval(TimeSpan.FromSeconds(20.0), TimeSpan.FromSeconds(25.0), TimeSpan.FromSeconds(5.0))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 10.0,
            hydratedSentences: null,
            sectionTitle: null,
            boundaryToleranceSec: 0.150);

        Assert.Equal(30.0, result.ContentEnd, precision: 2);
    }

    [Fact]
    public void FindSpeechStart_AbsorbsLeadingSilencesFragmentedByShortBlips()
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: 1000, length: 30_000); // 30s
        // Symmetrical head case: studio pre-roll silence is fragmented by a 50ms click.
        // Speech start should walk forward across the blip to 0.95s.
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.00), TimeSpan.FromSeconds(0.50), TimeSpan.FromSeconds(0.50)),
            new SilenceInterval(TimeSpan.FromSeconds(0.55), TimeSpan.FromSeconds(0.95), TimeSpan.FromSeconds(0.40)),
            new SilenceInterval(TimeSpan.FromSeconds(29.5), TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(0.5))
        };

        var result = AudioTreatmentService.FindSpeechBoundaries(
            buffer,
            silences,
            gapThreshold: 5.0,
            hydratedSentences: null,
            sectionTitle: null,
            boundaryToleranceSec: 0.150);

        Assert.Equal(0.95, result.ContentStart, precision: 2);
    }

    private static HydratedSentence BuildSentence(
        int id,
        double startSec,
        double endSec,
        int bookStart,
        int bookEnd,
        int? scriptStart,
        int? scriptEnd,
        string bookText,
        string scriptText)
    {
        return new HydratedSentence(
            id,
            new HydratedRange(bookStart, bookEnd),
            new HydratedScriptRange(scriptStart, scriptEnd),
            bookText,
            scriptText,
            new SentenceMetrics(0, 0, 0, 0, 0),
            "ok",
            new TimingRange(startSec, endSec),
            null);
    }

    private static HydratedWord BuildWord(int bookIdx, string bookWord, double startSec, double endSec)
    {
        return new HydratedWord(bookIdx, bookIdx, bookWord, bookWord, "Match", "test", 0.0)
        {
            StartSec = startSec,
            EndSec = endSec,
            DurationSec = endSec - startSec
        };
    }

    private static HydratedTranscript BuildTranscript(
        IReadOnlyList<HydratedWord> words,
        IReadOnlyList<HydratedSentence> sentences)
    {
        return new HydratedTranscript(
            AudioPath: "chapter.wav",
            ScriptPath: "chapter.txt",
            BookIndexPath: "book-index.json",
            CreatedAtUtc: DateTime.UtcNow,
            NormalizationVersion: "test",
            Words: words,
            Sentences: sentences,
            Paragraphs: Array.Empty<HydratedParagraph>());
    }
}
