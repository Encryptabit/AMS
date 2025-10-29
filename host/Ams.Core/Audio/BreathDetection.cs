using System.Numerics;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

/// <summary>
/// Tunable options for the frame-level breath detector.
/// </summary>
public sealed record FrameBreathDetectorOptions
{
    public int SampleRate { get; init; } = 0; // 0 => infer from audio input
    public int FrameMs { get; init; } = 25;
    public int HopMs { get; init; } = 10;
    public int FftSize { get; init; } = 512;
    public double PreEmphasis { get; init; } = 0.97;

    public double HiSplitHz { get; init; } = 4000.0;

    public double AbsFloorDb { get; init; } = -55.0;
    public double AmpMarginDb { get; init; } = 3.0;

    public double WFlat { get; init; } = 0.45;
    public double WHfRatio { get; init; } = 0.35;
    public double WZcr { get; init; } = 0.15;
    public double WInvNacf { get; init; } = 0.20;
    public double WSlope { get; init; } = 0.10;

    public double ScoreHigh { get; init; } = 0.35;
    public double ScoreLow { get; init; } = 0.25;
    public int MinRunMs { get; init; } = 60;
    public int MergeGapMs { get; init; } = 40;

    public int GuardLeftMs { get; init; } = 20;
    public int GuardRightMs { get; init; } = 20;
    public int FricativeGuardMs { get; init; } = 25;

    public double Aggressiveness { get; init; } = 10.0;

    public bool ApplyEnergyGate { get; init; } = true;
}

public readonly record struct PhoneSpan(double StartSec, double EndSec, string Label);

public readonly record struct Region(double StartSec, double EndSec)
{
    public double DurationSec => Math.Max(0d, EndSec - StartSec);

    public Region Expand(double sec) => new(Math.Max(0d, StartSec - sec), EndSec + sec);

    public bool Overlaps(Region other) => !(EndSec <= other.StartSec || other.EndSec <= StartSec);

    public static Region Merge(Region a, Region b) => new(Math.Min(a.StartSec, b.StartSec), Math.Max(a.EndSec, b.EndSec));
}

public sealed class FrameFeatures
{
    public double[] Times { get; init; } = Array.Empty<double>();
    public double[] Db { get; init; } = Array.Empty<double>();
    public double[] Zcr { get; init; } = Array.Empty<double>();
    public double[] Nacf { get; init; } = Array.Empty<double>();
    public double[] HfLf { get; init; } = Array.Empty<double>();
    public double[] Flat { get; init; } = Array.Empty<double>();
    public double[] Slope { get; init; } = Array.Empty<double>();
}

/// <summary>
/// Frame-level breath detector that combines spectral and temporal cues with hysteresis.
/// </summary>
public static class FrameBreathDetector
{
    /// <summary>
    /// Detects breath regions inside a gap using an <see cref="AudioBuffer"/> as the source.
    /// </summary>
    public static IReadOnlyList<Region> Detect(
        AudioBuffer audio,
        double startSec,
        double endSec,
        FrameBreathDetectorOptions options,
        IReadOnlyList<PhoneSpan>? leftPhones = null,
        IReadOnlyList<PhoneSpan>? rightPhones = null)
    {
        if (audio is null) throw new ArgumentNullException(nameof(audio));
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (endSec <= startSec || audio.Length == 0) return Array.Empty<Region>();

        int sampleRate = audio.SampleRate;
        if (sampleRate <= 0)
        {
            throw new ArgumentException("Audio buffer must have a positive sample rate.", nameof(audio));
        }

        if (options.SampleRate > 0 && options.SampleRate != sampleRate)
        {
            throw new ArgumentException($"Breath detector options specify {options.SampleRate} Hz, but audio is {sampleRate} Hz. Set SampleRate=0 to infer.", nameof(options));
        }

        var effectiveOptions = options.SampleRate == 0 ? options with { SampleRate = sampleRate } : options;

        var mono = GetOrCreateMonoReference(audio);
        return Detect(mono, sampleRate, startSec, endSec, effectiveOptions, leftPhones, rightPhones);
    }

