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
            BuildSentence(63, 0.29, 0.97, 0, 1, "Chapter 1", "Chapter 1."),
            BuildSentence(64, 1.35, 4.20, 2, 7, "The first line", "The first line."),
            BuildSentence(65, 4.25, 9.75, 8, 18, "More content", "More content.")
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
        int? scriptStart,
        int? scriptEnd,
        string bookText,
        string scriptText)
    {
        return new HydratedSentence(
            id,
            new HydratedRange(0, 0),
            new HydratedScriptRange(scriptStart, scriptEnd),
            bookText,
            scriptText,
            new SentenceMetrics(0, 0, 0, 0, 0),
            "ok",
            new TimingRange(startSec, endSec),
            null);
    }
}
