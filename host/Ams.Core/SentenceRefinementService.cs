using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Align;
using Ams.Core.Align.Tx;
using Ams.Core.Util;

namespace Ams.Core;

public record SentenceRefined(double Start, double End, int StartWordIdx, int EndWordIdx);

public record SilenceInfo(double Start, double End, double Duration, double Confidence);

public sealed record SentenceRefinementContext(
    IReadOnlyDictionary<string, FragmentTiming> Fragments,
    IReadOnlyList<SilenceEvent> Silences,
    double MinTailSec,
    double MaxSnapAheadSec)
{
    public bool TryGetFragment(int sentenceId, out FragmentTiming fragment)
    {
        if (Fragments.TryGetValue(sentenceId.ToString(CultureInfo.InvariantCulture), out var value))
        {
            fragment = value;
            return true;
        }

        fragment = null!;
        return false;
    }
}

public sealed class SentenceRefinementService
{
    public Task<IReadOnlyList<SentenceRefined>> RefineAsync(
        string audioPath,
        TranscriptIndex tx,
        AsrResponse asr,
        SentenceRefinementContext context,
        CancellationToken ct)
    {
        if (tx is null) throw new ArgumentNullException(nameof(tx));
        if (asr is null) throw new ArgumentNullException(nameof(asr));
        if (context is null) throw new ArgumentNullException(nameof(context));

        var sentences = BuildSentences(tx, asr, context);
        return Task.FromResult<IReadOnlyList<SentenceRefined>>(sentences);
    }

    private static List<SentenceRefined> BuildSentences(
        TranscriptIndex tx,
        AsrResponse asr,
        SentenceRefinementContext context)
    {
        var orderedSentences = tx.Sentences.OrderBy(s => s.Id).ToList();
        var results = new List<SentenceRefined>(orderedSentences.Count);

        double previousEnd = 0.0;
        int previousTokenEnd = -1;
        int fragmentBacked = 0;
        int fragmentFallback = 0;

        foreach (var sentence in orderedSentences)
        {
            var (startIdx, endIdx) = ResolveTokenRange(sentence, asr.Tokens.Length, previousTokenEnd);

            var start = DetermineStart(sentence.Id, asr, startIdx, context, previousEnd, out var usedFragment);
            var end = DetermineEnd(sentence.Id, asr, endIdx, context, start);

            if (start < previousEnd)
            {
                start = previousEnd;
            }

            if (end < start + context.MinTailSec)
            {
                end = start + context.MinTailSec;
            }

            start = Precision.RoundToMicroseconds(start);
            end = Precision.RoundToMicroseconds(end);

            results.Add(new SentenceRefined(start, end, startIdx, endIdx));

            previousEnd = end;
            previousTokenEnd = endIdx;

            if (usedFragment) fragmentBacked++;
            else fragmentFallback++;
        }

        Console.WriteLine($"[refine] mapped sentences: fragments-backed={fragmentBacked}, fallback={fragmentFallback}");
        return results;
    }

    private static (int start, int end) ResolveTokenRange(SentenceAlign sentence, int tokenLength, int previousTokenEnd)
    {
        int start = sentence.ScriptRange?.Start ?? (previousTokenEnd + 1);
        int end = sentence.ScriptRange?.End ?? start;

        if (tokenLength == 0)
        {
            start = 0;
            end = -1;
        }
        else
        {
            start = Math.Clamp(start, 0, tokenLength - 1);
            end = Math.Clamp(end, start, tokenLength - 1);
        }

        return (start, end);
    }

    private static double DetermineStart(
        int sentenceId,
        AsrResponse asr,
        int tokenStart,
        SentenceRefinementContext context,
        double previousEnd,
        out bool usedFragment)
    {
        if (context.TryGetFragment(sentenceId, out var fragment))
        {
            usedFragment = true;
            return Math.Max(previousEnd, fragment.Start);
        }

        usedFragment = false;
        return tokenStart >= 0 && tokenStart < asr.Tokens.Length
            ? asr.Tokens[tokenStart].StartTime
            : previousEnd;
    }

    private static double DetermineEnd(
        int sentenceId,
        AsrResponse asr,
        int tokenEnd,
        SentenceRefinementContext context,
        double start)
    {
        double candidate = start + context.MinTailSec;

        if (context.TryGetFragment(sentenceId, out var fragment))
        {
            candidate = Math.Max(candidate, fragment.End);
        }
        else if (tokenEnd >= 0 && tokenEnd < asr.Tokens.Length)
        {
            candidate = Math.Max(candidate, GetTokenEnd(asr, tokenEnd));
        }

        var snapped = FindSilenceAfter(asr, tokenEnd, context, candidate);
        if (snapped.HasValue)
        {
            candidate = Math.Max(candidate, snapped.Value);
        }

        return candidate;
    }

    private static double GetTokenEnd(AsrResponse asr, int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= asr.Tokens.Length)
            return 0.0;

        var token = asr.Tokens[tokenIndex];
        return token.StartTime + token.Duration;
    }

    private static double? FindSilenceAfter(
        AsrResponse asr,
        int tokenEnd,
        SentenceRefinementContext context,
        double fallbackEnd)
    {
        if (context.Silences.Count == 0 || context.MaxSnapAheadSec <= 0)
            return null;

        var lastTokenEnd = tokenEnd >= 0 && tokenEnd < asr.Tokens.Length
            ? GetTokenEnd(asr, tokenEnd)
            : fallbackEnd;

        foreach (var silence in context.Silences)
        {
            if (silence.Start < lastTokenEnd - 1e-6)
            {
                continue;
            }

            var delta = silence.Start - lastTokenEnd;
            if (delta <= context.MaxSnapAheadSec)
            {
                return Math.Max(lastTokenEnd, silence.Start);
            }

            break;
        }

        return null;
    }
}

