using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Core.Application.Runs;
using Ams.Core.Audio.QualityControl;

namespace Ams.Core.Application.Benchmark;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkRunPhase
{
    Gated,
    Invalid,
    Running,
    Failed,
    Completed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkMetricsStatus
{
    NotRun,
    Completed,
    Partial,
    Failed
}

public sealed record BenchmarkMetricsFailure
{
    [JsonConstructor]
    public BenchmarkMetricsFailure(
        RunFailureKind kind,
        string message,
        string operation,
        string? chapterId = null,
        string? resourcePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        Kind = kind;
        Message = message;
        Operation = operation;
        ChapterId = NormalizeOptionalText(chapterId);
        ResourcePath = NormalizeResourcePath(resourcePath);
    }

    public RunFailureKind Kind { get; }

    public string Message { get; }

    public string Operation { get; }

    public string? ChapterId { get; }

    public string? ResourcePath { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizeResourcePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace('\\', '/');
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }
}

public sealed record BenchmarkAudioProcessingActivity
{
    [JsonConstructor]
    public BenchmarkAudioProcessingActivity(
        string function,
        DateTimeOffset startedAtUtc,
        long durationMs,
        bool succeeded,
        string? failureKind = null,
        string? detail = null,
        long durationUs = -1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(function);

        if (durationMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration cannot be negative.");
        }

        if (durationUs < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(durationUs), "Duration cannot be negative.");
        }

        Function = function.Trim();
        StartedAtUtc = startedAtUtc;
        DurationMs = durationMs;
        DurationUs = durationUs < 0
            ? checked(durationMs * 1000L)
            : durationUs;
        Succeeded = succeeded;
        FailureKind = NormalizeOptionalText(failureKind);
        Detail = NormalizeOptionalText(detail);
    }

    public string Function { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public long DurationMs { get; }

    public long DurationUs { get; }

    public bool Succeeded { get; }

    public string? FailureKind { get; }

    public string? Detail { get; }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public sealed record BenchmarkChapterRuntimeMetrics
{
    [JsonConstructor]
    public BenchmarkChapterRuntimeMetrics(
        long? pipelineRuntimeMs = null,
        long? analysisRuntimeMs = null)
    {
        ValidateOptionalNonNegative(pipelineRuntimeMs, nameof(pipelineRuntimeMs));
        ValidateOptionalNonNegative(analysisRuntimeMs, nameof(analysisRuntimeMs));

        PipelineRuntimeMs = pipelineRuntimeMs;
        AnalysisRuntimeMs = analysisRuntimeMs;
    }

    public long? PipelineRuntimeMs { get; }

    public long? AnalysisRuntimeMs { get; }

    public long? TotalRuntimeMs => PipelineRuntimeMs.HasValue || AnalysisRuntimeMs.HasValue
        ? (PipelineRuntimeMs ?? 0L) + (AnalysisRuntimeMs ?? 0L)
        : null;

    public static BenchmarkChapterRuntimeMetrics Empty { get; } = new();

    private static void ValidateOptionalNonNegative(long? value, string parameterName)
    {
        if (value.HasValue && value.Value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Runtime values cannot be negative.");
        }
    }
}

public sealed record BenchmarkAudioIntegrityMetrics
{
    [JsonConstructor]
    public BenchmarkAudioIntegrityMetrics(
        double durationSec,
        double rawSpeechSec,
        double treatedSpeechSec,
        double missingSpeechSec,
        double extraSpeechSec,
        int mismatchCount)
    {
        ValidateFiniteNonNegative(durationSec, nameof(durationSec));
        ValidateFiniteNonNegative(rawSpeechSec, nameof(rawSpeechSec));
        ValidateFiniteNonNegative(treatedSpeechSec, nameof(treatedSpeechSec));
        ValidateFiniteNonNegative(missingSpeechSec, nameof(missingSpeechSec));
        ValidateFiniteNonNegative(extraSpeechSec, nameof(extraSpeechSec));

        if (mismatchCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mismatchCount), "Mismatch count cannot be negative.");
        }

        DurationSec = durationSec;
        RawSpeechSec = rawSpeechSec;
        TreatedSpeechSec = treatedSpeechSec;
        MissingSpeechSec = missingSpeechSec;
        ExtraSpeechSec = extraSpeechSec;
        MismatchCount = mismatchCount;
    }

    public double DurationSec { get; }

    public double RawSpeechSec { get; }

    public double TreatedSpeechSec { get; }

    public double MissingSpeechSec { get; }

    public double ExtraSpeechSec { get; }

    public int MismatchCount { get; }

