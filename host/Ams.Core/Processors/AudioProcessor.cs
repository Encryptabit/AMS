using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ams.Core.Artifacts;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Processors;

/// <summary>
/// Central place for FFmpeg-backed audio primitives.
/// </summary>
public static partial class AudioProcessor
{
    public const int DefaultAsrSampleRate = 16_000;

    public static AudioInfo Probe(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FfDecoder.Probe(path);
    }

    public static AudioBuffer Decode(string path, AudioDecodeOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var effective = options ?? new AudioDecodeOptions();
        return FfDecoder.Decode(path, effective);
    }

    public static void EncodeWav(string path, AudioBuffer buffer, AudioEncodeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effective = options ?? new AudioEncodeOptions();

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        FfEncoder.EncodeToCustomStream(buffer, stream, effective);
        stream.Flush();
    }

    public static MemoryStream EncodeWavToStream(AudioBuffer buffer, AudioEncodeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var effective = options ?? new AudioEncodeOptions();

        var ms = new MemoryStream();
        FfEncoder.EncodeToCustomStream(buffer, ms, effective);
        ms.Position = 0;
        return ms;
    }

    public static AudioBuffer Resample(AudioBuffer buffer, ulong targetSampleRate)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (targetSampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSampleRate));
        }

        if (buffer.SampleRate == (int)targetSampleRate)
        {
            return buffer;
        }
        
        var graph = FfFilterGraph.FromBuffer(buffer);
        
        graph.Resample(new ResampleFilterParams(targetSampleRate));
       
        return graph.ToBuffer();
    }

    public static IReadOnlyList<SilenceInterval> DetectSilence(AudioBuffer buffer, SilenceDetectOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var opts = options ?? new SilenceDetectOptions();
        var filter = FormattableString.Invariant($"silencedetect=noise={opts.NoiseDb:F2}dB:d={opts.MinimumDuration.TotalSeconds:F3}");

        var logs = FfFilterGraph.FromBuffer(buffer)
            .Custom(filter)
            .CaptureLogs();

        return SilenceLogParser.Parse(logs);
    }

    public static AudioBuffer Trim(AudioBuffer buffer, TimeSpan start, TimeSpan? end = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        double startSeconds = Math.Max(0, start.TotalSeconds);
        string filter = end.HasValue
            ? FormattableString.Invariant($"atrim=start={startSeconds:F6}:end={Math.Max(startSeconds, end.Value.TotalSeconds):F6},asetpts=PTS-STARTPTS")
            : FormattableString.Invariant($"atrim=start={startSeconds:F6},asetpts=PTS-STARTPTS");

        return FfFilterGraphRunner.Apply(buffer, filter);
    }

    public static AudioBuffer FadeIn(AudioBuffer buffer, TimeSpan duration)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (duration <= TimeSpan.Zero)
        {
            return buffer;
        }

        var filter = FormattableString.Invariant($"afade=t=in:ss=0:d={duration.TotalSeconds:F6}");
        return FfFilterGraphRunner.Apply(buffer, filter);
    }

    public static AudioBuffer FadeOut(AudioBuffer buffer, TimeSpan start, TimeSpan duration)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (duration <= TimeSpan.Zero)
        {
            return buffer;
        }

        double startSeconds = Math.Max(0, start.TotalSeconds);
        var filter = FormattableString.Invariant($"afade=t=out:st={startSeconds:F6}:d={duration.TotalSeconds:F6}");
        return FfFilterGraphRunner.Apply(buffer, filter);
    }

    public static AudioBuffer AdjustVolume(AudioBuffer buffer, double gain)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var filter = FormattableString.Invariant($"volume={gain:F6}");
        return FfFilterGraphRunner.Apply(buffer, filter);
    }

    public static LoudnessNormalizationResult NormalizeLoudness(AudioBuffer buffer, LoudnessNormalizeOptions? options = null)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        var opts = options ?? new LoudnessNormalizeOptions();
        var firstPassFilter = BuildLoudnessFirstPassFilter(opts);
        var measurements = FfFilterGraph
            .FromBuffer(buffer)
            .Custom(firstPassFilter)
            .Measure(LoudnessLogParser.Parse);

        var secondPassFilter = BuildLoudnessSecondPassFilter(opts, measurements);
        var normalized = FfFilterGraph.FromBuffer(buffer)
            .Custom(secondPassFilter)
            .ToBuffer();
        return new LoudnessNormalizationResult(normalized, measurements, opts);
    }

    private static string BuildLoudnessFirstPassFilter(LoudnessNormalizeOptions options)
    {
        return FormattableString.Invariant($"loudnorm=I={options.TargetIntegrated:F2}:TP={options.TargetTruePeak:F2}:LRA={options.TargetLoudnessRange:F2}:print_format=json");
    }

    private static string BuildLoudnessSecondPassFilter(LoudnessNormalizeOptions options, LoudnessMeasurements measurements)
    {
        var linearValue = options.LinearScaling ? "true" : "false";
        return FormattableString.Invariant(
            $"loudnorm=I={options.TargetIntegrated:F2}:TP={options.TargetTruePeak:F2}:LRA={options.TargetLoudnessRange:F2}:measured_I={measurements.InputIntegrated:F2}:measured_TP={measurements.InputTruePeak:F2}:measured_LRA={measurements.InputLoudnessRange:F2}:measured_thresh={measurements.InputThreshold:F2}:offset={measurements.TargetOffset:F2}:linear={linearValue}:print_format=summary");
    }

    private static class SilenceLogParser
    {
        private static readonly Regex Pattern = new("silence_(?<kind>start|end|duration):\\s*(?<value>[-+]?\\d+(?:\\.\\d+)?)", RegexOptions.Compiled);

        public static IReadOnlyList<SilenceInterval> Parse(IEnumerable<string> logs)
        {
            var intervals = new List<SilenceInterval>();
            double? currentStart = null;
            double? lastDuration = null;

            foreach (var line in logs)
            {
                double? start = null;
                double? end = null;
                double? duration = null;

                foreach (Match match in Pattern.Matches(line))
                {
                    var kind = match.Groups["kind"].Value;
                    var value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
                    switch (kind)
                    {
                        case "start":
                            start = value;
                            break;
                        case "end":
                            end = value;
                            break;
                        case "duration":
                            duration = value;
                            break;
                    }
                }

                if (start.HasValue)
                {
                    currentStart = start.Value;
                    lastDuration = null;
                    continue;
                }

                if (duration.HasValue)
                {
                    lastDuration = duration.Value;
                }

                if (end.HasValue)
                {
                    var endSeconds = end.Value;
                    var durationSeconds = duration ?? lastDuration ?? (currentStart.HasValue ? endSeconds - currentStart.Value : 0);
                    double startSeconds = currentStart ?? (endSeconds - durationSeconds);

                    intervals.Add(new SilenceInterval(
                        TimeSpan.FromSeconds(Math.Max(0, startSeconds)),
                        TimeSpan.FromSeconds(Math.Max(0, endSeconds)),
                        TimeSpan.FromSeconds(Math.Max(0, durationSeconds))));

                    currentStart = null;
                    lastDuration = null;
                }
            }

            return intervals;
        }
    }

    private static class LoudnessLogParser
    {
        public static LoudnessMeasurements Parse(IEnumerable<string> logs)
        {
            var jsonLine = logs.LastOrDefault(line => line.TrimStart().StartsWith("{", StringComparison.Ordinal));
            if (jsonLine is null)
            {
                throw new InvalidOperationException("FFmpeg loudnorm filter did not produce measurement data.");
            }

            using var document = JsonDocument.Parse(jsonLine);
            var root = document.RootElement;

            double integrated = ParseDouble(root, "input_i");
            double truePeak = ParseDouble(root, "input_tp");
            double lra = ParseDouble(root, "input_lra");
            double threshold = ParseDouble(root, "input_thresh");
            double offset = ParseDouble(root, "target_offset");

            return new LoudnessMeasurements(integrated, truePeak, lra, threshold, offset);
        }

        private static double ParseDouble(JsonElement root, string property)
        {
            if (!root.TryGetProperty(property, out var element))
            {
                throw new InvalidOperationException(FormattableString.Invariant($"FFmpeg loudnorm output missing '{property}'."));
            }

            var value = element.ValueKind == JsonValueKind.Number
                ? element.GetDouble()
                : double.Parse(element.GetString() ?? "0", CultureInfo.InvariantCulture);

            return value;
        }
    }
}

