namespace Ams.Core.Align.Tx;

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
            if (a[i] == b[j]) { i++; j++; }
            else
            {
                if (++ed > 1) return false;
                if (na == nb) { i++; j++; }
                else if (na > nb) i++;
                else j++;
            }
        }
        return ed + (na - i) + (nb - j) <= 1;
    }

    public static bool Equivalent(string a, string b, IReadOnlyDictionary<string, string> equiv)
        => a == b || (equiv.TryGetValue(a, out var e) && e == b) || (equiv.TryGetValue(b, out var e2) && e2 == a);

    public static double SubCost(string bookTok, string asrTok, IReadOnlyDictionary<string, string> equiv)
    {
        if (Equivalent(bookTok, asrTok, equiv)) return 0.0;
        if (LevLe1(bookTok, asrTok)) return 0.3;
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
        int maxRun = 8, double maxAvg = 0.6)
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

            for (int i = 1; i <= n; i++) { dp[i, 0] = dp[i - 1, 0] + DelCost(bookNorm[bLo + i - 1]); bt[i, 0] = 1; }
            for (int j = 1; j <= m; j++) { dp[0, j] = dp[0, j - 1] + InsCost(asrNorm[aLo + j - 1], fillers); bt[0, j] = 2; }

            for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m; j++)
            {
                var sub = dp[i - 1, j - 1] + SubCost(bookNorm[bLo + i - 1], asrNorm[aLo + j - 1], equiv);
                var del = dp[i - 1, j] + DelCost(bookNorm[bLo + i - 1]);
                var ins = dp[i, j - 1] + InsCost(asrNorm[aLo + j - 1], fillers);
                var best = sub; byte move = 0;
                if (del < best) { best = del; move = 1; }
                if (ins < best) { best = ins; move = 2; }
                dp[i, j] = best; bt[i, j] = move;
            }

            int ii = n, jj = m, run = 0; double sum = 0.0;
            var winOps = new List<(int?, int?, AlignOp, string, double)>(n + m);
            while (ii > 0 || jj > 0)
            {
                switch (bt[ii, jj])
                {
                    case 0:
                    {
                        int bi = bLo + ii - 1;
                        int aj = aLo + jj - 1;
                        var c = SubCost(bookNorm[bi], asrNorm[aj], equiv);
                        var isMatch = c == 0.0;
                        winOps.Add(isMatch
                            ? (bi, aj, AlignOp.Match, "equal_or_equiv", 1.0)
                            : (bi, aj, AlignOp.Sub, "near_or_diff", Math.Max(0.0, 1.0 - c)));
                        ii--; jj--; run = 0; sum = 0; break;
                    }
                    case 1:
                    {
                        int bi = bLo + ii - 1; var c = DelCost(bookNorm[bi]);
                        run++; sum += c;
                        // Note: run/avg can be surfaced to window status by caller if needed
                        winOps.Add((bi, null, AlignOp.Del, "missing_book", 0.0)); ii--; break;
                    }
                    default:
                    {
                        int aj = aLo + jj - 1; var c = InsCost(asrNorm[aj], fillers);
                        run++; sum += c;
                        winOps.Add((null, aj, AlignOp.Ins, (fillers.Contains(asrNorm[aj]) ? "filler" : "extra"), Math.Max(0.0, 1.0 - c))); jj--; break;
                    }
                }
            }
            winOps.Reverse();
            all.AddRange(winOps);
        }

        return all;
    }

    // (3.3) Sentence/Paragraph rollups from word ops
    public static (List<SentenceAlign> sents, List<ParagraphAlign> paras) Rollup(
        IReadOnlyList<WordAlign> ops,
        IReadOnlyList<(int Id, int Start, int End)> bookSentences,
        IReadOnlyList<(int Id, int Start, int End)> bookParagraphs)
    {
        // Sentences
        var sentsOut = new List<SentenceAlign>(bookSentences.Count);
        foreach (var s in bookSentences)
        {
            int start = s.Start, end = s.End, n = Math.Max(0, end - start + 1);
            var inRange = ops.Where(o => o.BookIdx is int bi && bi >= start && bi <= end).ToList();

            int subs = inRange.Count(o => o.Op == AlignOp.Sub);
            int dels = inRange.Count(o => o.Op == AlignOp.Del);
            // Limit insertions to those whose ASR positions fall within the ASR span
            // covered by aligned tokens for this sentence (prevents global insertions inflating WER).
            var asrIdxs = inRange.Where(o => o.AsrIdx is not null).Select(o => o.AsrIdx!.Value).ToList();
            int ins = 0;
            if (asrIdxs.Count > 0)
            {
                int aMin = asrIdxs.Min();
                int aMax = asrIdxs.Max();
                ins = ops.Count(o => o.Op == AlignOp.Ins && o.AsrIdx is int aj && aj >= aMin && aj <= aMax);
            }

            double wer = (subs + dels + ins) / Math.Max(1.0, n);
            double coverage = 1.0 - (double)dels / Math.Max(1.0, n);
            string status = wer <= 0.10 && dels < 3 ? "ok" : (wer <= 0.25 ? "attention" : "unreliable");

            ScriptRange? aRange = asrIdxs.Count > 0 ? new ScriptRange(asrIdxs.Min(), asrIdxs.Max()) : null;

            var metrics = new SentenceMetrics(wer, 0.0, wer, 0, 0);
            sentsOut.Add(new SentenceAlign(s.Id, new IntRange(start, end), aRange, metrics, status));
        }

        // Paragraphs
        var parasOut = new List<ParagraphAlign>(bookParagraphs.Count);
        foreach (var p in bookParagraphs)
        {
            var sIds = sentsOut.Where(x => x.BookRange.Start >= p.Start && x.BookRange.End <= p.End).Select(x => x.Id).ToList();
            var sub = sentsOut.Where(x => sIds.Contains(x.Id)).ToList();
            double werAvg = sub.Count > 0 ? sub.Average(x => x.Metrics.Wer) : 1.0;
            double covAvg = sub.Count > 0 ? sub.Average(x => 1.0 - x.Metrics.MissingRuns / Math.Max(1.0, x.BookRange.End - x.BookRange.Start + 1)) : 0.0;
            string status = werAvg <= 0.10 ? "ok" : (werAvg <= 0.25 ? "attention" : "unreliable");

            parasOut.Add(new ParagraphAlign(p.Id, new IntRange(p.Start, p.End), sIds, new ParagraphMetrics(werAvg, 0.0, covAvg), status));
        }
        return (sentsOut, parasOut);
    }
}