    /// <summary>
    /// Detects breath regions using a monophonic buffer that shares the audio sample rate.
    /// </summary>
    public static IReadOnlyList<Region> Detect(
        float[] monoSamples,
        int sampleRate,
        double startSec,
        double endSec,
        FrameBreathDetectorOptions options,
        IReadOnlyList<PhoneSpan>? leftPhones = null,
        IReadOnlyList<PhoneSpan>? rightPhones = null)
    {
        if (monoSamples is null) throw new ArgumentNullException(nameof(monoSamples));
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (monoSamples.Length == 0 || endSec <= startSec) return Array.Empty<Region>();

        var opt = options.SampleRate == 0 ? options with { SampleRate = sampleRate } : options;
        if (opt.SampleRate != sampleRate)
        {
            throw new ArgumentException($"Breath detector options specify {opt.SampleRate} Hz, but samples are {sampleRate} Hz.", nameof(options));
        }

        var features = ExtractFeatures(monoSamples, sampleRate, startSec, endSec, opt);
        if (features.Times.Length == 0) return Array.Empty<Region>();

        double baseDb = Percentile(features.Db, 20);
        double rmsGate = Math.Max(opt.AbsFloorDb, baseDb + opt.AmpMarginDb);

        var flatZ = ZNorm(features.Flat);
        var hfZ = ZNorm(Log10Shift(features.HfLf));
        var zcrZ = ZNorm(features.Zcr);
        var invN = features.Nacf.Select(v => 1.0 - v).ToArray();
        var invNZ = ZNorm(invN);
        var slopeZ = ZNorm(features.Slope);
        for (int i = 0; i < slopeZ.Length; i++) slopeZ[i] = -slopeZ[i];

        var score = new double[features.Times.Length];
        for (int i = 0; i < score.Length; i++)
        {
            double s = 0;
            s += opt.WFlat * Clamp01(flatZ[i]);
            s += opt.WHfRatio * Clamp01(hfZ[i]);
            s += opt.WZcr * Clamp01(zcrZ[i]);
            s += opt.WInvNacf * Clamp01(invNZ[i]);
            s += opt.WSlope * Clamp01(slopeZ[i]);
            if (opt.ApplyEnergyGate && features.Db[i] < rmsGate) s *= 0.25;
            score[i] = Sigmoid((s - 0.5) * 3.0 * opt.Aggressiveness);
        }

        var protect = BuildProtectionMask(features.Times, startSec, endSec, opt, leftPhones, rightPhones);

        var regions = new List<Region>();
        bool on = false;
        int runStart = -1;

        for (int i = 0; i < score.Length; i++)
        {
            double sc = score[i];
            if (!on)
            {
                if (!protect[i] && sc >= opt.ScoreHigh)
                {
                    on = true;
                    runStart = i;
                }
            }
            else
            {
                if (protect[i] || sc < opt.ScoreLow)
                {
                    int runEnd = i - 1;
                    if (runStart >= 0) AddRegion(runStart, runEnd);
                    on = false;
                    runStart = -1;
                }
            }
        }

        if (on && runStart >= 0) AddRegion(runStart, score.Length - 1);

        double mergeGapSec = opt.MergeGapMs / 1000.0;
        double minRunSec = opt.MinRunMs / 1000.0;
        return MergeAndFilter(regions, mergeGapSec, minRunSec);

        void AddRegion(int a, int b)
        {
            if (a < 0 || b < a) return;
            double t0 = features.Times[a];
            double t1 = features.Times[b];
            regions.Add(new Region(Math.Max(startSec, t0), Math.Min(endSec, t1)));
        }
    }

