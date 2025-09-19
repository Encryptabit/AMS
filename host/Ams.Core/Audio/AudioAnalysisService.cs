using System;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

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
