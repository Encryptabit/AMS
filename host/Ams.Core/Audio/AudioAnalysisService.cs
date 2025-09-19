using System;
using System.Collections.Generic;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio
{
    public sealed record GapRmsStats(double MinRmsDb, double MaxRmsDb, double MeanRmsDb, double SilenceFraction);

    public sealed class AudioAnalysisService
    {
        private readonly AudioBuffer _buffer;
        private const double Eps = 1e-20;

        public AudioAnalysisService(AudioBuffer buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        /// <summary>
        /// Expands/shrinks a seed timing to the nearest energy boundaries using an RMS threshold.
        /// Scans backward (pre-roll) while below threshold, then forward until it falls below threshold.
        /// </summary>
        public TimingRange SnapToEnergy(
        TimingRange seed,
        double rmsThresholdDb = -35.0,
        double searchWindowSec = 0.5,
        double stepMs = 10.0)
    {
        double thresholdLinear = Math.Pow(10.0, rmsThresholdDb / 20.0);
        int stepSamples = Math.Max(1, (int)Math.Round(stepMs * 0.001 * _buffer.SampleRate));
        int searchSamples = Math.Max(stepSamples, (int)Math.Round(searchWindowSec * _buffer.SampleRate));

        int startSample = ClampToBuffer(seed.StartSec);
        int endSample = ClampToBuffer(seed.EndSec);

        if (endSample <= startSample)
        {
            endSample = Math.Min(_buffer.Length, startSample + stepSamples);
        }

        if (endSample <= startSample)
        {
            double startFallback = Math.Max(0, (double)startSample / _buffer.SampleRate);
            return new TimingRange(startFallback, startFallback);
        }

        int guardSamples = Math.Max(1, stepSamples / 2);

        int newStart = ShrinkStart(startSample, endSample, thresholdLinear, stepSamples, searchSamples, guardSamples);
        int newEnd = ShrinkEnd(newStart, endSample, thresholdLinear, stepSamples, searchSamples, guardSamples);

        double startSec = Math.Max(0, (double)newStart / _buffer.SampleRate);
        double endSec = Math.Max(startSec, (double)newEnd / _buffer.SampleRate);
        return new TimingRange(startSec, endSec);
    }

    private int ShrinkStart(
        int startSample,
        int endSample,
        double thresholdLinear,
        int stepSamples,
        int searchSamples,
        int guardSamples)
    {
        int newStart = startSample;
        int maxSearch = Math.Min(_buffer.Length, startSample + searchSamples);
        for (int pos = startSample; pos < maxSearch; pos += stepSamples)
        {
            int windowStart = pos;
            int windowSize = Math.Min(stepSamples, endSample - windowStart);
            if (windowSize <= 0) break;

            double rms = CalculateRms(windowStart, windowSize);
            if (rms >= thresholdLinear)
            {
                newStart = Math.Max(startSample, windowStart - guardSamples);
                return newStart;
            }
        }

        return newStart;
    }

    private int ShrinkEnd(
        int startSample,
        int endSample,
        double thresholdLinear,
        int stepSamples,
        int searchSamples,
        int guardSamples)
    {
        int newEnd = endSample;
        int minSearch = Math.Max(startSample, endSample - searchSamples);
        for (int windowEnd = endSample; windowEnd > minSearch; windowEnd -= stepSamples)
        {
            int windowStart = Math.Max(startSample, windowEnd - stepSamples);
            int windowSize = Math.Max(1, windowEnd - windowStart);
            if (windowSize <= 0) continue;

            double rms = CalculateRms(windowStart, windowSize);
            if (rms >= thresholdLinear)
            {
                newEnd = Math.Min(endSample, windowEnd + guardSamples);
                return Math.Max(newEnd, startSample);
            }
        }

        return Math.Max(newEnd, startSample);
    }

        /// <summary>
        /// Returns RMS over [startSec, endSec) as dBFS. -Inf if the range is empty.
        /// </summary>
        public double MeasureRms(double startSec, double endSec)
        {
            double lo = Math.Min(startSec, endSec);
            double hi = Math.Max(startSec, endSec);
            int startSample = ClampToBuffer(lo);
            int endSample = ClampToBuffer(hi);
            if (endSample <= startSample) return double.NegativeInfinity;
            double rms = CalculateRms(startSample, endSample - startSample);
            return ToDb(rms);
        }

        /// <summary>
        /// Slides a fixed window across the gap and summarizes RMS in dB.
        /// SilenceFraction is % of windows below silenceThresholdDb.
        /// </summary>
        public GapRmsStats AnalyzeGap(
            double startSec,
            double endSec,
            double stepMs = 10.0,
            double silenceThresholdDb = -45.0)
        {
            double lo = Math.Min(startSec, endSec);
            double hi = Math.Max(startSec, endSec);

            int start = ClampToBuffer(lo);
            int end = ClampToBuffer(hi);
            if (end <= start)
            {
                return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);
            }

            int step = Math.Max(1, (int)Math.Round(stepMs * 0.001 * _buffer.SampleRate));

            var valuesDb = new List<double>();
            for (int pos = start; pos < end; pos += step)
            {
                int len = Math.Min(step, end - pos);
                if (len <= 0) break;
                double rms = CalculateRms(pos, len);
                valuesDb.Add(ToDb(rms));
            }

            if (valuesDb.Count == 0)
            {
                return new GapRmsStats(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, 1.0);
            }

            double min = double.PositiveInfinity, max = double.NegativeInfinity, sum = 0;
            int sil = 0;
            foreach (var db in valuesDb)
            {
                if (db < min) min = db;
                if (db > max) max = db;
                sum += db;
                if (db <= silenceThresholdDb) sil++;
            }

            double mean = sum / valuesDb.Count;
            double silenceFraction = (double)sil / valuesDb.Count;
            return new GapRmsStats(min, max, mean, silenceFraction);
        }

        /// <summary>
        /// Per-window RMS over all channels (planar), equal-weighting channels and samples.
        /// </summary>
        private double CalculateRms(int startSample, int length)
        {
            if (length <= 0) return 0.0;

            double sum = 0.0;
            int count = 0;

            for (int ch = 0; ch < _buffer.Channels; ch++)
            {
                var span = _buffer.Planar[ch];
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

        private static double ToDb(double linear) =>
            linear <= 0 ? double.NegativeInfinity : 20.0 * Math.Log10(linear);

        private int ClampToBuffer(double sec)
        {
            int s = (int)Math.Round(sec * _buffer.SampleRate);
            return Math.Clamp(s, 0, _buffer.Length);
        }
    }
}
