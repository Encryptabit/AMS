using Ams.Core.Processors.Alignment.Mfa;

public sealed class MfaTimingMergerTests
{
    [Fact]
    public void MergeAndApply_AssignsTimingsToQuoteWrappedBoundaryWords()
    {
        var wordTiming = new Dictionary<int, (double Start, double End, double Duration)>();
        (double Start, double End, double Duration)? sentenceTiming = null;

        var report = MfaTimingMerger.MergeAndApply(
            new[]
            {
                new TextGridWord("master", 1.00, 1.39),
                new TextGridWord("sergeant", 1.39, 2.05)
            },
            idx => idx switch
            {
                0 => "\"Master",
                1 => "Sergeant.\"",
                _ => string.Empty
            },
            chapterStartBookIdx: 0,
            chapterEndBookIdx: 1,
            wordTargets:
            [
                new WordTarget(0, (start, end, duration) => wordTiming[0] = (start, end, duration)),
                new WordTarget(1, (start, end, duration) => wordTiming[1] = (start, end, duration))
            ],
            sentenceTargets:
            [
                new SentenceTarget(0, 1, (start, end, duration) => sentenceTiming = (start, end, duration))
            ]);

        Assert.Equal(2, report.WordsUpdated);
        Assert.Equal(1, report.SentencesUpdated);

        Assert.True(wordTiming.TryGetValue(0, out var firstWord));
        Assert.True(wordTiming.TryGetValue(1, out var secondWord));
        Assert.InRange(firstWord.Start, 0.999, 1.001);
        Assert.InRange(firstWord.End, 1.389, 1.391);
        Assert.InRange(secondWord.Start, 1.389, 1.391);
        Assert.InRange(secondWord.End, 2.049, 2.051);

        Assert.NotNull(sentenceTiming);
        Assert.InRange(sentenceTiming!.Value.Start, 0.999, 1.001);
        Assert.InRange(sentenceTiming.Value.End, 2.049, 2.051);
    }

    [Fact]
    public void MergeAndApply_NormalizesNumericBookTokensConsistentlyWithMfaCorpus()
    {
        (double Start, double End, double Duration)? updated = null;

        var report = MfaTimingMerger.MergeAndApply(
            new[]
            {
                new TextGridWord("thirteen", 0.68, 1.25)
            },
            _ => "13",
            chapterStartBookIdx: 0,
            chapterEndBookIdx: 0,
            wordTargets:
            [
                new WordTarget(0, (start, end, duration) => updated = (start, end, duration))
            ],
            sentenceTargets: Array.Empty<SentenceTarget>());

        Assert.Equal(1, report.WordsUpdated);
        Assert.NotNull(updated);
        Assert.InRange(updated!.Value.Start, 0.679, 0.681);
        Assert.InRange(updated.Value.End, 1.249, 1.251);
        Assert.InRange(updated.Value.Duration, 0.569, 0.571);
    }
}
