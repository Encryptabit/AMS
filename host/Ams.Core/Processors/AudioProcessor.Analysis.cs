using Ams.Core.Artifacts;

namespace Ams.Core.Processors;

public static partial class AudioProcessor
{
    private const double EnergyEpsilon = 1e-20;

    public static TimingRange SnapToEnergyAuto(
        AudioBuffer buffer,
        TimingRange seed,
        AutoTuneStyle style = AutoTuneStyle.Tight,
        double searchWindowSec = 0.8,
        double windowMs = 50.0,
        double stepMs = 2.0,
        double? preRollMs = null,
        double? postRollMs = null,
        double? hangoverMs = null)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));

        var tuned = AutoTune(buffer, seed, style, searchWindowSec, windowMs, stepMs);

        double pre = preRollMs ?? tuned.PreRollMs;
        double post = postRollMs ?? tuned.PostRollMs;
        double hang = hangoverMs ?? tuned.HangoverMs;

        return SnapToEnergy(
            buffer,
            seed,
            tuned.EnterThresholdDb,
            tuned.ExitThresholdDb,
            searchWindowSec,
            tuned.WindowMs,
            tuned.StepMs,
            pre,
            post,
            hang);
    }

    public static TimingRange SnapToEnergy(
        AudioBuffer buffer,
        TimingRange seed,
        double enterThresholdDb = -45.0,
        double exitThresholdDb = -57.0,
        double searchWindowSec = 0.8,
        double windowMs = 25.0,
        double stepMs = 5.0,
        double preRollMs = 10.0,
        double postRollMs = 10.0,
        double hangoverMs = 90.0)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));

        int sr = buffer.SampleRate;
        int step = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));
        int win = Math.Max(step, (int)Math.Round(windowMs * 0.001 * sr));
        int search = Math.Max(win, (int)Math.Round(searchWindowSec * sr));

        int pre = Math.Max(0, (int)Math.Round(preRollMs * 0.001 * sr));
        int post = Math.Max(0, (int)Math.Round(postRollMs * 0.001 * sr));
        int hang = Math.Max(0, (int)Math.Round(hangoverMs * 0.001 * sr));

        double enterLin = DbToLinear(enterThresholdDb);
        double exitLin = DbToLinear(exitThresholdDb);

        int start = ClampToBuffer(buffer, seed.StartSec);
        int end = ClampToBuffer(buffer, seed.EndSec);
        if (end <= start)
        {
            end = Math.Min(buffer.Length, start + step);
        }

        if (end <= start)
        {
            double s = Math.Max(0, (double)start / sr);
            return new TimingRange(s, s);
        }

        int lastEnergyWindowLength = win;
        int lastHangSamples = hang;

        double WindowRms(int windowStart)
        {
            int lenRef = lastEnergyWindowLength > 0 ? lastEnergyWindowLength : Math.Max(step, 1);
            int sIdx = Math.Clamp(windowStart, 0, Math.Max(0, buffer.Length - 1));
            int len = Math.Min(lenRef, Math.Max(0, buffer.Length - sIdx));
            if (len <= 0)
            {
                return 0.0;
            }

            return CalculateRms(buffer, sIdx, len);
        }

        int ResolveStart(int seedStart, int seedEnd, int localStep, int localSearch, double localEnterLin, double localExitLin)
        {
            int maxStart = Math.Max(0, buffer.Length - Math.Max(1, lastEnergyWindowLength));
            int limitRight = Math.Min(maxStart, seedStart + localSearch);
            int limitLeft = Math.Max(0, seedStart - localSearch);

            int pos = Math.Clamp(seedStart, 0, maxStart);
            double rms = WindowRms(pos);
            if (rms >= localEnterLin)
            {
                while (pos - localStep >= limitLeft && WindowRms(pos - localStep) >= localExitLin)
                {
                    pos -= localStep;
                }

                return Math.Max(0, pos);
            }

            int hard = -1, soft = -1;
            while (pos <= limitRight)
            {
                rms = WindowRms(pos);
                if (rms >= localEnterLin)
                {
                    hard = pos;
                    break;
                }

                if (soft < 0 && rms >= localExitLin)
                {
                    soft = pos;
                }

                pos += localStep;
            }

            if (hard >= 0) return hard;
            if (soft >= 0) return soft;
            return seedStart;
        }

        int sIdx = start;
        double RmsForward(int pos)
        {
            int p = Math.Clamp(pos, 0, Math.Max(0, buffer.Length - 1));
            int len = Math.Min(win, Math.Max(0, buffer.Length - p));
            if (len <= 0) return 0.0;
            return CalculateRms(buffer, p, len);
        }

        double RmsBackward(int pos)
        {
            int eIdx = Math.Clamp(pos, 0, buffer.Length);
            int sIdxLocal = Math.Max(0, eIdx - win);
            int len = eIdx - sIdxLocal;
            if (len <= 0) return 0.0;
            return CalculateRms(buffer, sIdxLocal, len);
        }

        if (RmsForward(sIdx) >= enterLin)
        {
            int limit = Math.Max(0, sIdx - search);
            while (sIdx - step >= limit && RmsBackward(sIdx) >= exitLin)
            {
                sIdx -= step;
            }
        }
        else
        {
            int limit = Math.Min(buffer.Length - Math.Max(1, win), sIdx + search);
            while (sIdx <= limit && RmsForward(sIdx) < enterLin)
            {
                sIdx += step;
            }
        }

        if (sIdx == start && RmsForward(sIdx) < enterLin)
        {
            sIdx = ResolveStart(start, end, step, search, enterLin, exitLin);
        }

        sIdx = Math.Clamp(sIdx, 0, buffer.Length);

        int eIdx = end;
        if (RmsBackward(Math.Max(sIdx, eIdx)) >= enterLin)
        {
            int limit = Math.Min(buffer.Length - Math.Max(1, win), eIdx + search);
            int lastSpeech = eIdx;
            int belowAccum = 0;
            while (eIdx <= limit)
            {
                double rms = RmsBackward(Math.Max(sIdx, eIdx));
                if (rms >= exitLin)
                {
                    lastSpeech = eIdx;
                    belowAccum = 0;
                }
                else
                {
                    belowAccum += step;
                    if (belowAccum >= hang) break;
                }

                eIdx += step;
            }

            eIdx = lastSpeech;
        }
        else
        {
            int limit = Math.Max(sIdx, eIdx - search);
            while (eIdx - step >= limit)
            {
                eIdx -= step;
                if (RmsBackward(Math.Max(sIdx, eIdx)) >= enterLin)
                {
                    break;
                }
            }
        }

        const double TailGuardLookbackMs = 60.0;
        const double TailGuardExtendMs = 20.0;
        const double TailGuardExitDeltaDb = 2.0;
        int lookback = Math.Max(win, (int)Math.Round(TailGuardLookbackMs * 0.001 * sr));
        int extendMax = (int)Math.Round(TailGuardExtendMs * 0.001 * sr);
        double exitGuardLin = DbToLinear(exitThresholdDb - TailGuardExitDeltaDb);

        int guardStart = Math.Max(sIdx + step, eIdx - lookback);
        int guardCount = 0;
        for (int p = guardStart; p < eIdx; p += step)
        {
            if (RmsBackward(Math.Max(sIdx, p)) >= exitLin)
            {
                guardCount++;
            }
        }

        if (guardCount >= 2)
        {
            int limit = Math.Min(buffer.Length - 1, eIdx + extendMax);
            int lastSpeech = eIdx;
            int belowAccum = 0;
            int guardHang = Math.Max(1, lastHangSamples / 2);

            int cursor = eIdx;
            while (cursor <= limit)
            {
                double rms = RmsBackward(Math.Max(sIdx, cursor));
                if (rms >= exitGuardLin)
                {
                    lastSpeech = cursor;
                    belowAccum = 0;
                }
                else
                {
                    belowAccum += step;
                    if (belowAccum >= guardHang) break;
                }

                cursor += step;
            }

            eIdx = Math.Max(eIdx, lastSpeech);
        }

        eIdx = Math.Clamp(eIdx, sIdx, buffer.Length);

        int finalStart = Math.Max(0, sIdx - pre);
        int finalEnd = Math.Min(buffer.Length, Math.Max(finalStart, eIdx + post));
        return new TimingRange(finalStart / (double)sr, finalEnd / (double)sr);
    }

    public static double MeasureRms(AudioBuffer buffer, double startSec, double endSec)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        double lo = Math.Min(startSec, endSec);
        double hi = Math.Max(startSec, endSec);
        int s = ClampToBuffer(buffer, lo);
        int e = ClampToBuffer(buffer, hi);
        if (e <= s) return double.NegativeInfinity;
        return ToDecibels(CalculateRms(buffer, s, e - s));
    }

    public static GapRmsStats AnalyzeGap(
        AudioBuffer buffer,
        double startSec,
        double endSec,
        double stepMs = 10.0,
        double silenceThresholdDb = -45.0)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        double lo = Math.Min(startSec, endSec);
        double hi = Math.Max(startSec, endSec);
        int s = ClampToBuffer(buffer, lo);
        int e = ClampToBuffer(buffer, hi);
        if (e <= s)
        {
            return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);
        }

        int step = Math.Max(1, (int)Math.Round(stepMs * 0.001 * buffer.SampleRate));
        var valuesDb = new List<double>();
        for (int p = s; p < e; p += step)
        {
            int len = Math.Min(step, e - p);
            if (len <= 0) break;
            valuesDb.Add(ToDecibels(CalculateRms(buffer, p, len)));
        }

        if (valuesDb.Count == 0)
        {
            return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);
        }

        double min = double.PositiveInfinity;
        double max = double.NegativeInfinity;
        double sum = 0;
        int silence = 0;

        foreach (var db in valuesDb)
        {
            if (db < min) min = db;
            if (db > max) max = db;
            sum += db;
            if (db <= silenceThresholdDb) silence++;
        }

        double mean = sum / valuesDb.Count;
        double silenceFraction = (double)silence / valuesDb.Count;
        return new GapRmsStats(min, max, mean, silenceFraction);
    }

    public static double FindSpeechEndFromGap(
        AudioBuffer buffer,
        double gapStartSec,
        double gapEndSec,
        double silenceThresholdDb = -55.0,
        double stepMs = 5.0,
        double backoffMs = 3.0)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));

        double lo = Math.Min(gapStartSec, gapEndSec);
        double hi = Math.Max(gapStartSec, gapEndSec);
        int start = ClampToBuffer(buffer, lo);
        int end = ClampToBuffer(buffer, hi);
        if (end <= start) return lo;

        int sr = buffer.SampleRate;
        int window = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));
        int backoff = Math.Max(0, (int)Math.Round(backoffMs * 0.001 * sr));

        for (int pos = end; pos > start; pos -= window)
        {
            int segStart = Math.Max(start, pos - window);
            int len = pos - segStart;
            if (len <= 0) continue;
            double rmsDb = ToDecibels(CalculateRms(buffer, segStart, len));
            if (rmsDb > silenceThresholdDb)
            {
                int boundary = Math.Min(end, pos + backoff);
                return boundary / (double)sr;
            }
        }

        return lo;
    }

    private static SnapAutoTuneResult AutoTune(
        AudioBuffer buffer,
        TimingRange seed,
        AutoTuneStyle style,
        double searchWindowSec,
        double windowMs,
        double stepMs)
    {
        int sr = buffer.SampleRate;
        int win = Math.Max(1, (int)Math.Round(windowMs * 0.001 * sr));
        int hop = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));

        int ctxLo = ClampToBuffer(buffer, Math.Max(0, seed.StartSec - searchWindowSec));
        int ctxHi = ClampToBuffer(buffer, seed.EndSec + searchWindowSec);
        if (ctxHi <= ctxLo)
        {
            ctxLo = 0;
            ctxHi = buffer.Length;
        }

        var dbs = new List<double>(Math.Max(16, (ctxHi - ctxLo) / Math.Max(1, hop)));
        for (int p = ctxLo; p < ctxHi; p += hop)
        {
            int len = Math.Min(win, buffer.Length - p);
            if (len <= 0) break;
            double val = ToDecibels(CalculateRms(buffer, p, len));
            if (!double.IsNaN(val) && !double.IsInfinity(val))
            {
                dbs.Add(val);
            }
        }

        if (dbs.Count == 0)
        {
            return new SnapAutoTuneResult(-45, -57, -70, -50, -30, windowMs, stepMs, 35, 80, 50);
        }

        dbs.Sort();

        double minDb = dbs.First();
        double maxDb = dbs.Last();
        if (!double.IsFinite(minDb) || !double.IsFinite(maxDb) || Math.Abs(maxDb - minDb) < 1e-6)
        {
            double mid = double.IsFinite(minDb) ? minDb : -50.0;
            return new SnapAutoTuneResult(mid + 6, mid - 2, mid - 10, mid - 4, mid + 12, windowMs, stepMs, 35, 80, 50);
        }

        double Percentile(double q)
        {
            if (dbs.Count == 1) return dbs[0];
            double rank = (dbs.Count - 1) * q;
            int i0 = (int)Math.Floor(rank);
            int i1 = (int)Math.Ceiling(rank);
            if (i0 == i1) return dbs[i0];
            double w = rank - i0;
            return dbs[i0] * (1 - w) + dbs[i1] * w;
        }

        double noise = Percentile(0.10);
        double speech = Percentile(0.80);
        double dyn = Math.Max(0, speech - noise);

        const double binW = 1.0;
        double span = Math.Max(binW, maxDb - minDb);
        int bins = Math.Max(8, Math.Min(4096, (int)Math.Ceiling(span / binW)));
        var hist = new int[bins];
        foreach (var v in dbs)
        {
            int b = Math.Clamp((int)Math.Floor((v - minDb) / binW), 0, bins - 1);
            hist[b]++;
        }

        double left = noise + 5;
        double right = speech - 5;
        if (right <= left)
        {
            left = noise + dyn * 0.3;
            right = noise + dyn * 0.6;
        }

        int li = Math.Clamp((int)Math.Floor((left - minDb) / binW), 0, bins - 1);
        int ri = Math.Clamp((int)Math.Ceiling((right - minDb) / binW), 0, bins - 1);
        if (ri < li) (li, ri) = (ri, li);
        int minIdx = li;
        int minCnt = int.MaxValue;
        for (int i = li; i <= ri; i++)
        {
            if (hist[i] < minCnt)
            {
                minCnt = hist[i];
                minIdx = i;
            }
        }

        double valley = minDb + (minIdx + 0.5) * binW;

        double enterDb;
        double exitDb;
        double preMs;
        double postMs;
        double hangMs;

        switch (style)
        {
            case AutoTuneStyle.Balanced:
                enterDb = Clamp(valley + 2, noise + 12, speech - 8);
                exitDb = Clamp(Math.Min(enterDb - 10, valley - 4), noise + 6, enterDb - 7);
                preMs = Math.Max(30, windowMs);
                postMs = Clamp(windowMs * 4.5, 110, 140);
                hangMs = Clamp(windowMs * 3.5, 70, 100);
                break;

            case AutoTuneStyle.Gentle:
                enterDb = Clamp(valley + 1, noise + 10, speech - 6);
                exitDb = Clamp(Math.Min(enterDb - 12, valley - 5), noise + 5, enterDb - 8);
                preMs = Math.Max(40, windowMs * 1.2);
                postMs = Clamp(windowMs * 5.5, 130, 170);
                hangMs = Clamp(windowMs * 4.0, 80, 120);
                break;

            case AutoTuneStyle.Tight:
            default:
                enterDb = Clamp(valley + 3, noise + 14, speech - 10);
                exitDb = Clamp(Math.Min(enterDb - 8, valley - 3), noise + 8, enterDb - 6);
                preMs = Math.Max(30, windowMs);
                postMs = Clamp(windowMs * 4.0, 100, 130);
                hangMs = Clamp(windowMs * 3.0, 60, 90);
                break;
        }

        return new SnapAutoTuneResult(
            enterDb,
            exitDb,
            noise,
            valley,
            speech,
            windowMs,
            stepMs,
            preMs,
            postMs,
            hangMs);
    }

    private static double CalculateRms(AudioBuffer buffer, int startSample, int length)
    {
        if (length <= 0) return 0.0;

        double sum = 0.0;
        int count = 0;
        for (int ch = 0; ch < buffer.Channels; ch++)
        {
            var span = buffer.Planar[ch];
            int end = Math.Min(span.Length, startSample + length);
            for (int i = startSample; i < end; i++)
            {
                double s = span[i];
                sum += s * s;
                count++;
            }
        }

        if (count == 0) return 0.0;
        return Math.Sqrt(sum / count);
    }

    private static int ClampToBuffer(AudioBuffer buffer, double sec)
    {
        int s = (int)Math.Round(sec * buffer.SampleRate);
        return Math.Clamp(s, 0, buffer.Length);
    }

    private static double ToDecibels(double linear) =>
        linear <= 0 ? double.NegativeInfinity : 20.0 * Math.Log10(linear);

    private static double DbToLinear(double db) =>
        db <= -120 ? 0.0 : Math.Pow(10.0, db / 20.0);

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}

public enum AutoTuneStyle
{
    Tight,
    Balanced,
    Gentle
}

public sealed record GapRmsStats(double MinRmsDb, double MaxRmsDb, double MeanRmsDb, double SilenceFraction);

public sealed record SnapAutoTuneResult(
    double EnterThresholdDb,
    double ExitThresholdDb,
    double NoiseFloorDb,
    double ValleyDb,
    double SpeechDb,
    double WindowMs,
    double StepMs,
    double PreRollMs,
    double PostRollMs,
    double HangoverMs);
