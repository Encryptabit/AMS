using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Processors.Alignment.Tx;

// (3) Cost helpers and windowed DP aligner + rollups
public static class TranscriptAligner
{
    // (3.1) Edit distance shortcuts
    public static bool LevLe1(string a, string b)
    {
        if (a == b) return true;
        int na = a.Length, nb = b.Length;
        if (Math.Abs(na - nb) > 1) return false;
        int i = 0, j = 0, ed = 0;
        while (i < na && j < nb)
        {
            if (a[i] == b[j])
            {
                i++;
                j++;
            }
            else
            {
                if (++ed > 1) return false;
                if (na == nb)
                {
                    i++;
                    j++;
                }
                else if (na > nb) i++;
                else j++;
            }
        }

        return ed + (na - i) + (nb - j) <= 1;
    }

    public static bool Equivalent(string a, string b, IReadOnlyDictionary<string, string> equiv)
        => a == b || (equiv.TryGetValue(a, out var e) && e == b) || (equiv.TryGetValue(b, out var e2) && e2 == a);

    public static double SubCost(
        string bookTok,
        string asrTok,
        IReadOnlyDictionary<string, string> equiv,
        string[]? bookPhonemes = null,
        string[]? asrPhonemes = null,
        double phonemeSoftThreshold = 0.8)
    {
        if (Equivalent(bookTok, asrTok, equiv)) return 0.0;

        if (HasExactPhonemeMatch(bookPhonemes, asrPhonemes))
        {
            return 0.0;
        }

        if (LevLe1(bookTok, asrTok) || HasSoftPhonemeMatch(bookPhonemes, asrPhonemes, phonemeSoftThreshold))
        {
            return 0.3;
        }

        return 1.0;
    }

    public static double InsCost(string asrTok, ISet<string> fillers) => fillers.Contains(asrTok) ? 0.3 : 1.0;
    public static double DelCost(string bookTok) => 1.0;

    // (3.2) Windowed DP aligner on normalized views; indices are in the filtered/normalized coordinate space.
    public static List<(int? bi, int? aj, AlignOp op, string reason, double score)> AlignWindows(
        IReadOnlyList<string> bookNorm,
        IReadOnlyList<string> asrNorm,
        IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows,
        IReadOnlyDictionary<string, string> equiv,
        ISet<string> fillers,
        IReadOnlyList<string[]>? bookPhonemes = null,
        IReadOnlyList<string[]>? asrPhonemes = null,
        int maxRun = 8,
        double maxAvg = 0.6,
        double phonemeSoftThreshold = 0.8)
    {
        var all = new List<(int?, int?, AlignOp, string, double)>(bookNorm.Count + asrNorm.Count);

        foreach (var (bLo, bHi, aLo, aHi) in windows)
        {
            int n = Math.Max(0, bHi - bLo);
            int m = Math.Max(0, aHi - aLo);

            // Edges: permit empty sides
            if (n == 0 && m == 0) continue;

            var dp = new double[n + 1, m + 1];
            var bt = new byte[n + 1, m + 1]; // 0=diag,1=up(del),2=left(ins)

            for (int i = 1; i <= n; i++)
            {
                dp[i, 0] = dp[i - 1, 0] + DelCost(bookNorm[bLo + i - 1]);
                bt[i, 0] = 1;
            }

            for (int j = 1; j <= m; j++)
            {
                dp[0, j] = dp[0, j - 1] + InsCost(asrNorm[aLo + j - 1], fillers);
                bt[0, j] = 2;
            }

            for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m; j++)
            {
                var sub = dp[i - 1, j - 1] + SubCost(
                    bookNorm[bLo + i - 1],
                    asrNorm[aLo + j - 1],
                    equiv,
                    GetPhonemes(bookPhonemes, bLo + i - 1),
                    GetPhonemes(asrPhonemes, aLo + j - 1),
                    phonemeSoftThreshold);
                var del = dp[i - 1, j] + DelCost(bookNorm[bLo + i - 1]);
                var ins = dp[i, j - 1] + InsCost(asrNorm[aLo + j - 1], fillers);
                var best = sub;
                byte move = 0;
                if (del < best)
                {
                    best = del;
                    move = 1;
                }

                if (ins < best)
                {
                    best = ins;
                    move = 2;
                }

                dp[i, j] = best;
                bt[i, j] = move;
            }

            int ii = n, jj = m, run = 0;
            double sum = 0.0;
            var winOps = new List<(int?, int?, AlignOp, string, double)>(n + m);
            while (ii > 0 || jj > 0)
            {
                switch (bt[ii, jj])
                {
                    case 0:
                    {
                        int bi = bLo + ii - 1;
                        int aj = aLo + jj - 1;
                        var cost = SubCost(
                            bookNorm[bi],
                            asrNorm[aj],
                            equiv,
                            GetPhonemes(bookPhonemes, bi),
                            GetPhonemes(asrPhonemes, aj),
                            phonemeSoftThreshold);
                        var isMatch = cost == 0.0;
                        var reason = isMatch ? "equal_or_equiv" : "near_or_diff";
                        winOps.Add((bi, aj, isMatch ? AlignOp.Match : AlignOp.Sub, reason, cost));
                        ii--;
                        jj--;
                        run = 0;
                        sum = 0;
                        break;
                    }
                    case 1:
                    {
                        int bi = bLo + ii - 1;
                        var cost = DelCost(bookNorm[bi]);
                        run++;
                        sum += cost;
                        winOps.Add((bi, null, AlignOp.Del, "missing_book", cost));
                        ii--;
                        break;
                    }
                    default:
                    {
                        int aj = aLo + jj - 1;
                        var cost = InsCost(asrNorm[aj], fillers);
                        run++;
                        sum += cost;
                        var reason = fillers.Contains(asrNorm[aj]) ? "filler" : "extra";
                        winOps.Add((null, aj, AlignOp.Ins, reason, cost));
                        jj--;
                        break;
                    }
                }
            }

            winOps.Reverse();
            all.AddRange(winOps);
        }