    public static FrameFeatures ExtractFeatures(float[] samples, int sampleRate, double startSec, double endSec, FrameBreathDetectorOptions options)
    {
        int n0 = Math.Max(0, (int)Math.Floor(startSec * sampleRate));
        int n1 = Math.Min(samples.Length, (int)Math.Ceiling(endSec * sampleRate));
        int frame = Math.Max(8, (int)Math.Round(options.FrameMs * 1e-3 * sampleRate));
        int hop = Math.Max(4, (int)Math.Round(options.HopMs * 1e-3 * sampleRate));
        if (frame > samples.Length || n1 - n0 < frame)
        {
            return new FrameFeatures();
        }

        int fftSize = Math.Max(options.FftSize, NextPow2(frame));

        var segment = new float[n1 - n0];
        if (options.PreEmphasis > 0 && options.PreEmphasis < 1)
        {
            segment[0] = samples[n0];
            for (int i = 1; i < segment.Length; i++)
            {
                segment[i] = (float)(samples[n0 + i] - options.PreEmphasis * samples[n0 + i - 1]);
            }
        }
        else
        {
            Array.Copy(samples, n0, segment, 0, segment.Length);
        }

        int frameCount = 1 + (segment.Length - frame) / hop;
        if (frameCount <= 0)
        {
            return new FrameFeatures();
        }

        var times = new double[frameCount];
        var dbs = new double[frameCount];
        var zcr = new double[frameCount];
        var hfLf = new double[frameCount];
        var flat = new double[frameCount];
        var slope = new double[frameCount];
        var nacf = new double[frameCount];

        var window = Hann(frame);
        var fft = new Complex[fftSize];
        int nyquistBin = fftSize / 2;
        int hiSplitBin = (int)Math.Round(Math.Clamp(options.HiSplitHz, 0, sampleRate / 2.0) / (sampleRate / 2.0) * nyquistBin);

        var power = new double[nyquistBin + 1];

        for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            int start = frameIndex * hop;
            times[frameIndex] = (n0 + start + frame / 2.0) / sampleRate;

            double rms = 0.0;
            int zeroCrossings = 0;
            double previous = segment[start];

            for (int j = 0; j < frame; j++)
            {
                double value = segment[start + j] * window[j];
                rms += value * value;
                if ((previous >= 0 && value < 0) || (previous < 0 && value >= 0))
                {
                    zeroCrossings++;
                }
                previous = value;
                fft[j] = new Complex(value, 0);
            }

            for (int j = frame; j < fftSize; j++)
            {
                fft[j] = Complex.Zero;
            }

            rms = Math.Sqrt(rms / frame) + 1e-12;
            dbs[frameIndex] = 20.0 * Math.Log10(rms);
            zcr[frameIndex] = (double)zeroCrossings / frame;

            FFT(fft, forward: true);

            for (int k = 0; k <= nyquistBin; k++)
            {
                var c = fft[k];
                power[k] = c.Real * c.Real + c.Imaginary * c.Imaginary + 1e-12;
            }

            double energyLow = 0.0;
            double energyHigh = 0.0;
            for (int k = 1; k <= nyquistBin; k++)
            {
                if (k <= hiSplitBin)
                {
                    energyLow += power[k];
                }
                else
                {
                    energyHigh += power[k];
                }
            }
            hfLf[frameIndex] = energyHigh / Math.Max(1e-9, energyLow);

            double sumLog = 0.0;
            double sumLin = 0.0;
            int count = 0;
            for (int k = Math.Max(1, hiSplitBin); k <= nyquistBin; k++)
            {
                double magnitude = power[k];
                sumLog += Math.Log(magnitude);
                sumLin += magnitude;
                count++;
            }

            flat[frameIndex] = count > 0 ? Math.Clamp(Math.Exp(sumLog / count) / Math.Max(sumLin / count, 1e-12), 0, 1) : 0;

            double sx = 0.0, sy = 0.0, sxy = 0.0, sxx = 0.0;
            int samplesUsed = 0;
            for (int k = 1; k <= nyquistBin; k++)
            {
                double freqNorm = (double)k / nyquistBin;
                double x = Math.Log(freqNorm + 1e-9);
                double y = Math.Log(power[k]);
                sx += x;
                sy += y;
                sxy += x * y;
                sxx += x * x;
                samplesUsed++;
            }

            slope[frameIndex] = samplesUsed > 1 ? (samplesUsed * sxy - sx * sy) / (samplesUsed * sxx - sx * sx + 1e-12) : 0;

            nacf[frameIndex] = NormalizedAutocorrelation(segment, start, frame, sampleRate);
        }

