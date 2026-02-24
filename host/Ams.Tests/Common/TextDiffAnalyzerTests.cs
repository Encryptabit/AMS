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
}
