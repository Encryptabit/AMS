using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
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

    public enum AutoTuneStyle { Tight, Balanced, Gentle }

    public sealed class AudioAnalysisService
    {
        private readonly AudioBuffer _buffer;
        private const double Eps = 1e-20;
        private int _lastEnergyWindowLength;
        private int _lastHangSamples;

        public AudioAnalysisService(AudioBuffer buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        // --------------------------------------------------------------------
        // PUBLIC API
        // --------------------------------------------------------------------

        /// <summary>
        /// Auto-tunes thresholds and paddings from audio near <paramref name="seed"/>,
        /// then snaps using a style (Tight/Balanced/Gentle). Default is Tight (clean edges).
        /// </summary>
        public TimingRange SnapToEnergyAuto(
            TimingRange seed,
            AutoTuneStyle style = AutoTuneStyle.Tight,
            double searchWindowSec = 0.8,
            double windowMs = 50.0,
            double stepMs = 2.0,
            double? preRollMs = null,
            double? postRollMs = null,
            double? hangoverMs = null)
        {
            var tuned = AutoTune(seed, style, searchWindowSec, windowMs, stepMs);

            // allow overrides for the time paddings
            double pre  = preRollMs  ?? tuned.PreRollMs;
            double post = postRollMs ?? tuned.PostRollMs;
            double hang = hangoverMs ?? tuned.HangoverMs;

            return SnapToEnergy(
                seed,
                enterThresholdDb: tuned.EnterThresholdDb,
                exitThresholdDb : tuned.ExitThresholdDb,
                searchWindowSec : searchWindowSec,
                windowMs        : tuned.WindowMs,
                stepMs          : tuned.StepMs,
                preRollMs       : pre,
                postRollMs      : post,
                hangoverMs      : hang);
        }

        /// <summary>
        /// Hysteresis-based RMS snap with tail-friendly defaults.
        /// TailGuard will extend ends slightly only when the boundary is risky.
        /// </summary>
        public TimingRange SnapToEnergy(
            TimingRange seed,
            double enterThresholdDb = -45.0,
            double exitThresholdDb  = -57.0,
            double searchWindowSec  = 0.8,
            double windowMs         = 25.0,
            double stepMs           = 5.0,
            double preRollMs        = 10.0,
            double postRollMs       = 10.0,
            double hangoverMs       = 10.0)
        {
            int sr    = _buffer.SampleRate;
            int step  = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));
            int win   = Math.Max(step, (int)Math.Round(windowMs * 0.001 * sr));
            int search= Math.Max(win,   (int)Math.Round(searchWindowSec * sr));

            int pre   = Math.Max(0, (int)Math.Round(preRollMs  * 0.001 * sr));
            int post  = Math.Max(0, (int)Math.Round(postRollMs * 0.001 * sr));
            int hang  = Math.Max(0, (int)Math.Round(hangoverMs * 0.001 * sr));

            double enterLin = DbToLin(enterThresholdDb);
            double exitLin  = DbToLin(exitThresholdDb);

            int start = ClampToBuffer(seed.StartSec);
            int end   = ClampToBuffer(seed.EndSec);
            if (end <= start) end = Math.Min(_buffer.Length, start + step);
            if (end <= start)
            {
                double s = Math.Max(0, (double)start / sr);
                return new TimingRange(s, s);
            }

            _lastEnergyWindowLength = win;
            _lastHangSamples = hang;

            // RMS helpers
            double RmsFwd(int pos)  // [pos, pos+win)
            {
                int p = Math.Clamp(pos, 0, Math.Max(0, _buffer.Length - 1));
                int len = Math.Min(win, Math.Max(0, _buffer.Length - p));
                if (len <= 0) return 0.0;
                return CalculateRms(p, len);
            }
            double RmsBack(int pos) // [pos-win, pos)
            {
                int e = Math.Clamp(pos, 0, _buffer.Length);
                int s = Math.Max(0, e - win);
                int len = e - s;
                if (len <= 0) return 0.0;
                return CalculateRms(s, len);
            }

            // ---- START: move toward first speech window
            int sIdx = start;
            if (RmsFwd(sIdx) >= enterLin)
            {
                int limit = Math.Max(0, sIdx - search);
                while (sIdx - step >= limit && RmsBack(sIdx) >= exitLin) sIdx -= step;
            }
            else
            {
                int limit = Math.Min(_buffer.Length - Math.Max(1, win), sIdx + search);
                while (sIdx <= limit && RmsFwd(sIdx) < enterLin) sIdx += step;
            }
            if (sIdx == start && RmsFwd(sIdx) < enterLin)
                sIdx = ResolveStart(start, end, step, search, enterLin, exitLin);
            sIdx = Math.Clamp(sIdx, 0, _buffer.Length);

            // ---- END: tail-friendly with backward windows + hangover
            int eIdx = end;
            if (RmsBack(Math.Max(sIdx, eIdx)) >= enterLin)
            {
                int limit = Math.Min(_buffer.Length - Math.Max(1, win), eIdx + search);
                int lastSpeech = eIdx;
                int belowAccum = 0;
                while (eIdx <= limit)
                {
                    double rms = RmsBack(Math.Max(sIdx, eIdx));
                    if (rms >= exitLin) { lastSpeech = eIdx; belowAccum = 0; }
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
                    if (RmsBack(Math.Max(sIdx, eIdx)) >= enterLin) break;
                }
            }

            // ---- TailGuard: extend slightly if boundary likely cut a tail
            // Look at the last ~60ms; if multiple windows are still above exit, do a short extension
            const double TailGuardLookbackMs = 60.0;
            const double TailGuardExtendMs   = 20.0;
            const double TailGuardExitDeltaDb= 2.0;   // lower exit by 2 dB during guard
            int lookback = Math.Max(win, (int)Math.Round(TailGuardLookbackMs * 0.001 * sr));
            int extendMax= (int)Math.Round(TailGuardExtendMs   * 0.001 * sr);
            double exitGuardLin = DbToLin(exitThresholdDb - TailGuardExitDeltaDb);

            int guardStart = Math.Max(sIdx + step, eIdx - lookback);
            int guardCount = 0;
            for (int p = guardStart; p < eIdx; p += step)
                if (RmsBack(Math.Max(sIdx, p)) >= exitLin) guardCount++;

            if (guardCount >= 2) // only if there was meaningful energy right at the end
            {
                int limit = Math.Min(_buffer.Length - 1, eIdx + extendMax);
                int lastSpeech = eIdx;
                int belowAccum = 0;
                int guardHang  = Math.Max(1, _lastHangSamples / 2); // shorter hang in the guard pass

                int cursor = eIdx;
                while (cursor <= limit)
                {
                    double rms = RmsBack(Math.Max(sIdx, cursor));
                    if (rms >= exitGuardLin) { lastSpeech = cursor; belowAccum = 0; }
                    else
                    {
                        belowAccum += step;
                        if (belowAccum >= guardHang) break;
                    }
                    cursor += step;
                }
                eIdx = Math.Max(eIdx, lastSpeech);
            }

            eIdx = Math.Clamp(eIdx, sIdx, _buffer.Length);

            // Apply paddings
            int finalStart = Math.Max(0, sIdx - pre);
            int finalEnd   = Math.Min(_buffer.Length, Math.Max(finalStart, eIdx + post));
            return new TimingRange(finalStart / (double)sr, finalEnd / (double)sr);
        }

        /// <summary>Returns RMS over [startSec, endSec) as dBFS. -Inf if the range is empty.</summary>
        public double MeasureRms(double startSec, double endSec)
        {
            double lo = Math.Min(startSec, endSec);
            double hi = Math.Max(startSec, endSec);
            int s = ClampToBuffer(lo);
            int e = ClampToBuffer(hi);
            if (e <= s) return double.NegativeInfinity;
            return ToDb(CalculateRms(s, e - s));
        }

        /// <summary>Slides a fixed window across the gap and summarizes RMS in dB.</summary>
        public GapRmsStats AnalyzeGap(double startSec, double endSec, double stepMs = 10.0, double silenceThresholdDb = -45.0)
        {
            double lo = Math.Min(startSec, endSec);
            double hi = Math.Max(startSec, endSec);
            int s = ClampToBuffer(lo);
            int e = ClampToBuffer(hi);
            if (e <= s) return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);

            int step = Math.Max(1, (int)Math.Round(stepMs * 0.001 * _buffer.SampleRate));
            var valuesDb = new List<double>();
            for (int p = s; p < e; p += step)
            {
                int len = Math.Min(step, e - p);
                if (len <= 0) break;
                valuesDb.Add(ToDb(CalculateRms(p, len)));
            }
            if (valuesDb.Count == 0) return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);

            double min = double.PositiveInfinity, max = double.NegativeInfinity, sum = 0;
            int sil = 0;
            foreach (var db in valuesDb)
            {
                if (db < min) min = db;
                if (db > max) max = db;
                sum += db;
                if (db <= silenceThresholdDb) sil++;
            }
            return new GapRmsStats(min, max, sum / valuesDb.Count, (double)sil / valuesDb.Count);
        }

        /// <summary>
        /// Walks a gap from the right edge toward the left, stopping on the first window
        /// that exceeds <paramref name="silenceThresholdDb"/>. Returns the boundary (in seconds)
        /// slightly past the detected speech using <paramref name="backoffMs"/>.
        /// </summary>
        public double FindSpeechEndFromGap(double gapStartSec, double gapEndSec, double silenceThresholdDb = -55.0, double stepMs = 5.0, double backoffMs = 3.0)
        {
            double lo = Math.Min(gapStartSec, gapEndSec);
            double hi = Math.Max(gapStartSec, gapEndSec);
            int start = ClampToBuffer(lo);
            int end = ClampToBuffer(hi);
            if (end <= start) return lo;

            int sr = _buffer.SampleRate;
            int window = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));
            int backoff = Math.Max(0, (int)Math.Round(backoffMs * 0.001 * sr));

            for (int pos = end; pos > start; pos -= window)
            {
                int segStart = Math.Max(start, pos - window);
                int len = pos - segStart;
                if (len <= 0) continue;
                double rmsDb = ToDb(CalculateRms(segStart, len));
                if (rmsDb > silenceThresholdDb)
                {
                    int boundary = Math.Min(end, pos + backoff);
                    return boundary / (double)sr;
                }
            }

            return lo;
        }

        // --------------------------------------------------------------------
        // AUTO-TUNER
        // --------------------------------------------------------------------

        public SnapAutoTuneResult AutoTune(
            TimingRange seed,
            AutoTuneStyle style = AutoTuneStyle.Tight,
            double searchWindowSec = 0.8,
            double windowMs = 25.0,
            double stepMs = 5.0)
        {
            int sr  = _buffer.SampleRate;
            int win = Math.Max(1, (int)Math.Round(windowMs * 0.001 * sr));
            int hop = Math.Max(1, (int)Math.Round(stepMs * 0.001 * sr));

            int ctxLo = ClampToBuffer(Math.Max(0, seed.StartSec - searchWindowSec));
            int ctxHi = ClampToBuffer(seed.EndSec + searchWindowSec);
            if (ctxHi <= ctxLo)
            {
                ctxLo = 0;
                ctxHi = _buffer.Length;
            }

            var dbs = new List<double>(Math.Max(16, (ctxHi - ctxLo) / Math.Max(1, hop)));
            for (int p = ctxLo; p < ctxHi; p += hop)
            {
                int len = Math.Min(win, _buffer.Length - p);
                if (len <= 0) break;
                double val = ToDb(CalculateRms(p, len));
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

            double P(double q)
            {
                if (dbs.Count == 1) return dbs[0];
                double rank = (dbs.Count - 1) * q;
                int i0 = (int)Math.Floor(rank);
                int i1 = (int)Math.Ceiling(rank);
                if (i0 == i1) return dbs[i0];
                double w = rank - i0;
                return dbs[i0] * (1 - w) + dbs[i1] * w;
            }

            double noise = P(0.10);
            double speech= P(0.80);
            double dyn   = Math.Max(0, speech - noise);

            const double binW = 1.0;
            double span = Math.Max(binW, maxDb - minDb);
            int bins = Math.Max(8, Math.Min(4096, (int)Math.Ceiling(span / binW)));
            var hist = new int[bins];
            foreach (var v in dbs)
            {
                int b = Math.Clamp((int)Math.Floor((v - minDb) / binW), 0, bins - 1);
                hist[b]++;
            }

            double left  = noise + 5;
            double right = speech - 5;
            if (right <= left)
            {
                left  = noise + dyn * 0.3;
                right = noise + dyn * 0.6;
            }
            int li = Math.Clamp((int)Math.Floor((left  - minDb) / binW), 0, bins - 1);
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

            double enterDb, exitDb, preMs, postMs, hangMs;
            switch (style)
            {
                case AutoTuneStyle.Balanced:
                    enterDb = Clamp(valley + 2, noise + 12, speech - 8);
                    exitDb  = Clamp(Math.Min(enterDb - 10, valley - 4), noise + 6, enterDb - 7);
                    preMs   = Math.Max(30, windowMs);
                    postMs  = Clamp(windowMs * 4.5, 110, 140);
                    hangMs  = Clamp(windowMs * 3.5, 70, 100);
                    break;

                case AutoTuneStyle.Gentle:
                    enterDb = Clamp(valley + 1, noise + 10, speech - 6);
                    exitDb  = Clamp(Math.Min(enterDb - 12, valley - 5), noise + 5, enterDb - 8);
                    preMs   = Math.Max(40, windowMs * 1.2);
                    postMs  = Clamp(windowMs * 5.5, 130, 170);
                    hangMs  = Clamp(windowMs * 4.0, 80, 120);
                    break;

                case AutoTuneStyle.Tight:
                default:
                    enterDb = Clamp(valley + 3, noise + 14, speech - 10);
                    exitDb  = Clamp(Math.Min(enterDb - 8, valley - 3), noise + 8, enterDb - 6);
                    preMs   = Math.Max(30, windowMs);
                    postMs  = Clamp(windowMs * 4.0, 100, 130);
                    hangMs  = Clamp(windowMs * 3.0,  60,  90);
                    break;
            }

            double pre  = preMs;
            double post = postMs;
            double hang = hangMs;

            return new SnapAutoTuneResult(
                enterDb,
                exitDb,
                noise,
                valley,
                speech,
                windowMs,
                stepMs,
                pre,
                post,
                hang);
        }

        // --------------------------------------------------------------------
        // IMPLEMENTATION
        // --------------------------------------------------------------------

        private int ResolveStart(int seedStart, int seedEnd, int step, int search, double enterLin, double exitLin)
        {
            int win = _lastEnergyWindowLength > 0 ? _lastEnergyWindowLength : Math.Max(step, 1);
            int maxStart = Math.Max(0, _buffer.Length - Math.Max(1, win));
            int limitRight = Math.Min(maxStart, seedStart + search);
            int limitLeft  = Math.Max(0, seedStart - search);

            int pos = Math.Clamp(seedStart, 0, maxStart);
            double rms = WindowRms(pos);
            if (rms >= enterLin)
            {
                while (pos - step >= limitLeft && WindowRms(pos - step) >= exitLin) pos -= step;
                return Math.Max(0, pos);
            }

            int hard = -1, soft = -1;
            while (pos <= limitRight)
            {
                rms = WindowRms(pos);
                if (rms >= enterLin) { hard = pos; break; }
                if (soft < 0 && rms >= exitLin) soft = pos;
                pos += step;
            }
            if (hard >= 0) return hard;
            if (soft >= 0) return soft;
            return seedStart;
        }

        private int ResolveEnd(int resolvedStart, int seedEnd, int step, int search, double enterLin, double exitLin)
        {
            int win = _lastEnergyWindowLength > 0 ? _lastEnergyWindowLength : Math.Max(step, 1);
            int hang = _lastHangSamples;
            int maxStart = Math.Max(0, _buffer.Length - Math.Max(1, win));
            int limitRight = Math.Min(maxStart, seedEnd + search);
            int limitLeft  = Math.Max(resolvedStart, seedEnd - search);

            int pos = Math.Clamp(seedEnd, resolvedStart, _buffer.Length);
            double rms = WindowRms(Math.Max(resolvedStart, pos - win));
            if (rms >= enterLin)
            {
                int lastSpeech = pos;
                int below = 0;
                while (pos <= limitRight)
                {
                    rms = WindowRms(pos);
                    if (rms >= exitLin) { lastSpeech = pos; below = 0; }
                    else
                    {
                        below += step;
                        if (below >= hang) break;
                    }
                    pos += step;
                }
                return Math.Clamp(lastSpeech, resolvedStart, _buffer.Length);
            }

            int hard = -1, soft = -1;
            while (pos - step >= limitLeft)
            {
                pos -= step;
                rms = WindowRms(pos);
                if (rms >= enterLin) { hard = pos; break; }
                if (soft < 0 && rms >= exitLin) soft = pos;
            }
            if (hard >= 0) return Math.Clamp(hard, resolvedStart, _buffer.Length);
            if (soft >= 0) return Math.Clamp(soft, resolvedStart, _buffer.Length);
            return Math.Clamp(seedEnd, resolvedStart, _buffer.Length);
        }

        private double WindowRms(int windowStart)
        {
            int lenRef = _lastEnergyWindowLength > 0 ? _lastEnergyWindowLength : 1;
            int s = Math.Clamp(windowStart, 0, Math.Max(0, _buffer.Length - 1));
            int len = Math.Min(lenRef, Math.Max(0, _buffer.Length - s));
            if (len <= 0) return 0.0;
            return CalculateRms(s, len);
        }

        private double CalculateRms(int startSample, int length)
        {
            if (length <= 0) return 0.0;
            double sum = 0.0; int count = 0;
            for (int ch = 0; ch < _buffer.Channels; ch++)
            {
                var span = _buffer.Planar[ch];
                int end = Math.Min(span.Length, startSample + length);
                for (int i = startSample; i < end; i++) { double s = span[i]; sum += s * s; count++; }
            }
            if (count == 0) return 0.0;
            return Math.Sqrt(sum / count);
        }

        private static double ToDb(double linear) =>
            linear <= 0 ? double.NegativeInfinity : 20.0 * Math.Log10(linear);
        private static double DbToLin(double db) => db <= -120 ? 0.0 : Math.Pow(10.0, db / 20.0);
        private static double Clamp(double v, double lo, double hi) => v < lo ? lo : (v > hi ? hi : v);

        private int ClampToBuffer(double sec)
        {
            int s = (int)Math.Round(sec * _buffer.SampleRate);
            return Math.Clamp(s, 0, _buffer.Length);
        }
    }
}
