using System.Diagnostics;
using System.Text.Json;
using Ams.Core.Application.Runs;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Audio.QualityControl;
using Ams.Core.Processors;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkMetricsCollector : IBenchmarkMetricsCollector
{
    private readonly Func<string, IReadOnlyDictionary<int, SentenceTiming>> _readHydrateTimings;
    private readonly Func<string, AudioBuffer> _decodeAudio;
    private readonly Func<AudioBuffer, AudioLoudnessAnalysisOptions?, AudioLoudnessMetrics> _analyzeLoudness;
    private readonly Func<
        float[],
        int,
        float[],
        int,
        IReadOnlyDictionary<int, SentenceTiming>,
        IReadOnlyDictionary<int, SentenceTiming>,
        double,
        double,
        double,
        double,
        double,
        AudioVerificationResult> _verifyIntegrity;
    private readonly Func<string, double, double, QcThresholds, string?, ChapterQcResult> _analyzeQc;
    private readonly Func<Stopwatch> _stopwatchFactory;

    public BenchmarkMetricsCollector()
        : this(
            readHydrateTimings: BenchmarkHydrateTimingReader.ReadStrict,
            decodeAudio: path => AudioProcessor.Decode(path),
            analyzeLoudness: (buffer, options) => AudioProcessor.AnalyzeLoudness(buffer, options),
            verifyIntegrity: AudioIntegrityVerifier.Verify,
            analyzeQc: AudioQcAnalyzer.AnalyzeFile,
            stopwatchFactory: Stopwatch.StartNew)
    {
    }

    internal BenchmarkMetricsCollector(
        Func<string, IReadOnlyDictionary<int, SentenceTiming>> readHydrateTimings,
        Func<string, AudioBuffer> decodeAudio,
        Func<AudioBuffer, AudioLoudnessAnalysisOptions?, AudioLoudnessMetrics> analyzeLoudness,
        Func<
            float[],
            int,
            float[],
            int,
            IReadOnlyDictionary<int, SentenceTiming>,
            IReadOnlyDictionary<int, SentenceTiming>,
            double,
            double,
            double,
            double,
            double,
            AudioVerificationResult> verifyIntegrity,
        Func<string, double, double, QcThresholds, string?, ChapterQcResult> analyzeQc,
        Func<Stopwatch>? stopwatchFactory = null)
    {
        _readHydrateTimings = readHydrateTimings ?? throw new ArgumentNullException(nameof(readHydrateTimings));
        _decodeAudio = decodeAudio ?? throw new ArgumentNullException(nameof(decodeAudio));
        _analyzeLoudness = analyzeLoudness ?? throw new ArgumentNullException(nameof(analyzeLoudness));
        _verifyIntegrity = verifyIntegrity ?? throw new ArgumentNullException(nameof(verifyIntegrity));
        _analyzeQc = analyzeQc ?? throw new ArgumentNullException(nameof(analyzeQc));
        _stopwatchFactory = stopwatchFactory ?? Stopwatch.StartNew;
    }

    public Task<BenchmarkChapterMetrics> CollectAsync(
        BenchmarkMetricsCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = _stopwatchFactory();
        var audioProcessingActivities = new List<BenchmarkAudioProcessingActivity>();
        var audioProcessingActivityLock = new object();

        using var activityCapture = AudioProcessor.BeginActivityCapture(activity =>
        {
            var mapped = MapAudioProcessingActivity(activity);
            lock (audioProcessingActivityLock)
            {
                audioProcessingActivities.Add(mapped);
            }
        });

        try
        {
            if (!request.Policy.Enabled)
            {
                var disabledRuntime = new BenchmarkChapterRuntimeMetrics(
                    pipelineRuntimeMs: request.PipelineRuntimeMs,
                    analysisRuntimeMs: 0);

                return Task.FromResult(new BenchmarkChapterMetrics(
                    BenchmarkMetricsStatus.NotRun,
                    runtime: disabledRuntime,
                    audioProcessingActivities: audioProcessingActivities));
            }

            var requiredFileFailure = ValidateRequiredFiles(request);
            if (requiredFileFailure is not null)
            {
                return Task.FromResult(CreateFailedMetrics(
                    request,
                    stopwatch.ElapsedMilliseconds,
                    requiredFileFailure,
                    audioProcessingActivities));
            }

            BenchmarkMetricsFailure? firstFailure = null;

            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyDictionary<int, SentenceTiming>? rawTimings = null;
            IReadOnlyDictionary<int, SentenceTiming>? treatedTimings = null;

            try
            {
                rawTimings = _readHydrateTimings(request.HydrateFile.FullName);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex, "hydrate-parse", request.ChapterId, request.HydrateFile.FullName);
            }

            try
            {
                treatedTimings = PathsEqual(request.HydrateFile.FullName, request.TreatedHydrateFile.FullName)
                    ? rawTimings ?? _readHydrateTimings(request.HydrateFile.FullName)
                    : _readHydrateTimings(request.TreatedHydrateFile.FullName);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex,
                    "hydrate-parse",
                    request.ChapterId,
                    request.TreatedHydrateFile.FullName);
            }

            cancellationToken.ThrowIfCancellationRequested();

            AudioBuffer? rawBuffer = null;
            AudioBuffer? treatedBuffer = null;

            try
            {
                rawBuffer = _decodeAudio(request.RawAudioFile.FullName);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex, "audio-decode", request.ChapterId, request.RawAudioFile.FullName);
            }

            try
            {
                treatedBuffer = _decodeAudio(request.TreatedAudioFile.FullName);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex,
                    "audio-decode",
                    request.ChapterId,
                    request.TreatedAudioFile.FullName);
            }

            BenchmarkAudioIntegrityMetrics? integrityMetrics = null;
            if (rawBuffer is not null
                && treatedBuffer is not null
                && rawTimings is not null
                && treatedTimings is not null)
            {
                try
                {
                    var integrity = _verifyIntegrity(
                        ToMono(rawBuffer),
                        rawBuffer.SampleRate,
                        ToMono(treatedBuffer),
                        treatedBuffer.SampleRate,
                        rawTimings,
                        treatedTimings,
                        request.Policy.IntegrityWindowMs,
                        request.Policy.IntegrityStepMs,
                        request.Policy.IntegrityMinMismatchMs,
                        request.Policy.IntegrityMergeGapMs,
                        request.Policy.IntegrityMinDeltaDb);

                    integrityMetrics = new BenchmarkAudioIntegrityMetrics(
                        integrity.DurationSec,
                        integrity.RawSpeechSec,
                        integrity.TreatedSpeechSec,
                        integrity.MissingSpeechSec,
                        integrity.ExtraSpeechSec,
                        integrity.Mismatches.Count);
                }
                catch (Exception ex)
                {
                    firstFailure ??= CreateFailure(ex,
                        "integrity-analysis",
                        request.ChapterId,
                        request.TreatedAudioFile.FullName);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            BenchmarkQcMetrics? rawQcMetrics = null;
            BenchmarkQcMetrics? treatedQcMetrics = null;

            try
            {
                var rawQc = _analyzeQc(
                    request.RawAudioFile.FullName,
                    request.Policy.QcNoiseFloorDb,
                    request.Policy.QcMinSilenceDurationSec,
                    request.Policy.ToQcThresholds(),
                    request.SectionTitle);

                rawQcMetrics = new BenchmarkQcMetrics(
                    rawQc.HeadSilenceSec,
                    rawQc.TitleBodyGapSec,
                    rawQc.TailSilenceSec,
                    rawQc.Flags.Count,
                    rawQc.Flags);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex, "qc-analysis", request.ChapterId, request.RawAudioFile.FullName);
            }

            try
            {
                var treatedQc = _analyzeQc(
                    request.TreatedAudioFile.FullName,
                    request.Policy.QcNoiseFloorDb,
                    request.Policy.QcMinSilenceDurationSec,
                    request.Policy.ToQcThresholds(),
                    request.SectionTitle);

                treatedQcMetrics = new BenchmarkQcMetrics(
                    treatedQc.HeadSilenceSec,
                    treatedQc.TitleBodyGapSec,
                    treatedQc.TailSilenceSec,
                    treatedQc.Flags.Count,
                    treatedQc.Flags);
            }
            catch (Exception ex)
            {
                firstFailure ??= CreateFailure(ex,
                    "qc-analysis",
                    request.ChapterId,
                    request.TreatedAudioFile.FullName);
            }

            cancellationToken.ThrowIfCancellationRequested();

            BenchmarkLoudnessMetrics? rawLoudnessMetrics = null;
            BenchmarkLoudnessMetrics? treatedLoudnessMetrics = null;

            var loudnessOptions = new AudioLoudnessAnalysisOptions
            {
                WindowDuration = TimeSpan.FromSeconds(request.Policy.LoudnessWindowSec),
                ComputeIntegratedLufs = true
            };

            if (rawBuffer is not null)
            {
                try
                {
                    var loudness = _analyzeLoudness(rawBuffer, loudnessOptions);
                    rawLoudnessMetrics = ToLoudnessMetrics(loudness);
                }
                catch (Exception ex)
                {
                    firstFailure ??= CreateFailure(ex,
                        "loudness-analysis",
                        request.ChapterId,
                        request.RawAudioFile.FullName);
                }
            }

            if (treatedBuffer is not null)
            {
                try
                {
                    var loudness = _analyzeLoudness(treatedBuffer, loudnessOptions);
                    treatedLoudnessMetrics = ToLoudnessMetrics(loudness);
                }
                catch (Exception ex)
                {
                    firstFailure ??= CreateFailure(ex,
                        "loudness-analysis",
                        request.ChapterId,
                        request.TreatedAudioFile.FullName);
                }
            }

            var quality = new BenchmarkChapterQualityMetrics(
                integrityMetrics,
                rawQcMetrics,
                treatedQcMetrics,
                rawLoudnessMetrics,
                treatedLoudnessMetrics);

            var runtime = new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: request.PipelineRuntimeMs,
                analysisRuntimeMs: stopwatch.ElapsedMilliseconds);

            if (firstFailure is null)
            {
                return Task.FromResult(new BenchmarkChapterMetrics(
                    BenchmarkMetricsStatus.Completed,
                    runtime,
                    quality,
                    audioProcessingActivities: audioProcessingActivities));
            }

            if (quality.HasAnyData)
            {
                return Task.FromResult(new BenchmarkChapterMetrics(
                    BenchmarkMetricsStatus.Partial,
                    runtime,
                    quality,
                    firstFailure,
                    audioProcessingActivities));
            }

            return Task.FromResult(new BenchmarkChapterMetrics(
                BenchmarkMetricsStatus.Failed,
                runtime,
                quality: null,
                metricsFailure: firstFailure,
                audioProcessingActivities: audioProcessingActivities));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var fallbackFailure = CreateFailure(ex, "collector", request.ChapterId, null);
            return Task.FromResult(CreateFailedMetrics(
                request,
                stopwatch.ElapsedMilliseconds,
                fallbackFailure,
                audioProcessingActivities));
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private static BenchmarkChapterMetrics CreateFailedMetrics(
        BenchmarkMetricsCollectionRequest request,
        long analysisRuntimeMs,
        BenchmarkMetricsFailure failure,
        IReadOnlyList<BenchmarkAudioProcessingActivity>? audioProcessingActivities = null)
    {
        var runtime = new BenchmarkChapterRuntimeMetrics(
            pipelineRuntimeMs: request.PipelineRuntimeMs,
            analysisRuntimeMs: analysisRuntimeMs);

        return new BenchmarkChapterMetrics(
            BenchmarkMetricsStatus.Failed,
            runtime,
            quality: null,
            metricsFailure: failure,
            audioProcessingActivities: audioProcessingActivities);
    }

    private static BenchmarkMetricsFailure? ValidateRequiredFiles(BenchmarkMetricsCollectionRequest request)
    {
        BenchmarkMetricsFailure? failure = null;

        void Check(FileInfo file, string operation)
        {
            if (failure is not null)
            {
                return;
            }

            file.Refresh();
            if (!file.Exists)
            {
                failure = CreateFailure(
                    new FileNotFoundException($"Required input file was not found: {file.FullName}", file.FullName),
                    operation,
                    request.ChapterId,
                    file.FullName);
            }
        }

        Check(request.RawAudioFile, "audio-decode");
        Check(request.TreatedAudioFile, "audio-decode");
        Check(request.HydrateFile, "hydrate-parse");

        if (!PathsEqual(request.HydrateFile.FullName, request.TreatedHydrateFile.FullName))
        {
            Check(request.TreatedHydrateFile, "hydrate-parse");
        }

        return failure;
    }

    private static BenchmarkLoudnessMetrics ToLoudnessMetrics(AudioLoudnessMetrics loudness)
    {
        if (!double.IsFinite(loudness.DurationSec) || loudness.DurationSec < 0)
        {
            throw new InvalidDataException("Loudness payload included invalid duration.");
        }

        ValidateOptionalFinite(loudness.SamplePeakDbFs, nameof(loudness.SamplePeakDbFs));
        ValidateOptionalFinite(loudness.TruePeakDbFs, nameof(loudness.TruePeakDbFs));
        ValidateOptionalFinite(loudness.OverallRmsDbFs, nameof(loudness.OverallRmsDbFs));
        ValidateOptionalFinite(loudness.IntegratedLufs, nameof(loudness.IntegratedLufs));

        return new BenchmarkLoudnessMetrics(
            loudness.DurationSec,
            loudness.SamplePeakDbFs,
            loudness.TruePeakDbFs,
            loudness.OverallRmsDbFs,
            loudness.IntegratedLufs);
    }

    private static BenchmarkMetricsFailure CreateFailure(
        Exception exception,
        string operation,
        string chapterId,
        string? resourcePath)
    {
        var kind = exception switch
        {
            TimeoutException => RunFailureKind.Timeout,
            OperationCanceledException => RunFailureKind.Cancelled,
            FileNotFoundException => RunFailureKind.Validation,
            DirectoryNotFoundException => RunFailureKind.Validation,
            BenchmarkHydrateTimingReadException => RunFailureKind.Validation,
            JsonException => RunFailureKind.Validation,
            InvalidDataException => RunFailureKind.Validation,
            FormatException => RunFailureKind.Validation,
            ArgumentException => RunFailureKind.Validation,
            IOException => RunFailureKind.Dependency,
            UnauthorizedAccessException => RunFailureKind.Dependency,
            _ => RunFailureKind.Execution
        };

        var message = operation switch
        {
            "hydrate-parse" when exception is FileNotFoundException => "Hydrate timing file was not found.",
            "hydrate-parse" when exception is BenchmarkHydrateTimingReadException =>
                "Hydrate timing payload was malformed.",
            "loudness-analysis" when exception is InvalidDataException =>
                "Loudness analyzer returned invalid metrics payload.",
            _ => exception.Message
        };

        return new BenchmarkMetricsFailure(
            kind,
            message,
            operation,
            chapterId,
            SanitizeResourcePath(resourcePath));
    }

    private static BenchmarkAudioProcessingActivity MapAudioProcessingActivity(AudioProcessorActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        return new BenchmarkAudioProcessingActivity(
            function: activity.Function,
            startedAtUtc: activity.StartedAtUtc,
            durationMs: activity.DurationMs,
            succeeded: activity.Succeeded,
            failureKind: activity.FailureKind,
            detail: activity.Detail,
            durationUs: activity.DurationUs);
    }

    private static string? SanitizeResourcePath(string? resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        var normalized = resourcePath.Trim().Replace('\\', '/');
        if (Path.IsPathRooted(normalized))
        {
            var fileName = Path.GetFileName(normalized);
            return string.IsNullOrWhiteSpace(fileName) ? "unknown" : fileName;
        }

        return normalized;
    }

    private static bool PathsEqual(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private static void ValidateOptionalFinite(double? value, string parameterName)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (!double.IsFinite(value.Value))
        {
            throw new InvalidDataException($"Loudness payload field '{parameterName}' was not finite.");
        }
    }

    private static float[] ToMono(AudioBuffer buffer)
    {
        if (buffer.Channels <= 1)
        {
            return buffer.GetChannel(0).ToArray();
        }

        var mono = new float[buffer.Length];
        var scale = 1f / buffer.Channels;

        for (var channel = 0; channel < buffer.Channels; channel++)
        {
            var source = buffer.GetChannel(channel).Span;
            for (var index = 0; index < mono.Length; index++)
            {
                mono[index] += source[index] * scale;
            }
        }

        return mono;
    }
}
