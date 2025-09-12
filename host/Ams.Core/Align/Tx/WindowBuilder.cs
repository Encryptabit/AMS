namespace Ams.Core.Align.Tx;

// (2) Anchors -> clamped half-open windows builder
public static class WindowBuilder
{
    public static List<(int bLo, int bHi, int aLo, int aHi)> Build(
        IReadOnlyList<(int Bp, int Ap)> anchors,
        int bookStart, int bookEnd,
        int asrStart, int asrEnd)
    {
        var list = anchors.ToList();
        list.Insert(0, (bookStart - 1, asrStart - 1)); // sentinel start
        list.Add((bookEnd + 1, asrEnd + 1));           // sentinel end

        var wins = new List<(int, int, int, int)>(Math.Max(1, list.Count - 1));
        for (int k = 0; k < list.Count - 1; k++)
        {
            var L = list[k];
            var R = list[k + 1];

            int bLo = Math.Max(bookStart, L.Bp + 1);
            int bHi = Math.Min(bookEnd + 1, R.Bp);
            int aLo = Math.Max(asrStart, L.Ap + 1);
            int aHi = Math.Min(asrEnd + 1, R.Ap);

            // allow empty side; add if any side has span
            if (bLo < bHi || aLo < aHi)
                wins.Add((bLo, bHi, aLo, aHi));
        }
        return wins;
    }
}

