using Ams.Core.Processors.Diffing;

namespace Ams.Tests.Common;

public sealed class TextDiffAnalyzerTests
{
    [Fact]
    public void Analyze_WithoutPhonemeScoring_KeepsSubstitutionPenalty()
    {
        var result = TextDiffAnalyzer.Analyze("colour", "color");

        Assert.Equal(1.0, result.Metrics.Wer);
        Assert.Equal(1, result.Metrics.MissingRuns);
        Assert.Equal(1, result.Metrics.ExtraRuns);
    }

    [Fact]
    public void Analyze_WithExactPhonemeEquivalence_RemovesSubstitutionPenalty()
    {
        var options = new TextDiffScoringOptions(
            ReferenceTokens: new[] { "colour" },
            HypothesisTokens: new[] { "color" },
            ReferencePhonemeVariants: new string[]?[] { new[] { "K AH1 L ER0" } },
            HypothesisPhonemeVariants: new string[]?[] { new[] { "K AH1 L ER0" } },
            UseExactPhonemeEquivalence: true);

        var result = TextDiffAnalyzer.Analyze("colour", "color", options);

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
    }

    [Fact]
    public void Analyze_ApostropheSuffixSplit_KeepsSentenceEqual()
    {
        var result = TextDiffAnalyzer.Analyze(
            "Bloodlust's effect is wearing off.",
            "bloodlust s effect is wearing off");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Fact]
    public void Analyze_ApostropheSuffixSplit_WithProvidedScoringTokens_KeepsSentenceEqual()
    {
        var options = new TextDiffScoringOptions(
            ReferenceTokens: new[] { "bloodlust's", "effect", "is", "wearing", "off" },
            HypothesisTokens: new[] { "bloodlust", "s", "effect", "is", "wearing", "off" },
            ReferencePhonemeVariants:
            [
                new[] { "B L AH1 D L AH2 S T S" },
                null,
                null,
                null,
                null
            ],
            HypothesisPhonemeVariants:
            [
                new[] { "B L AH1 D L AH2 S T" },
                null,
                null,
                null,
                null,
                null
            ],
            UseExactPhonemeEquivalence: true);

        var result = TextDiffAnalyzer.Analyze(
            "Bloodlust's effect is wearing off.",
            "bloodlust s effect is wearing off",
            options);

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
    }
}
