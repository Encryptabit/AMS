using Ams.Core.Audio;
using Ams.Core.Processors;

namespace Ams.Tests.Audio;

public sealed class SpliceBoundaryServiceTests
{
    [Fact]
    public void MergeSilencesAcrossTransientBursts_Merges_WhenBurstIsShort()
    {
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.00), TimeSpan.FromSeconds(0.20), TimeSpan.FromSeconds(0.20)),
            new SilenceInterval(TimeSpan.FromSeconds(0.22), TimeSpan.FromSeconds(0.40), TimeSpan.FromSeconds(0.18))
        };

        var merged = SpliceBoundaryService.MergeSilencesAcrossTransientBursts(
            silences,
            maxBurstSec: 0.03);

        var only = Assert.Single(merged);
        Assert.Equal(0.00, only.Start.TotalSeconds, precision: 3);
        Assert.Equal(0.40, only.End.TotalSeconds, precision: 3);
        Assert.Equal(0.40, only.Duration.TotalSeconds, precision: 3);
    }

    [Fact]
    public void MergeSilencesAcrossTransientBursts_DoesNotMerge_WhenBurstIsLong()
    {
        var silences = new[]
        {
            new SilenceInterval(TimeSpan.FromSeconds(0.00), TimeSpan.FromSeconds(0.20), TimeSpan.FromSeconds(0.20)),
            new SilenceInterval(TimeSpan.FromSeconds(0.28), TimeSpan.FromSeconds(0.40), TimeSpan.FromSeconds(0.12))
        };

        var merged = SpliceBoundaryService.MergeSilencesAcrossTransientBursts(
            silences,
            maxBurstSec: 0.03);

        Assert.Equal(2, merged.Count);
    }

    [Fact]
    public void ApplyBoundaryPadding_AddsFrontAndBack_AndClampsToBounds()
    {
        var opts = new SpliceBoundaryOptions
        {
            StartPaddingSec = 0.015,
            EndPaddingSec = 0.025
        };

        var (start, end) = SpliceBoundaryService.ApplyBoundaryPadding(
            startSec: 1.000,
            endSec: 2.000,
            minStartSec: 0.995,
            maxEndSec: 2.010,
            opts);

        Assert.Equal(0.995, start, precision: 3); // 1.000 - 0.015 clamped by min
        Assert.Equal(2.010, end, precision: 3);   // 2.000 + 0.025 clamped by max
    }
}
