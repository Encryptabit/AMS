using Ams.Cli.Utilities;

namespace Ams.Tests.Cli;

public sealed class AudioTierResolverTests
{
    [Theory]
    [InlineData(null, "source")]
    [InlineData("source", "source")]
    [InlineData("raw", "source")]
    [InlineData("treated", "treated")]
    [InlineData("filtered", "filtered")]
    public void Parse_ResolvesSupportedInputTiers(string? value, string expected)
    {
        var tier = AudioTierResolver.Parse(value, AudioTier.Source);

        Assert.Equal(expected, AudioTierResolver.Describe(tier));
    }

    [Fact]
    public void Parse_RejectsAdjustedTierForInput()
    {
        Assert.Throws<ArgumentException>(() =>
            AudioTierResolver.Parse("adjusted", AudioTier.Source));
    }

    [Theory]
    [InlineData("chapter.treated.wav", "chapter.wav")]
    [InlineData("chapter.filtered.wav", "chapter.wav")]
    [InlineData("chapter.dsp.filtered.wav", "chapter.wav")]
    [InlineData("chapter.pause-adjusted.wav", "chapter.wav")]
    [InlineData("chapter.wav", "chapter.wav")]
    public void StripVariantMarkers_ReturnsDeliveryName(string fileName, string expected)
    {
        var stripped = AudioTierResolver.StripVariantMarkers(fileName);

        Assert.Equal(expected, stripped);
    }
}
