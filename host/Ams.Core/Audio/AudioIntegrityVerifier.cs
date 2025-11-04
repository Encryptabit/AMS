using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public enum AudioMismatchType { MissingSpeech, ExtraSpeech }

public sealed record SentenceSpan(int SentenceId, double StartSec, double EndSec);
public sealed record AudioMismatch(
    double StartSec, double EndSec,
    AudioMismatchType Type,
    double RawDb, double TreatedDb, double DeltaDb,
    IReadOnlyList<SentenceSpan> Sentences);

public sealed record AudioVerificationResult(
    double WindowMs, double StepMs,
    double RawSpeechThresholdDb, double TreatedSpeechThresholdDb,
    double DurationSec,
    double RawSpeechSec, double TreatedSpeechSec,
    double MissingSpeechSec, double ExtraSpeechSec,
    IReadOnlyList<AudioMismatch> Mismatches);

public static class AudioIntegrityVerifier 
{
    private const double MinDb = -120.0;

    public static AudioVerificationResult Verify(
        float[] rawMono, int sampleRateRaw,
        float[] treatedMono, int sampleRateTreated,
        IReadOnlyDictionary<int, SentenceTiming> rawTimingsById,
        IReadOnlyDictionary<int, SentenceTiming> treatedTimingsById,
        double windowMs = 30.0, double stepMs = 15.0,
        double minMismatchMs = 60.0, double minGapToMergeMs = 40.0,
        double minDeltaDb = 20.0)
    {
        if (sampleRateRaw != sampleRateTreated)
            throw new InvalidOperationException($"Sample rate mismatch: raw={sampleRateRaw}, treated={sampleRateTreated}");

        int sr = sampleRateRaw;
        double windowSec = windowMs / 1000.0;
        double stepSec   = stepMs   / 1000.0;

        // 1) Build chapter‑level series
        var (rawDb, treatedDb) = (ComputeDbSeries(rawMono, sr, windowSec, stepSec),
            ComputeDbSeries(treatedMono, sr, windowSec, stepSec));

        double durationSec = Math.Max((rawMono.Length - 1) / (double)sr, (treatedMono.Length - 1) / (double)sr);
        int frames = treatedDb.Length;

        // 2) Infer thresholds + speech masks
        double rawThr     = InferSpeechThreshold(rawDb);
        double treatedThr = InferSpeechThreshold(treatedDb);
        var rawMask     = BuildSpeechMask(rawDb, rawThr);
        var treatedMask = BuildSpeechMask(treatedDb, treatedThr);

        // 3) Build treated→raw time map (piecewise linear by sentence)
        var map = BuildPiecewiseMap(rawTimingsById, treatedTimingsById); // sorted by treated start

        // 4) Align raw onto treated timeline in O(N)
        var alignedRawDb     = new double[frames];
        var alignedRawSpeech = new bool[frames];
        for (int i = 0; i < frames; i++)
        {
            double t = i * stepSec;
            double tr = MapToRaw(map, t);
            alignedRawDb[i]     = SampleSeries(rawDb,     stepSec, tr);
            alignedRawSpeech[i] = SampleMask  (rawMask,   stepSec, tr);
        }

        // 5) Find mismatch runs (missing / extra)
        double deltaThreshold = Math.Max(0.1, minDeltaDb);
        var missingMask = new bool[frames];
        var extraMask   = new bool[frames];
        for (int i = 0; i < frames; i++)
        {
            bool rawSpeech = alignedRawSpeech[i];
            bool treatedSpeech = treatedMask[i];
            double rawDbFrame = alignedRawDb[i];
            double treatedDbFrame = treatedDb[i];

            if (rawSpeech && !treatedSpeech && rawDbFrame - treatedDbFrame >= deltaThreshold)
            {
                missingMask[i] = true;
            }

            if (!rawSpeech && treatedSpeech && treatedDbFrame - rawDbFrame >= deltaThreshold)
            {
                extraMask[i] = true;
            }
        }

        var missing = CollectRuns(missingMask, stepSec, windowSec, minMismatchMs / 1000.0, AudioMismatchType.MissingSpeech);
        var extra   = CollectRuns(extraMask,   stepSec, windowSec, minMismatchMs / 1000.0, AudioMismatchType.ExtraSpeech);

        // Merge runs that are close
        var merged = MergeRuns(missing.Concat(extra).ToList(), (int)Math.Round((minGapToMergeMs/1000.0)/stepSec));

        // 6) Aggregate each run to averages and attach sentence context (no full scans)
        // Prebuild an index: time -> sentences covering that time
        var sentenceIndex = BuildSentenceIndex(treatedTimingsById);

        var mismatches = new List<AudioMismatch>(merged.Count);
        var coveredSentences = new HashSet<int>();
        double missingSec = 0, extraSec = 0;
        foreach (var seg in merged)
        {
            int i0 = Math.Max(0, seg.StartIndex);
            int i1 = Math.Min(treatedDb.Length - 1, seg.EndIndex);
            if (i0 > i1) continue;

            // mean over the run (no allocations)
            double rawSum = 0, treatedSum = 0; int count = 0;
            for (int i = i0; i <= i1; i++)
            {
                rawSum     += alignedRawDb[i];
                treatedSum += treatedDb[i];
                count++;
            }
            if (count == 0) continue;

            double rawMean     = rawSum / count;
            double treatedMean = treatedSum / count;
            double delta       = seg.Type == AudioMismatchType.MissingSpeech
                ? rawMean - treatedMean
                : treatedMean - rawMean;

            double startSec = i0 * stepSec;
            double endSec = i1 * stepSec + windowSec;

            var context = LookupSentenceContext(sentenceIndex, startSec, endSec);
            if (context.Count == 0)
            {
                continue;
            }

            double dur = endSec - startSec;
            if (seg.Type == AudioMismatchType.MissingSpeech) missingSec += dur;
            else                                             extraSec   += dur;

            foreach (var span in context)
            {
                coveredSentences.Add(span.SentenceId);
            }

            mismatches.Add(new AudioMismatch(
                StartSec: startSec,
                EndSec:   endSec,
                Type:     seg.Type,
                RawDb:    rawMean,
                TreatedDb:treatedMean,
                DeltaDb:  delta,
                Sentences: context));
        }

        // Ensure each sentence still present in treated timeline has coverage; if audio is missing,
        // add an explicit mismatch spanning the sentence.
        foreach (var kvp in treatedTimingsById)
        {
            int sentenceId = kvp.Key;
            if (coveredSentences.Contains(sentenceId)) continue;
            if (!rawTimingsById.ContainsKey(sentenceId)) continue;

            var treatedSpan = kvp.Value;
            double spanStart = treatedSpan.StartSec;
            double spanEnd = treatedSpan.EndSec;
            if (!(spanEnd > spanStart)) continue;

            int iStart = Math.Clamp((int)Math.Floor(spanStart / Math.Max(stepSec, 1e-9)), 0, frames - 1);
            int iEnd = Math.Clamp((int)Math.Ceiling(spanEnd / Math.Max(stepSec, 1e-9)), 0, frames - 1);
            if (iEnd < iStart) continue;

            double rawSum = 0, treatedSum = 0;
            int count = 0;
            for (int i = iStart; i <= iEnd; i++)
            {
                double frameTime = i * stepSec;
                if (frameTime < spanStart || frameTime > spanEnd) continue;
                rawSum += alignedRawDb[i];
                treatedSum += treatedDb[i];
                count++;
            }
            if (count == 0) continue;

            double rawMean = rawSum / count;
            double treatedMean = treatedSum / count;
            double diff = rawMean - treatedMean;
            var sentenceSpan = new SentenceSpan(sentenceId, treatedSpan.StartSec, treatedSpan.EndSec);
            double duration = Math.Max(0.0, spanEnd - spanStart);

            if (diff >= deltaThreshold && treatedMean < treatedThr)
            {
                missingSec += duration;
                mismatches.Add(new AudioMismatch(
                    StartSec: spanStart,
                    EndSec: spanEnd,
                    Type: AudioMismatchType.MissingSpeech,
                    RawDb: rawMean,
                    TreatedDb: treatedMean,
                    DeltaDb: diff,
                    Sentences: new[] { sentenceSpan }));
                coveredSentences.Add(sentenceId);
            }
            else if (-diff >= deltaThreshold && rawMean < rawThr)
            {
                extraSec += duration;
                mismatches.Add(new AudioMismatch(
                    StartSec: spanStart,
                    EndSec: spanEnd,
                    Type: AudioMismatchType.ExtraSpeech,
                    RawDb: rawMean,
                    TreatedDb: treatedMean,
                    DeltaDb: -diff,
                    Sentences: new[] { sentenceSpan }));
                coveredSentences.Add(sentenceId);
            }
        }

        mismatches.Sort((a, b) => a.StartSec.CompareTo(b.StartSec));

        double rawSpeechSec     = SumMask(alignedRawSpeech, stepSec, windowSec);
        double treatedSpeechSec = SumMask(treatedMask,      stepSec, windowSec);

        return new AudioVerificationResult(
            WindowMs: windowMs,
            StepMs:   stepMs,
            RawSpeechThresholdDb: rawThr,
            TreatedSpeechThresholdDb: treatedThr,
            DurationSec: durationSec,
            RawSpeechSec: rawSpeechSec,
            TreatedSpeechSec: treatedSpeechSec,
            MissingSpeechSec: missingSec,
            ExtraSpeechSec: extraSec,
            Mismatches: mismatches);
    }

