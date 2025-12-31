using Ams.Core.Artifacts;

namespace Ams.Core.Processors;

public static partial class AudioProcessor
{
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

        int ResolveStart(int seedStart, int seedEnd, int localStep, int localSearch, double localEnterLin,
            double localExitLin)
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
}