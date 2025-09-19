using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core;
using Ams.Core.Align.Tx;
using Ams.Core.Util;

namespace Ams.Core.Align;

public sealed record FragmentTiming(string ChunkId, int FragmentIndex, double Start, double End)
{
    public double Duration => Math.Max(0d, End - Start);
}

public sealed record SentenceTokenRange(int SentenceId, int? TokenStart, int? TokenEnd);

public sealed record TokenMapping(
    AsrResponse Asr,
    IReadOnlyList<SentenceTokenRange> Sentences,
    IReadOnlyList<WordAlign> WordAlignments)
{
    public SentenceTokenRange? TryGetSentence(int sentenceId)
        => Sentences.FirstOrDefault(s => s.SentenceId == sentenceId);
}

public sealed class ChapterAlignmentIndex
{
    private ChapterAlignmentIndex(Dictionary<int, FragmentTiming> fragments)
    {
        Fragments = fragments;
    }

    public IReadOnlyDictionary<int, FragmentTiming> Fragments { get; }

    public bool TryGetFragment(int sentenceId, out FragmentTiming timing)
        => Fragments.TryGetValue(sentenceId, out timing);

    public static ChapterAlignmentIndex Build(IReadOnlyList<ChunkAlignment> alignments, TokenMapping mapping)
    {
        if (alignments is null) throw new ArgumentNullException(nameof(alignments));
        if (mapping is null) throw new ArgumentNullException(nameof(mapping));

        var fragments = alignments
            .Where(a => a?.Fragments is not null && a.Fragments.Count > 0)
            .OrderBy(a => a.OffsetSec)
            .SelectMany(alignment => alignment.Fragments.Select((fragment, index) =>
                new FragmentTiming(
                    alignment.ChunkId,
                    index,
                    Precision.RoundToMicroseconds(alignment.OffsetSec + fragment.Begin),
                    Precision.RoundToMicroseconds(alignment.OffsetSec + fragment.End))))
            .Where(f => f.End > f.Start)
            .ToList();

        var used = new bool[fragments.Count];
        var bySentence = new Dictionary<int, FragmentTiming>(mapping.Sentences.Count);
        var orderedSentences = mapping.Sentences.OrderBy(s => s.SentenceId).ToList();

        int searchStart = 0;
        foreach (var sentence in orderedSentences)
        {
            var expectedStart = GetTokenStart(mapping.Asr, sentence.TokenStart);
            var (fragment, index) = FindBestFragment(fragments, used, searchStart, expectedStart);

            if (fragment is not null && index >= 0)
            {
                bySentence[sentence.SentenceId] = fragment;
                used[index] = true;
                searchStart = Math.Min(index + 1, fragments.Count);
            }
        }

        return new ChapterAlignmentIndex(bySentence);
    }

    private static (FragmentTiming? fragment, int index) FindBestFragment(
        IReadOnlyList<FragmentTiming> fragments,
        bool[] used,
        int searchStart,
        double? expectedStart)
    {
        const double MaxDriftSec = 1.5;

        if (!expectedStart.HasValue)
        {
            for (var i = searchStart; i < fragments.Count; i++)
            {
                if (used[i]) continue;
                return (fragments[i], i);
            }

            for (var i = 0; i < searchStart; i++)
            {
                if (used[i]) continue;
                return (fragments[i], i);
            }

        }
        else
        {
            FragmentTiming? best = null;
            var bestIndex = -1;
            var bestDelta = double.MaxValue;

            for (var i = searchStart; i < fragments.Count; i++)
            {
                if (used[i]) continue;
                var fragment = fragments[i];
                var delta = Math.Abs(fragment.Start - expectedStart.Value);

                if (delta < bestDelta)
                {
                    best = fragment;
                    bestIndex = i;
                    bestDelta = delta;
                }

                if (fragment.Start >= expectedStart.Value && delta <= bestDelta)
                {
                    break;
                }

                if (fragment.Start > expectedStart.Value + MaxDriftSec)
                {
                    break;
                }
            }

            if (best is not null)
            {
                return (best, bestIndex);
            }

            for (var i = 0; i < searchStart; i++)
            {
                if (used[i]) continue;
                var fragment = fragments[i];
                var delta = Math.Abs(fragment.Start - expectedStart.Value);
                if (delta <= MaxDriftSec)
                {
                    return (fragment, i);
                }
            }
        }

        return (null, -1);
    }

    private static double? GetTokenStart(AsrResponse asr, int? tokenIndex)
    {
        if (tokenIndex is null)
            return null;

        var idx = tokenIndex.Value;
        if (idx < 0 || idx >= asr.Tokens.Length)
            return null;

        return asr.Tokens[idx].StartTime;
    }
}