    // ---- helpers (tight loops; all O(N)) ------------------------------------

    private static double[] ComputeDbSeries(float[] samples, int sr, double windowSec, double stepSec)
    {
        int win  = Math.Max(32, (int)Math.Round(windowSec*sr));
        int step = Math.Max(1,  (int)Math.Round(stepSec  *sr));
        int frames = Math.Max(1, (samples.Length + step - 1)/step);

        var db = new double[frames];
        for (int f=0; f<frames; f++)
        {
            int start = f*step;
            if (start >= samples.Length) { db[f] = MinDb; continue; }
            int end   = Math.Min(samples.Length, start + win);
            double sum = 0.0;
            for (int i=start; i<end; i++) { double s = samples[i]; sum += s*s; }
            double rms = Math.Sqrt(sum / Math.Max(1, end - start));
            db[f] = rms > 0 ? 20.0*Math.Log10(rms) : MinDb;
        }
        return db;
    }

    private static double InferSpeechThreshold(double[] db)
    {
        // Robust, cheap: 30th percentile of values above absolute floor
        var vals = new List<double>(db.Length);
        for (int i=0;i<db.Length;i++) if (db[i] > MinDb+1) vals.Add(db[i]);
        if (vals.Count==0) return MinDb + 6;
        vals.Sort();
        return Percentile(vals, 0.30);
    }

