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
