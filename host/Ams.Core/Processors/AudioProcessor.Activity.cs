using System.Diagnostics;
using System.Threading;

namespace Ams.Core.Processors;

public sealed record AudioProcessorActivity(
    string Function,
    DateTimeOffset StartedAtUtc,
    long DurationMs,
    bool Succeeded,
    string? FailureKind = null,
    string? Detail = null,
    long DurationUs = -1)
{
    public string Function { get; } = string.IsNullOrWhiteSpace(Function)
        ? throw new ArgumentException("Function must be provided.", nameof(Function))
        : Function.Trim();

    public DateTimeOffset StartedAtUtc { get; } = StartedAtUtc;

    public long DurationMs { get; } = DurationMs < 0
        ? throw new ArgumentOutOfRangeException(nameof(DurationMs), "Duration cannot be negative.")
        : DurationMs;

    public long DurationUs { get; } = DurationUs switch
    {
        < -1 => throw new ArgumentOutOfRangeException(nameof(DurationUs), "Duration cannot be negative."),
        -1 => checked(DurationMs * 1000L),
        _ => DurationUs
    };

    public bool Succeeded { get; } = Succeeded;

    public string? FailureKind { get; } = NormalizeOptionalText(FailureKind);

    public string? Detail { get; } = NormalizeOptionalText(Detail);

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public static partial class AudioProcessor
{
    private static readonly ActivitySource ProcessingActivitySource = new("Ams.Core.Processors.AudioProcessor");
    private static readonly AsyncLocal<Action<AudioProcessorActivity>?> ProcessingActivitySink = new();

    internal static IDisposable BeginActivityCapture(Action<AudioProcessorActivity> sink)
    {
        ArgumentNullException.ThrowIfNull(sink);

        var previous = ProcessingActivitySink.Value;
        ProcessingActivitySink.Value = previous is null
            ? sink
            : activity =>
            {
                previous(activity);
                sink(activity);
            };

        return new ActivityCaptureScope(previous);
    }

    private static T MeasureActivity<T>(string function, Func<T> operation, string? detail = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(function);
        ArgumentNullException.ThrowIfNull(operation);

        var startedAtUtc = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        using var activity = ProcessingActivitySource.StartActivity(
            $"audio-processor.{function}",
            ActivityKind.Internal);

        activity?.SetTag("audio.processor.function", function);
        if (!string.IsNullOrWhiteSpace(detail))
        {
            activity?.SetTag("audio.processor.detail", detail);
        }

        try
        {
            var result = operation();
            var (durationMs, durationUs) = StopAndCaptureDurations(stopwatch);

            activity?.SetTag("audio.processor.duration_ms", durationMs);
            activity?.SetTag("audio.processor.duration_us", durationUs);
            activity?.SetStatus(ActivityStatusCode.Ok);

            EmitProcessingActivity(new AudioProcessorActivity(
                Function: function,
                StartedAtUtc: startedAtUtc,
                DurationMs: durationMs,
                Succeeded: true,
                FailureKind: null,
                Detail: detail,
                DurationUs: durationUs));

            return result;
        }
        catch (Exception ex)
        {
            var (durationMs, durationUs) = StopAndCaptureDurations(stopwatch);

            activity?.SetTag("audio.processor.duration_ms", durationMs);
            activity?.SetTag("audio.processor.duration_us", durationUs);
            activity?.SetTag("audio.processor.failure_kind", ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            EmitProcessingActivity(new AudioProcessorActivity(
                Function: function,
                StartedAtUtc: startedAtUtc,
                DurationMs: durationMs,
                Succeeded: false,
                FailureKind: ex.GetType().Name,
                Detail: detail,
                DurationUs: durationUs));

            throw;
        }
    }

    private static void MeasureActivity(string function, Action operation, string? detail = null)
    {
        _ = MeasureActivity<object?>(
            function,
            () =>
            {
                operation();
                return null;
            },
            detail);
    }

    private static (long DurationMs, long DurationUs) StopAndCaptureDurations(Stopwatch stopwatch)
    {
        stopwatch.Stop();

        var durationMs = stopwatch.ElapsedMilliseconds;
        var durationUs = ConvertTicksToMicroseconds(stopwatch.ElapsedTicks);
        return (durationMs, durationUs);
    }

    private static long ConvertTicksToMicroseconds(long elapsedTicks)
    {
        if (elapsedTicks <= 0)
        {
            return 0;
        }

        var microseconds = elapsedTicks * (1_000_000d / Stopwatch.Frequency);
        var rounded = Math.Round(microseconds, MidpointRounding.AwayFromZero);

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

    private static void EmitProcessingActivity(AudioProcessorActivity activity)
    {
        try
        {
            ProcessingActivitySink.Value?.Invoke(activity);
        }
        catch (Exception sinkException)
        {
            Log.Debug(
                "Audio processing activity sink failed for {Function}: {Message}",
                activity.Function,
                sinkException.Message);
        }

        if (activity.Succeeded)
        {
            Log.Debug(
                "Audio processing activity: function={Function}, durationMs={DurationMs}, durationUs={DurationUs}, detail={Detail}",
                activity.Function,
                activity.DurationMs,
                activity.DurationUs,
                activity.Detail ?? "none");

            return;
        }

        Log.Debug(
            "Audio processing activity: function={Function}, durationMs={DurationMs}, durationUs={DurationUs}, failureKind={FailureKind}, detail={Detail}",
            activity.Function,
            activity.DurationMs,
            activity.DurationUs,
            activity.FailureKind ?? "unknown",
            activity.Detail ?? "none");
    }

    private sealed class ActivityCaptureScope : IDisposable
    {
        private readonly Action<AudioProcessorActivity>? _previous;
        private bool _disposed;

        public ActivityCaptureScope(Action<AudioProcessorActivity>? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ProcessingActivitySink.Value = _previous;
            _disposed = true;
        }
    }
}
