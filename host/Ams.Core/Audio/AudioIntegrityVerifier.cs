using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public sealed record AudioIntegrityVerifierOptions
{
    /// <summary>
    /// Analysis window length in milliseconds when computing RMS.
    /// </summary>
    public double WindowMs { get; init; } = 30.0;

    /// <summary>
    /// Hop size in milliseconds between RMS windows.
    /// </summary>
    public double StepMs { get; init; } = 15.0;

    /// <summary>
    /// Minimum duration (milliseconds) a mismatch must span to be reported.
    /// </summary>
    public double MinMismatchDurationMs { get; init; } = 60.0;

    /// <summary>
    /// Minimum dB delta between raw and treated audio required to classify a mismatch.
    /// </summary>
    public double MinDeltaDb { get; init; } = 12.0;

    /// <summary>
    /// Optional manual override for the raw speech threshold (dBFS). If null the threshold is inferred.
    /// </summary>
    public double? RawSpeechThresholdDb { get; init; }

    /// <summary>
    /// Optional manual override for the treated speech threshold (dBFS). If null the threshold is inferred.
    /// </summary>
    public double? TreatedSpeechThresholdDb { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AudioMismatchType
{
    MissingSpeech,
    ExtraSpeech
}

public sealed record SentenceSpan(
    int SentenceId,
    double StartSec,
    double EndSec,
    string? BookText,
    string? ScriptText);

public sealed record AudioMismatch(
    double StartSec,
    double EndSec,
    AudioMismatchType Type,
    double RawDb,
    double TreatedDb,
    double DeltaDb,
    IReadOnlyList<SentenceSpan> Sentences);

public sealed record AudioVerificationResult(
    string ChapterId,
    string RawPath,
    string TreatedPath,
    int SampleRate,
    double WindowMs,
    double StepMs,
    double RawSpeechThresholdDb,
    double TreatedSpeechThresholdDb,
    double TotalAudioDurationSec,
    double RawSpeechDurationSec,
    double TreatedSpeechDurationSec,
    double MissingSpeechDurationSec,
    double ExtraSpeechDurationSec,
    int MissingBreathSuppressedCount,
    IReadOnlyList<AudioMismatch> Mismatches);

public static class AudioIntegrityVerifier
{
    private const double MinDb = -120.0;
    private static readonly FrameBreathDetectorOptions BreathDetectorOptions = new()
    {
        ApplyEnergyGate = false,
        ScoreHigh = 0.55,
        ScoreLow = 0.16,
        MinRunMs = 10,
        MergeGapMs = 90,
        GuardLeftMs = 2,
        GuardRightMs = 2,
        FricativeGuardMs = 5,
        Aggressiveness = 10 
    };

    private sealed record SegmentWindow(int StartIndex, int EndIndex, AudioMismatchType Type);

    public static AudioVerificationResult Verify(
        string chapterId,
        string rawPath,
        AudioBuffer rawBuffer,
        string treatedPath,
        AudioBuffer treatedBuffer,
        AudioIntegrityVerifierOptions? options = null,
        IReadOnlyList<SentenceSpan>? sentences = null)
    {
        if (rawBuffer is null) throw new ArgumentNullException(nameof(rawBuffer));
        if (treatedBuffer is null) throw new ArgumentNullException(nameof(treatedBuffer));

        var opt = options ?? new AudioIntegrityVerifierOptions();

        if (rawBuffer.SampleRate <= 0 || treatedBuffer.SampleRate <= 0)
        {
            throw new ArgumentException("Both buffers must provide valid sample rates.");
        }

        int sampleRate = rawBuffer.SampleRate;
        if (treatedBuffer.SampleRate != sampleRate)
        {
            throw new InvalidOperationException(
                $"Sample rate mismatch between raw ({rawBuffer.SampleRate} Hz) and treated ({treatedBuffer.SampleRate} Hz) buffers.");
        }

        var rawSamples = DownmixToMono(rawBuffer);
        var treatedSamples = DownmixToMono(treatedBuffer);

        int windowSamples = Math.Max(32, (int)Math.Round(opt.WindowMs * 0.001 * sampleRate));
        int stepSamples = Math.Max(1, (int)Math.Round(opt.StepMs * 0.001 * sampleRate));

        int maxSamples = Math.Max(rawSamples.Length, treatedSamples.Length);
        int frameCount = Math.Max(1, (int)Math.Ceiling((maxSamples - 1) / (double)stepSamples)) + 1;

        var rawDb = ComputeDbSeries(rawSamples, windowSamples, stepSamples, frameCount);
        var treatedDb = ComputeDbSeries(treatedSamples, windowSamples, stepSamples, frameCount);

        double rawThreshold = opt.RawSpeechThresholdDb ?? InferSpeechThreshold(rawDb);
        double treatedThreshold = opt.TreatedSpeechThresholdDb ?? InferSpeechThreshold(treatedDb);

        var rawSpeechMask = BuildSpeechMask(rawDb, rawThreshold);
        var treatedSpeechMask = BuildSpeechMask(treatedDb, treatedThreshold);

        var missingMask = new bool[frameCount];
        var extraMask = new bool[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            bool rawOn = rawSpeechMask[i];
            bool treatedOn = treatedSpeechMask[i];
            double delta = rawDb[i] - treatedDb[i];

            if (rawOn && !treatedOn && delta >= opt.MinDeltaDb)
            {
                missingMask[i] = true;
            }
            else if (treatedOn && !rawOn && -delta >= opt.MinDeltaDb)
            {
                extraMask[i] = true;
            }
        }

        double stepSec = stepSamples / (double)sampleRate;
        double windowSec = windowSamples / (double)sampleRate;
        double minMismatchSec = opt.MinMismatchDurationMs * 0.001;

        var missingSegments = CollectSegments(
            missingMask,
            stepSec,
            windowSec,
            minMismatchSec,
            AudioMismatchType.MissingSpeech);

        var (filteredMissingSegments, missingSuppressed) = FilterMissingSpeechWithBreath(
            missingSegments,
            rawSamples,
            sampleRate,
            rawDb,
            rawThreshold,
            stepSec,
            windowSec,
            minMismatchSec);

        var extraSegments = CollectSegments(
            extraMask,
            stepSec,
            windowSec,
            minMismatchSec,
            AudioMismatchType.ExtraSpeech);

        var missingMismatches = BuildMismatches(filteredMissingSegments, rawDb, treatedDb, stepSec, windowSec, sentences);
        var extraMismatches = BuildMismatches(extraSegments, rawDb, treatedDb, stepSec, windowSec, sentences);

        var mismatches = missingMismatches
            .Concat(extraMismatches)
            .OrderBy(m => m.StartSec)
            .ToList();

        double totalAudioDurationSec = maxSamples / (double)sampleRate;
        double rawSpeechDurationSec = rawSpeechMask.Count(on => on) * stepSec;
        double treatedSpeechDurationSec = treatedSpeechMask.Count(on => on) * stepSec;
        double missingDuration = missingMismatches.Sum(m => m.EndSec - m.StartSec);
        double extraDuration = extraMismatches.Sum(m => m.EndSec - m.StartSec);

        return new AudioVerificationResult(
            chapterId,
            rawPath,
            treatedPath,
            sampleRate,
            opt.WindowMs,
            opt.StepMs,
            rawThreshold,
            treatedThreshold,
            totalAudioDurationSec,
            rawSpeechDurationSec,
            treatedSpeechDurationSec,
            missingDuration,
            extraDuration,
            missingSuppressed,
            mismatches);
    }

    private static float[] DownmixToMono(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return buffer.Planar[0];
        }

        var mono = new float[buffer.Length];
        for (int ch = 0; ch < buffer.Channels; ch++)
        {
            var channel = buffer.Planar[ch];
            for (int i = 0; i < buffer.Length; i++)
            {
                mono[i] += channel[i];
            }
        }

        float scale = 1f / buffer.Channels;
        for (int i = 0; i < mono.Length; i++)
        {
            mono[i] *= scale;
        }

        return mono;
    }

    private static double[] ComputeDbSeries(float[] samples, int windowSamples, int stepSamples, int frameCount)
    {
        var series = new double[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            int start = i * stepSamples;
            if (start >= samples.Length)
            {
                series[i] = MinDb;
                continue;
            }

            int end = Math.Min(samples.Length, start + windowSamples);
            int length = Math.Max(1, end - start);

            double sum = 0d;
            for (int j = start; j < end; j++)
            {
                double s = samples[j];
                sum += s * s;
            }

            double rms = Math.Sqrt(sum / length);
            series[i] = rms > 0d ? 20.0 * Math.Log10(rms) : MinDb;
        }

        return series;
    }

    private static double InferSpeechThreshold(double[] dbSeries)
    {
        var valid = dbSeries.Where(v => v > MinDb + 1).ToArray();
        if (valid.Length == 0)
        {
            return MinDb + 6;
        }

        Array.Sort(valid);

        double noise = Percentile(valid, 0.15);
        double speech = Percentile(valid, 0.85);

        double threshold = Math.Max(noise + 6.0, speech - 16.0);
        threshold = Math.Min(threshold, speech - 3.0);
        threshold = Math.Max(threshold, MinDb + 6.0);

        return threshold;
    }

    private static bool[] BuildSpeechMask(double[] dbSeries, double thresholdDb)
    {
        var mask = new bool[dbSeries.Length];
        for (int i = 0; i < dbSeries.Length; i++)
        {
            mask[i] = dbSeries[i] >= thresholdDb;
        }

        return mask;
    }

    private static List<SegmentWindow> CollectSegments(
        bool[] mask,
        double stepSec,
        double windowSec,
        double minDurationSec,
        AudioMismatchType type)
    {
        var segments = new List<SegmentWindow>();

        int i = 0;
        while (i < mask.Length)
        {
            if (!mask[i])
            {
                i++;
                continue;
            }

            int startIndex = i;
            while (i < mask.Length && mask[i]) i++;
            int endIndex = i - 1;

            double startSec = Math.Max(0d, startIndex * stepSec);
            double endSec = Math.Max(startSec, endIndex * stepSec + windowSec);
            double duration = endSec - startSec;
            if (duration < minDurationSec)
            {
                continue;
            }

            segments.Add(new SegmentWindow(startIndex, endIndex, type));
        }

        return segments;
    }

    private static IReadOnlyList<SentenceSpan> ResolveSentenceSpans(
        IReadOnlyList<SentenceSpan>? sentences,
        double startSec,
        double endSec)
    {
        if (sentences is null || sentences.Count == 0)
        {
            return Array.Empty<SentenceSpan>();
        }

        var result = new List<SentenceSpan>();
        foreach (var sentence in sentences)
        {
            if (sentence.EndSec <= startSec || sentence.StartSec >= endSec)
            {
                continue;
            }

            result.Add(sentence);
        }

        return result;
    }

    private static List<AudioMismatch> BuildMismatches(
        IReadOnlyList<SegmentWindow> segments,
        double[] rawDb,
        double[] treatedDb,
        double stepSec,
        double windowSec,
        IReadOnlyList<SentenceSpan>? sentences)
    {
        var mismatches = new List<AudioMismatch>(segments.Count);
        foreach (var segment in segments)
        {
            int startIndex = Math.Max(0, segment.StartIndex);
            int endIndex = Math.Min(treatedDb.Length - 1, Math.Max(segment.StartIndex, segment.EndIndex));
            if (startIndex > endIndex)
            {
                continue;
            }

            double startSec = Math.Max(0d, startIndex * stepSec);
            double endSec = Math.Max(startSec, endIndex * stepSec + windowSec);
            double duration = endSec - startSec;
            if (duration <= 0)
            {
                continue;
            }

            double rawSum = 0d;
            double treatedSum = 0d;
            int count = 0;
            for (int idx = startIndex; idx <= endIndex && idx < rawDb.Length; idx++)
            {
                rawSum += rawDb[idx];
                treatedSum += treatedDb[idx];
                count++;
            }

            if (count == 0)
            {
                continue;
            }

            double rawMean = rawSum / count;
            double treatedMean = treatedSum / count;
            double delta = segment.Type == AudioMismatchType.MissingSpeech
                ? rawMean - treatedMean
                : treatedMean - rawMean;

            var context = ResolveSentenceSpans(sentences, startSec, endSec);

            mismatches.Add(new AudioMismatch(
                startSec,
                endSec,
                segment.Type,
                rawMean,
                treatedMean,
                delta,
                context));
        }

        return mismatches;
    }

    private static (List<SegmentWindow> Segments, int SuppressedCount) FilterMissingSpeechWithBreath(
        IReadOnlyList<SegmentWindow> segments,
        float[] rawSamples,
        int sampleRate,
        double[] rawDb,
        double rawThreshold,
        double stepSec,
        double windowSec,
        double minDurationSec)
    {
        if (segments.Count == 0 || rawSamples.Length == 0 || sampleRate <= 0)
        {
            return (segments.ToList(), 0);
        }

        double audioDurationSec = rawSamples.Length / (double)sampleRate;
        var filtered = new List<SegmentWindow>(segments.Count);
        int suppressedCount = 0;

        const double BreathMarginSec = 0.15;
        const double QuietBreathMarginDb = 8.0;
        const double QuietBreathDurationSec = 0.45;
        const double MaxQuietBreathDbMargin = 10.0;

        foreach (var segment in segments)
        {
            double segStart = Math.Max(0d, segment.StartIndex * stepSec);
            double segEnd = Math.Min(audioDurationSec, segment.EndIndex * stepSec + windowSec);
            if (segEnd <= segStart)
            {
                continue;
            }

            double detectStart = Math.Max(0d, segStart - BreathMarginSec);
            double detectEnd = Math.Min(audioDurationSec, segEnd + BreathMarginSec);
            double segmentDuration = segEnd - segStart;

            double maxRawDb = double.NegativeInfinity;
            double sumRawDb = 0d;
            int frameCount = 0;
            for (int idx = segment.StartIndex; idx <= segment.EndIndex && idx < rawDb.Length; idx++)
            {
                maxRawDb = Math.Max(maxRawDb, rawDb[idx]);
                sumRawDb += rawDb[idx];
                frameCount++;
            }

            double meanRawDb = frameCount > 0 ? sumRawDb / frameCount : double.NegativeInfinity;

            IReadOnlyList<Region> breathRegions;
            try
            {
                breathRegions = FeatureExtraction.Detect(
                    rawSamples,
                    sampleRate,
                    detectStart,
                    detectEnd,
                    BreathDetectorOptions);
            }
            catch
            {
                breathRegions = Array.Empty<Region>();
            }

            if (breathRegions.Count == 0)
            {
                if (segmentDuration <= QuietBreathDurationSec
                    && meanRawDb <= rawThreshold + QuietBreathMarginDb
                    && maxRawDb <= rawThreshold + MaxQuietBreathDbMargin)
                {
                    suppressedCount++;
                    continue;
                }

                filtered.Add(segment);
                continue;
            }

            frameCount = segment.EndIndex - segment.StartIndex + 1;
            if (frameCount <= 0)
            {
                continue;
            }

            var keep = new bool[frameCount];
            Array.Fill(keep, true);
            bool anyOverlap = false;

            foreach (var region in breathRegions)
            {
                double regionStart = Math.Max(segStart, region.StartSec);
                double regionEnd = Math.Min(segEnd, region.EndSec);
                if (regionEnd <= regionStart)
                {
                    continue;
                }

                for (int idx = segment.StartIndex; idx <= segment.EndIndex; idx++)
                {
                    double frameStart = idx * stepSec;
                    double frameEnd = frameStart + windowSec;
                    if (frameEnd <= regionStart || frameStart >= regionEnd)
                    {
                        continue;
                    }

                    keep[idx - segment.StartIndex] = false;
                    anyOverlap = true;
                }
            }

            if (!anyOverlap)
            {
                filtered.Add(segment);
                continue;
            }

            bool removedAny = false;
            int localIndex = 0;
            while (localIndex < keep.Length)
            {
                while (localIndex < keep.Length && !keep[localIndex])
                {
                    removedAny = true;
                    localIndex++;
                }

                if (localIndex >= keep.Length)
                {
                    break;
                }

                int startOffset = localIndex;
                while (localIndex < keep.Length && keep[localIndex]) localIndex++;
                int endOffset = localIndex - 1;

                int startIndex = segment.StartIndex + startOffset;
                int endIndex = segment.StartIndex + endOffset;

                double startSec = Math.Max(0d, startIndex * stepSec);
                double endSec = Math.Max(startSec, endIndex * stepSec + windowSec);
                if (endSec - startSec < minDurationSec)
                {
                    removedAny = true;
                    continue;
                }

                filtered.Add(new SegmentWindow(startIndex, endIndex, segment.Type));
            }

            if (removedAny)
            {
                suppressedCount++;
            }
            else
            {
                filtered.Add(segment);
            }
        }

        return (filtered, suppressedCount);
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double fraction)
    {
        if (sortedValues.Count == 0)
        {
            return MinDb;
        }

        fraction = Math.Clamp(fraction, 0d, 1d);

        double index = fraction * (sortedValues.Count - 1);
        int lower = (int)Math.Floor(index);
        int upper = (int)Math.Ceiling(index);

        if (lower == upper)
        {
            return sortedValues[lower];
        }

        double weight = index - lower;
        return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * weight;
    }
}