    private static void ValidateFiniteNonNegative(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}

public sealed record BenchmarkQcMetrics
{
    [JsonConstructor]
    public BenchmarkQcMetrics(
        double headSilenceSec,
        double? titleBodyGapSec,
        double tailSilenceSec,
        int flagCount,
        IReadOnlyList<string>? flags = null)
    {
        ValidateFiniteNonNegative(headSilenceSec, nameof(headSilenceSec));
        ValidateFiniteOptionalNonNegative(titleBodyGapSec, nameof(titleBodyGapSec));
        ValidateFiniteNonNegative(tailSilenceSec, nameof(tailSilenceSec));

        if (flagCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(flagCount), "QC flag count cannot be negative.");
        }

        var normalizedFlags = flags?
            .Where(flag => !string.IsNullOrWhiteSpace(flag))
            .Select(flag => flag.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? [];

        HeadSilenceSec = headSilenceSec;
        TitleBodyGapSec = titleBodyGapSec;
        TailSilenceSec = tailSilenceSec;
        Flags = normalizedFlags;
        FlagCount = Math.Max(flagCount, normalizedFlags.Length);
    }

    public double HeadSilenceSec { get; }

    public double? TitleBodyGapSec { get; }

    public double TailSilenceSec { get; }

    public int FlagCount { get; }

    public IReadOnlyList<string> Flags { get; }

    private static void ValidateFiniteNonNegative(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }

    private static void ValidateFiniteOptionalNonNegative(double? value, string parameterName)
    {
        if (value is null)
        {
            return;
        }

        ValidateFiniteNonNegative(value.Value, parameterName);
    }
}

public sealed record BenchmarkLoudnessMetrics
{
    [JsonConstructor]
    public BenchmarkLoudnessMetrics(
        double durationSec,
        double? samplePeakDbFs,
        double? truePeakDbFs,
        double? overallRmsDbFs,
        double? integratedLufs)
    {
        ValidateFiniteNonNegative(durationSec, nameof(durationSec));
        ValidateOptionalFinite(samplePeakDbFs, nameof(samplePeakDbFs));
        ValidateOptionalFinite(truePeakDbFs, nameof(truePeakDbFs));
        ValidateOptionalFinite(overallRmsDbFs, nameof(overallRmsDbFs));
        ValidateOptionalFinite(integratedLufs, nameof(integratedLufs));

        DurationSec = durationSec;
        SamplePeakDbFs = samplePeakDbFs;
        TruePeakDbFs = truePeakDbFs;
        OverallRmsDbFs = overallRmsDbFs;
        IntegratedLufs = integratedLufs;
    }

    public double DurationSec { get; }

    public double? SamplePeakDbFs { get; }

    public double? TruePeakDbFs { get; }

    public double? OverallRmsDbFs { get; }

    public double? IntegratedLufs { get; }

    private static void ValidateFiniteNonNegative(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }

    private static void ValidateOptionalFinite(double? value, string parameterName)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite when provided.");
        }
    }
}

public sealed record BenchmarkChapterQualityMetrics
{
    [JsonConstructor]
    public BenchmarkChapterQualityMetrics(
        BenchmarkAudioIntegrityMetrics? integrity = null,
        BenchmarkQcMetrics? rawQc = null,
        BenchmarkQcMetrics? treatedQc = null,
        BenchmarkLoudnessMetrics? rawLoudness = null,
        BenchmarkLoudnessMetrics? treatedLoudness = null)
    {
        Integrity = integrity;
        RawQc = rawQc;
        TreatedQc = treatedQc;
        RawLoudness = rawLoudness;
        TreatedLoudness = treatedLoudness;
    }

    public BenchmarkAudioIntegrityMetrics? Integrity { get; }

    public BenchmarkQcMetrics? RawQc { get; }

    public BenchmarkQcMetrics? TreatedQc { get; }

    public BenchmarkLoudnessMetrics? RawLoudness { get; }

    public BenchmarkLoudnessMetrics? TreatedLoudness { get; }

    public bool HasAnyData => Integrity is not null
                              || RawQc is not null
                              || TreatedQc is not null
                              || RawLoudness is not null
                              || TreatedLoudness is not null;

    public bool IsComplete => Integrity is not null
                              && RawQc is not null
                              && TreatedQc is not null
                              && RawLoudness is not null
                              && TreatedLoudness is not null;
}

