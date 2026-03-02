using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using DiffMatchPatch;

namespace Ams.Core.Processors.Diffing;

public static class TextDiffAnalyzer
{
    private static readonly HashSet<string> ApostropheSuffixTokens = new(StringComparer.Ordinal)
    {
        "s",
        "d",
        "m",
        "re",
        "ve",
        "ll",
        "t"
    };

    public static TextDiffResult Analyze(string? referenceText, string? hypothesisText)
        => Analyze(referenceText, hypothesisText, null);

    public static TextDiffResult Analyze(
        string? referenceText,
        string? hypothesisText,
        TextDiffScoringOptions? scoringOptions)
    {
        var scoringReference = NormalizeForScoring(referenceText);
        var scoringHypothesis = NormalizeForScoring(hypothesisText);
        var scoringReferencePayload = ResolveScoringPayload(
            scoringReference,
            scoringOptions?.ReferenceTokens,
            scoringOptions?.ReferencePhonemeVariants);
        var scoringHypothesisPayload = ResolveScoringPayload(
            scoringHypothesis,
            scoringOptions?.HypothesisTokens,
            scoringOptions?.HypothesisPhonemeVariants);
        var scoringReferenceTokens = scoringReferencePayload.Tokens;
        var scoringHypothesisTokens = scoringHypothesisPayload.Tokens;

        var scoringReferencePhonemes = AlignPhonemePayload(
            scoringReferencePayload.Phonemes,
            scoringReferenceTokens.Count);
        var scoringHypothesisPhonemes = AlignPhonemePayload(
            scoringHypothesisPayload.Phonemes,
            scoringHypothesisTokens.Count);

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
        if (scoringOptions?.UseExactPhonemeEquivalence == true)
        {
            scoringStats = ApplyExactPhonemeEquivalence(
                scoringStats,
                scoringTokenDiff.Diffs,
                scoringReferencePhonemes,
                scoringHypothesisPhonemes);
        }

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
            : TextNormalizer.Normalize(value, expandContractions: false, removeNumbers: false);

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

        var tokens = new List<string>();
        TextNormalizer.TokenizeWords(text, tokens);
        CollapseApostropheSuffixTokens(tokens);
        return tokens;
    }

    private static ScoringTokenPayload ResolveScoringPayload(
        string normalizedFallback,
        IReadOnlyList<string>? providedTokens,
        IReadOnlyList<string[]?>? providedPhonemes)
    {
        if (providedTokens is null)
        {
            return new ScoringTokenPayload(Tokenize(normalizedFallback), providedPhonemes);
        }

        var tokens = new List<string>(providedTokens.Count);
        List<string[]?>? phonemes = providedPhonemes is null ? null : new List<string[]?>(providedTokens.Count);
        for (var i = 0; i < providedTokens.Count; i++)
        {
            var token = providedTokens[i];
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            tokens.Add(token.Trim());

            if (phonemes is not null)
            {
                phonemes.Add(i < providedPhonemes!.Count ? providedPhonemes[i] : null);
            }
        }

        CollapseApostropheSuffixTokens(tokens, phonemes);
        return new ScoringTokenPayload(tokens, phonemes);
    }

    private static void CollapseApostropheSuffixTokens(List<string> tokens, List<string[]?>? phonemes = null)
    {
        if (tokens.Count < 2)
        {
            return;
        }

        var write = 1;
        for (var read = 1; read < tokens.Count; read++)
        {
            var current = tokens[read];
            var previous = tokens[write - 1];

            if (TryGetApostropheSuffix(current, out var suffix) && CanAttachApostropheSuffix(previous, suffix))
            {
                tokens[write - 1] = $"{previous}'{suffix}";
                if (phonemes is not null)
                {
                    phonemes[write - 1] = MergePhonemeVariants(phonemes[write - 1], phonemes[read]);
                }

                continue;
            }

            if (write != read)
            {
                tokens[write] = current;
                if (phonemes is not null)
                {
                    phonemes[write] = phonemes[read];
                }
            }

            write++;
        }

        if (write < tokens.Count)
        {
            tokens.RemoveRange(write, tokens.Count - write);
        }

        if (phonemes is not null && write < phonemes.Count)
        {
            phonemes.RemoveRange(write, phonemes.Count - write);
        }
    }

