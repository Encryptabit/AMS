using System;
using System.Collections.Generic;
using System.Linq;

namespace Ams.Core;

public interface IChunkPlanner
{
    ChunkPlan Plan(double durationSec, IReadOnlyList<double> candidateCutsSec, SegmentationParams p);
}

public sealed class SilenceWindowPlanner : IChunkPlanner
{
    public ChunkPlan Plan(double durationSec, IReadOnlyList<double> candidateCutsSec, SegmentationParams p)
    {
        if (durationSec <= 0) throw new ArgumentOutOfRangeException(nameof(durationSec));
        var cuts = new List<double> { 0.0 };
        // Only add candidates strictly inside (0, D)
        if (candidateCutsSec != null)
            cuts.AddRange(candidateCutsSec.Where(t => t > 0 && t < durationSec).OrderBy(t => t));
        cuts.Add(durationSec);

        int n = cuts.Count;
        var cost = new double[n];
        var prev = new int[n];
        for (int i = 0; i < n; i++) { cost[i] = double.PositiveInfinity; prev[i] = -1; }
        cost[0] = 0.0;

        // For each i, try advancing to any j with segment length in [Min,Max]
        for (int i = 0; i < n; i++)
        {
            if (double.IsPositiveInfinity(cost[i])) continue;
            var ti = cuts[i];
            // find first j where len >= Min
            for (int j = i + 1; j < n; j++)
            {
                var len = cuts[j] - ti;
                if (len + 1e-9 < p.Min) continue;
                if (len > p.Max + 1e-9) break;
                var w = Math.Abs(len - p.Target);
                var c = cost[i] + w;
                if (c < cost[j] - 1e-12 || (Math.Abs(c - cost[j]) <= 1e-12 && i < prev[j]))
                {
                    cost[j] = c;
                    prev[j] = i;
                }
            }
        }

        if (double.IsPositiveInfinity(cost[n - 1]))
        {
            if (p.StrictTail)
                throw new InvalidOperationException("No valid segmentation path with strict [Min,Max] and silence-only boundaries.");
            // Relax tail: allow last hop length in [Min*0.75, Max*1.25]
            var relaxed = RelaxTail(cuts, p, cost, prev);
            if (relaxed is null)
                throw new InvalidOperationException("No valid segmentation path even under relaxed tail policy.");
            return relaxed!;
        }

        var spans = Reconstruct(cuts, prev);
        return new ChunkPlan(spans, cost[n - 1], false);
    }

    private static ChunkPlan? RelaxTail(List<double> cuts, SegmentationParams p, double[] baseCost, int[] basePrev)
    {
        int n = cuts.Count;
        var cost = (double[])baseCost.Clone();
        var prev = (int[])basePrev.Clone();
        double minR = p.Min * 0.75, maxR = p.Max * 1.25;

        // Allow relaxing only on final edge into D
        int bestI = -1; double bestC = double.PositiveInfinity;
        for (int i = 0; i < n - 1; i++)
        {
            if (double.IsPositiveInfinity(cost[i])) continue;
            var len = cuts[n - 1] - cuts[i];
            if (len + 1e-9 < minR || len > maxR + 1e-9) continue;
            var c = cost[i] + Math.Abs(len - p.Target);
            if (c < bestC - 1e-12 || (Math.Abs(c - bestC) <= 1e-12 && i < bestI))
            {
                bestC = c; bestI = i;
            }
        }
        if (bestI < 0) return null;
        prev[n - 1] = bestI;
        cost[n - 1] = bestC;
        var spans = Reconstruct(cuts, prev);
        return new ChunkPlan(spans, bestC, true);
    }

    private static List<ChunkSpan> Reconstruct(List<double> cuts, int[] prev)
    {
        var idx = new List<int>();
        for (int at = prev.Length - 1; at >= 0; at = prev[at])
        {
            idx.Add(at);
            if (prev[at] == -1) break;
        }
        idx.Reverse();
        var spans = new List<ChunkSpan>(Math.Max(0, idx.Count - 1));
        for (int k = 1; k < idx.Count; k++)
        {
            var i = idx[k - 1];
            var j = idx[k];
            spans.Add(new ChunkSpan(cuts[i], cuts[j]));
        }
        return spans;
    }
}