public sealed record BenchmarkChapterMetrics
{
    [JsonConstructor]
    public BenchmarkChapterMetrics(
        BenchmarkMetricsStatus status,
        BenchmarkChapterRuntimeMetrics? runtime = null,
        BenchmarkChapterQualityMetrics? quality = null,
        BenchmarkMetricsFailure? metricsFailure = null,
        IReadOnlyList<BenchmarkAudioProcessingActivity>? audioProcessingActivities = null)
    {
        Validate(status, quality, metricsFailure);

        Status = status;
        Runtime = runtime ?? BenchmarkChapterRuntimeMetrics.Empty;
        Quality = quality;
        MetricsFailure = metricsFailure;
        AudioProcessingActivities = audioProcessingActivities?
            .Where(activity => activity is not null)
            .ToArray()
            ?? [];
    }

    public BenchmarkMetricsStatus Status { get; }

    public BenchmarkChapterRuntimeMetrics Runtime { get; }

    public BenchmarkChapterQualityMetrics? Quality { get; }

    public BenchmarkMetricsFailure? MetricsFailure { get; }

    public IReadOnlyList<BenchmarkAudioProcessingActivity> AudioProcessingActivities { get; }

    public long TotalAudioProcessingDurationUs => AudioProcessingActivities.Sum(activity => activity.DurationUs);

    public long TotalAudioProcessingDurationMs => RoundMicrosecondsToMilliseconds(TotalAudioProcessingDurationUs);

    public static BenchmarkChapterMetrics NotRun { get; } = new(BenchmarkMetricsStatus.NotRun);

    private static long RoundMicrosecondsToMilliseconds(long durationUs)
    {
        if (durationUs <= 0)
        {
            return 0;
        }

        var milliseconds = durationUs / 1000d;
        var rounded = Math.Round(milliseconds, MidpointRounding.AwayFromZero);

        if (rounded <= 0)
        {
            return 0;
        }

        if (rounded >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return (long)rounded;
    }

    private static void Validate(
        BenchmarkMetricsStatus status,
        BenchmarkChapterQualityMetrics? quality,
        BenchmarkMetricsFailure? metricsFailure)
    {
        if (status == BenchmarkMetricsStatus.Completed)
        {
            if (quality is null || !quality.IsComplete)
            {
                throw new ArgumentException("Completed metrics must include a full quality payload.", nameof(quality));
            }

            if (metricsFailure is not null)
            {
                throw new ArgumentException("Completed metrics cannot include failure metadata.", nameof(metricsFailure));
            }

            return;
        }

        if (status == BenchmarkMetricsStatus.Partial)
        {
            if (quality is null || !quality.HasAnyData)
            {
                throw new ArgumentException("Partial metrics must include at least one quality signal.", nameof(quality));
            }

            if (metricsFailure is null)
            {
                throw new ArgumentException("Partial metrics must include failure metadata.", nameof(metricsFailure));
            }

            return;
        }

        if (status == BenchmarkMetricsStatus.Failed)
        {
            if (metricsFailure is null)
            {
                throw new ArgumentException("Failed metrics must include failure metadata.", nameof(metricsFailure));
            }

            if (quality?.HasAnyData == true)
            {
                throw new ArgumentException("Failed metrics cannot include quality payload data; use Partial status.", nameof(quality));
            }

            return;
        }

        if (status == BenchmarkMetricsStatus.NotRun && metricsFailure is not null)
        {
            throw new ArgumentException("Not-run metrics cannot include failure metadata.", nameof(metricsFailure));
        }
    }
}

public sealed record BenchmarkRunChapterStateCounts
{
    [JsonConstructor]
    public BenchmarkRunChapterStateCounts(int pending, int running, int failed, int completed)
    {
        ValidateCount(pending, nameof(pending));
        ValidateCount(running, nameof(running));
        ValidateCount(failed, nameof(failed));
        ValidateCount(completed, nameof(completed));

        Pending = pending;
        Running = running;
        Failed = failed;
        Completed = completed;
    }

    public int Pending { get; }

    public int Running { get; }

    public int Failed { get; }

    public int Completed { get; }

    public int Total => Pending + Running + Failed + Completed;

    private static void ValidateCount(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Aggregate counters cannot be negative.");
        }
    }
}

public sealed record BenchmarkMetricsStateCounts
{
    [JsonConstructor]
    public BenchmarkMetricsStateCounts(int notRun, int completed, int partial, int failed)
    {
        ValidateCount(notRun, nameof(notRun));
        ValidateCount(completed, nameof(completed));
        ValidateCount(partial, nameof(partial));
        ValidateCount(failed, nameof(failed));

        NotRun = notRun;
        Completed = completed;
        Partial = partial;
        Failed = failed;
    }

    public int NotRun { get; }

    public int Completed { get; }

    public int Partial { get; }

    public int Failed { get; }

    public int Total => NotRun + Completed + Partial + Failed;

    private static void ValidateCount(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Aggregate counters cannot be negative.");
        }
    }
}