    private static bool TryGetApostropheSuffix(string token, out string suffix)
    {
        suffix = string.Empty;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var span = token.AsSpan().Trim();
        var start = 0;
        while (start < span.Length && span[start] == '\'')
        {
            start++;
        }

        if (start >= span.Length)
        {
            return false;
        }

        for (var i = start; i < span.Length; i++)
        {
            if (!char.IsLetter(span[i]))
            {
                return false;
            }
        }

        suffix = span[start..].ToString().ToLowerInvariant();
        return ApostropheSuffixTokens.Contains(suffix);
    }

    private static bool CanAttachApostropheSuffix(string baseToken, string suffix)
    {
        if (string.IsNullOrWhiteSpace(baseToken))
        {
            return false;
        }

        var hasWordChars = false;
        for (var i = 0; i < baseToken.Length; i++)
        {
            if (char.IsLetterOrDigit(baseToken[i]))
            {
                hasWordChars = true;
                break;
            }
        }

        if (!hasWordChars)
        {
            return false;
        }

        if (string.Equals(suffix, "t", StringComparison.Ordinal))
        {
            return baseToken.EndsWith("n", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private static string[]? MergePhonemeVariants(string[]? primary, string[]? secondary)
    {
        var primaryHasValues = primary is { Length: > 0 };
        var secondaryHasValues = secondary is { Length: > 0 };

        if (!primaryHasValues)
        {
            return secondaryHasValues ? secondary : null;
        }

        if (!secondaryHasValues)
        {
            return primary;
        }

        var merged = new List<string>(primary!.Length + secondary!.Length);
        foreach (var variant in primary)
        {
            if (!string.IsNullOrWhiteSpace(variant))
            {
                merged.Add(variant);
            }
        }

        foreach (var variant in secondary)
        {
            if (string.IsNullOrWhiteSpace(variant))
            {
                continue;
            }

            if (!merged.Any(existing => string.Equals(existing, variant, StringComparison.OrdinalIgnoreCase)))
            {
                merged.Add(variant);
            }
        }

        return merged.Count == 0 ? null : merged.ToArray();
    }

    private static IReadOnlyList<string[]?>? AlignPhonemePayload(IReadOnlyList<string[]?>? variants, int tokenCount)
    {
        if (variants is null)
        {
            return null;
        }

        if (tokenCount <= 0)
        {
            return Array.Empty<string[]?>();
        }

        if (variants.Count == tokenCount)
        {
            return variants;
        }

        var aligned = new string[]?[tokenCount];
        var copyCount = Math.Min(tokenCount, variants.Count);
        for (int i = 0; i < copyCount; i++)
        {
            aligned[i] = variants[i];
        }

        return aligned;
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

    private static HydratedDiffStats ApplyExactPhonemeEquivalence(
        HydratedDiffStats stats,
        IReadOnlyList<Diff> diffs,
        IReadOnlyList<string[]?>? referencePhonemes,
        IReadOnlyList<string[]?>? hypothesisPhonemes)
    {
        if (referencePhonemes is null || hypothesisPhonemes is null || diffs.Count == 0)
        {
            return stats;
        }

        int equivalentPairs = 0;
        int referenceCursor = 0;
        int hypothesisCursor = 0;

        for (int i = 0; i < diffs.Count; i++)
        {
            var diff = diffs[i];
            var tokenCount = diff.text.Length;
            if (tokenCount == 0)
            {
                continue;
            }

            if (i + 1 < diffs.Count &&
                TryGetDeleteInsertPair(diff, diffs[i + 1], out var deleteCount, out var insertCount))
            {
                var pairCount = Math.Min(deleteCount, insertCount);
                for (int p = 0; p < pairCount; p++)
                {
                    var refIdx = referenceCursor + p;
                    var hypIdx = hypothesisCursor + p;
                    if (HasExactPhonemeMatch(GetPhonemes(referencePhonemes, refIdx),
                            GetPhonemes(hypothesisPhonemes, hypIdx)))
                    {
                        equivalentPairs++;
                    }
                }

                referenceCursor += deleteCount;
                hypothesisCursor += insertCount;
                i++;
                continue;
            }

            switch (diff.operation)
            {
                case Operation.EQUAL:
                    referenceCursor += tokenCount;
                    hypothesisCursor += tokenCount;
                    break;
                case Operation.DELETE:
                    referenceCursor += tokenCount;
                    break;
                case Operation.INSERT:
                    hypothesisCursor += tokenCount;
                    break;
            }
        }

        if (equivalentPairs <= 0)
        {
            return stats;
        }

        return new HydratedDiffStats(
            stats.ReferenceTokens,
            stats.HypothesisTokens,
            Matches: stats.Matches + equivalentPairs,
            Insertions: Math.Max(0, stats.Insertions - equivalentPairs),
            Deletions: Math.Max(0, stats.Deletions - equivalentPairs));
    }

    private static bool TryGetDeleteInsertPair(Diff current, Diff next, out int deleteCount, out int insertCount)
    {
        deleteCount = 0;
        insertCount = 0;

        if (current.operation == Operation.DELETE && next.operation == Operation.INSERT)
        {
            deleteCount = current.text.Length;
            insertCount = next.text.Length;
            return true;
        }

        if (current.operation == Operation.INSERT && next.operation == Operation.DELETE)
        {
            deleteCount = next.text.Length;
            insertCount = current.text.Length;
            return true;
        }

        return false;
    }

    private static string[]? GetPhonemes(IReadOnlyList<string[]?> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            return null;
        }

        var entry = list[index];
        return entry is { Length: > 0 } ? entry : null;
    }

    private static bool HasExactPhonemeMatch(string[]? referenceVariants, string[]? hypothesisVariants)
    {
        if (referenceVariants is not { Length: > 0 } refList || hypothesisVariants is not { Length: > 0 } hypList)
        {
            return false;
        }

        foreach (var reference in refList)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                continue;
            }

            var normalizedReference = NormalizePhonemeVariant(reference);
            if (normalizedReference.Length == 0)
            {
                continue;
            }

            foreach (var hypothesis in hypList)
            {
                if (string.IsNullOrWhiteSpace(hypothesis))
                {
                    continue;
                }

                var normalizedHypothesis = NormalizePhonemeVariant(hypothesis);
                if (normalizedHypothesis.Length == 0)
                {
                    continue;
                }

                if (string.Equals(normalizedReference, normalizedHypothesis, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string NormalizePhonemeVariant(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string MapOperation(Operation op) => op switch
    {
        Operation.DELETE => "delete",
        Operation.INSERT => "insert",
        _ => "equal"
    };

    private sealed record ScoringTokenPayload(List<string> Tokens, IReadOnlyList<string[]?>? Phonemes);

    private sealed record TokenDiffResult(List<Diff> Diffs, List<string> Dictionary);
}

public sealed record TextDiffResult(SentenceMetrics Metrics, HydratedDiff Diff, double Coverage);

public sealed record TextDiffScoringOptions(
    IReadOnlyList<string>? ReferenceTokens = null,
    IReadOnlyList<string>? HypothesisTokens = null,
    IReadOnlyList<string[]?>? ReferencePhonemeVariants = null,
    IReadOnlyList<string[]?>? HypothesisPhonemeVariants = null,
    bool UseExactPhonemeEquivalence = false);
