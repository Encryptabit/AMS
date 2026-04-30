using Ams.Core.Asr;
using Whisper.net.Ggml;

namespace Ams.Tests.Asr;

public sealed class AmsAsrModelTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseStrict_ReturnsNullForEmptyInput(string? input)
    {
        Assert.Null(AmsAsrModelExtensions.ParseStrict(input));
    }

    [Theory]
    [InlineData("large-v3", AmsAsrModel.LargeV3)]
    [InlineData("LARGE-V3", AmsAsrModel.LargeV3)]
    [InlineData("ggml-large-v3.bin", AmsAsrModel.LargeV3)]
    [InlineData("large-v3-turbo", AmsAsrModel.LargeV3Turbo)]
    [InlineData("ggml-large-v3-turbo.bin", AmsAsrModel.LargeV3Turbo)]
    public void ParseStrict_AcceptsKnownAliases(string input, AmsAsrModel expected)
    {
        Assert.Equal(expected, AmsAsrModelExtensions.ParseStrict(input));
    }

    [Theory]
    [InlineData("tiny")]
    [InlineData("base")]
    [InlineData("medium")]
    [InlineData("large-v2")]
    [InlineData("not-a-model")]
    public void ParseStrict_RejectsUnknownAliases(string input)
    {
        var ex = Assert.Throws<ArgumentException>(() => AmsAsrModelExtensions.ParseStrict(input));
        Assert.Contains("Supported values", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("tiny")]
    [InlineData("base")]
    public void TryParseLenient_ReturnsNullForUnknownOrEmpty(string? input)
    {
        Assert.Null(AmsAsrModelExtensions.TryParseLenient(input));
    }

    [Theory]
    [InlineData("large-v3", AmsAsrModel.LargeV3)]
    [InlineData("large-v3-turbo", AmsAsrModel.LargeV3Turbo)]
    public void TryParseLenient_RecognizesKnownAliases(string input, AmsAsrModel expected)
    {
        Assert.Equal(expected, AmsAsrModelExtensions.TryParseLenient(input));
    }

    [Fact]
    public void GetDefaultFallback_ProducesSymmetricCrossPair()
    {
        Assert.Equal(AmsAsrModel.LargeV3Turbo, AmsAsrModel.LargeV3.GetDefaultFallback());
        Assert.Equal(AmsAsrModel.LargeV3, AmsAsrModel.LargeV3Turbo.GetDefaultFallback());
    }

    [Fact]
    public void ToAlias_RoundTripsThroughParseStrict()
    {
        foreach (var model in Enum.GetValues<AmsAsrModel>())
        {
            var alias = model.ToAlias();
            Assert.Equal(model, AmsAsrModelExtensions.ParseStrict(alias));
        }
    }

    [Theory]
    [InlineData(AmsAsrModel.LargeV3, GgmlType.LargeV3)]
    [InlineData(AmsAsrModel.LargeV3Turbo, GgmlType.LargeV3Turbo)]
    public void ToGgmlType_MapsToWhisperEnum(AmsAsrModel input, GgmlType expected)
    {
        Assert.Equal(expected, input.ToGgmlType());
    }
}
