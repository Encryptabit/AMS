namespace Ams.Core.Processors.Alignment.Anchors;

public sealed record Anchor(int Bp, int Ap);

public sealed record AnchorPolicy(
    int NGram = 3,
    int TargetPerTokens = 50,  // aim ~1 anchor per 50 book tokens
    bool AllowDuplicates = false,
    int MinSeparation = 100,   // tokens between duplicate occurrences
    ISet<string>? Stopwords = null,
    bool DisallowBoundaryCross = true
);

public static class AnchorDiscovery
{
    // Convenience overload: restrict selection to a book word range [bookStart, bookEnd] (inclusive)
    public static IReadOnlyList<Anchor> SelectAnchors(
        IReadOnlyList<string> bookTokens,
        IReadOnlyList<int> bookSentenceIndex,
        IReadOnlyList<string> asrTokens,
        AnchorPolicy policy,
        int bookStart,
        int bookEnd)
    {
        if (bookTokens.Count != bookSentenceIndex.Count) throw new ArgumentException("Mismatched book arrays");
        if (bookStart < 0 || bookEnd >= bookTokens.Count || bookStart > bookEnd)
            throw new ArgumentOutOfRangeException(nameof(bookStart), "Invalid book window");

        int len = bookEnd - bookStart + 1;
        var subBook = bookTokens.Skip(bookStart).Take(len).ToList();
        var subSent = bookSentenceIndex.Skip(bookStart).Take(len).ToList();
        var anchors = SelectAnchors(subBook, subSent, asrTokens, policy);
        return anchors.Select(a => new Anchor(a.Bp + bookStart, a.Ap)).ToList();
    }

    // tokens: normalized tokens for the chapter slice; sentenceIndex parallel to bookTokens
    public static IReadOnlyList<Anchor> SelectAnchors(
        IReadOnlyList<string> bookTokens,
        IReadOnlyList<int> bookSentenceIndex,
        IReadOnlyList<string> asrTokens,
        AnchorPolicy policy)
    {
        var stop = policy.Stopwords ?? new HashSet<string>(StringComparer.Ordinal);
        var n = policy.NGram;

        var anchors = Collect(bookTokens, bookSentenceIndex, asrTokens, n, stop,
                              okBook: list => list.Count == 1,
                              okAsr: list => list.Count == 1,
                              policy);

        // Density control
        int desired = Math.Max(1, bookTokens.Count / Math.Max(1, policy.TargetPerTokens));
        if (anchors.Count < desired)
        {
            static bool FarApart(List<int> pos, int minSep)
                => pos.Count <= 2 && (pos.Count == 1 || Math.Abs(pos[1] - pos[0]) >= minSep);

            var relaxed = Collect(bookTokens, bookSentenceIndex, asrTokens, n, stop,
                                  okBook: list => FarApart(list, policy.MinSeparation),
                                  okAsr: list => FarApart(list, policy.MinSeparation),
                                  policy);
            if (relaxed.Count > anchors.Count) anchors = relaxed;

            if (anchors.Count < desired && n > 2)
            {
                var subPolicy = policy with { NGram = n - 1 };
                anchors = SelectAnchors(bookTokens, bookSentenceIndex, asrTokens, subPolicy).ToList();
            }
        }

        // Monotonicity: sort by bp, then LIS by ap
        anchors.Sort((x, y) => x.Bp.CompareTo(y.Bp));
        var lisPairs = LisByAp(anchors.Select(a => (a.Bp, a.Ap)).ToList());
        return lisPairs.Select(p => new Anchor(p.bp, p.ap)).ToList();
    }

