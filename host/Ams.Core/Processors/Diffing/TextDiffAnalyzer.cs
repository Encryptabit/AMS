using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using DiffMatchPatch;

namespace Ams.Core.Processors.Diffing;

public static class TextDiffAnalyzer
{
    public static TextDiffResult Analyze(string? referenceText, string? hypothesisText)
    {
        var scoringReference = NormalizeForScoring(referenceText);
        var scoringHypothesis = NormalizeForScoring(hypothesisText);
        var scoringReferenceTokens = Tokenize(scoringReference);
        var scoringHypothesisTokens = Tokenize(scoringHypothesis);

        var displayReference = NormalizeForDisplay(referenceText);
        var displayHypothesis = NormalizeForDisplay(hypothesisText);
        var displayReferenceTokens = Tokenize(displayReference);
        var displayHypothesisTokens = Tokenize(displayHypothesis);

        var displayTokenDiff = BuildTokenDiff(displayReferenceTokens, displayHypothesisTokens);

        var ops = new List<HydratedDiffOp>(displayTokenDiff.Diffs.Count);
        foreach (var diff in displayTokenDiff.Diffs)
        {
            var tokens = DecodeTokens(diff.text, displayTokenDiff.Dictionary);
            if (tokens.Count == 0)
            {
                continue;
            }

            var op = new HydratedDiffOp(MapOperation(diff.operation), tokens.ToArray());
            ops.Add(op);
        }

        var displayStats = BuildStats(displayReferenceTokens.Count, displayHypothesisTokens.Count, displayTokenDiff.Diffs);

        // Keep scoring normalization independent from reviewer-facing diff tokens.
        var scoringTokenDiff = BuildTokenDiff(scoringReferenceTokens, scoringHypothesisTokens);
        var scoringStats = BuildStats(scoringReferenceTokens.Count, scoringHypothesisTokens.Count, scoringTokenDiff.Diffs);

        var metrics = BuildMetrics(scoringReference, scoringHypothesis, scoringStats);
        var coverage = scoringStats.ReferenceTokens == 0
            ? (scoringStats.HypothesisTokens == 0 ? 1.0 : 0.0)
            : Math.Max(0.0,
                1.0 - Math.Min(1.0, (double)scoringStats.Deletions / Math.Max(1.0, scoringStats.ReferenceTokens)));

        var diffPayload = new HydratedDiff(ops, displayStats);
        return new TextDiffResult(metrics, diffPayload, coverage);
    }

    private static HydratedDiffStats BuildStats(int referenceTokenCount, int hypothesisTokenCount, IReadOnlyList<Diff> diffs)
    {
        int equalTokens = 0;
        int insertTokens = 0;
        int deleteTokens = 0;

        foreach (var diff in diffs)
        {
            var tokenCount = diff.text.Length;
            if (tokenCount == 0)
            {
                continue;
            }

            switch (diff.operation)
            {
                case Operation.EQUAL:
                    equalTokens += tokenCount;
                    break;
                case Operation.INSERT:
                    insertTokens += tokenCount;
                    break;
                case Operation.DELETE:
                    deleteTokens += tokenCount;
                    break;
            }
        }

        return new HydratedDiffStats(
            ReferenceTokens: referenceTokenCount,
            HypothesisTokens: hypothesisTokenCount,
            Matches: equalTokens,
            Insertions: insertTokens,
            Deletions: deleteTokens);
    }

    private static SentenceMetrics BuildMetrics(string reference, string hypothesis, HydratedDiffStats stats)
    {
        var denominator = Math.Max(1.0, stats.ReferenceTokens);
        var wer = stats.ReferenceTokens == 0
            ? (stats.HypothesisTokens > 0 ? 1.0 : 0.0)
            : Math.Min(1.0, (stats.Deletions + stats.Insertions) / denominator);
        var spanWer = stats.ReferenceTokens == 0
            ? 0.0
            : Math.Min(1.0, stats.Deletions / denominator);

        var cer = ComputeCer(reference, hypothesis);
        return new SentenceMetrics(wer, cer, spanWer, stats.Deletions, stats.Insertions);
    }

    private static double ComputeCer(string reference, string hypothesis)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return string.IsNullOrEmpty(hypothesis) ? 0.0 : 1.0;
        }

        var dmp = new diff_match_patch();
        var diffs = dmp.diff_main(reference, hypothesis, false);
        dmp.diff_cleanupSemantic(diffs);

        int equalChars = 0;
        int insertChars = 0;
        foreach (var diff in diffs)
        {
            switch (diff.operation)
            {
                case Operation.EQUAL:
                    equalChars += diff.text.Length;
                    break;
                case Operation.INSERT:
                    insertChars += diff.text.Length;
                    break;
            }
        }

        var refLength = Math.Max(1, reference.Length);
        return Math.Min(1.0, ((reference.Length - equalChars) + insertChars) / refLength);
    }

    private static string NormalizeForScoring(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : TextNormalizer.Normalize(value, expandContractions: true, removeNumbers: false);

    private static string NormalizeForDisplay(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : TextNormalizer.Normalize(value, expandContractions: false, removeNumbers: false);

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        return TextNormalizer.TokenizeWords(text).ToList();
    }

    private static TokenDiffResult BuildTokenDiff(IReadOnlyList<string> referenceTokens,
        IReadOnlyList<string> hypothesisTokens)
    {
        if (referenceTokens.Count == 0 && hypothesisTokens.Count == 0)
        {
            return new TokenDiffResult(new List<Diff>(), new List<string>());
        }

        var dictionary = new List<string>();
        var map = new Dictionary<string, int>(StringComparer.Ordinal);

        string Encode(IReadOnlyList<string> tokens)
        {
            if (tokens.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(tokens.Count);
            foreach (var token in tokens)
            {
                if (!map.TryGetValue(token, out var index))
                {
                    if (dictionary.Count >= char.MaxValue)
                    {
                        throw new InvalidOperationException("Diff token dictionary exceeded supported size.");
                    }

                    index = dictionary.Count;
                    dictionary.Add(token);
                    map[token] = index;
                }

                sb.Append((char)index);
            }

            return sb.ToString();
        }

        var encodedReference = Encode(referenceTokens);
        var encodedHypothesis = Encode(hypothesisTokens);

        var dmp = new diff_match_patch();
        var diffs = dmp.diff_main(encodedReference, encodedHypothesis, false);
        dmp.diff_cleanupSemantic(diffs);

        return new TokenDiffResult(diffs, dictionary);
    }

    private static IReadOnlyList<string> DecodeTokens(string encoded, IReadOnlyList<string> dictionary)
    {
        if (string.IsNullOrEmpty(encoded) || dictionary.Count == 0)
        {
            return Array.Empty<string>();
        }

        var tokens = new List<string>(encoded.Length);
        foreach (var ch in encoded)
        {
            var index = ch;
            if (index >= 0 && index < dictionary.Count)
            {
                tokens.Add(dictionary[index]);
            }
        }

        return tokens;
    }

    private static string MapOperation(Operation op) => op switch
    {
        Operation.DELETE => "delete",
        Operation.INSERT => "insert",
        _ => "equal"
    };

    private sealed record TokenDiffResult(List<Diff> Diffs, List<string> Dictionary);
}

public sealed record TextDiffResult(SentenceMetrics Metrics, HydratedDiff Diff, double Coverage);
