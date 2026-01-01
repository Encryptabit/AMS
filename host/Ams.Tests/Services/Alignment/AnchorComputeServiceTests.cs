using Ams.Core.Services.Alignment;

namespace Ams.Tests.Services.Alignment;

/// <summary>
/// Tests for <see cref="AnchorComputeService"/> and related options.
/// </summary>
public sealed class AnchorComputeServiceTests
{
    [Fact]
    public void AnchorComputeService_CanBeInstantiated()
    {
        var service = new AnchorComputeService();

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IAnchorComputeService>(service);
    }

    [Fact]
    public void AnchorComputationOptions_DefaultValues_AreCorrect()
    {
        var options = new AnchorComputationOptions();

        Assert.Equal(3, options.NGram);
        Assert.Equal(50, options.TargetPerTokens);
        Assert.Equal(100, options.MinSeparation);
        Assert.False(options.AllowBoundaryCross);
        Assert.True(options.UseDomainStopwords);
        Assert.True(options.DetectSection);
        Assert.Equal(8, options.AsrPrefixTokens);
        Assert.True(options.EmitWindows);
        Assert.Null(options.SectionOverride);
        Assert.True(options.TryResolveSectionFromLabels);
    }

    [Fact]
    public void AnchorComputationOptions_CustomValues_ArePreserved()
    {
        var options = new AnchorComputationOptions
        {
            NGram = 5,
            TargetPerTokens = 100,
            MinSeparation = 200,
            AllowBoundaryCross = true,
            UseDomainStopwords = false,
            DetectSection = false,
            AsrPrefixTokens = 16,
            EmitWindows = false,
            TryResolveSectionFromLabels = false
        };

        Assert.Equal(5, options.NGram);
        Assert.Equal(100, options.TargetPerTokens);
        Assert.Equal(200, options.MinSeparation);
        Assert.True(options.AllowBoundaryCross);
        Assert.False(options.UseDomainStopwords);
        Assert.False(options.DetectSection);
        Assert.Equal(16, options.AsrPrefixTokens);
        Assert.False(options.EmitWindows);
        Assert.False(options.TryResolveSectionFromLabels);
    }

    [Fact]
    public void TranscriptBuildOptions_ContainsAnchorOptions()
    {
        var options = new TranscriptBuildOptions();

        Assert.NotNull(options.AnchorOptions);
        Assert.Equal(3, options.AnchorOptions.NGram); // Verify defaults are applied
    }

    [Fact]
    public void TranscriptBuildOptions_CustomAnchorOptions_ArePreserved()
    {
        var anchorOpts = new AnchorComputationOptions { NGram = 7 };
        var options = new TranscriptBuildOptions { AnchorOptions = anchorOpts };

        Assert.Equal(7, options.AnchorOptions.NGram);
    }

    [Fact]
    public void HydrationOptions_DefaultValues_AreCorrect()
    {
        var options = new HydrationOptions();

        Assert.True(options.RecomputeDiffs);
    }

    [Fact]
    public async Task ComputeAnchorsAsync_NullContext_ThrowsArgumentNullException()
    {
        var service = new AnchorComputeService();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.ComputeAnchorsAsync(null!));
    }
}