        return all;
    }

    private static string[]? GetPhonemes(IReadOnlyList<string[]>? list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return null;
        var entry = list[index];
        return entry is { Length: > 0 } ? entry : null;
    }

    private static bool HasExactPhonemeMatch(string[]? bookPhonemes, string[]? asrPhonemes)
    {
        if (bookPhonemes is not { Length: > 0 } bookList || asrPhonemes is not { Length: > 0 } asrList)
        {
            return false;
        }

        foreach (var bookVariant in bookList)
        {
            if (string.IsNullOrWhiteSpace(bookVariant)) continue;
            foreach (var asrVariant in asrList)
            {
                if (string.IsNullOrWhiteSpace(asrVariant)) continue;
                if (PhonemeComparer.Equals(bookVariant, asrVariant))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasSoftPhonemeMatch(string[]? bookPhonemes, string[]? asrPhonemes, double threshold)
    {
        if (bookPhonemes is not { Length: > 0 } bookList || asrPhonemes is not { Length: > 0 } asrList)
        {
            return false;
        }

        double best = 0.0;
        foreach (var bookVariant in bookList)
        {
            if (string.IsNullOrWhiteSpace(bookVariant)) continue;
            var bookSeq = PhonemeComparer.Tokenize(bookVariant);
            if (bookSeq.Length == 0) continue;

            foreach (var asrVariant in asrList)
            {
                if (string.IsNullOrWhiteSpace(asrVariant)) continue;
                var asrSeq = PhonemeComparer.Tokenize(asrVariant);
                if (asrSeq.Length == 0) continue;

                var sim = PhonemeComparer.Similarity(bookSeq, asrSeq);
                if (sim > best)
                {
                    best = sim;
                }

                if (best >= threshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static class PhonemeComparer
    {
        public static bool Equals(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
        }

        public static string[] Tokenize(string phonemeVariant)
            => string.IsNullOrWhiteSpace(phonemeVariant)
                ? Array.Empty<string>()
                : phonemeVariant.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        public static double Similarity(string[] a, string[] b)
        {
            if (a.Length == 0 || b.Length == 0)
            {
                return 0.0;
            }

            return LevenshteinMetrics.Similarity(a.AsSpan(), b.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value)
            => string.Join(' ', Tokenize(value));
    }

    // (3.3) Sentence/Paragraph rollups from word ops
    public static (List<SentenceAlign> sents, List<ParagraphAlign> paras) Rollup(
        IReadOnlyList<WordAlign> ops,
        IReadOnlyList<(int Id, int Start, int End)> bookSentences,
        IReadOnlyList<(int Id, int Start, int End)> bookParagraphs) =>
        Rollup(ops, bookSentences, bookParagraphs, null, null);

    public static (List<SentenceAlign> sents, List<ParagraphAlign> paras) Rollup(
        IReadOnlyList<WordAlign> ops,
        IReadOnlyList<(int Id, int Start, int End)> bookSentences,
        IReadOnlyList<(int Id, int Start, int End)> bookParagraphs,
        BookIndex? book,
        AsrResponse? asr)
    {
        var opsList = ops.ToList();
        var guardRanges = new (int? Start, int? End)[bookSentences.Count];

        // Sentences
        var sentsOut = new List<SentenceAlign>(bookSentences.Count);
        for (int sentenceIndex = 0; sentenceIndex < bookSentences.Count; sentenceIndex++)
        {
            var s = bookSentences[sentenceIndex];
            int start = s.Start, end = s.End, n = Math.Max(0, end - start + 1);

            var candidateIndices = new List<int>();
            for (int i = 0; i < opsList.Count; i++)
            {
                var op = opsList[i];
                if (op.BookIdx is int bi && bi >= start && bi <= end)
                {
                    candidateIndices.Add(i);
                }
            }

            if (candidateIndices.Count == 0)
            {
                var emptyMetrics = new SentenceMetrics(1.0, 0.0, 1.0, n, 0);
                guardRanges[sentenceIndex] = ComputeGuardRangeForMissingSentence(opsList, start, end);
                sentsOut.Add(new SentenceAlign(s.Id, new IntRange(start, end), null, TimingRange.Empty, emptyMetrics,
                    "unreliable"));
                continue;
            }

            int minIndex = candidateIndices.Min();
            int maxIndex = candidateIndices.Max();

            int left = minIndex - 1;
            while (left >= 0)
            {
                var op = opsList[left];
                if (op.BookIdx.HasValue) break;
                if (op.AsrIdx is null) break;
                minIndex = left;
                left--;
            }

            int right = maxIndex + 1;
            while (right < opsList.Count)
            {
                var op = opsList[right];
                if (op.BookIdx.HasValue) break;
                if (op.AsrIdx is null) break;
                maxIndex = right;
                right++;
            }

            var segment = opsList.GetRange(minIndex, maxIndex - minIndex + 1);
            var inRange = segment.Where(o => o.BookIdx is { } bi && bi >= start && bi <= end).ToList();

            int subs = 0;
            int dels = 0;
            double costSum = 0.0;

            foreach (var op in inRange)
            {
                costSum += Math.Max(0.0, op.Score);
                if (op.Op == AlignOp.Sub) subs++;
                if (op.Op == AlignOp.Del) dels++;
            }

            int? guardStart = null;
            int? guardEnd = null;
            int? sentenceStartAsr = null;
            int? sentenceEndAsr = null;

            var anchorAsrIdxs = segment
                .Where(o => string.Equals(o.Reason, "anchor", StringComparison.Ordinal) &&
                            o is { AsrIdx: { } aj, BookIdx: { } bi } && bi >= start && bi <= end)
                .Select(o => o.AsrIdx!.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var sentenceAsrIdxs = inRange
                .Where(o => o.AsrIdx != null)
                .Select(o => o.AsrIdx!.Value)
                .ToList();

            int? baseStart = sentenceAsrIdxs.Count > 0
                ? sentenceAsrIdxs.Min()
                : (anchorAsrIdxs.Count > 0 ? anchorAsrIdxs.First() : (int?)null);
            int? baseEnd = sentenceAsrIdxs.Count > 0
                ? sentenceAsrIdxs.Max()
                : (anchorAsrIdxs.Count > 0 ? anchorAsrIdxs.Last() : (int?)null);

            ScriptRange? aRange = null;
            if (baseStart.HasValue && baseEnd.HasValue)
            {
                int startAsr = baseStart.Value;
                int endAsr = baseEnd.Value;

                int prevAnchorAsr = opsList
                    .Take(minIndex)
                    .Where(o => string.Equals(o.Reason, "anchor", StringComparison.Ordinal) && o.AsrIdx != null)
                    .Select(o => o.AsrIdx!.Value)
                    .LastOrDefault(-1);
                int nextAnchorAsr = opsList
                    .Skip(maxIndex + 1)
                    .Where(o => string.Equals(o.Reason, "anchor", StringComparison.Ordinal) && o.AsrIdx != null)
                    .Select(o => o.AsrIdx!.Value)
                    .FirstOrDefault(int.MaxValue);

                int leftGuard = prevAnchorAsr + 1;
                int rightGuard = nextAnchorAsr - 1;

                int probe = minIndex - 1;
                while (probe >= 0)
                {
                    var probeOp = opsList[probe];
                    if (probeOp.AsrIdx is not { } probeAsr)
                    {
                        probe--;
                        continue;
                    }

                    if (probeOp.BookIdx is { } bi && bi < start) break;
                    if (probeOp.Op != AlignOp.Ins) break;
                    if (probeAsr < leftGuard) break;
                    startAsr = Math.Min(startAsr, probeAsr);
                    probe--;
                }

                probe = maxIndex + 1;
                while (probe < opsList.Count)
                {
                    var probeOp = opsList[probe];
                    if (probeOp.AsrIdx is not int probeAsr)
                    {
                        probe++;
                        continue;
                    }

                    if (probeOp.BookIdx is int bi && bi > end) break;
                    if (probeOp.Op != AlignOp.Ins) break;
                    if (probeAsr > rightGuard) break;
                    endAsr = Math.Max(endAsr, probeAsr);
                    probe++;
                }

                if (startAsr > endAsr)
                {
                    startAsr = baseStart.Value;
                    endAsr = baseEnd.Value;
                }

                guardStart = startAsr;
                guardEnd = endAsr;
                aRange = new ScriptRange(startAsr, endAsr);
                sentenceStartAsr = startAsr;
                sentenceEndAsr = endAsr;
            }

            int insCount = 0;
            double insCost = 0.0;
            if (guardStart.HasValue && guardEnd.HasValue)
            {
                int startGuard = guardStart.Value;
                int endGuard = guardEnd.Value;
                foreach (var op in segment)
                {
                    if (op is { Op: AlignOp.Ins, AsrIdx: { } aj } && aj >= startGuard && aj <= endGuard)
                    {
                        insCount++;
                        insCost += Math.Max(0.0, op.Score);
                    }
                }
            }
            else
            {
                foreach (var op in segment)
                {
                    if (op is { Op: AlignOp.Ins, AsrIdx: not null })
                    {
                        insCount++;
                        insCost += Math.Max(0.0, op.Score);
                    }
                }
            }

            costSum += insCost;

            double tokenCount = Math.Max(1.0, n);
            double weightedWer = Math.Min(1.0, costSum / tokenCount);
            double legacyWer = Math.Min(1.0, (subs + dels + insCount) / tokenCount);
            double coverage = 1.0 - dels / tokenCount;

            if ((!sentenceStartAsr.HasValue || !sentenceEndAsr.HasValue) && sentenceAsrIdxs.Count > 0)
            {
                sentenceStartAsr = sentenceAsrIdxs.Min();
                sentenceEndAsr = sentenceAsrIdxs.Max();
            }

            double cer = ComputeCer(book, asr, start, end, sentenceStartAsr, sentenceEndAsr);

            bool normalizedMatch = false;
            if (book is not null && asr is not null)
            {
                var normalizedReference = BuildNormalizedWordString(book, start, end);
                var normalizedHypothesis = BuildNormalizedWordString(asr, sentenceStartAsr, sentenceEndAsr);

                if (normalizedReference.Length == 0 && normalizedHypothesis.Length == 0)
                {
                    normalizedMatch = true;
                }
                else if (normalizedReference.Length > 0 &&
                         string.Equals(normalizedReference, normalizedHypothesis, StringComparison.Ordinal))
                {
                    normalizedMatch = true;
                }
            }

            if (normalizedMatch)
            {
                weightedWer = 0.0;
                legacyWer = 0.0;
                coverage = 1.0;
                cer = 0.0;
                dels = 0;
                insCount = 0;
            }

            string status = weightedWer <= 0.10 && dels < 3 ? "ok" : (weightedWer <= 0.25 ? "attention" : "unreliable");

            guardRanges[sentenceIndex] = (guardStart, guardEnd);

            var metrics = new SentenceMetrics(weightedWer, cer, legacyWer, dels, insCount);
            sentsOut.Add(new SentenceAlign(s.Id, new IntRange(start, end), aRange, TimingRange.Empty, metrics, status));
        }

        SynthesizeMissingScriptRanges(sentsOut, asr?.WordCount ?? 0, guardRanges);

        // Paragraphs
        var parasOut = new List<ParagraphAlign>(bookParagraphs.Count);
        foreach (var p in bookParagraphs)
        {
            var sIds = sentsOut.Where(x => x.BookRange.Start >= p.Start && x.BookRange.End <= p.End).Select(x => x.Id)
                .ToList();
            var sub = sentsOut.Where(x => sIds.Contains(x.Id)).ToList();
            double werAvg = sub.Count > 0 ? sub.Average(x => x.Metrics.Wer) : 1.0;
            double cerAvg = sub.Count > 0 ? sub.Average(x => x.Metrics.Cer) : 1.0;
            double covAvg = sub.Count > 0
                ? sub.Average(x => 1.0 - x.Metrics.MissingRuns / Math.Max(1.0, x.BookRange.End - x.BookRange.Start + 1))
                : 0.0;
            string status = werAvg <= 0.10 ? "ok" : (werAvg <= 0.25 ? "attention" : "unreliable");

            parasOut.Add(new ParagraphAlign(p.Id, new IntRange(p.Start, p.End), sIds,
                new ParagraphMetrics(werAvg, cerAvg, covAvg), status));
        }

        return (sentsOut, parasOut);
    }

    private static void SynthesizeMissingScriptRanges(List<SentenceAlign> sentences, int asrTokenCount,
        (int? Start, int? End)[] guardRanges)
    {
        if (sentences.Count == 0 || asrTokenCount <= 0)
        {
            return;
        }

        var missing = new List<int>();
        for (int i = 0; i < sentences.Count; i++)
        {
            if (!TryGetConcreteRange(sentences[i], out _, out _))
            {
                missing.Add(i);
            }
        }

        if (missing.Count == 0)
        {
            return;
        }

        int maxToken = asrTokenCount - 1;
        int cursor = 0;

        while (cursor < missing.Count)
        {
            int blockStartIndex = missing[cursor];
            int blockEndIndex = blockStartIndex;

            while (cursor + 1 < missing.Count && missing[cursor + 1] == blockEndIndex + 1)
            {
                cursor++;
                blockEndIndex = missing[cursor];
            }

            cursor++;

            var prevRange = FindPreviousRange(sentences, blockStartIndex - 1);
            var nextRange = FindNextRange(sentences, blockEndIndex + 1);

            int blockSize = blockEndIndex - blockStartIndex + 1;
            int? guardMin = null;
            int? guardMax = null;
            for (int offset = 0; offset < blockSize; offset++)
            {
                int guardIndex = blockStartIndex + offset;
                var guard = guardIndex < guardRanges.Length
                    ? guardRanges[guardIndex]
                    : (null, null);

                if (guard.Start.HasValue)
                {
                    guardMin = guardMin.HasValue
                        ? Math.Min(guardMin.Value, guard.Start.Value)
                        : guard.Start.Value;
                }

                if (guard.End.HasValue)
                {
                    guardMax = guardMax.HasValue
                        ? Math.Max(guardMax.Value, guard.End.Value)
                        : guard.End.Value;
                }
            }

            int lower = guardMin ?? (prevRange?.End + 1 ?? 0);
            int upper = guardMax ?? (nextRange?.Start - 1 ?? maxToken);

            if (prevRange.HasValue)
            {
                lower = Math.Max(lower, Math.Min(maxToken, prevRange.Value.End + 1));
            }

            if (nextRange.HasValue)
            {
                upper = Math.Min(upper, Math.Max(0, nextRange.Value.Start - 1));
            }

            lower = Math.Clamp(lower, 0, maxToken);
            upper = Math.Clamp(upper, 0, maxToken);

            if (upper < lower)
            {
                int anchor = prevRange?.End ?? nextRange?.Start ?? 0;
                anchor = Math.Clamp(anchor, 0, maxToken);
                lower = anchor;
                upper = anchor;
            }

            int span = Math.Max(0, upper - lower + 1);

            for (int offset = 0; offset < blockSize; offset++)
            {
                int sentenceIndex = blockStartIndex + offset;
                int startIdx;
                int endIdx;

                if (span == 0)
                {
                    startIdx = lower;
                    endIdx = lower;
                }
                else
                {
                    double portionStart = (double)offset / blockSize;
                    double portionEnd = (double)(offset + 1) / blockSize;
                    startIdx = lower + (int)Math.Floor(span * portionStart);
                    endIdx = lower + (int)Math.Floor(span * portionEnd) - 1;
                    startIdx = Math.Clamp(startIdx, lower, upper);
                    endIdx = Math.Clamp(endIdx, startIdx, upper);
                }

                if (sentenceIndex > 0 && TryGetConcreteRange(sentences[sentenceIndex - 1], out _, out var prevEnd))
                {
                    if (startIdx <= prevEnd)
                    {
                        startIdx = Math.Min(upper, prevEnd + 1);
                        if (startIdx > upper)
                        {
                            startIdx = upper;
                        }

                        if (endIdx < startIdx)
                        {
                            endIdx = startIdx;
                        }
                    }
                }

                if (sentenceIndex < sentences.Count - 1 &&
                    TryGetConcreteRange(sentences[sentenceIndex + 1], out var nextStart, out _))
                {
                    if (endIdx >= nextStart)
                    {
                        endIdx = Math.Max(startIdx, Math.Clamp(nextStart - 1, startIdx, upper));
                    }
                }

                var synthesized = new ScriptRange(startIdx, endIdx);
                sentences[sentenceIndex] = sentences[sentenceIndex] with { ScriptRange = synthesized };
            }
        }
    }

    private static bool TryGetConcreteRange(SentenceAlign sentence, out int start, out int end)
    {
        if (sentence.ScriptRange is { Start: int s, End: int e })
        {
            start = s;
            end = e;
            return true;
        }

        start = default;
        end = default;
        return false;
    }

    private static (int Start, int End)? FindPreviousRange(IReadOnlyList<SentenceAlign> sentences, int index)
    {
        for (int i = index; i >= 0; i--)
        {
            if (TryGetConcreteRange(sentences[i], out var start, out var end))
            {
                return (start, end);
            }
        }

        return null;
    }

    private static (int Start, int End)? FindNextRange(IReadOnlyList<SentenceAlign> sentences, int index)
    {
        for (int i = index; i < sentences.Count; i++)
        {
            if (TryGetConcreteRange(sentences[i], out var start, out var end))
            {
                return (start, end);
            }
        }

        return null;
    }

    private static (int? Start, int? End) ComputeGuardRangeForMissingSentence(IReadOnlyList<WordAlign> ops,
        int sentenceStartWord, int sentenceEndWord)
    {
        if (ops.Count == 0)
        {
            return (null, null);
        }

        int? prevAsr = null;
        int? nextAsr = null;

        foreach (var op in ops)
        {
            if (op.AsrIdx is not int asr)
            {
                continue;
            }

            if (op.BookIdx is int bi)
            {
                if (bi < sentenceStartWord)
                {
                    prevAsr = asr;
                    continue;
                }

                if (bi > sentenceEndWord)
                {
                    nextAsr = asr;
                    break;
                }
            }
        }

        if (nextAsr is null)
        {
            for (int i = ops.Count - 1; i >= 0; i--)
            {
                var op = ops[i];
                if (op.AsrIdx is not int asr)
                {
                    continue;
                }

                if (op.BookIdx is int bi && bi > sentenceEndWord)
                {
                    nextAsr = asr;
                }
            }
        }

        var inserted = new List<int>();
        foreach (var op in ops)
        {
            if (op.AsrIdx is not int asr)
            {
                continue;
            }

            if (op.BookIdx.HasValue)
            {
                if (op.BookIdx.Value >= sentenceStartWord && op.BookIdx.Value <= sentenceEndWord)
                {
                    inserted.Add(asr);
                }

                continue;
            }

            bool afterPrev = !prevAsr.HasValue || asr > prevAsr.Value;
            bool beforeNext = !nextAsr.HasValue || asr < nextAsr.Value;

            if (afterPrev && beforeNext)
            {
                inserted.Add(asr);
            }
        }

        if (inserted.Count > 0)
        {
            inserted.Sort();
            return (inserted[0], inserted[^1]);
        }

        if (prevAsr.HasValue && nextAsr.HasValue)
        {
            if (nextAsr.Value - prevAsr.Value >= 2)
            {
                return (prevAsr.Value + 1, nextAsr.Value - 1);
            }

            return (prevAsr.Value + 1, prevAsr.Value + 1);
        }

        if (prevAsr.HasValue)
        {
            return (prevAsr.Value + 1, prevAsr.Value + 1);
        }

        if (nextAsr.HasValue)
        {
            return (nextAsr.Value - 1, nextAsr.Value - 1);
        }

        return (null, null);
    }

    private static double ComputeCer(BookIndex? book, AsrResponse? asr, int bookStart, int bookEnd, int? asrStart,
        int? asrEnd)
    {
        var reference = BuildNormalizedWordString(book, bookStart, bookEnd);
        var hypothesis = BuildNormalizedWordString(asr, asrStart, asrEnd);

        if (reference.Length == 0)
        {
            return hypothesis.Length == 0 ? 0.0 : 1.0;
        }

        int distance = LevenshteinMetrics.Distance(reference.AsSpan(), hypothesis.AsSpan());
        return distance / Math.Max(1.0, reference.Length);
    }

    private static string BuildNormalizedWordString(BookIndex? book, int start, int end)
    {
        if (book is null || start < 0 || end < start || book.Words.Length == 0)
        {
            return string.Empty;
        }

        int safeEnd = Math.Min(end, book.Words.Length - 1);
        var builder = new StringBuilder();

        for (int i = Math.Max(0, start); i <= safeEnd; i++)
        {
            AppendNormalized(builder, book.Words[i].Text);
        }

        return builder.ToString();
    }

    private static string BuildNormalizedWordString(AsrResponse? asr, int? start, int? end)
    {
        if (asr is null || !start.HasValue || !end.HasValue || !asr.HasWords)
        {
            return string.Empty;
        }

        int s = Math.Clamp(start.Value, 0, asr.WordCount - 1);
        int e = Math.Clamp(end.Value, s, asr.WordCount - 1);
        var builder = new StringBuilder();

        for (int i = s; i <= e; i++)
        {
            var word = asr.GetWord(i);
            if (!string.IsNullOrEmpty(word))
            {
                AppendNormalized(builder, word);
            }
        }

        return builder.ToString();
    }

    private static void AppendNormalized(StringBuilder builder, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }
    }
}