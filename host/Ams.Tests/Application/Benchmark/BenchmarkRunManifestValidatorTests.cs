using System.Security.Cryptography;
using System.Text;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Runs;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkRunManifestValidatorTests
{
    private readonly BenchmarkRunManifestValidator _validator = new();

    [Fact]
    public void Validate_ManifestWithMismatchedRollups_ReturnsFieldExpectedActualDiagnostics()
    {
        var outcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "completed",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 100,
                    analysisRuntimeMs: 20,
                    mismatchCount: 2,
                    missingSpeechSec: 0.3,
                    extraSpeechSec: 0.10,
                    rawQcFlags: 1,
                    treatedQcFlags: 0)),
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-02",
                state: RunState.Failed,
                summary: "failed",
                failure: new RunFailure(RunFailureKind.Execution, "hydrate failed", "hydrate"),
                stageSummaries: [],
                artifacts: [],
                metrics: CreatePartialMetrics(
                    chapterId: "chapter-02",
                    pipelineRuntimeMs: 80,
                    analysisRuntimeMs: 15,
                    mismatchCount: 1,
                    missingSpeechSec: 0.2,
                    extraSpeechSec: 0.05,
                    rawQcFlags: 0,
                    treatedQcFlags: 1))
        };

        var malformedAggregate = new BenchmarkRunMetricsAggregate(
            chapterStates: new BenchmarkRunChapterStateCounts(
                pending: 0,
                running: 0,
                failed: 0,
                completed: 0),
            metricsStates: new BenchmarkMetricsStateCounts(
                notRun: 0,
                completed: 0,
                partial: 0,
                failed: 0),
            totalPipelineRuntimeMs: 0,
            totalAnalysisRuntimeMs: 0,
            totalMismatchCount: 0,
            totalMissingSpeechSec: 0,
            totalExtraSpeechSec: 0,
            totalQcFlags: 0);

        var manifest = new BenchmarkRunManifest(
            runId: "validator-manifest-001",
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 18, 0, 0, TimeSpan.Zero),
            completedAtUtc: new DateTimeOffset(2026, 4, 15, 18, 1, 0, TimeSpan.Zero),
            deterministic: false,
            phase: BenchmarkRunPhase.Failed,
            state: RunState.Failed,
            determinism: null,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            chapterSet: ["chapter-01", "chapter-02"],
            chapterSetFingerprint: "wrong-fingerprint",
            chapterOutcomes: outcomes,
            failure: new RunFailure(RunFailureKind.Execution, "chapter failure", "running"),
            aggregateMetrics: malformedAggregate);

        var validation = _validator.Validate(manifest);

        Assert.False(validation.IsValid);
        Assert.NotEmpty(validation.Diagnostics);

        Assert.Contains(validation.Diagnostics, diagnostic =>
            string.Equals(diagnostic.Field, "chapterSetFingerprint", StringComparison.Ordinal));

        var runtimeDiagnostic = Assert.Single(
            validation.Diagnostics,
            diagnostic => string.Equals(diagnostic.Field, "aggregateMetrics.totalPipelineRuntimeMs", StringComparison.Ordinal));

        Assert.Equal("180", runtimeDiagnostic.Expected);
        Assert.Equal("0", runtimeDiagnostic.Actual);

        Assert.All(validation.Diagnostics, diagnostic =>
        {
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Field));
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Expected));
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Actual));
        });
    }

    [Fact]
    public void Validate_ManifestWithMatchingRollups_ReturnsValid()
    {
        var outcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "completed",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 90,
                    analysisRuntimeMs: 10,
                    mismatchCount: 1,
                    missingSpeechSec: 0.10,
                    extraSpeechSec: 0.05,
                    rawQcFlags: 1,
                    treatedQcFlags: 1))
        };

        var chapterSet = new[] { "chapter-01" };
        var manifest = new BenchmarkRunManifest(
            runId: "validator-manifest-002",
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 18, 10, 0, TimeSpan.Zero),
            completedAtUtc: new DateTimeOffset(2026, 4, 15, 18, 11, 0, TimeSpan.Zero),
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            chapterSet: chapterSet,
            chapterSetFingerprint: ComputeFingerprint(chapterSet),
            chapterOutcomes: outcomes);

        var validation = _validator.Validate(manifest);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Diagnostics);
    }

    [Fact]
    public void Validate_RunResultContractPath_UsesSameAggregateDiagnostics()
    {
        var outcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "completed",
                stageSummaries: [],
                artifacts: [],
                metrics: CreateCompletedMetrics(
                    pipelineRuntimeMs: 60,
                    analysisRuntimeMs: 12,
                    mismatchCount: 1,
                    missingSpeechSec: 0.05,
                    extraSpeechSec: 0.02,
                    rawQcFlags: 0,
                    treatedQcFlags: 1))
        };

        var malformedAggregate = new BenchmarkRunMetricsAggregate(
            chapterStates: new BenchmarkRunChapterStateCounts(
                pending: 1,
                running: 0,
                failed: 0,
                completed: 0),
            metricsStates: new BenchmarkMetricsStateCounts(
                notRun: 1,
                completed: 0,
                partial: 0,
                failed: 0),
            totalPipelineRuntimeMs: 0,
            totalAnalysisRuntimeMs: 0,
            totalMismatchCount: 0,
            totalMissingSpeechSec: 0,
            totalExtraSpeechSec: 0,
            totalQcFlags: 0);

        var result = new BenchmarkRunResult(
            runId: "validator-result-001",
            moduleId: ModuleIds.BenchmarkRun,
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: ComputeFingerprint(["chapter-01"]),
            chapterOutcomes: outcomes,
            manifestFile: new FileInfo(Path.Combine(Path.GetTempPath(), $"validator-result-{Guid.NewGuid():N}.json")),
            invalidRunFile: null,
            aggregateMetrics: malformedAggregate);

        var validation = _validator.Validate(result);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Diagnostics, diagnostic =>
            string.Equals(diagnostic.Field, "aggregateMetrics.chapterStates.pending", StringComparison.Ordinal));
        Assert.Contains(validation.Diagnostics, diagnostic =>
            string.Equals(diagnostic.Field, "aggregateMetrics.totalPipelineRuntimeMs", StringComparison.Ordinal));
    }

    private static BenchmarkChapterMetrics CreateCompletedMetrics(
        long pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags)
    {
        return new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: analysisRuntimeMs),
            quality: CreateQualityMetrics(
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            metricsFailure: null);
    }

    private static BenchmarkChapterMetrics CreatePartialMetrics(
        string chapterId,
        long pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags)
    {
        return new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Partial,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: analysisRuntimeMs),
            quality: CreateQualityMetrics(
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Timeout,
                message: "QC analyzer timeout.",
                operation: "qc-analysis",
                chapterId: chapterId,
                resourcePath: $"{chapterId}.treated.wav"));
    }

    private static BenchmarkChapterQualityMetrics CreateQualityMetrics(
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags)
    {
        return new BenchmarkChapterQualityMetrics(
            integrity: new BenchmarkAudioIntegrityMetrics(
                durationSec: 10,
                rawSpeechSec: 8,
                treatedSpeechSec: 7.7,
                missingSpeechSec: missingSpeechSec,
                extraSpeechSec: extraSpeechSec,
                mismatchCount: mismatchCount),
            rawQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.7,
                titleBodyGapSec: 1.1,
                tailSilenceSec: 2.2,
                flagCount: rawQcFlags,
                flags: Enumerable.Range(1, rawQcFlags).Select(index => $"RAW_{index}").ToArray()),
            treatedQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.8,
                titleBodyGapSec: 1.2,
                tailSilenceSec: 2.0,
                flagCount: treatedQcFlags,
                flags: Enumerable.Range(1, treatedQcFlags).Select(index => $"TRT_{index}").ToArray()),
            rawLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 10,
                samplePeakDbFs: -1.2,
                truePeakDbFs: -1.0,
                overallRmsDbFs: -21,
                integratedLufs: -18.1),
            treatedLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 9.9,
                samplePeakDbFs: -1.1,
                truePeakDbFs: -0.9,
                overallRmsDbFs: -20.7,
                integratedLufs: -17.8));
    }

    private static string ComputeFingerprint(IReadOnlyList<string> chapterSet)
    {
        var canonical = string.Join("\n", chapterSet.Select(chapter => chapter.Trim()));
        var bytes = Encoding.UTF8.GetBytes(canonical);

        using var hash = SHA256.Create();
        var digest = hash.ComputeHash(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
