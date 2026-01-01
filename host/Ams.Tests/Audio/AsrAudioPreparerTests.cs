using Ams.Core.Audio;

namespace Ams.Tests.Audio;

/// <summary>
/// Tests for <see cref="AsrAudioPreparer"/> mono downmix and ASR preparation.
/// </summary>
public sealed class AsrAudioPreparerTests
{
    [Fact]
    public void BuildMonoPanClause_SingleChannel_ReturnsIdentity()
    {
        var clause = AsrAudioPreparer.BuildMonoPanClause(1);

        Assert.Equal("pan=mono|c0=c0", clause);
    }

    [Fact]
    public void BuildMonoPanClause_StereoChannels_ReturnsEqualWeights()
    {
        var clause = AsrAudioPreparer.BuildMonoPanClause(2);

        // 1/2 = 0.5
        Assert.Contains("pan=mono|c0=", clause);
        Assert.Contains("0.500000*c0", clause);
        Assert.Contains("0.500000*c1", clause);
        Assert.Contains("+", clause);
    }

    [Fact]
    public void BuildMonoPanClause_SurroundChannels_ReturnsCorrectWeights()
    {
        var clause = AsrAudioPreparer.BuildMonoPanClause(6);

        // 1/6 = 0.166667
        Assert.Contains("pan=mono|c0=", clause);
        Assert.Contains("0.166667*c0", clause);
        Assert.Contains("0.166667*c5", clause);

        // Should have 5 plus signs for 6 channels
        Assert.Equal(5, clause.Count(c => c == '+'));
    }

    [Fact]
    public void BuildMonoPanClause_ZeroChannels_ReturnsIdentity()
    {
        // Edge case: zero channels treated as identity
        var clause = AsrAudioPreparer.BuildMonoPanClause(0);

        Assert.Equal("pan=mono|c0=c0", clause);
    }

    [Theory]
    [InlineData(1, "pan=mono|c0=c0")]
    [InlineData(3, "pan=mono|c0=0.333333*c0+0.333333*c1+0.333333*c2")]
    [InlineData(4, "pan=mono|c0=0.250000*c0+0.250000*c1+0.250000*c2+0.250000*c3")]
    public void BuildMonoPanClause_VariousChannels_MatchesExpected(int channels, string expected)
    {
        var clause = AsrAudioPreparer.BuildMonoPanClause(channels);

        Assert.Equal(expected, clause);
    }

    [Fact]
    public void BuildMonoPanClause_WeightsAreInvariantCulture()
    {
        // Ensure decimal separator is always dot, not comma
        var clause = AsrAudioPreparer.BuildMonoPanClause(3);

        Assert.DoesNotContain(",", clause);
        Assert.Contains(".", clause);
    }
}
