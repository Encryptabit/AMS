using System.Text.Json;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Services.Alignment;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkRunContractTests
{
    [Fact]
    public void BenchmarkDeterminismContract_SerializesRequiredFieldsAndStableReasonCodes()
    {
        var contract = new BenchmarkDeterminismContract(
            evaluatedAtUtc: new DateTimeOffset(2026, 4, 10, 8, 30, 0, TimeSpan.Zero),
            verdict: BenchmarkDeterminismVerdict.Invalid,
            reasonCodes:
            [
                BenchmarkDeterminismReasonCode.AliasOnlyModel,
                BenchmarkDeterminismReasonCode.FfmpegNotReady
            ],
            modelProvenance: new BenchmarkModelProvenance(
                state: BenchmarkReadinessState.Warning,
                sourceKind: BenchmarkModelProvenanceKind.AliasOnly,
                requestedModel: "large-v3",
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "Alias-only model input.",
                guidance: "Provide pinned model path."),
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Failed,
                summary: "FFmpeg probe failed.",
                detail: "Binaries missing.",
                exitCode: 2),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA ready."),
            cachePolicy: new BenchmarkCachePolicy(forcePipelineRebuild: false, forceBookIndexRebuild: true),
            chunkPolicy: new BenchmarkChunkPolicy(
                disableChunkPlan: false,
                disableChunkedMfa: true,
                usedDefaultPlanningPolicy: false,
                silenceThresholdDb: -42.5,
                minSilenceDurationMs: 450,
                minChunkDurationSec: 9,
                maxChunkDurationSec: 21),
            guidance:
            [
                "Alias-only models are not deterministic.",
                "FFmpeg probe failed."
            ]);

        var json = BenchmarkDeterminismContract.Serialize(contract);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("evaluatedAtUtc", out _));
        Assert.True(root.TryGetProperty("verdict", out _));
        Assert.True(root.TryGetProperty("reasonCodes", out var reasonCodesNode));
        Assert.True(root.TryGetProperty("modelProvenance", out _));
        Assert.True(root.TryGetProperty("ffmpeg", out _));
        Assert.True(root.TryGetProperty("mfa", out _));
        Assert.True(root.TryGetProperty("cachePolicy", out var cachePolicyNode));
        Assert.True(root.TryGetProperty("chunkPolicy", out var chunkPolicyNode));

        var reasonCodes = reasonCodesNode
            .EnumerateArray()
            .Select(element => element.GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal(["AliasOnlyModel", "FfmpegNotReady"], reasonCodes);
        Assert.True(cachePolicyNode.GetProperty("forceBookIndexRebuild").GetBoolean());
        Assert.Equal(-42.5, chunkPolicyNode.GetProperty("silenceThresholdDb").GetDouble(), 3);
        Assert.Equal(21, chunkPolicyNode.GetProperty("maxChunkDurationSec").GetDouble(), 3);

        var roundTrip = BenchmarkDeterminismContract.Deserialize(json);
        Assert.Equal(contract.Verdict, roundTrip.Verdict);
        Assert.Equal(contract.ReasonCodes, roundTrip.ReasonCodes);
        Assert.Equal(contract.CachePolicy.ForceBookIndexRebuild, roundTrip.CachePolicy.ForceBookIndexRebuild);
        Assert.Equal(contract.ChunkPolicy.DisableChunkedMfa, roundTrip.ChunkPolicy.DisableChunkedMfa);
    }

    [Fact]
    public void BenchmarkDeterminismContract_DeserializeRejectsMissingRequiredFields()
    {
        const string malformedJson = """
                                     {
                                       "evaluatedAtUtc": "2026-04-10T08:30:00Z",
                                       "verdict": "Invalid",
                                       "reasonCodes": ["AliasOnlyModel"],
                                       "ffmpeg": {
                                         "dependency": "FFmpeg",
                                         "state": "Ready",
                                         "summary": "FFmpeg ready."
                                       },
                                       "mfa": {
                                         "dependency": "MFA",
                                         "state": "Ready",
                                         "summary": "MFA ready."
                                       },
                                       "cachePolicy": {
                                         "forcePipelineRebuild": false,
                                         "forceBookIndexRebuild": false
                                       },
                                       "chunkPolicy": {
                                         "disableChunkPlan": false,
                                         "disableChunkedMfa": false,
                                         "usedDefaultPlanningPolicy": true,
                                         "silenceThresholdDb": -40,
                                         "minSilenceDurationMs": 300,
                                         "minChunkDurationSec": 15,
                                         "maxChunkDurationSec": 29.5
                                       }
                                     }
                                     """;

        var exception = Assert.ThrowsAny<Exception>(() => BenchmarkDeterminismContract.Deserialize(malformedJson));

        Assert.Contains("model", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BenchmarkDeterminismGateRequest_FromPipelineOptions_CapturesCacheAndChunkPolicy()
    {
        var options = new PipelineRunOptions
        {
            BookFile = new FileInfo(Path.Combine(Path.GetTempPath(), "book.md")),
            BookIndexFile = new FileInfo(Path.Combine(Path.GetTempPath(), "book-index.json")),
            AudioFile = new FileInfo(Path.Combine(Path.GetTempPath(), "chapter-01.wav")),
            ChapterId = "chapter-01",
            Force = true,
            ForceIndex = false,
            DisableChunkPlan = true,
            DisableChunkedMfa = true,
            ChunkPlanningPolicy = new ChunkPlanningPolicy
            {
                SilenceThresholdDb = -48,
                MinSilenceDuration = TimeSpan.FromMilliseconds(650),
                MinChunkDuration = TimeSpan.FromSeconds(8),
                MaxChunkDuration = TimeSpan.FromSeconds(22)
            }
        };

        var request = BenchmarkDeterminismGateRequest.FromPipelineOptions(
            requestedModel: "/tmp/models/ggml-large-v3.bin",
            options: options);

        Assert.Equal("/tmp/models/ggml-large-v3.bin", request.RequestedModel);
        Assert.True(request.CachePolicy.ForcePipelineRebuild);
        Assert.False(request.CachePolicy.ForceBookIndexRebuild);
        Assert.False(request.CachePolicy.AllowsCachedPipelineArtifacts);
        Assert.True(request.CachePolicy.AllowsCachedBookIndex);

        Assert.True(request.ChunkPolicy.DisableChunkPlan);
        Assert.True(request.ChunkPolicy.DisableChunkedMfa);
        Assert.False(request.ChunkPolicy.UsedDefaultPlanningPolicy);
        Assert.Equal(-48, request.ChunkPolicy.SilenceThresholdDb, 3);
        Assert.Equal(650, request.ChunkPolicy.MinSilenceDurationMs, 3);
        Assert.Equal(8, request.ChunkPolicy.MinChunkDurationSec, 3);
        Assert.Equal(22, request.ChunkPolicy.MaxChunkDurationSec, 3);
    }

    [Fact]
    public void BenchmarkRunChapterOutcome_DefaultsMetricsNodeToNotRun()
    {
        var outcome = new BenchmarkRunChapterOutcome(
            chapterId: "chapter-01",
            state: RunState.Completed,
            summary: "Pipeline chapter completed.");

        Assert.NotNull(outcome.Metrics);
        Assert.Equal(BenchmarkMetricsStatus.NotRun, outcome.Metrics.Status);
        Assert.Null(outcome.Metrics.MetricsFailure);
    }

    [Fact]
    public void BenchmarkRunManifest_SerializesMetricsPolicyChapterNodesAndAggregateCounters()
    {
        var quality = new BenchmarkChapterQualityMetrics(
            integrity: new BenchmarkAudioIntegrityMetrics(
                durationSec: 12.5,
                rawSpeechSec: 10.1,
                treatedSpeechSec: 9.6,
                missingSpeechSec: 0.4,
                extraSpeechSec: 0.2,
                mismatchCount: 2),
            rawQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.7,
                titleBodyGapSec: 1.1,
                tailSilenceSec: 2.4,
                flagCount: 1,
                flags: ["HEAD_SILENCE_SHORT"]),
            treatedQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.8,
                titleBodyGapSec: 1.2,
                tailSilenceSec: 2.2,
                flagCount: 0,
                flags: []),
            rawLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.5,
                samplePeakDbFs: -1.2,
                truePeakDbFs: -0.9,
                overallRmsDbFs: -21.4,
                integratedLufs: -18.3),
            treatedLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.4,
                samplePeakDbFs: -1.0,
                truePeakDbFs: -0.8,
                overallRmsDbFs: -21.0,
                integratedLufs: -17.9));

        var chapterMetrics = new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Partial,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: 14_200,
                analysisRuntimeMs: 680),
            quality: quality,
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Timeout,
                message: "QC analyzer timeout.",
                operation: "qc-analysis",
                chapterId: "chapter-01",
                resourcePath: "chapter-01.treated.wav"));

        var manifest = new BenchmarkRunManifest(
            runId: "run-contract-001",
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 19, 0, 0, TimeSpan.Zero),
            completedAtUtc: new DateTimeOffset(2026, 4, 15, 19, 1, 0, TimeSpan.Zero),
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "contract-fingerprint",
            chapterOutcomes:
            [
                new BenchmarkRunChapterOutcome(
                    chapterId: "chapter-01",
                    state: RunState.Completed,
                    summary: "Completed with partial metrics.",
                    stageSummaries: [],
                    artifacts: [],
                    metrics: chapterMetrics)
            ],
            metricsPolicy: new BenchmarkMetricsPolicySnapshot(
                enabled: true,
                integrityWindowMs: 40,
                integrityStepMs: 20,
                integrityMinMismatchMs: 75,
                integrityMergeGapMs: 55,
                integrityMinDeltaDb: 18,
                qcNoiseFloorDb: -38,
                qcMinSilenceDurationSec: 0.4,
                qcThresholds: new BenchmarkQcThresholdSnapshot(
                    minHeadSilence: 0.6,
                    maxHeadSilence: 1.1,
                    minTailSilence: 2.1,
                    maxTailSilence: 4.8,
                    minTitleBodyGap: 0.9,
                    maxTitleBodyGap: 2.3),
                loudnessWindowSec: 0.75));

        var json = BenchmarkRunManifest.Serialize(manifest);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("metricsPolicy", out var metricsPolicyNode));
        Assert.True(root.TryGetProperty("aggregateMetrics", out var aggregateNode));

        var chapterNode = root
            .GetProperty("chapterOutcomes")
            .EnumerateArray()
            .Single();

        Assert.Equal("Partial", chapterNode.GetProperty("metrics").GetProperty("status").GetString());
        Assert.Equal(
            "qc-analysis",
            chapterNode.GetProperty("metrics").GetProperty("metricsFailure").GetProperty("operation").GetString());

        Assert.Equal(40, metricsPolicyNode.GetProperty("integrityWindowMs").GetDouble(), 3);
        Assert.Equal(1, aggregateNode.GetProperty("metricsStates").GetProperty("partial").GetInt32());
        Assert.Equal(2, aggregateNode.GetProperty("totalMismatchCount").GetInt32());

        var roundTrip = BenchmarkRunManifest.Deserialize(json);
        Assert.Equal(BenchmarkMetricsStatus.Partial, roundTrip.ChapterOutcomes[0].Metrics.Status);
        Assert.Equal(1, roundTrip.AggregateMetrics.MetricsStates.Partial);
        Assert.Equal(2, roundTrip.AggregateMetrics.TotalMismatchCount);
    }

    [Fact]
    public void BenchmarkRunManifest_DeserializeLegacyAudioActivityWithoutDurationUs_MapsFromDurationMs()
    {
        var activity = new BenchmarkAudioProcessingActivity(
            function: "EncodeWavToStream",
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 19, 0, 0, TimeSpan.Zero),
            durationMs: 7,
            succeeded: true,
            failureKind: null,
            detail: "sample",
            durationUs: 7425);

        var metrics = new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Failed,
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs: 100, analysisRuntimeMs: 12),
            quality: null,
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Validation,
                message: "treated audio missing",
                operation: "audio-decode",
                chapterId: "chapter-01",
                resourcePath: "chapter-01.treated.wav"),
            audioProcessingActivities: [activity]);

        var manifest = new BenchmarkRunManifest(
            runId: "run-legacy-activity-001",
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 19, 0, 0, TimeSpan.Zero),
            completedAtUtc: new DateTimeOffset(2026, 4, 15, 19, 1, 0, TimeSpan.Zero),
            deterministic: false,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: null,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: "legacy-fingerprint",
            chapterOutcomes:
            [
                new BenchmarkRunChapterOutcome(
                    chapterId: "chapter-01",
                    state: RunState.Completed,
                    summary: "Completed with metrics failure.",
                    stageSummaries: [],
                    artifacts: [],
                    metrics: metrics)
            ]);

        var serialized = BenchmarkRunManifest.Serialize(manifest);
        Assert.Contains("\"durationUs\"", serialized, StringComparison.Ordinal);

        var legacyPayload = serialized
            .Replace("\"durationUs\":7425,", string.Empty, StringComparison.Ordinal)
            .Replace("\"durationUs\": 7425,", string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("\"durationUs\"", legacyPayload, StringComparison.Ordinal);

        var legacyRoundTrip = BenchmarkRunManifest.Deserialize(legacyPayload);

        var roundTripActivity = Assert.Single(legacyRoundTrip.ChapterOutcomes[0].Metrics.AudioProcessingActivities);
        Assert.Equal(7, roundTripActivity.DurationMs);
        Assert.Equal(7000, roundTripActivity.DurationUs);
    }

    [Fact]
    public void BenchmarkRunMetricsAggregate_FromChapterOutcomes_ComputesMixedStateRollups()
    {
        var completedMetrics = new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs: 100, analysisRuntimeMs: 20),
            quality: new BenchmarkChapterQualityMetrics(
                integrity: new BenchmarkAudioIntegrityMetrics(
                    durationSec: 12.0,
                    rawSpeechSec: 9.8,
                    treatedSpeechSec: 9.4,
                    missingSpeechSec: 0.3,
                    extraSpeechSec: 0.1,
                    mismatchCount: 2),
                rawQc: new BenchmarkQcMetrics(0.7, 1.1, 2.3, flagCount: 1, flags: ["RAW_A"]),
                treatedQc: new BenchmarkQcMetrics(0.8, 1.2, 2.2, flagCount: 0, flags: []),
                rawLoudness: new BenchmarkLoudnessMetrics(12.0, -1.1, -0.9, -21.1, -18.1),
                treatedLoudness: new BenchmarkLoudnessMetrics(11.9, -1.0, -0.8, -20.8, -17.8)),
            metricsFailure: null);

        var partialMetrics = new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Partial,
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs: 80, analysisRuntimeMs: 15),
            quality: new BenchmarkChapterQualityMetrics(
                integrity: new BenchmarkAudioIntegrityMetrics(
                    durationSec: 11.5,
                    rawSpeechSec: 9.2,
                    treatedSpeechSec: 8.9,
                    missingSpeechSec: 0.2,
                    extraSpeechSec: 0.05,
                    mismatchCount: 1),
                rawQc: new BenchmarkQcMetrics(0.6, 1.0, 2.1, flagCount: 0, flags: []),
                treatedQc: new BenchmarkQcMetrics(0.7, 1.1, 2.0, flagCount: 1, flags: ["TREATED_A"]),
                rawLoudness: new BenchmarkLoudnessMetrics(11.5, -1.2, -1.0, -21.4, -18.4),
                treatedLoudness: new BenchmarkLoudnessMetrics(11.4, -1.1, -0.9, -21.0, -18.0)),
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Timeout,
                message: "QC analyzer timeout.",
                operation: "qc-analysis",
                chapterId: "chapter-02",
                resourcePath: "chapter-02.treated.wav"));

        var runningCompletedMetrics = new BenchmarkChapterMetrics(
            status: BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs: 50, analysisRuntimeMs: 10),
            quality: new BenchmarkChapterQualityMetrics(
                integrity: new BenchmarkAudioIntegrityMetrics(
                    durationSec: 10.0,
                    rawSpeechSec: 8.7,
                    treatedSpeechSec: 8.5,
                    missingSpeechSec: 0.0,
                    extraSpeechSec: 0.0,
                    mismatchCount: 0),
                rawQc: new BenchmarkQcMetrics(0.5, 1.0, 2.0, flagCount: 0, flags: []),
                treatedQc: new BenchmarkQcMetrics(0.6, 1.1, 2.1, flagCount: 2, flags: ["TREATED_B", "TREATED_C"]),
                rawLoudness: new BenchmarkLoudnessMetrics(10.0, -1.3, -1.1, -21.8, -18.8),
                treatedLoudness: new BenchmarkLoudnessMetrics(9.9, -1.2, -1.0, -21.2, -18.2)),
            metricsFailure: null);

        var chapterOutcomes = new[]
        {
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-01",
                state: RunState.Completed,
                summary: "completed",
                stageSummaries: [],
                artifacts: [],
                metrics: completedMetrics),
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-02",
                state: RunState.Failed,
                summary: "failed",
                failure: new RunFailure(RunFailureKind.Execution, "hydrate failed", "hydrate"),
                stageSummaries: [],
                artifacts: [],
                metrics: partialMetrics),
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-03",
                state: RunState.Pending,
                summary: "pending",
                stageSummaries: [],
                artifacts: [],
                metrics: BenchmarkChapterMetrics.NotRun),
            new BenchmarkRunChapterOutcome(
                chapterId: "chapter-04",
                state: RunState.Running,
                summary: "running",
                stageSummaries: [],
                artifacts: [],
                metrics: runningCompletedMetrics)
        };

        var aggregate = BenchmarkRunMetricsAggregate.FromChapterOutcomes(chapterOutcomes);

        Assert.Equal(1, aggregate.ChapterStates.Pending);
        Assert.Equal(1, aggregate.ChapterStates.Running);
        Assert.Equal(1, aggregate.ChapterStates.Failed);
        Assert.Equal(1, aggregate.ChapterStates.Completed);
        Assert.Equal(4, aggregate.ChapterStates.Total);

        Assert.Equal(1, aggregate.MetricsStates.NotRun);
        Assert.Equal(2, aggregate.MetricsStates.Completed);
        Assert.Equal(1, aggregate.MetricsStates.Partial);
        Assert.Equal(0, aggregate.MetricsStates.Failed);
        Assert.Equal(4, aggregate.MetricsStates.Total);

        Assert.Equal(230, aggregate.TotalPipelineRuntimeMs);
        Assert.Equal(45, aggregate.TotalAnalysisRuntimeMs);
        Assert.Equal(3, aggregate.TotalMismatchCount);
        Assert.Equal(0.5, aggregate.TotalMissingSpeechSec, 3);
        Assert.Equal(0.15, aggregate.TotalExtraSpeechSec, 3);
        Assert.Equal(4, aggregate.TotalQcFlags);
    }

    [Fact]
    public void BenchmarkRunRequest_PersistsExplicitMetricsPolicySnapshot()
    {
        var options = new PipelineRunOptions
        {
            BookFile = new FileInfo(Path.Combine(Path.GetTempPath(), "book.md")),
            BookIndexFile = new FileInfo(Path.Combine(Path.GetTempPath(), "book-index.json")),
            AudioFile = new FileInfo(Path.Combine(Path.GetTempPath(), "chapter-01.wav")),
            ChapterId = "chapter-01"
        };

        var metricsPolicy = new BenchmarkMetricsPolicySnapshot(
            enabled: true,
            integrityWindowMs: 25,
            integrityStepMs: 10,
            integrityMinMismatchMs: 55,
            integrityMergeGapMs: 35,
            integrityMinDeltaDb: 19,
            qcNoiseFloorDb: -36,
            qcMinSilenceDurationSec: 0.25,
            qcThresholds: new BenchmarkQcThresholdSnapshot(
                minHeadSilence: 0.5,
                maxHeadSilence: 1.2,
                minTailSilence: 2.0,
                maxTailSilence: 4.5,
                minTitleBodyGap: 0.8,
                maxTitleBodyGap: 2.1),
            loudnessWindowSec: 0.6);

        var request = new BenchmarkRunRequest(
            deterministic: false,
            requestedModel: null,
            pipelineOptions: options,
            chapters:
            [
                new BenchmarkRunChapterRequest(
                    chapterId: "chapter-01",
                    audioFile: new FileInfo(Path.Combine(Path.GetTempPath(), "chapter-01.wav")))
            ],
            outputRoot: new DirectoryInfo(Path.Combine(Path.GetTempPath(), "benchmark-contract-tests")),
            metricsPolicy: metricsPolicy);

        Assert.Equal(25, request.MetricsPolicy.IntegrityWindowMs, 3);
        Assert.Equal(0.6, request.MetricsPolicy.LoudnessWindowSec, 3);
        Assert.Equal(0.8, request.MetricsPolicy.QcThresholds.MinTitleBodyGap, 3);
    }

    [Fact]
    public void BenchmarkRunManifest_DeserializeRejectsMissingAggregateMetricsNode()
    {
        const string missingAggregateJson = """
                                          {
                                            "runId": "run-missing-aggregate",
                                            "startedAtUtc": "2026-04-15T20:00:00Z",
                                            "completedAtUtc": "2026-04-15T20:01:00Z",
                                            "deterministic": false,
                                            "phase": "Completed",
                                            "state": "Completed",
                                            "determinism": null,
                                            "cachePolicy": {
                                              "forcePipelineRebuild": false,
                                              "forceBookIndexRebuild": false
                                            },
                                            "chunkPolicy": {
                                              "disableChunkPlan": false,
                                              "disableChunkedMfa": false,
                                              "usedDefaultPlanningPolicy": true,
                                              "silenceThresholdDb": -40,
                                              "minSilenceDurationMs": 300,
                                              "minChunkDurationSec": 15,
                                              "maxChunkDurationSec": 29.5
                                            },
                                            "chapterSet": ["chapter-01"],
                                            "chapterSetFingerprint": "fingerprint-001",
                                            "chapterOutcomes": []
                                          }
                                          """;

        var exception = Assert.Throws<InvalidDataException>(() => BenchmarkRunManifest.Deserialize(missingAggregateJson));

        Assert.Contains("aggregateMetrics", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