public sealed record BenchmarkRunMetricsAggregate
{
    [JsonConstructor]
    public BenchmarkRunMetricsAggregate(
        BenchmarkRunChapterStateCounts chapterStates,
        BenchmarkMetricsStateCounts metricsStates,
        long totalPipelineRuntimeMs,
        long totalAnalysisRuntimeMs,
        int totalMismatchCount,
        double totalMissingSpeechSec,
        double totalExtraSpeechSec,
        int totalQcFlags)
    {
        ArgumentNullException.ThrowIfNull(chapterStates);
        ArgumentNullException.ThrowIfNull(metricsStates);

        if (totalPipelineRuntimeMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPipelineRuntimeMs), "Aggregate runtime cannot be negative.");
        }

        if (totalAnalysisRuntimeMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAnalysisRuntimeMs), "Aggregate runtime cannot be negative.");
        }

        if (totalMismatchCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalMismatchCount), "Mismatch count cannot be negative.");
        }

        if (totalQcFlags < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalQcFlags), "QC flag count cannot be negative.");
        }

        ValidateFiniteNonNegative(totalMissingSpeechSec, nameof(totalMissingSpeechSec));
        ValidateFiniteNonNegative(totalExtraSpeechSec, nameof(totalExtraSpeechSec));

        ChapterStates = chapterStates;
        MetricsStates = metricsStates;
        TotalPipelineRuntimeMs = totalPipelineRuntimeMs;
        TotalAnalysisRuntimeMs = totalAnalysisRuntimeMs;
        TotalMismatchCount = totalMismatchCount;
        TotalMissingSpeechSec = totalMissingSpeechSec;
        TotalExtraSpeechSec = totalExtraSpeechSec;
        TotalQcFlags = totalQcFlags;
    }

    public BenchmarkRunChapterStateCounts ChapterStates { get; }

    public BenchmarkMetricsStateCounts MetricsStates { get; }

    public long TotalPipelineRuntimeMs { get; }

    public long TotalAnalysisRuntimeMs { get; }

    public int TotalMismatchCount { get; }

    public double TotalMissingSpeechSec { get; }

    public double TotalExtraSpeechSec { get; }

    public int TotalQcFlags { get; }

    public static BenchmarkRunMetricsAggregate Empty { get; } = new(
        chapterStates: new BenchmarkRunChapterStateCounts(0, 0, 0, 0),
        metricsStates: new BenchmarkMetricsStateCounts(0, 0, 0, 0),
        totalPipelineRuntimeMs: 0,
        totalAnalysisRuntimeMs: 0,
        totalMismatchCount: 0,
        totalMissingSpeechSec: 0,
        totalExtraSpeechSec: 0,
        totalQcFlags: 0);

    public static BenchmarkRunMetricsAggregate FromChapterOutcomes(
        IReadOnlyList<BenchmarkRunChapterOutcome>? chapterOutcomes)
    {
        var outcomes = chapterOutcomes?.ToArray() ?? [];

        var chapterStates = new BenchmarkRunChapterStateCounts(
            pending: outcomes.Count(outcome => outcome.State == RunState.Pending),
            running: outcomes.Count(outcome => outcome.State == RunState.Running),
            failed: outcomes.Count(outcome => outcome.State == RunState.Failed),
            completed: outcomes.Count(outcome => outcome.State == RunState.Completed));

        var metricsStates = new BenchmarkMetricsStateCounts(
            notRun: outcomes.Count(outcome => outcome.Metrics.Status == BenchmarkMetricsStatus.NotRun),
            completed: outcomes.Count(outcome => outcome.Metrics.Status == BenchmarkMetricsStatus.Completed),
            partial: outcomes.Count(outcome => outcome.Metrics.Status == BenchmarkMetricsStatus.Partial),
            failed: outcomes.Count(outcome => outcome.Metrics.Status == BenchmarkMetricsStatus.Failed));

        var totalPipelineRuntimeMs = outcomes.Sum(outcome => outcome.Metrics.Runtime.PipelineRuntimeMs ?? 0L);
        var totalAnalysisRuntimeMs = outcomes.Sum(outcome => outcome.Metrics.Runtime.AnalysisRuntimeMs ?? 0L);

        var totalMismatchCount = outcomes.Sum(outcome => outcome.Metrics.Quality?.Integrity?.MismatchCount ?? 0);
        var totalMissingSpeechSec = outcomes.Sum(outcome => outcome.Metrics.Quality?.Integrity?.MissingSpeechSec ?? 0d);
        var totalExtraSpeechSec = outcomes.Sum(outcome => outcome.Metrics.Quality?.Integrity?.ExtraSpeechSec ?? 0d);

        var totalQcFlags = outcomes.Sum(outcome =>
            (outcome.Metrics.Quality?.RawQc?.FlagCount ?? 0)
            + (outcome.Metrics.Quality?.TreatedQc?.FlagCount ?? 0));

        return new BenchmarkRunMetricsAggregate(
            chapterStates,
            metricsStates,
            totalPipelineRuntimeMs,
            totalAnalysisRuntimeMs,
            totalMismatchCount,
            totalMissingSpeechSec,
            totalExtraSpeechSec,
            totalQcFlags);
    }

    private static void ValidateFiniteNonNegative(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}

