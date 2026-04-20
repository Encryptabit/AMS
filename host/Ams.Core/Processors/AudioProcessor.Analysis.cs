using System.Globalization;
using System.Text.RegularExpressions;
using Ams.Core.Artifacts;
using Ams.Core.Common;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Processors;

public static partial class AudioProcessor
{
    private static readonly Regex IntegratedLufsRegex = new(
        @"\bI:\s*(?<value>[+-]?(?:\d+(?:\.\d+)?|\d*\.\d+))\s*LUFS\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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
        return MeasureActivity(
            nameof(SnapToEnergy),
            () => SnapToEnergyCore(
                buffer,
                seed,
                enterThresholdDb,
                exitThresholdDb,
                searchWindowSec,
                windowMs,
                stepMs,
                preRollMs,
                postRollMs,
                hangoverMs),
            detail: FormattableString.Invariant(
                $"enterDb={enterThresholdDb:F2},exitDb={exitThresholdDb:F2},searchWindowSec={searchWindowSec:F3}"));
    }

    private static TimingRange SnapToEnergyCore(
        AudioBuffer buffer,
        TimingRange seed,
        double enterThresholdDb,
        double exitThresholdDb,
        double searchWindowSec,
        double windowMs,
        double stepMs,
        double preRollMs,
        double postRollMs,
        double hangoverMs)
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
        return MeasureActivity(
            nameof(MeasureRms),
            () => MeasureRmsCore(buffer, startSec, endSec),
            detail: FormattableString.Invariant($"startSec={startSec:F3},endSec={endSec:F3}"));
    }

    private static double MeasureRmsCore(AudioBuffer buffer, double startSec, double endSec)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        double lo = Math.Min(startSec, endSec);
        double hi = Math.Max(startSec, endSec);
        int s = ClampToBuffer(buffer, lo);
        int e = ClampToBuffer(buffer, hi);
        if (e <= s) return double.NegativeInfinity;
        return ToDecibels(CalculateRms(buffer, s, e - s));
    }

    public static AudioLoudnessMetrics AnalyzeLoudness(
        string filePath,
        AudioDecodeOptions? decodeOptions = null,
        AudioLoudnessAnalysisOptions? options = null)
    {
        return MeasureActivity(
            nameof(AnalyzeLoudness),
            () =>
            {
                var buffer = Decode(filePath, decodeOptions);
                return AnalyzeLoudness(buffer, options);
            },
            detail: Path.GetFileName(filePath));
    }

    public static AudioLoudnessMetrics AnalyzeLoudness(
        AudioBuffer buffer,
        AudioLoudnessAnalysisOptions? options = null)
    {
        return MeasureActivity(
            nameof(AnalyzeLoudness),
            () => AnalyzeLoudnessCore(buffer, options),
            detail: FormattableString.Invariant(
                $"sampleRate={buffer?.SampleRate ?? 0},channels={buffer?.Channels ?? 0},length={buffer?.Length ?? 0}"));
    }

    private static AudioLoudnessMetrics AnalyzeLoudnessCore(
        AudioBuffer buffer,
        AudioLoudnessAnalysisOptions? options)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effectiveOptions = options ?? new AudioLoudnessAnalysisOptions();
        var windowDurationSec = Math.Max(0.01, effectiveOptions.WindowDuration.TotalSeconds);

        var totalSamples = buffer.Length;
        var channels = buffer.Channels;
        if (totalSamples == 0 || channels == 0)
        {
            return new AudioLoudnessMetrics(
                DurationSec: 0,
                SamplePeakLinear: 0,
                SamplePeakDbFs: null,
                TruePeakLinear: 0,
                TruePeakDbFs: null,
                OverallRmsLinear: 0,
                OverallRmsDbFs: null,
                MinWindowRmsLinear: 0,
                MinWindowRmsDbFs: null,
                MaxWindowRmsLinear: 0,
                MaxWindowRmsDbFs: null,
                WindowDurationSec: windowDurationSec,
                IntegratedLufs: null);
        }

        var sampleRate = buffer.SampleRate;
        var perSampleMeanSquare = new double[totalSamples];
        double totalSquares = 0;
        double samplePeak = 0;
        double truePeak = 0;

        for (int ch = 0; ch < channels; ch++)
        {
            var samples = buffer.GetChannel(ch).Span;
            double previous = 0;
            for (int i = 0; i < totalSamples; i++)
            {
                double sample = samples[i];
                double abs = Math.Abs(sample);
                if (abs > samplePeak)
                {
                    samplePeak = abs;
                }

                double square = sample * sample;
                perSampleMeanSquare[i] += square;
                totalSquares += square;

                if (i > 0)
                {
                    var s0 = previous;
                    var s1 = sample;
                    for (int step = 1; step <= 3; step++)
                    {
                        double t = step / 4.0;
                        double interpolated = s0 + (s1 - s0) * t;
                        double interpolatedAbs = Math.Abs(interpolated);
                        if (interpolatedAbs > truePeak)
                        {
                            truePeak = interpolatedAbs;
                        }
                    }
                }

                previous = sample;
            }
        }

        truePeak = Math.Max(truePeak, samplePeak);

        for (int i = 0; i < totalSamples; i++)
        {
            perSampleMeanSquare[i] /= channels;
        }

        var overallRms = Math.Sqrt(totalSquares / (channels * (double)totalSamples));

        int windowSamples = Math.Max(1, (int)Math.Round(sampleRate * windowDurationSec));
        if (windowSamples > totalSamples)
        {
            windowSamples = totalSamples;
        }

        double minWindowRms = double.PositiveInfinity;
        double maxWindowRms = double.NegativeInfinity;

        for (int start = 0; start < totalSamples; start += windowSamples)
        {
            int end = Math.Min(totalSamples, start + windowSamples);
            if (end <= start)
            {
                continue;
            }

            double sum = 0;
            for (int i = start; i < end; i++)
            {
                sum += perSampleMeanSquare[i];
            }

            double meanSquare = sum / (end - start);
            double rms = Math.Sqrt(meanSquare);

            if (rms < minWindowRms)
            {
                minWindowRms = rms;
            }

            if (rms > maxWindowRms)
            {
                maxWindowRms = rms;
            }
        }

        if (double.IsPositiveInfinity(minWindowRms))
        {
            minWindowRms = overallRms;
        }

        if (double.IsNegativeInfinity(maxWindowRms))
        {
            maxWindowRms = overallRms;
        }

        var durationSec = totalSamples / (double)sampleRate;
        var integratedLufs = effectiveOptions.ComputeIntegratedLufs ? TryMeasureIntegratedLufs(buffer) : null;

        return new AudioLoudnessMetrics(
            DurationSec: durationSec,
            SamplePeakLinear: samplePeak,
            SamplePeakDbFs: ToDecibelsOrNull(samplePeak),
            TruePeakLinear: truePeak,
            TruePeakDbFs: ToDecibelsOrNull(truePeak),
            OverallRmsLinear: overallRms,
            OverallRmsDbFs: ToDecibelsOrNull(overallRms),
            MinWindowRmsLinear: minWindowRms,
            MinWindowRmsDbFs: ToDecibelsOrNull(minWindowRms),
            MaxWindowRmsLinear: maxWindowRms,
            MaxWindowRmsDbFs: ToDecibelsOrNull(maxWindowRms),
            WindowDurationSec: windowDurationSec,
            IntegratedLufs: integratedLufs);
    }

    internal static bool TryParseIntegratedLufs(IReadOnlyList<string> logs, out double integratedLufs)
    {
        integratedLufs = 0;
        if (logs.Count == 0)
        {
            return false;
        }

        for (int i = logs.Count - 1; i >= 0; i--)
        {
            var match = IntegratedLufsRegex.Match(logs[i]);
            if (!match.Success)
            {
                continue;
            }

            if (!double.TryParse(
                    match.Groups["value"].Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                continue;
            }

            if (double.IsNaN(parsed) || double.IsInfinity(parsed))
            {
                continue;
            }

            integratedLufs = parsed;
            return true;
        }

        return false;
    }

    private static double? TryMeasureIntegratedLufs(AudioBuffer buffer)
    {
        try
        {
            var logs = FfFilterGraph
                .FromBuffer(buffer)
                .EbuR128(new EbuR128FilterParams("verbose"))
                .CaptureLogs();

            return TryParseIntegratedLufs(logs, out var integratedLufs)
                ? integratedLufs
                : null;
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to compute integrated LUFS from ebur128 logs: {Message}", ex.Message);
            return null;
        }
    }

    private static double? ToDecibelsOrNull(double linear)
    {
        var db = ToDecibels(linear);
        return double.IsFinite(db) ? db : null;
    }

    private static double CalculateRms(AudioBuffer buffer, int startSample, int length)
    {
        if (length <= 0) return 0.0;

        double sum = 0.0;
        int count = 0;
        for (int ch = 0; ch < buffer.Channels; ch++)
        {
            var channelSpan = buffer.GetChannel(ch).Span;
            int end = Math.Min(channelSpan.Length, startSample + length);
            for (int i = startSample; i < end; i++)
            {
                double s = channelSpan[i];
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

public sealed record AudioLoudnessAnalysisOptions
{
    public TimeSpan WindowDuration { get; init; } = TimeSpan.FromMilliseconds(500);
    public bool ComputeIntegratedLufs { get; init; } = true;
}

public sealed record AudioLoudnessMetrics(
    double DurationSec,
    double SamplePeakLinear,
    double? SamplePeakDbFs,
    double TruePeakLinear,
    double? TruePeakDbFs,
    double OverallRmsLinear,
    double? OverallRmsDbFs,
    double MinWindowRmsLinear,
    double? MinWindowRmsDbFs,
    double MaxWindowRmsLinear,
    double? MaxWindowRmsDbFs,
    double WindowDurationSec,
    double? IntegratedLufs);