    private static bool[] BuildSpeechMask(double[] db, double thr)
    {
        var m = new bool[db.Length];
        for (int i=0;i<db.Length;i++) m[i] = db[i] >= thr;
        return m;
    }

    private static double SampleSeries(double[] series, double stepSec, double t)
    {
        if (series.Length == 0) return MinDb;
        if (t <= 0) return series[0];
        double idx = t / Math.Max(stepSec, 1e-9);
        int i0 = Math.Clamp((int)Math.Floor(idx), 0, series.Length-1);
        int i1 = Math.Min(series.Length-1, i0+1);
        double frac = idx - i0;
        return series[i0] + (series[i1]-series[i0])*frac;
    }

    private static bool SampleMask(bool[] mask, double stepSec, double t)
    {
        if (mask.Length == 0) return false;
        int i = (int)Math.Round(t / Math.Max(stepSec, 1e-9));
        i = Math.Clamp(i, 0, mask.Length-1);
        return mask[i];
    }

    private sealed record Segment(int StartIndex, int EndIndex, AudioMismatchType Type);

    private static List<Segment> CollectRuns(bool[] mask, double stepSec, double windowSec, double minDurSec, AudioMismatchType type)
    {
        var list = new List<Segment>();
        int i=0, n=mask.Length;
        int minLen = Math.Max(1, (int)Math.Ceiling((minDurSec - windowSec) / stepSec));
        while (i<n)
        {
            if (!mask[i]) { i++; continue; }
            int start = i;
            while (i<n && mask[i]) i++;
            int end = i-1;
            if (end-start+1 >= minLen) list.Add(new Segment(start,end,type));
        }
        return list;
    }