public sealed record BenchmarkQcThresholdSnapshot
{
    [JsonConstructor]
    public BenchmarkQcThresholdSnapshot(
        double minHeadSilence = 0.5,
        double maxHeadSilence = 1.0,
        double minTailSilence = 2.0,
        double maxTailSilence = 5.0,
        double minTitleBodyGap = 1.0,
        double maxTitleBodyGap = 2.5)
    {
        ValidateFiniteNonNegative(minHeadSilence, nameof(minHeadSilence));
        ValidateFiniteNonNegative(maxHeadSilence, nameof(maxHeadSilence));
        ValidateFiniteNonNegative(minTailSilence, nameof(minTailSilence));
        ValidateFiniteNonNegative(maxTailSilence, nameof(maxTailSilence));
        ValidateFiniteNonNegative(minTitleBodyGap, nameof(minTitleBodyGap));
        ValidateFiniteNonNegative(maxTitleBodyGap, nameof(maxTitleBodyGap));

        if (maxHeadSilence < minHeadSilence)
        {
            throw new ArgumentException("Maximum head silence must be greater than or equal to minimum head silence.", nameof(maxHeadSilence));
        }

        if (maxTailSilence < minTailSilence)
        {
            throw new ArgumentException("Maximum tail silence must be greater than or equal to minimum tail silence.", nameof(maxTailSilence));
        }

        if (maxTitleBodyGap < minTitleBodyGap)
        {
            throw new ArgumentException("Maximum title-body gap must be greater than or equal to minimum title-body gap.", nameof(maxTitleBodyGap));
        }

        MinHeadSilence = minHeadSilence;
        MaxHeadSilence = maxHeadSilence;
        MinTailSilence = minTailSilence;
        MaxTailSilence = maxTailSilence;
        MinTitleBodyGap = minTitleBodyGap;
        MaxTitleBodyGap = maxTitleBodyGap;
    }

    public double MinHeadSilence { get; }

    public double MaxHeadSilence { get; }

    public double MinTailSilence { get; }

    public double MaxTailSilence { get; }

    public double MinTitleBodyGap { get; }

    public double MaxTitleBodyGap { get; }

    public QcThresholds ToQcThresholds()
    {
        return new QcThresholds
        {
            MinHeadSilence = MinHeadSilence,
            MaxHeadSilence = MaxHeadSilence,
            MinTailSilence = MinTailSilence,
            MaxTailSilence = MaxTailSilence,
            MinTitleBodyGap = MinTitleBodyGap,
            MaxTitleBodyGap = MaxTitleBodyGap
        };
    }

    public static BenchmarkQcThresholdSnapshot Default { get; } = new();

    private static void ValidateFiniteNonNegative(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}

public sealed record BenchmarkMetricsPolicySnapshot
{
    [JsonConstructor]
    public BenchmarkMetricsPolicySnapshot(
        bool enabled = true,
        double integrityWindowMs = 30.0,
        double integrityStepMs = 15.0,
        double integrityMinMismatchMs = 60.0,
        double integrityMergeGapMs = 40.0,
        double integrityMinDeltaDb = 20.0,
        double qcNoiseFloorDb = -40.0,
        double qcMinSilenceDurationSec = 0.30,
        BenchmarkQcThresholdSnapshot? qcThresholds = null,
        double loudnessWindowSec = 0.5)
    {
        ValidateFinitePositive(integrityWindowMs, nameof(integrityWindowMs));
        ValidateFinitePositive(integrityStepMs, nameof(integrityStepMs));
        ValidateFinitePositive(integrityMinMismatchMs, nameof(integrityMinMismatchMs));
        ValidateFinitePositive(integrityMergeGapMs, nameof(integrityMergeGapMs));
        ValidateFinitePositive(integrityMinDeltaDb, nameof(integrityMinDeltaDb));
        ValidateFinite(qcNoiseFloorDb, nameof(qcNoiseFloorDb));
        ValidateFinitePositive(qcMinSilenceDurationSec, nameof(qcMinSilenceDurationSec));
        ValidateFinitePositive(loudnessWindowSec, nameof(loudnessWindowSec));

        Enabled = enabled;
        IntegrityWindowMs = integrityWindowMs;
        IntegrityStepMs = integrityStepMs;
        IntegrityMinMismatchMs = integrityMinMismatchMs;
        IntegrityMergeGapMs = integrityMergeGapMs;
        IntegrityMinDeltaDb = integrityMinDeltaDb;
        QcNoiseFloorDb = qcNoiseFloorDb;
        QcMinSilenceDurationSec = qcMinSilenceDurationSec;
        QcThresholds = qcThresholds ?? BenchmarkQcThresholdSnapshot.Default;
        LoudnessWindowSec = loudnessWindowSec;
    }

