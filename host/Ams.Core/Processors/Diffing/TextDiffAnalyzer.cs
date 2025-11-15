using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using DiffMatchPatch;

namespace Ams.Core.Processors.Diffing;

public static class TextDiffAnalyzer
{

    public static TextDiffResult Analyze(string? referenceText, string? hypothesisText)
    {
        var reference = Normalize(referenceText);
        var hypothesis = Normalize(hypothesisText);

        var referenceTokens = Tokenize(reference);
        var hypothesisTokens = Tokenize(hypothesis);

        var tokenDiff = BuildTokenDiff(referenceTokens, hypothesisTokens);

        var ops = new List<HydratedDiffOp>(tokenDiff.Diffs.Count);
        int equalTokens = 0;
        int insertTokens = 0;
        int deleteTokens = 0;

        foreach (var diff in tokenDiff.Diffs)
        {
            var tokens = DecodeTokens(diff.text, tokenDiff.Dictionary);
            if (tokens.Count == 0)
            {
                continue;
            }

            switch (diff.operation)
            {
                case Operation.EQUAL:
                    equalTokens += tokens.Count;
                    break;
                case Operation.INSERT:
                    insertTokens += tokens.Count;
                    break;
                case Operation.DELETE:
                    deleteTokens += tokens.Count;
                    break;
            }

            var op = new HydratedDiffOp(MapOperation(diff.operation), tokens.ToArray());
            ops.Add(op);
        }

        var stats = new HydratedDiffStats(
            ReferenceTokens: referenceTokens.Count,
            HypothesisTokens: hypothesisTokens.Count,
            Matches: equalTokens,
            Insertions: insertTokens,
            Deletions: deleteTokens);

        var metrics = BuildMetrics(reference, hypothesis, stats);
        var coverage = stats.ReferenceTokens == 0
            ? (stats.HypothesisTokens == 0 ? 1.0 : 0.0)
            : Math.Max(0.0, 1.0 - Math.Min(1.0, (double)stats.Deletions / Math.Max(1.0, stats.ReferenceTokens)));

        var diffPayload = new HydratedDiff(ops, stats);
        return new TextDiffResult(metrics, diffPayload, coverage);
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

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : TextNormalizer.Normalize(value, expandContractions: true, removeNumbers: false);

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        return TextNormalizer.TokenizeWords(text).ToList();
    }

    private static TokenDiffResult BuildTokenDiff(IReadOnlyList<string> referenceTokens, IReadOnlyList<string> hypothesisTokens)
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
        PostProcessGlueTokens(diffs, dictionary);

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


    private static void PostProcessGlueTokens(List<Diff> diffs, IReadOnlyList<string> dictionary)
    {
        if (diffs.Count < 2)
        {
            return;
        }

        var normalizedCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string NormalizeToken(string token)
        {
            if (normalizedCache.TryGetValue(token, out var existing))
            {
                return existing;
            }

            var normalized = string.Concat(PronunciationHelper.ExtractPronunciationParts(token));
            normalizedCache[token] = normalized;
            return normalized;
        }

        int i = 0;
        while (i < diffs.Count - 1)
        {
            var first = diffs[i];
            var second = diffs[i + 1];

            if (!IsGlueCandidate(first, second))
            {
                i++;
                continue;
            }

            var deletes = DecodeTokens(first.text, dictionary).ToList();
            var inserts = DecodeTokens(second.text, dictionary).ToList();
            if (deletes.Count == 0 || inserts.Count == 0)
            {
                i++;
                continue;
            }

            if (!TryMatchGlue(deletes, inserts, NormalizeToken, out var deleteIndex, out var insertSpan, out var mergedText))
            {
                i++;
                continue;
            }

            var replacement = new List<Diff>();

            if (deleteIndex >= 0 && deleteIndex < deletes.Count)
            {
                deletes.RemoveAt(deleteIndex);
                if (deletes.Count > 0)
                {
                    replacement.Add(new Diff(Operation.DELETE, EncodeTokens(deletes, dictionary)));
                }
            }
            else
            {
                replacement.Add(new Diff(first.operation, first.text));
            }

            replacement.Add(new Diff(Operation.EQUAL, EncodeTokens(new[] { mergedText }, dictionary)));

            if (insertSpan.length > 0 && insertSpan.start < inserts.Count)
            {
                inserts.RemoveRange(insertSpan.start, Math.Min(insertSpan.length, inserts.Count - insertSpan.start));
                if (inserts.Count > 0)
                {
                    replacement.Add(new Diff(Operation.INSERT, EncodeTokens(inserts, dictionary)));
                }
            }
            else
            {
                replacement.Add(new Diff(second.operation, second.text));
            }

            diffs.RemoveAt(i + 1);
            diffs.RemoveAt(i);
            diffs.InsertRange(i, replacement);

            i = Math.Max(0, i - 1);
        }
    }