public readonly record struct AudioInfo(
    string Format,
    int SampleRate,
    int Channels,
    TimeSpan Duration);

public sealed record AudioDecodeOptions(
    TimeSpan? Start = null,
    TimeSpan? Duration = null,
    int? TargetSampleRate = null,
    int? TargetChannels = null);

public sealed record AudioEncodeOptions(
    int? TargetSampleRate = null,
    int? TargetBitDepth = null);

public sealed record SilenceDetectOptions
{
    public double NoiseDb { get; init; } = -50.0;
    public TimeSpan MinimumDuration { get; init; } = TimeSpan.FromMilliseconds(500);
}

public sealed record SilenceInterval(TimeSpan Start, TimeSpan End, TimeSpan Duration);

public sealed record LoudnessNormalizeOptions
{
    public double TargetIntegrated { get; init; } = -16.0;
    public double TargetTruePeak { get; init; } = -1.5;
    public double TargetLoudnessRange { get; init; } = 11.0;
    public bool LinearScaling { get; init; } = true;
}

public sealed record LoudnessMeasurements(
    double InputIntegrated,
    double InputTruePeak,
    double InputLoudnessRange,
    double InputThreshold,
    double TargetOffset);

public sealed record LoudnessNormalizationResult(
    AudioBuffer Buffer,
    LoudnessMeasurements Measurements,
    LoudnessNormalizeOptions Options);