    public bool Enabled { get; }

    public double IntegrityWindowMs { get; }

    public double IntegrityStepMs { get; }

    public double IntegrityMinMismatchMs { get; }

    public double IntegrityMergeGapMs { get; }

    public double IntegrityMinDeltaDb { get; }

    public double QcNoiseFloorDb { get; }

    public double QcMinSilenceDurationSec { get; }

    public BenchmarkQcThresholdSnapshot QcThresholds { get; }

    public double LoudnessWindowSec { get; }

    public QcThresholds ToQcThresholds() => QcThresholds.ToQcThresholds();

    public static BenchmarkMetricsPolicySnapshot Default { get; } = new();

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }

    private static void ValidateFinitePositive(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
        }
    }
}

public sealed record BenchmarkRunPhaseTransition
{
    [JsonConstructor]
    public BenchmarkRunPhaseTransition(
        BenchmarkRunPhase phase,
        DateTimeOffset occurredAtUtc,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Phase = phase;
        OccurredAtUtc = occurredAtUtc;
        Message = message;
    }

    public BenchmarkRunPhase Phase { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public string Message { get; }
}

public sealed record BenchmarkRunStageSummary
{
    [JsonConstructor]
    public BenchmarkRunStageSummary(
        string stage,
        RunState state,
        bool executed,
        string message,
        RunFailure? failure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed stage summaries must include failure metadata.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Only failed stage summaries may include failure metadata.", nameof(failure));
        }

        Stage = stage;
        State = state;
        Executed = executed;
        Message = message;
        Failure = failure;
    }

    public string Stage { get; }

    public RunState State { get; }

    public bool Executed { get; }

    public string Message { get; }

    public RunFailure? Failure { get; }
}

public sealed record BenchmarkRunChapterOutcome
{
    [JsonConstructor]
    public BenchmarkRunChapterOutcome(
        string chapterId,
        RunState state,
        string summary,
        RunFailure? failure = null,
        IReadOnlyList<BenchmarkRunStageSummary>? stageSummaries = null,
        IReadOnlyList<RunArtifact>? artifacts = null,
        BenchmarkChapterMetrics? metrics = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed chapter outcomes must include failure metadata.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Only failed chapter outcomes may include failure metadata.", nameof(failure));
        }

        ChapterId = chapterId;
        State = state;
        Summary = summary;
        Failure = failure;
        StageSummaries = stageSummaries?.ToArray() ?? [];
        Artifacts = artifacts?.ToArray() ?? [];
        Metrics = metrics ?? BenchmarkChapterMetrics.NotRun;
    }

    public string ChapterId { get; }

    public RunState State { get; }

    public string Summary { get; }

    public RunFailure? Failure { get; }

    public IReadOnlyList<BenchmarkRunStageSummary> StageSummaries { get; }

    public IReadOnlyList<RunArtifact> Artifacts { get; }

    public BenchmarkChapterMetrics Metrics { get; }
}

public sealed record BenchmarkRunManifest
{
    [JsonConstructor]
    public BenchmarkRunManifest(
        string runId,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        bool deterministic,
        BenchmarkRunPhase phase,
        RunState state,
        BenchmarkDeterminismContract? determinism,
        BenchmarkCachePolicy cachePolicy,
        BenchmarkChunkPolicy chunkPolicy,
        IReadOnlyList<string> chapterSet,
        string chapterSetFingerprint,
        IReadOnlyList<BenchmarkRunChapterOutcome>? chapterOutcomes = null,
        RunFailure? failure = null,
        IReadOnlyList<BenchmarkRunPhaseTransition>? phaseTransitions = null,
        BenchmarkMetricsPolicySnapshot? metricsPolicy = null,
        BenchmarkRunMetricsAggregate? aggregateMetrics = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(cachePolicy);
        ArgumentNullException.ThrowIfNull(chunkPolicy);
        ArgumentNullException.ThrowIfNull(chapterSet);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterSetFingerprint);

        var normalizedChapterSet = chapterSet
            .Where(chapter => !string.IsNullOrWhiteSpace(chapter))
            .Select(chapter => chapter.Trim())
            .ToArray();