    private static bool IsGlueCandidate(Diff first, Diff second)
        => ((first.operation == Operation.DELETE && second.operation == Operation.INSERT)
            || (first.operation == Operation.INSERT && second.operation == Operation.DELETE))
           && first.text.Length > 0
           && second.text.Length > 0;

    private static bool TryMatchGlue(
        List<string> deletes,
        List<string> inserts,
        Func<string, string> normalize,
        out int deleteIndex,
        out (int start, int length) insertSpan,
        out string mergedText)
    {
        deleteIndex = -1;
        insertSpan = (0, 0);
        mergedText = string.Empty;

        var normalizedInserts = inserts.Select(normalize).ToList();
        var normalizedDeletes = deletes.Select(normalize).ToList();

        for (int di = 0; di < normalizedDeletes.Count; di++)
        {
            var deleteToken = normalizedDeletes[di];
            if (string.IsNullOrEmpty(deleteToken))
            {
                continue;
            }

            for (int start = 0; start < normalizedInserts.Count; start++)
            {
                var builder = new StringBuilder();
                int length = 0;
                for (int span = start; span < normalizedInserts.Count; span++)
                {
                    builder.Append(normalizedInserts[span]);
                    length++;

                    if (builder.ToString().Equals(deleteToken, StringComparison.OrdinalIgnoreCase))
                    {
                        deleteIndex = di;
                        insertSpan = (start, length);
                        mergedText = deletes[di];
                        return true;
                    }
                }
            }
        }

        for (int ii = 0; ii < normalizedInserts.Count; ii++)
        {
            var insertToken = normalizedInserts[ii];
            if (string.IsNullOrEmpty(insertToken))
            {
                continue;
            }

            for (int start = 0; start < normalizedDeletes.Count; start++)
            {
                var builder = new StringBuilder();
                int length = 0;
                for (int span = start; span < normalizedDeletes.Count; span++)
                {
                    builder.Append(normalizedDeletes[span]);
                    length++;

                    if (builder.ToString().Equals(insertToken, StringComparison.OrdinalIgnoreCase))
                    {
                        deleteIndex = start;
                        insertSpan = (ii, 1);
                        mergedText = inserts[ii];
                        return true;
                    }
                }
            }
        }

        return false;
    }


    private static int IndexOfToken(IReadOnlyList<string> dictionary, string token)
    {
        for (int i = 0; i < dictionary.Count; i++)
        {
            if (string.Equals(dictionary[i], token, StringComparison.Ordinal))
            {
                return i;
            }
        }
        return -1;
    }
    private static string EncodeTokens(IEnumerable<string> tokens, IReadOnlyList<string> dictionary)
    {
        var indices = new List<char>();
        foreach (var token in tokens)
        {
            var index = IndexOfToken(dictionary, token);
            if (index < 0)
            {
                continue;
            }

            indices.Add((char)index);
        }

        return new string(indices.ToArray());
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
