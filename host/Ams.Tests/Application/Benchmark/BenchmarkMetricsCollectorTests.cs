using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Runs;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Audio.QualityControl;
using Ams.Core.Processors;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkMetricsCollectorTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public async Task CollectAsync_AllAnalyzersSucceed_ReturnsCompletedMetricsPayload()
    {
        var root = CreateTempDirectory();
        var files = CreateInputFiles(root, "chapter-01");

        var timings = new Dictionary<int, SentenceTiming>
        {
            [1] = new(0.0, 1.2),
            [2] = new(1.3, 2.5)
        };

        var collector = new BenchmarkMetricsCollector(
            readHydrateTimings: _ => timings,
            decodeAudio: _ => CreateMonoBuffer(sampleRate: 16_000, length: 3_200),
            analyzeLoudness: (_, options) => new AudioLoudnessMetrics(
                DurationSec: 12.5,
                SamplePeakLinear: 0.82,
                SamplePeakDbFs: -1.72,
                TruePeakLinear: 0.83,
                TruePeakDbFs: -1.61,
                OverallRmsLinear: 0.09,
                OverallRmsDbFs: -20.92,
                MinWindowRmsLinear: 0.04,
                MinWindowRmsDbFs: -27.96,
                MaxWindowRmsLinear: 0.13,
                MaxWindowRmsDbFs: -17.72,
                WindowDurationSec: options?.WindowDuration.TotalSeconds ?? 0.5,
                IntegratedLufs: -18.4),
            verifyIntegrity: (_, _, _, _, _, _, _, _, _, _, _) => new AudioVerificationResult(
                WindowMs: 30,
                StepMs: 15,
                RawSpeechThresholdDb: -45,
                TreatedSpeechThresholdDb: -46,
                DurationSec: 12.5,
                RawSpeechSec: 10.4,
                TreatedSpeechSec: 10.0,
                MissingSpeechSec: 0.3,
                ExtraSpeechSec: 0.1,
                Mismatches:
                [
                    new AudioMismatch(
                        StartSec: 4.2,
                        EndSec: 4.7,
                        Type: AudioMismatchType.MissingSpeech,
                        RawDb: -26,
                        TreatedDb: -51,
                        DeltaDb: 25,
                        Sentences: [new SentenceSpan(2, 4.0, 5.0)])
                ]),
            analyzeQc: (path, _, _, _, _) => new ChapterQcResult
            {
                FileName = Path.GetFileName(path),
                DurationSec = 12.5,
                HeadSilenceSec = 0.7,
                TitleBodyGapSec = 1.2,
                TailSilenceSec = 2.3,
                Flags = string.Equals(Path.GetFileName(path), Path.GetFileName(files.TreatedAudio.FullName), StringComparison.OrdinalIgnoreCase)
                    ? ["TAIL_SILENCE_LONG"]
                    : []
            });

        var request = new BenchmarkMetricsCollectionRequest(
            chapterId: "chapter-01",
            rawAudioFile: files.RawAudio,
            treatedAudioFile: files.TreatedAudio,
            hydrateFile: files.Hydrate,
            policy: BenchmarkMetricsPolicySnapshot.Default,
            pipelineRuntimeMs: 4_200);

        var metrics = await collector.CollectAsync(request);

        Assert.Equal(BenchmarkMetricsStatus.Completed, metrics.Status);
        Assert.Null(metrics.MetricsFailure);
        Assert.NotNull(metrics.Quality);
        Assert.True(metrics.Quality!.IsComplete);

        Assert.Equal(4_200, metrics.Runtime.PipelineRuntimeMs);
        Assert.True(metrics.Runtime.AnalysisRuntimeMs >= 0);

        Assert.Equal(1, metrics.Quality.Integrity!.MismatchCount);
        Assert.Equal(0.3, metrics.Quality.Integrity.MissingSpeechSec, 3);
        Assert.Equal(1, metrics.Quality.TreatedQc!.FlagCount);
        Assert.NotNull(metrics.Quality.RawLoudness!.IntegratedLufs);
        Assert.Equal(-18.4, metrics.Quality.RawLoudness.IntegratedLufs!.Value, 3);
    }

    [Fact]
    public async Task CollectAsync_MissingHydrateFile_ReturnsFailedMetricsFailurePayload()
    {
        var root = CreateTempDirectory();
        var files = CreateInputFiles(root, "chapter-02");
        var missingHydrate = new FileInfo(Path.Combine(root, "chapter-02", "chapter-02.align.hydrate.missing.json"));

        var readCalls = 0;

        var collector = new BenchmarkMetricsCollector(
            readHydrateTimings: _ =>
            {
                readCalls++;
                return new Dictionary<int, SentenceTiming>();
            },
            decodeAudio: _ => CreateMonoBuffer(sampleRate: 16_000, length: 1_600),
            analyzeLoudness: (_, _) => new AudioLoudnessMetrics(
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
                WindowDurationSec: 0.5,
                IntegratedLufs: null),
            verifyIntegrity: (_, _, _, _, _, _, _, _, _, _, _) => new AudioVerificationResult(
                WindowMs: 30,
                StepMs: 15,
                RawSpeechThresholdDb: -45,
                TreatedSpeechThresholdDb: -46,
                DurationSec: 0,
                RawSpeechSec: 0,
                TreatedSpeechSec: 0,
                MissingSpeechSec: 0,
                ExtraSpeechSec: 0,
                Mismatches: []),
            analyzeQc: (_, _, _, _, _) => new ChapterQcResult
            {
                FileName = "chapter-02.wav",
                DurationSec = 0,
                HeadSilenceSec = 0,
                TailSilenceSec = 0
            });

        var request = new BenchmarkMetricsCollectionRequest(
            chapterId: "chapter-02",
            rawAudioFile: files.RawAudio,
            treatedAudioFile: files.TreatedAudio,
            hydrateFile: missingHydrate,
            pipelineRuntimeMs: 900);

        var metrics = await collector.CollectAsync(request);

        Assert.Equal(BenchmarkMetricsStatus.Failed, metrics.Status);
        Assert.NotNull(metrics.MetricsFailure);
        Assert.Equal(RunFailureKind.Validation, metrics.MetricsFailure!.Kind);
        Assert.Equal("hydrate-parse", metrics.MetricsFailure.Operation);
        Assert.Contains("hydrate", metrics.MetricsFailure.ResourcePath ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, readCalls);
    }

    [Fact]
    public async Task CollectAsync_QcTimeoutAfterIntegrity_ReturnsPartialPayloadWithMetricsFailure()
    {
        var root = CreateTempDirectory();
        var files = CreateInputFiles(root, "chapter-03");

        var timings = new Dictionary<int, SentenceTiming>
        {
            [1] = new(0.0, 1.0)
        };

        var collector = new BenchmarkMetricsCollector(
            readHydrateTimings: _ => timings,
            decodeAudio: _ => CreateMonoBuffer(sampleRate: 16_000, length: 3_200),
            analyzeLoudness: (_, _) => new AudioLoudnessMetrics(
                DurationSec: 11.0,
                SamplePeakLinear: 0.8,
                SamplePeakDbFs: -1.9,
                TruePeakLinear: 0.8,
                TruePeakDbFs: -1.9,
                OverallRmsLinear: 0.1,
                OverallRmsDbFs: -20,
                MinWindowRmsLinear: 0.04,
                MinWindowRmsDbFs: -28,
                MaxWindowRmsLinear: 0.13,
                MaxWindowRmsDbFs: -17.7,
                WindowDurationSec: 0.5,
                IntegratedLufs: -19.1),
            verifyIntegrity: (_, _, _, _, _, _, _, _, _, _, _) => new AudioVerificationResult(
                WindowMs: 30,
                StepMs: 15,
                RawSpeechThresholdDb: -45,
                TreatedSpeechThresholdDb: -46,
                DurationSec: 11,
                RawSpeechSec: 9.1,
                TreatedSpeechSec: 8.8,
                MissingSpeechSec: 0.2,
                ExtraSpeechSec: 0.1,
                Mismatches: []),
            analyzeQc: (path, _, _, _, _) =>
            {
                if (path.EndsWith("treated.wav", StringComparison.OrdinalIgnoreCase))
                {
                    throw new TimeoutException("qc decode timeout");
                }

                return new ChapterQcResult
                {
                    FileName = Path.GetFileName(path),
                    DurationSec = 11,
                    HeadSilenceSec = 0.6,
                    TailSilenceSec = 2.1,
                    Flags = []
                };
            });

        var request = new BenchmarkMetricsCollectionRequest(
            chapterId: "chapter-03",
            rawAudioFile: files.RawAudio,
            treatedAudioFile: files.TreatedAudio,
            hydrateFile: files.Hydrate,
            pipelineRuntimeMs: 1_500);

        var metrics = await collector.CollectAsync(request);

        Assert.Equal(BenchmarkMetricsStatus.Partial, metrics.Status);
        Assert.NotNull(metrics.MetricsFailure);
        Assert.Equal(RunFailureKind.Timeout, metrics.MetricsFailure!.Kind);
        Assert.Equal("qc-analysis", metrics.MetricsFailure.Operation);
        Assert.NotNull(metrics.Quality);
        Assert.True(metrics.Quality!.HasAnyData);
        Assert.NotNull(metrics.Quality.Integrity);
        Assert.NotNull(metrics.Quality.RawQc);
        Assert.NotNull(metrics.Quality.RawLoudness);
    }

    [Fact]
    public void ReadStrict_RejectsNonNumericTimingFields()
    {
        var root = CreateTempDirectory();
        var hydratePath = Path.Combine(root, "bad-timing.align.hydrate.json");

        File.WriteAllText(
            hydratePath,
            """
            {
              "sentences": [
                {
                  "id": 1,
                  "timing": {
                    "startSec": "not-a-number",
                    "endSec": 1.2
                  }
                }
              ]
            }
            """);

        var exception = Assert.Throws<BenchmarkHydrateTimingReadException>(
            () => BenchmarkHydrateTimingReader.ReadStrict(hydratePath));

        Assert.Contains("malformed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadStrict_RejectsEmptySentenceArray()
    {
        var root = CreateTempDirectory();
        var hydratePath = Path.Combine(root, "empty.align.hydrate.json");

        File.WriteAllText(
            hydratePath,
            """
            {
              "sentences": []
            }
            """);

        var exception = Assert.Throws<BenchmarkHydrateTimingReadException>(
            () => BenchmarkHydrateTimingReader.ReadStrict(hydratePath));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        foreach (var directory in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp cleanup.
            }
        }
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ams-benchmark-metrics-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }

    private static (FileInfo RawAudio, FileInfo TreatedAudio, FileInfo Hydrate) CreateInputFiles(string root, string chapterId)
    {
        var chapterDir = Path.Combine(root, chapterId);
        Directory.CreateDirectory(chapterDir);

        var rawAudioPath = Path.Combine(chapterDir, $"{chapterId}.wav");
        var treatedAudioPath = Path.Combine(chapterDir, $"{chapterId}.treated.wav");
        var hydratePath = Path.Combine(chapterDir, $"{chapterId}.align.hydrate.json");

        File.WriteAllBytes(rawAudioPath, [0x52, 0x49, 0x46, 0x46]);
        File.WriteAllBytes(treatedAudioPath, [0x52, 0x49, 0x46, 0x46]);
        File.WriteAllText(
            hydratePath,
            """
            {
              "sentences": [
                {
                  "id": 1,
                  "timing": {
                    "startSec": 0.0,
                    "endSec": 1.2
                  }
                }
              ]
            }
            """);

        return (
            new FileInfo(rawAudioPath),
            new FileInfo(treatedAudioPath),
            new FileInfo(hydratePath));
    }

    private static AudioBuffer CreateMonoBuffer(int sampleRate, int length)
    {
        return new AudioBuffer(channels: 1, sampleRate: sampleRate, length: length);
    }
}