        if (normalizedChapterSet.Length == 0)
        {
            throw new ArgumentException("Benchmark run manifests require at least one chapter identifier.", nameof(chapterSet));
        }

        if (deterministic && determinism is null)
        {
            throw new ArgumentException("Deterministic run manifests must include determinism contract details.", nameof(determinism));
        }

        if (phase == BenchmarkRunPhase.Invalid)
        {
            throw new ArgumentException("Invalid deterministic verdicts must be persisted as invalid-run artifacts, not manifests.", nameof(phase));
        }

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed manifests must include failure metadata.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Only failed manifests may include failure metadata.", nameof(failure));
        }

        var normalizedChapterOutcomes = chapterOutcomes?.ToArray() ?? [];

        RunId = runId;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        Deterministic = deterministic;
        Phase = phase;
        State = state;
        Determinism = determinism;
        CachePolicy = cachePolicy;
        ChunkPolicy = chunkPolicy;
        ChapterSet = normalizedChapterSet;
        ChapterSetFingerprint = chapterSetFingerprint;
        ChapterOutcomes = normalizedChapterOutcomes;
        Failure = failure;
        PhaseTransitions = phaseTransitions?.ToArray() ?? [];
        MetricsPolicy = metricsPolicy ?? BenchmarkMetricsPolicySnapshot.Default;
        AggregateMetrics = aggregateMetrics ?? BenchmarkRunMetricsAggregate.FromChapterOutcomes(normalizedChapterOutcomes);
    }

    public string RunId { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset CompletedAtUtc { get; }

    public bool Deterministic { get; }

    public BenchmarkRunPhase Phase { get; }

    public RunState State { get; }

    public BenchmarkDeterminismContract? Determinism { get; }

    public BenchmarkCachePolicy CachePolicy { get; }

    public BenchmarkChunkPolicy ChunkPolicy { get; }

    public IReadOnlyList<string> ChapterSet { get; }

    public string ChapterSetFingerprint { get; }

    public IReadOnlyList<BenchmarkRunChapterOutcome> ChapterOutcomes { get; }

    public RunFailure? Failure { get; }

    public IReadOnlyList<BenchmarkRunPhaseTransition> PhaseTransitions { get; }

    public BenchmarkMetricsPolicySnapshot MetricsPolicy { get; }

    public BenchmarkRunMetricsAggregate AggregateMetrics { get; }

    public static string Serialize(BenchmarkRunManifest manifest, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, options ?? BenchmarkDeterminismJson.SerializerOptions);
    }

    public static BenchmarkRunManifest Deserialize(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("aggregateMetrics", out _))
        {
            throw new InvalidDataException("Benchmark manifest payload is missing required 'aggregateMetrics' node.");
        }

        if (!root.TryGetProperty("chapterSetFingerprint", out _))
        {
            throw new InvalidDataException("Benchmark manifest payload is missing required 'chapterSetFingerprint' node.");
        }

        var manifest = JsonSerializer.Deserialize<BenchmarkRunManifest>(json, options ?? BenchmarkDeterminismJson.SerializerOptions);
        return manifest ?? throw new InvalidDataException("Benchmark manifest payload deserialized to null.");
    }
}

public sealed record BenchmarkInvalidRunArtifact
{
    [JsonConstructor]
    public BenchmarkInvalidRunArtifact(
        string runId,
        DateTimeOffset rejectedAtUtc,
        bool deterministic,
        BenchmarkRunPhase phase,
        BenchmarkDeterminismContract determinism,
        IReadOnlyList<string> chapterSet,
        string chapterSetFingerprint,
        IReadOnlyList<BenchmarkRunPhaseTransition>? phaseTransitions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(determinism);
        ArgumentNullException.ThrowIfNull(chapterSet);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterSetFingerprint);

        var normalizedChapterSet = chapterSet
            .Where(chapter => !string.IsNullOrWhiteSpace(chapter))
            .Select(chapter => chapter.Trim())
            .ToArray();

        if (normalizedChapterSet.Length == 0)
        {
            throw new ArgumentException("Invalid-run artifacts require at least one chapter identifier.", nameof(chapterSet));
        }

        if (!deterministic)
        {
            throw new ArgumentException("Invalid-run artifacts are only valid for deterministic benchmark mode.", nameof(deterministic));
        }

        if (phase != BenchmarkRunPhase.Invalid)
        {
            throw new ArgumentException("Invalid-run artifact phase must be 'Invalid'.", nameof(phase));
        }

        if (determinism.Verdict != BenchmarkDeterminismVerdict.Invalid)
        {
            throw new ArgumentException("Invalid-run artifacts must embed an invalid determinism contract.", nameof(determinism));
        }

