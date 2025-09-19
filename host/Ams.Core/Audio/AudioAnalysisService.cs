using System;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public sealed record GapRmsStats(double MinRmsDb, double MaxRmsDb, double MeanRmsDb, double SilenceFraction);

public sealed class AudioAnalysisService
{
    private readonly AudioBuffer _buffer;

    public AudioAnalysisService(AudioBuffer buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
    }

    public TimingRange SnapToEnergy(TimingRange seed, double rmsThresholdDb = -35.0, double searchWindowSec = 0.5, double stepMs = 10)
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

        // Expand backwards until RMS exceeds threshold
        int newStart = startSample;
        for (int pos = startSample; pos >= Math.Max(0, startSample - searchSamples); pos -= stepSamples)
        {
            int window = Math.Min(stepSamples, endSample - pos);
            if (window <= 0) break;
            double rms = CalculateRms(pos, window);
            if (rms < thresholdLinear)
            {
                newStart = pos;
            }
            else
            {
                break;
            }
        }

        // Expand forwards until RMS falls below threshold
        int newEnd = endSample;
        for (int pos = endSample; pos <= Math.Min(_buffer.Length - 1, endSample + searchSamples); pos += stepSamples)
        {
            int start = Math.Max(newStart, pos - stepSamples);
            int window = Math.Min(stepSamples, _buffer.Length - start);
            if (window <= 0) break;
            double rms = CalculateRms(start, window);
            if (rms < thresholdLinear)
            {
                newEnd = start;
                break;
            }
            newEnd = start + window;
        }

        double startSec = Math.Max(0, (double)newStart / _buffer.SampleRate);
        double endSec = Math.Max(startSec, (double)newEnd / _buffer.SampleRate);
        return new TimingRange(startSec, endSec);
    }

    public double MeasureRms(double startSec, double endSec)
    {
        double lo = Math.Min(startSec, endSec);
        double hi = Math.Max(startSec, endSec);
        int startSample = ClampToBuffer(lo);
        int endSample = ClampToBuffer(hi);
        if (endSample <= startSample)
        {
            return double.NegativeInfinity;
        }

        double rms = CalculateRms(startSample, endSample - startSample);
        return ToDb(rms);
    }

    public GapRmsStats AnalyzeGap(double startSec, double endSec, double stepMs = 10.0, double silenceThresholdDb = -45.0)
    {
        double gapStart = Math.Min(startSec, endSec);
        double gapEnd = Math.Max(startSec, endSec);

        int startSample = ClampToBuffer(gapStart);
        int endSample = ClampToBuffer(gapEnd);
        if (endSample <= startSample)
        {
            return new GapRmsStats(-120.0, -120.0, -120.0, 1.0);
        }

        int stepSamples = Math.Max(1, (int)Math.Round(stepMs * 0.001 * _buffer.SampleRate));
        double silenceThresholdLinear = Math.Pow(10.0, silenceThresholdDb / 20.0);

        double minLinear = double.MaxValue;
        double maxLinear = 0.0;
        double sumLinear = 0.0;
        int windows = 0;
        int silentWindows = 0;

        for (int pos = startSample; pos < endSample; pos += stepSamples)
        {
            int window = Math.Min(stepSamples, endSample - pos);
            if (window <= 0) continue;

            double rms = CalculateRms(pos, window);
            minLinear = Math.Min(minLinear, rms);
            maxLinear = Math.Max(maxLinear, rms);
            sumLinear += rms;
            windows++;

            if (rms <= silenceThresholdLinear)
            {
                silentWindows++;
            }
        }

        if (windows == 0)
        {
            return new GapRmsStats(-120.0, -120.0, -120.0, 1.0);
        }

        double meanLinear = sumLinear / windows;
        return new GapRmsStats(ToDb(minLinear), ToDb(maxLinear), ToDb(meanLinear), silentWindows / (double)windows);
    }

    private static double ToDb(double linear)
    {
        return linear <= 0 ? -120.0 : 20.0 * Math.Log10(linear);
    }

    private int ClampToBuffer(double seconds)
    {
        var sample = (int)Math.Round(seconds * _buffer.SampleRate);
        return Math.Clamp(sample, 0, _buffer.Length);
    }

    private double CalculateRms(int startSample, int length)
    {
        if (length <= 0) return 0.0;
        double sum = 0.0;
        int count = 0;
        for (int ch = 0; ch < _buffer.Channels; ch++)
        {
            var span = _buffer.Planar[ch];
            for (int i = 0; i < length; i++)
            {
                int idx = startSample + i;
                if (idx >= span.Length) break;
                double sample = span[idx];
                sum += sample * sample;
                count++;
            }
        }
        if (count == 0) return 0.0;
        return Math.Sqrt(sum / count);
    }
}