    public static List<(int bLo, int bHi, int aLo, int aHi)> BuildWindows(
        IReadOnlyList<Anchor> anchors,
        int bookStart, int bookEnd,   // inclusive indices
        int asrStart, int asrEnd)     // inclusive indices
    {
        var list = anchors.ToList();
        // Add sentinels (never dereference)
        list.Insert(0, new Anchor(bookStart - 1, asrStart - 1));
        list.Add(new Anchor(bookEnd + 1, asrEnd + 1));

        var windows = new List<(int, int, int, int)>(Math.Max(1, list.Count - 1));
        for (int k = 0; k < list.Count - 1; k++)
        {
            var left = list[k];
            var right = list[k + 1];

            int bLo = Math.Max(bookStart, left.Bp + 1);
            int bHi = Math.Min(bookEnd + 1, right.Bp);   // half-open

            int aLo = Math.Max(asrStart, left.Ap + 1);
            int aHi = Math.Min(asrEnd + 1, right.Ap);     // half-open

            if (bLo < bHi || aLo < aHi)
                windows.Add((bLo, bHi, aLo, aHi));
        }
        return windows;
    }

    // ---------- Helpers ----------

    public static Dictionary<string, List<int>> IndexNGrams(IReadOnlyList<string> toks, int n)
    {
        var dict = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        if (toks.Count < n) return dict;
        for (int i = 0; i <= toks.Count - n; i++)
        {
            var key = string.Join("|", toks.Skip(i).Take(n));
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<int>(1);
                dict[key] = list;
            }
            list.Add(i);
        }
        return dict;
    }

    private static List<Anchor> Collect(
        IReadOnlyList<string> book,
        IReadOnlyList<int> bookSentIdx,
        IReadOnlyList<string> asr,
        int n,
        ISet<string> stop,
        Func<List<int>, bool> okBook,
        Func<List<int>, bool> okAsr,
        AnchorPolicy policy)
    {
        var bIdx = IndexNGrams(book, n);
        var aIdx = IndexNGrams(asr, n);
        var anchors = new List<Anchor>();

        foreach (var kv in bIdx)
        {
            var key = kv.Key;
            var bPosList = kv.Value;
            if (!okBook(bPosList)) continue;
            if (!aIdx.TryGetValue(key, out var aPosList) || !okAsr(aPosList)) continue;

            int bp = bPosList[0], ap = aPosList[0];

            // Content/stopword filter & boundary rule
            if (!PassContent(book, bp, n, stop)) continue;
            if (policy.DisallowBoundaryCross && CrossesSentence(bookSentIdx, bp, n)) continue;

            anchors.Add(new Anchor(bp, ap));
        }
        return anchors;
    }

    private static bool PassContent(IReadOnlyList<string> toks, int i, int n, ISet<string> stop)
    {
        int content = 0;
        for (int k = 0; k < n; k++)
        {
            var w = toks[i + k];
            if (!stop.Contains(w)) content++;
        }
        if (n >= 3 && content < 2) return false;

        var first = toks[i];
        var last = toks[i + n - 1];
        if (stop.Contains(first) || stop.Contains(last)) return false;
        return true;
    }

    private static bool CrossesSentence(IReadOnlyList<int> sentIdx, int i, int n)
    {
        int s0 = sentIdx[i];
        int s1 = sentIdx[i + n - 1];
        return s0 != s1;
    }

    // LIS on 'ap' after sorting by 'bp'
    public static List<(int bp, int ap)> LisByAp(IReadOnlyList<(int bp, int ap)> pairs)
    {
        int n = pairs.Count;
        if (n == 0) return new();

        var tails = new int[n];
        var prev = new int[n];
        int size = 0;

        for (int i = 0; i < n; i++)
        {
            int lo = 0, hi = size;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (pairs[tails[mid]].ap < pairs[i].ap) lo = mid + 1;
                else hi = mid;
            }
            prev[i] = lo > 0 ? tails[lo - 1] : -1;
            tails[lo] = i;
            if (lo == size) size++;
        }

        var lis = new List<(int bp, int ap)>(size);
        for (int k = tails[size - 1]; k >= 0; k = prev[k]) lis.Add(pairs[k]);
        lis.Reverse();
        return lis;
    }
}


