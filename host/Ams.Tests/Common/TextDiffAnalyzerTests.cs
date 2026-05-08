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

    [Fact]
    public void Analyze_HyphenCompoundVsMergedWord_KeepsSentenceEqual()
    {
        var result = TextDiffAnalyzer.Analyze(
            "The soul-bound blade carved cleanly through the air.",
            "the soulbound blade carved cleanly through the air");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Fact]
    public void Analyze_HyphenCompound_WithProvidedScoringTokens_KeepsSentenceEqual()
    {
        var options = new TextDiffScoringOptions(
            ReferenceTokens: new[] { "the", "soul", "bound", "blade" },
            HypothesisTokens: new[] { "the", "soulbound", "blade" },
            ReferencePhonemeVariants:
            [
                null,
                new[] { "S OW1 L" },
                new[] { "B AW1 N D" },
                null
            ],
            HypothesisPhonemeVariants:
            [
                null,
                new[] { "S OW1 L B AW1 N D" },
                null
            ],
            UseExactPhonemeEquivalence: false);

        var result = TextDiffAnalyzer.Analyze(
            "the soul-bound blade",
            "the soulbound blade",
            options);

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
    }

    [Theory]
    [InlineData("Tell you the story", "Tell \u2018you the story")]
    [InlineData("Tell you the story", "Tell 'you the story")]
    [InlineData("how are you doing", "\u2018how are you doing")]
    public void Analyze_SmartQuoteBeforeWord_NoFalsePositive(string reference, string hypothesis)
    {
        var result = TextDiffAnalyzer.Analyze(reference, hypothesis);

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
    }

    [Fact]
    public void Analyze_DisplayDiff_DoesNotVerbalizeGroupedNumbers()
    {
        var result = TextDiffAnalyzer.Analyze(
            "4,224 895x favorable",
            "4,224, 895 times favorable");

        var flattened = result.Diff.Ops
            .SelectMany(op => op.Tokens)
            .ToArray();

        Assert.DoesNotContain("four", flattened);
        Assert.DoesNotContain("hundred", flattened);
        Assert.Contains("4224", flattened);
        Assert.Contains("895", flattened);
    }

    [Fact]
    public void Analyze_NumberDigitVsWord_KeepsScoringAndDisplayEqual()
    {
        var result = TextDiffAnalyzer.Analyze("Chapter 2", "Chapter two");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Fact]
    public void Analyze_MultiWordNumberVsDigit_KeepsScoringAndDisplayEqual()
    {
        var result = TextDiffAnalyzer.Analyze("Chapter 21", "Chapter twenty one");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Fact]
    public void Analyze_NumberEquivalence_DoesNotHideNeighboringTextDifferences()
    {
        var result = TextDiffAnalyzer.Analyze("Chapter 2 ends", "Chapter two end");
        var changedTokens = result.Diff.Ops
            .Where(op => op.Operation != "equal")
            .SelectMany(op => op.Tokens)
            .ToArray();

        Assert.DoesNotContain("2", changedTokens);
        Assert.DoesNotContain("two", changedTokens);
        Assert.Contains("ends", changedTokens);
        Assert.Contains("end", changedTokens);
    }

    [Fact]
    public void Analyze_NextWordHeadBleed_KeepsPluralSBoundaryEqual()
    {
        var result = TextDiffAnalyzer.Analyze("fluid swirled", "fluids swirled");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Fact]
    public void Analyze_PreviousWordTailBleed_KeepsLeadingLetterBoundaryEqual()
    {
        var result = TextDiffAnalyzer.Analyze("and Ward found", "and Dward found");

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }

    [Theory]
    [InlineData("Ward found", "Dward found", "dward")]
    [InlineData("fluid swirled", "fluids twirled", "fluids")]
    public void Analyze_BoundaryBleedGuard_DoesNotHideUnsupportedExtraLetters(
        string reference,
        string hypothesis,
        string expectedChangedToken)
    {
        var result = TextDiffAnalyzer.Analyze(reference, hypothesis);
        var changedTokens = result.Diff.Ops
            .Where(op => op.Operation != "equal")
            .SelectMany(op => op.Tokens)
            .ToArray();

        Assert.True(result.Metrics.Wer > 0);
        Assert.Contains(expectedChangedToken, changedTokens);
    }

    [Fact]
    public void Analyze_ExactPhonemeEquivalentDisplayTokens_SuppressesNameSpellingNoise()
    {
        var options = new TextDiffScoringOptions(
            ReferenceTokens: new[] { "nudging", "haley" },
            HypothesisTokens: new[] { "nudging", "hayley" },
            ReferencePhonemeVariants: new string[]?[] { null, new[] { "HH EY1 L IY0" } },
            HypothesisPhonemeVariants: new string[]?[] { null, new[] { "HH EY1 L IY0" } },
            UseExactPhonemeEquivalence: true);

        var result = TextDiffAnalyzer.Analyze("nudging Haley", "nudging Hayley", options);

        Assert.Equal(0.0, result.Metrics.Wer);
        Assert.Equal(0, result.Metrics.MissingRuns);
        Assert.Equal(0, result.Metrics.ExtraRuns);
        Assert.Equal(0, result.Diff.Stats.Insertions);
        Assert.Equal(0, result.Diff.Stats.Deletions);
        Assert.All(result.Diff.Ops, op => Assert.Equal("equal", op.Operation));
    }
}