    private static List<Segment> MergeRuns(IReadOnlyList<Segment> runs, int maxGapFrames)
    {
        if (runs.Count == 0) return new List<Segment>();
        var outp = new List<Segment>();
        foreach (var g in runs.GroupBy(r => r.Type))
        {
            foreach (var r in g.OrderBy(r => r.StartIndex))
            {
                if (outp.Count == 0 || outp[^1].Type != r.Type || r.StartIndex - outp[^1].EndIndex > maxGapFrames)
                    outp.Add(r);
                else
                    outp[^1] = new Segment(outp[^1].StartIndex, Math.Max(outp[^1].EndIndex, r.EndIndex), r.Type);
            }
        }
        // keep type ordering separated
        return outp.OrderBy(s => s.StartIndex).ToList();
    }

    private static double SumMask(bool[] mask, double stepSec, double windowSec)
    {
        double total = 0;
        for (int i=0;i<mask.Length;i++) if (mask[i]) total += stepSec;
        // include last window tail
        return total + windowSec;
    }

    // ---- treated → raw mapping ------------------------------------------------

    private sealed record MapSeg(double T0, double T1, double A, double B);
    private static List<MapSeg> BuildPiecewiseMap(
        IReadOnlyDictionary<int, SentenceTiming> rawById,
        IReadOnlyDictionary<int, SentenceTiming> treatedById)
    {
        var ids = rawById.Keys.Intersect(treatedById.Keys).OrderBy(id => id);
        var map = new List<MapSeg>();
        foreach (var id in ids)
        {
            var r = rawById[id];
            var t = treatedById[id];
            double t0 = t.StartSec, t1 = Math.Max(t.StartSec, t.EndSec);
            double r0 = r.StartSec, r1 = Math.Max(r.StartSec, r.EndSec);
            double a  = (t1>t0) ? (r1-r0)/(t1-t0) : 0.0;
            double b  = r0 - a*t0;
            map.Add(new MapSeg(t0,t1,a,b));
        }
        // ensure sorted by treated time
        return map.OrderBy(m => m.T0).ToList();
    }

    private static double MapToRaw(IReadOnlyList<MapSeg> map, double t)
    {
        if (map.Count == 0) return t;
        // binary search on T0
        int lo=0, hi=map.Count-1, idx=0;
        while (lo<=hi)
        {
            int mid = (lo+hi)>>1;
            if (map[mid].T0 <= t) { idx = mid; lo = mid+1; }
            else hi = mid-1;
        }
        var s = map[idx];
        return s.A*t + s.B;
    }

    // ---- sentence context -----------------------------------------------------

    private static List<(double Start, double End, int SentenceId)> BuildSentenceIndex(
        IReadOnlyDictionary<int, SentenceTiming> treatedById)
    {
        var list = new List<(double,double,int)>(treatedById.Count);
        foreach (var kv in treatedById)
            list.Add((kv.Value.StartSec, kv.Value.EndSec, kv.Key));
        list.Sort((a,b) => a.Item1.CompareTo(b.Item1));
        return list;
    }

    private static IReadOnlyList<SentenceSpan> LookupSentenceContext(
        List<(double Start, double End, int SentenceId)> index,
        double startSec, double endSec)
    {
        // linear probe from lower bound; this is fast because we call this only per-run (dozens),
        // not per frame.
        var spans = new List<SentenceSpan>(2);
        int i = LowerBound(index, startSec);
        for (; i<index.Count && index[i].Start < endSec; i++)
        {
            if (index[i].End <= startSec) continue;
            spans.Add(new SentenceSpan(index[i].SentenceId, index[i].Start, index[i].End));
        }
        return spans;
    }

    private static int LowerBound(List<(double Start, double End, int SentenceId)> a, double x)
    {
        int lo=0, hi=a.Count;
        while (lo<hi) { int mid=(lo+hi)>>1; if (a[mid].Start < x) lo=mid+1; else hi=mid; }
        return lo;
    }

    private static double Percentile(List<double> sorted, double p)
    {
        if (sorted.Count == 0) return MinDb;
        p = Math.Clamp(p, 0.0, 1.0);
        double idx = p * (sorted.Count - 1);
        int    i0  = (int)Math.Floor(idx);
        int    i1  = Math.Min(sorted.Count - 1, i0 + 1);
        if (i0 == i1) return sorted[i0];
        double f = idx - i0;
        return sorted[i0] + (sorted[i1]-sorted[i0]) * f;
    }
}
