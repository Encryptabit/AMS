using Ams.Cli.Commands;
using Ams.Core.Application.Pipeline;
using Ams.Core.Asr;

namespace Ams.Tests.Cli;

public sealed class PipelineRecoveryHelpersTests
{
    // ResolveRecoveryModels — the CLI parser front-end for --asr-model and --fallback-model.

    [Fact]
    public void ResolveRecoveryModels_WithLargeV3_ReturnsCrossPairTurbo()
    {
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3", null);
        Assert.Equal("large-v3", primary);
        Assert.Equal("large-v3-turbo", fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithLargeV3Turbo_ReturnsCrossPairLargeV3()
    {
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3-turbo", null);
        Assert.Equal("large-v3-turbo", primary);
        Assert.Equal("large-v3", fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithFallbackNone_OptsOutOfAlternateModel()
    {
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3", "none");
        Assert.Equal("large-v3", primary);
        Assert.Null(fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithFallbackNoneCaseInsensitive()
    {
        var (_, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3", "NONE");
        Assert.Null(fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithExplicitFallback_OverridesCrossPair()
    {
        // Primary LargeV3 would default to LargeV3Turbo cross-pair; explicit fallback wins.
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3", "large-v3-turbo");
        Assert.Equal("large-v3", primary);
        Assert.Equal("large-v3-turbo", fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WhenPrimaryAndFallbackMatch_DropsFallback()
    {
        // No point in firing AlternateModel against the same model the primary tier failed on.
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels("large-v3", "large-v3");
        Assert.Equal("large-v3", primary);
        Assert.Null(fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithNullPrimary_PreservesEnvVarPath()
    {
        // Null primary signals "use AMS_WHISPER_MODEL_PATH / config default" downstream. With
        // no primary we cannot infer a cross-pair, so fallback is also null unless explicit.
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels(null, null);
        Assert.Null(primary);
        Assert.Null(fallback);
    }

    [Fact]
    public void ResolveRecoveryModels_WithNullPrimaryAndExplicitFallback_KeepsFallback()
    {
        var (primary, fallback) = PipelineCommand.ResolveRecoveryModels(null, "large-v3-turbo");
        Assert.Null(primary);
        Assert.Equal("large-v3-turbo", fallback);
    }

    [Theory]
    [InlineData("tiny")]
    [InlineData("base")]
    [InlineData("medium")]
    [InlineData("large-v2")]
    public void ResolveRecoveryModels_RejectsNonWhitelistedPrimary(string raw)
    {
        Assert.Throws<ArgumentException>(() => PipelineCommand.ResolveRecoveryModels(raw, null));
    }

    [Fact]
    public void ResolveRecoveryModels_RejectsNonWhitelistedFallback()
    {
        Assert.Throws<ArgumentException>(
            () => PipelineCommand.ResolveRecoveryModels("large-v3", "tiny"));
    }

    // SelectNextRecoveryTier — the tier escalation policy.

    [Fact]
    public void SelectNextRecoveryTier_FromNoneWithWhisperAndFallback_PicksAlternateModel()
    {
        var next = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.None, AsrEngine.Whisper, "large-v3-turbo");
        Assert.Equal(RecoveryTier.AlternateModel, next);
    }

    [Fact]
    public void SelectNextRecoveryTier_FromNoneWithWhisperAndNoFallback_SkipsToPromptless()
    {
        var next = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.None, AsrEngine.Whisper, null);
        Assert.Equal(RecoveryTier.Promptless, next);
    }

    [Fact]
    public void SelectNextRecoveryTier_FromNoneWithWhisperX_SkipsAlternateModelEvenWithFallback()
    {
        // WhisperX's model loading is opaque to the in-process Whisper.NET cache, so there's
        // no benefit to swapping to a cross-pair model — skip directly to Promptless.
        var next = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.None, AsrEngine.WhisperX, "large-v3-turbo");
        Assert.Equal(RecoveryTier.Promptless, next);
    }

    [Fact]
    public void SelectNextRecoveryTier_FromAlternateModel_PicksPromptless()
    {
        // From AlternateModel, the only remaining tier is Promptless regardless of engine or
        // fallback configuration.
        var nextWhisper = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.AlternateModel, AsrEngine.Whisper, "large-v3-turbo");
        var nextWhisperX = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.AlternateModel, AsrEngine.WhisperX, null);

        Assert.Equal(RecoveryTier.Promptless, nextWhisper);
        Assert.Equal(RecoveryTier.Promptless, nextWhisperX);
    }

    [Fact]
    public void SelectNextRecoveryTier_FromPromptless_ReturnsNull()
    {
        var next = PipelineCommand.SelectNextRecoveryTier(
            RecoveryTier.Promptless, AsrEngine.Whisper, "large-v3-turbo");
        Assert.Null(next);
    }

    // ResolveModelForTier — picks which model alias to send into RunPipelineAsync.

    [Fact]
    public void ResolveModelForTier_NoneUsesPrimary()
    {
        var alias = PipelineCommand.ResolveModelForTier(RecoveryTier.None, "large-v3", "large-v3-turbo");
        Assert.Equal("large-v3", alias);
    }

    [Fact]
    public void ResolveModelForTier_AlternateModelUsesFallback()
    {
        var alias = PipelineCommand.ResolveModelForTier(RecoveryTier.AlternateModel, "large-v3", "large-v3-turbo");
        Assert.Equal("large-v3-turbo", alias);
    }

    [Fact]
    public void ResolveModelForTier_PromptlessFallsBackToPrimary()
    {
        // Per recovery plan, Promptless preserves the user's primary model and only suppresses
        // the prompt — it does NOT continue using the alternate model from the previous tier.
        var alias = PipelineCommand.ResolveModelForTier(RecoveryTier.Promptless, "large-v3", "large-v3-turbo");
        Assert.Equal("large-v3", alias);
    }
}