        RunId = runId;
        RejectedAtUtc = rejectedAtUtc;
        Deterministic = deterministic;
        Phase = phase;
        Determinism = determinism;
        ChapterSet = normalizedChapterSet;
        ChapterSetFingerprint = chapterSetFingerprint;
        PhaseTransitions = phaseTransitions?.ToArray() ?? [];
    }

    public string RunId { get; }

    public DateTimeOffset RejectedAtUtc { get; }

    public bool Deterministic { get; }

    public BenchmarkRunPhase Phase { get; }

    public BenchmarkDeterminismContract Determinism { get; }

    public IReadOnlyList<string> ChapterSet { get; }

    public string ChapterSetFingerprint { get; }

    public IReadOnlyList<BenchmarkRunPhaseTransition> PhaseTransitions { get; }

    public static string Serialize(BenchmarkInvalidRunArtifact invalidRun, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invalidRun);
        return JsonSerializer.Serialize(invalidRun, options ?? BenchmarkDeterminismJson.SerializerOptions);
    }

    public static BenchmarkInvalidRunArtifact Deserialize(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var artifact = JsonSerializer.Deserialize<BenchmarkInvalidRunArtifact>(json, options ?? BenchmarkDeterminismJson.SerializerOptions);
        return artifact ?? throw new InvalidDataException("Benchmark invalid-run payload deserialized to null.");
    }
}

public sealed record BenchmarkRunResult
{
    public BenchmarkRunResult(
        string runId,
        ModuleId moduleId,
        bool deterministic,
        BenchmarkRunPhase phase,
        RunState state,
        BenchmarkDeterminismContract? determinism,
        IReadOnlyList<string>? chapterSet,
        string chapterSetFingerprint,
        IReadOnlyList<BenchmarkRunChapterOutcome>? chapterOutcomes,
        FileInfo? manifestFile,
        FileInfo? invalidRunFile,
        RunFailure? failure = null,
        IReadOnlyList<BenchmarkRunPhaseTransition>? phaseTransitions = null,
        BenchmarkMetricsPolicySnapshot? metricsPolicy = null,
        BenchmarkRunMetricsAggregate? aggregateMetrics = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(moduleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterSetFingerprint);

        if (state == RunState.Failed && failure is null)
        {
            throw new ArgumentException("Failed benchmark run results must include failure metadata.", nameof(failure));
        }

        if (state != RunState.Failed && failure is not null)
        {
            throw new ArgumentException("Only failed benchmark run results may include failure metadata.", nameof(failure));
        }

        if (phase == BenchmarkRunPhase.Invalid && invalidRunFile is null)
        {
            throw new ArgumentException("Invalid deterministic benchmark results must include an invalid-run artifact path.", nameof(invalidRunFile));
        }

        var normalizedOutcomes = chapterOutcomes?.ToArray() ?? [];

        RunId = runId;
        ModuleId = moduleId;
        Deterministic = deterministic;
        Phase = phase;
        State = state;
        Determinism = determinism;
        ChapterSet = chapterSet?.ToArray() ?? [];
        ChapterSetFingerprint = chapterSetFingerprint;
        ChapterOutcomes = normalizedOutcomes;
        ManifestFile = manifestFile;
        InvalidRunFile = invalidRunFile;
        Failure = failure;
        PhaseTransitions = phaseTransitions?.ToArray() ?? [];
        MetricsPolicy = metricsPolicy ?? BenchmarkMetricsPolicySnapshot.Default;
        AggregateMetrics = aggregateMetrics ?? BenchmarkRunMetricsAggregate.FromChapterOutcomes(normalizedOutcomes);
    }

    public string RunId { get; }

    public ModuleId ModuleId { get; }

    public bool Deterministic { get; }

    public BenchmarkRunPhase Phase { get; }

    public RunState State { get; }

    public BenchmarkDeterminismContract? Determinism { get; }

    public IReadOnlyList<string> ChapterSet { get; }

    public string ChapterSetFingerprint { get; }

    public IReadOnlyList<BenchmarkRunChapterOutcome> ChapterOutcomes { get; }

    public FileInfo? ManifestFile { get; }

    public FileInfo? InvalidRunFile { get; }

    public RunFailure? Failure { get; }

    public IReadOnlyList<BenchmarkRunPhaseTransition> PhaseTransitions { get; }

    public BenchmarkMetricsPolicySnapshot MetricsPolicy { get; }

    public BenchmarkRunMetricsAggregate AggregateMetrics { get; }

    public FileInfo? ArtifactFile => InvalidRunFile ?? ManifestFile;
}