        return new FrameFeatures
        {
            Times = times,
            Db = dbs,
            Zcr = zcr,
            HfLf = hfLf,
            Flat = flat,
            Slope = slope,
            Nacf = nacf
        };
    }

    private static bool[] BuildProtectionMask(
        IReadOnlyList<double> times,
        double startSec,
        double endSec,
        FrameBreathDetectorOptions options,
        IReadOnlyList<PhoneSpan>? leftPhones,
        IReadOnlyList<PhoneSpan>? rightPhones)
    {
        var protect = new bool[times.Count];
        double guardLeftSec = options.GuardLeftMs / 1000.0;
        double guardRightSec = options.GuardRightMs / 1000.0;

        for (int i = 0; i < protect.Length; i++)
        {
            double t = times[i];
            if (t < startSec + guardLeftSec || t > endSec - guardRightSec)
            {
                protect[i] = true;
            }
        }

        if (leftPhones != null)
        {
            double fricativeGuardSec = options.FricativeGuardMs / 1000.0;
            foreach (var ph in leftPhones)
            {
                if (!IsFricativeLike(ph.Label)) continue;
                double guardStart = ph.EndSec;
                double guardEnd = ph.EndSec + fricativeGuardSec;
                for (int i = 0; i < protect.Length; i++)
                {
                    double t = times[i];
                    if (t >= guardStart && t <= guardEnd)
                    {
                        protect[i] = true;
                    }
                }
            }
        }

        if (rightPhones != null)
        {
            foreach (var ph in rightPhones)
            {
                if (!IsFricativeLike(ph.Label)) continue;
                double guardStart = ph.StartSec - guardLeftSec;
                for (int i = 0; i < protect.Length; i++)
                {
                    double t = times[i];
                    if (t >= guardStart && t <= ph.StartSec)
                    {
                        protect[i] = true;
                    }
                }
            }
        }

        return protect;
    }

    private static float[] GetOrCreateMonoReference(AudioBuffer audio)
    {
        if (audio.Planar.Length == 0)
        {
            return Array.Empty<float>();
        }

        if (audio.Channels == 1)
        {
            return audio.Planar[0];
        }

        int length = audio.Length;
        var mono = new float[length];
        float scale = 1f / audio.Channels;
        for (int ch = 0; ch < audio.Channels; ch++)
        {
            var source = audio.Planar[ch];
            for (int i = 0; i < length; i++)
            {
                mono[i] += source[i] * scale;
            }
        }

        return mono;
    }

    private static double[] Hann(int n)
    {
        var window = new double[n];
        for (int i = 0; i < n; i++)
        {
            window[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / n));
        }

        return window;
    }

    private static int NextPow2(int n)
    {
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    private static double[] Log10Shift(double[] values)
    {
        var result = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = Math.Log10(values[i] + 1e-3);
        }

        return result;
    }

    private static double[] ZNorm(double[] values)
    {
        double mean = values.Average();
        double variance = values.Select(v => (v - mean) * (v - mean)).Average() + 1e-12;
        double std = Math.Sqrt(variance);
        var result = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = (values[i] - mean) / (std + 1e-12);
        }

        return result;
    }

    private static double Percentile(double[] values, double percentile)
    {
        if (values.Length == 0) return 0;
        var sorted = values.ToArray();
        Array.Sort(sorted);
        double idx = (percentile / 100.0) * (sorted.Length - 1);
        int lower = (int)Math.Floor(idx);
        int upper = (int)Math.Ceiling(idx);
        if (lower == upper) return sorted[lower];
        double weight = idx - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }

    private static double NormalizedAutocorrelation(float[] source, int start, int length, int sampleRate)
    {
        int lagMin = Math.Max(1, sampleRate / 400);
        int lagMax = Math.Min(length - 1, sampleRate / 80);
        double denom = 1e-9;
        for (int i = 0; i < length; i++)
        {
            denom += source[start + i] * source[start + i];
        }

        double best = 0.0;
        for (int lag = lagMin; lag <= lagMax; lag++)
        {
            double numerator = 0.0;
            int limit = length - lag;
            for (int i = 0; i < limit; i++)
            {
                numerator += source[start + i] * source[start + i + lag];
            }

            double value = numerator / denom;
            if (value > best) best = value;
        }

        return Math.Clamp(best, 0, 1);
    }

    private static void FFT(Complex[] buffer, bool forward)
    {
        int n = buffer.Length;
        int j = 0;
        for (int i = 1; i < n; i++)
        {
            int bit = n >> 1;
            while ((j & bit) != 0)
            {
                j ^= bit;
                bit >>= 1;
            }
            j ^= bit;
            if (i < j)
            {
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = 2 * Math.PI / len * (forward ? -1 : 1);
            Complex wLen = new(Math.Cos(angle), Math.Sin(angle));
            for (int i = 0; i < n; i += len)
            {
                Complex w = Complex.One;
                for (int k = 0; k < len / 2; k++)
                {
                    Complex u = buffer[i + k];
                    Complex v = buffer[i + k + len / 2] * w;
                    buffer[i + k] = u + v;
                    buffer[i + k + len / 2] = u - v;
                    w *= wLen;
                }
            }
        }

        if (!forward)
        {
            for (int i = 0; i < n; i++)
            {
                buffer[i] /= n;
            }
        }
    }

    private static double Clamp01(double value) => value switch
    {
        < 0 => 0,
        > 1 => 1,
        _ => value
    };

    private static double Sigmoid(double value) => 1.0 / (1.0 + Math.Exp(-value));

    private static bool IsFricativeLike(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return false;
        label = label.ToLowerInvariant();
        return label switch
        {
            "s" or "z" or "ʃ" or "ʒ" or "f" or "v" or "θ" or "ð" or "h" or "ɕ" or "ʑ" => true,
            "sh" or "zh" or "ch" or "jh" => true,
            _ => false
        };
    }

    private static List<Region> MergeAndFilter(List<Region> regions, double mergeGapSec, double minDurationSec)
    {
        var ordered = regions.OrderBy(r => r.StartSec).ToList();
        var merged = new List<Region>();
        foreach (var current in ordered)
        {
            if (merged.Count == 0)
            {
                merged.Add(current);
                continue;
            }

            var last = merged[^1];
            if (current.StartSec - last.EndSec <= mergeGapSec)
            {
                merged[^1] = Region.Merge(last, current);
            }
            else
            {
                merged.Add(current);
            }
        }

        merged.RemoveAll(region => region.DurationSec < minDurationSec);
        return merged;
    }
}
