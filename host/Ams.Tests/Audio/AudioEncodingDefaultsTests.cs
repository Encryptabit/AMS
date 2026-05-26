using Ams.Core.Artifacts;
using Ams.Core.Audio;

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
