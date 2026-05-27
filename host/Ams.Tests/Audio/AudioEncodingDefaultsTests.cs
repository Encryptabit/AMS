using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Processors;

namespace Ams.Tests.Audio;

public sealed class AudioEncodingDefaultsTests
{
    [Theory]
    [InlineData("pcm_s16le", 16)]
    [InlineData("pcm_s24le", 24)]
    [InlineData("pcm_s32le", 32)]
    [InlineData("pcm_f32le", 32)]
    [InlineData("mp3", 16)]
    [InlineData(null, 16)]
    public void ResolvePreferredBitDepth_PreservesPcmDepthAndFallsBackToSixteenBit(
        string? codecName,
        int expectedBitDepth)
    {
        var buffer = CreateBuffer(codecName);

        var bitDepth = AudioEncodingDefaults.ResolvePreferredBitDepth(buffer);

        Assert.Equal(expectedBitDepth, bitDepth);
    }

    [Fact]
    public void ForSource_PreservesCurrentSampleRateAndPreferredBitDepth()
    {
        var buffer = CreateBuffer("pcm_s24le", sampleRate: 44_100);

        var options = AudioEncodingDefaults.ForSource(buffer);

        Assert.Equal(44_100, options.TargetSampleRate);
        Assert.Equal(24, options.TargetBitDepth);
        Assert.Equal(AudioSampleEncoding.SignedInteger, options.TargetSampleEncoding);
    }

    [Theory]
    [InlineData("pcm_s32le", AudioSampleEncoding.SignedInteger)]
    [InlineData("pcm_f32le", AudioSampleEncoding.Float)]
    public void ResolvePreferredSampleEncoding_DistinguishesIntegerAndFloatPcm(
        string codecName,
        AudioSampleEncoding expected)
    {
        var buffer = CreateBuffer(codecName);

        var encoding = AudioEncodingDefaults.ResolvePreferredSampleEncoding(buffer);

        Assert.Equal(expected, encoding);
    }

    private static AudioBuffer CreateBuffer(string? codecName, int sampleRate = 48_000)
    {
        var metadata = AudioBufferMetadata.CreateDefault(sampleRate, channels: 1) with
        {
            CodecName = codecName
        };

        return new AudioBuffer(channels: 1, sampleRate, length: sampleRate / 10, metadata);
    }
}
