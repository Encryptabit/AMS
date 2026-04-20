using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Runs;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkCompareServiceTests : IDisposable
{
    private readonly List<string> _tempDirectories = [];

    [Fact]
    public async Task ExecuteAsync_CompatibleArtifacts_EmitsPerMetricVerdictsAndPersistsCompareArtifact()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var baselineManifest = CreateManifest(
            runId: "baseline-001",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 200,
            analysisRuntimeMs: 100,
            mismatchCount: 5,
            missingSpeechSec: 1.00,
            extraSpeechSec: 0.50,
            rawQcFlags: 2,
            treatedQcFlags: 2,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var candidateManifest = CreateManifest(
            runId: "candidate-001",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 150,
            analysisRuntimeMs: 80,
            mismatchCount: 7,
            missingSpeechSec: 1.05,
            extraSpeechSec: 0.40,
            rawQcFlags: 2,
            treatedQcFlags: 2,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var baselinePath = Path.Combine(directory, "baseline.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate.manifest.json");

        await File.WriteAllTextAsync(baselinePath, BenchmarkRunManifest.Serialize(baselineManifest));
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-compatible"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Compatible, result.Compatibility.Status);
        Assert.Empty(result.Compatibility.Reasons);
        Assert.NotNull(result.ArtifactFile);
        Assert.True(result.ArtifactFile!.Exists);

        Assert.Equal(6, result.MetricVerdicts.Count);

        var pipelineVerdict = Assert.Single(result.MetricVerdicts, verdict => verdict.Metric == "totalPipelineRuntimeMs");
        Assert.Equal(BenchmarkCompareVerdict.Improved, pipelineVerdict.Verdict);
        Assert.Equal(-50, pipelineVerdict.Delta, 3);

        var mismatchVerdict = Assert.Single(result.MetricVerdicts, verdict => verdict.Metric == "totalMismatchCount");
        Assert.Equal(BenchmarkCompareVerdict.Regressed, mismatchVerdict.Verdict);

        var missingSpeechVerdict = Assert.Single(result.MetricVerdicts, verdict => verdict.Metric == "totalMissingSpeechSec");
        Assert.Equal(BenchmarkCompareVerdict.NoChange, missingSpeechVerdict.Verdict);
        Assert.Equal(0.05, missingSpeechVerdict.Delta, 3);

        Assert.All(result.MetricVerdicts, verdict =>
        {
            Assert.NotNull(verdict.Threshold);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Threshold.Rationale));
            Assert.False(string.IsNullOrWhiteSpace(verdict.Rationale));
        });

        var persisted = await File.ReadAllTextAsync(result.ArtifactFile.FullName);
        var artifact = BenchmarkCompareArtifact.Deserialize(persisted);

        Assert.Equal("cmp-compatible", artifact.CompareId);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Compatible, artifact.Compatibility.Status);
        Assert.Equal(6, artifact.MetricVerdicts.Count);
    }

    [Fact]
    public async Task ExecuteAsync_MetricsContamination_ForcesInvalidVerdicts()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var baselineManifest = CreateManifest(
            runId: "baseline-contaminated",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 200,
            analysisRuntimeMs: 100,
            mismatchCount: 3,
            missingSpeechSec: 0.8,
            extraSpeechSec: 0.4,
            rawQcFlags: 1,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Partial);

        var candidateManifest = CreateManifest(
            runId: "candidate-contaminated",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 140,
            analysisRuntimeMs: 70,
            mismatchCount: 1,
            missingSpeechSec: 0.3,
            extraSpeechSec: 0.1,
            rawQcFlags: 0,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Partial);

        var baselinePath = Path.Combine(directory, "baseline-contaminated.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate-contaminated.manifest.json");

        await File.WriteAllTextAsync(baselinePath, BenchmarkRunManifest.Serialize(baselineManifest));
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-contaminated"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Compatible, result.Compatibility.Status);
        Assert.NotEmpty(result.MetricVerdicts);
        Assert.All(result.MetricVerdicts, verdict =>
        {
            Assert.Equal(BenchmarkCompareVerdict.Invalid, verdict.Verdict);
            Assert.Contains("contamination", verdict.Rationale, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task ExecuteAsync_MetricsPolicyDrift_ReturnsIncompatibleWithoutMetricVerdicts()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var baselineManifest = CreateManifest(
            runId: "baseline-policy",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 100,
            analysisRuntimeMs: 30,
            mismatchCount: 1,
            missingSpeechSec: 0.2,
            extraSpeechSec: 0.1,
            rawQcFlags: 0,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Completed,
            metricsPolicy: new BenchmarkMetricsPolicySnapshot(integrityWindowMs: 30));

        var candidateManifest = CreateManifest(
            runId: "candidate-policy",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 90,
            analysisRuntimeMs: 25,
            mismatchCount: 1,
            missingSpeechSec: 0.2,
            extraSpeechSec: 0.1,
            rawQcFlags: 0,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Completed,
            metricsPolicy: new BenchmarkMetricsPolicySnapshot(integrityWindowMs: 45));

        var baselinePath = Path.Combine(directory, "baseline-policy.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate-policy.manifest.json");

        await File.WriteAllTextAsync(baselinePath, BenchmarkRunManifest.Serialize(baselineManifest));
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-policy-drift"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Incompatible, result.Compatibility.Status);
        Assert.Empty(result.MetricVerdicts);
        Assert.Contains(result.Compatibility.Reasons, reason =>
            reason.Code == BenchmarkCompareCompatibilityReasonCode.MetricsPolicyMismatch
            && string.Equals(reason.Field, "metricsPolicy.integrityWindowMs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_MalformedManifestPayload_ReturnsMalformedCompatibilityReason()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        const string malformedBaselinePayload = """
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

        var candidateManifest = CreateManifest(
            runId: "candidate-valid",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 110,
            analysisRuntimeMs: 40,
            mismatchCount: 2,
            missingSpeechSec: 0.25,
            extraSpeechSec: 0.15,
            rawQcFlags: 0,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var baselinePath = Path.Combine(directory, "baseline-malformed.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate-valid.manifest.json");

        await File.WriteAllTextAsync(baselinePath, malformedBaselinePayload);
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-malformed"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Malformed, result.Compatibility.Status);
        Assert.Empty(result.MetricVerdicts);
        Assert.Contains(result.Compatibility.Reasons, reason =>
            reason.Code == BenchmarkCompareCompatibilityReasonCode.BaselineManifestMalformed);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidRunArtifactInput_ReturnsArtifactTypeMismatch()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var baselineInvalid = new BenchmarkInvalidRunArtifact(
            runId: "baseline-invalid",
            rejectedAtUtc: new DateTimeOffset(2026, 4, 15, 22, 0, 0, TimeSpan.Zero),
            deterministic: true,
            phase: BenchmarkRunPhase.Invalid,
            determinism: CreateInvalidDeterminismContract(),
            chapterSet: ["chapter-01"],
            chapterSetFingerprint: ComputeFingerprint(["chapter-01"]));

        var candidateManifest = CreateManifest(
            runId: "candidate-deterministic",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 120,
            analysisRuntimeMs: 55,
            mismatchCount: 3,
            missingSpeechSec: 0.3,
            extraSpeechSec: 0.12,
            rawQcFlags: 1,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Completed,
            deterministic: true,
            determinism: CreateValidDeterminismContract());

        var baselinePath = Path.Combine(directory, "baseline.invalid-run.json");
        var candidatePath = Path.Combine(directory, "candidate.manifest.json");

        await File.WriteAllTextAsync(baselinePath, BenchmarkInvalidRunArtifact.Serialize(baselineInvalid));
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-type-mismatch"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Incompatible, result.Compatibility.Status);
        Assert.Empty(result.MetricVerdicts);
        Assert.Contains(result.Compatibility.Reasons, reason =>
            reason.Code == BenchmarkCompareCompatibilityReasonCode.ArtifactTypeMismatch);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateAndBlankChapterIdentifiers_FailClosedAsIncompatible()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var baselineManifest = CreateManifest(
            runId: "baseline-duplicate",
            chapterSet: ["chapter-01", "chapter-01"],
            pipelineRuntimeMs: 100,
            analysisRuntimeMs: 50,
            mismatchCount: 2,
            missingSpeechSec: 0.20,
            extraSpeechSec: 0.10,
            rawQcFlags: 1,
            treatedQcFlags: 1,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var candidateManifest = CreateManifest(
            runId: "candidate-duplicate",
            chapterSet: ["chapter-01", "chapter-01"],
            pipelineRuntimeMs: 90,
            analysisRuntimeMs: 45,
            mismatchCount: 1,
            missingSpeechSec: 0.10,
            extraSpeechSec: 0.05,
            rawQcFlags: 1,
            treatedQcFlags: 1,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var baselineNode = JsonNode.Parse(BenchmarkRunManifest.Serialize(baselineManifest))!.AsObject();
        baselineNode["chapterSet"] = new JsonArray("chapter-01", "   ", "chapter-01");

        var baselinePath = Path.Combine(directory, "baseline-duplicate.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate-duplicate.manifest.json");

        await File.WriteAllTextAsync(
            baselinePath,
            baselineNode.ToJsonString(BenchmarkDeterminismJson.SerializerOptions));
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var service = CreateService();
        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-duplicate-chapter-set"));

        Assert.Null(result.Failure);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Incompatible, result.Compatibility.Status);
        Assert.Empty(result.MetricVerdicts);

        Assert.Contains(result.Compatibility.Reasons, reason =>
            reason.Code == BenchmarkCompareCompatibilityReasonCode.BaselineManifestInvalid
            && string.Equals(reason.Field, "chapterSet", StringComparison.Ordinal)
            && reason.Actual is not null
            && reason.Actual.Contains("blank", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_ArtifactReadFailure_ReturnsFailureAndNoMetricVerdicts()
    {
        var directory = CreateTempDirectory();
        var outputRoot = new DirectoryInfo(directory);

        var candidateManifest = CreateManifest(
            runId: "candidate-read-failure",
            chapterSet: ["chapter-01"],
            pipelineRuntimeMs: 90,
            analysisRuntimeMs: 30,
            mismatchCount: 1,
            missingSpeechSec: 0.1,
            extraSpeechSec: 0.05,
            rawQcFlags: 0,
            treatedQcFlags: 0,
            metricsStatus: BenchmarkMetricsStatus.Completed);

        var baselinePath = Path.Combine(directory, "baseline-read-failure.manifest.json");
        var candidatePath = Path.Combine(directory, "candidate-read-failure.manifest.json");

        await File.WriteAllTextAsync(baselinePath, "{}");
        await File.WriteAllTextAsync(candidatePath, BenchmarkRunManifest.Serialize(candidateManifest));

        var artifactStore = new BenchmarkRunArtifactStore();
        var validator = new BenchmarkRunManifestValidator();

        var service = new BenchmarkCompareService(
            artifactStore,
            validator,
            readArtifactAsync: (artifact, _) =>
            {
                if (artifact.FullName == baselinePath)
                {
                    throw new IOException("Read stream disconnected.");
                }

                return File.ReadAllTextAsync(artifact.FullName);
            },
            utcNow: () => new DateTimeOffset(2026, 4, 15, 23, 0, 0, TimeSpan.Zero),
            compareIdFactory: () => "cmp-read-failure");

        var result = await service.ExecuteAsync(new BenchmarkCompareRequest(
            baselineArtifact: new FileInfo(baselinePath),
            candidateArtifact: new FileInfo(candidatePath),
            outputRoot: outputRoot,
            compareId: "cmp-read-failure"));

        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Dependency, result.Failure!.Kind);
        Assert.Equal(BenchmarkCompareCompatibilityStatus.Malformed, result.Compatibility.Status);
        Assert.Empty(result.MetricVerdicts);
        Assert.Contains(result.Compatibility.Reasons, reason =>
            reason.Code == BenchmarkCompareCompatibilityReasonCode.BaselineManifestMalformed);
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
                // best effort cleanup
            }
        }
    }

    private static BenchmarkCompareService CreateService()
    {
        return new BenchmarkCompareService(
            artifactStore: new BenchmarkRunArtifactStore(),
            manifestValidator: new BenchmarkRunManifestValidator(),
            utcNow: () => new DateTimeOffset(2026, 4, 15, 22, 30, 0, TimeSpan.Zero),
            compareIdFactory: () => "cmp-generated");
    }

    private static BenchmarkRunManifest CreateManifest(
        string runId,
        IReadOnlyList<string> chapterSet,
        long pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount,
        double missingSpeechSec,
        double extraSpeechSec,
        int rawQcFlags,
        int treatedQcFlags,
        BenchmarkMetricsStatus metricsStatus,
        BenchmarkMetricsPolicySnapshot? metricsPolicy = null,
        bool deterministic = false,
        BenchmarkDeterminismContract? determinism = null)
    {
        var chapterId = chapterSet.FirstOrDefault() ?? "chapter-01";

        var metrics = metricsStatus switch
        {
            BenchmarkMetricsStatus.Completed => CreateCompletedMetrics(
                chapterId,
                pipelineRuntimeMs,
                analysisRuntimeMs,
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            BenchmarkMetricsStatus.Partial => CreatePartialMetrics(
                chapterId,
                pipelineRuntimeMs,
                analysisRuntimeMs,
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            _ => throw new InvalidOperationException($"Unsupported test metrics status: {metricsStatus}")
        };

        var outcome = new BenchmarkRunChapterOutcome(
            chapterId: chapterId,
            state: RunState.Completed,
            summary: "completed",
            stageSummaries: [],
            artifacts: [],
            metrics: metrics);

        return new BenchmarkRunManifest(
            runId: runId,
            startedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 0, 0, TimeSpan.Zero),
            completedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 1, 0, TimeSpan.Zero),
            deterministic: deterministic,
            phase: BenchmarkRunPhase.Completed,
            state: RunState.Completed,
            determinism: deterministic ? determinism ?? CreateValidDeterminismContract() : null,
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            chapterSet: chapterSet,
            chapterSetFingerprint: ComputeFingerprint(chapterSet),
            chapterOutcomes: [outcome],
            metricsPolicy: metricsPolicy ?? BenchmarkMetricsPolicySnapshot.Default);
    }

    private static BenchmarkChapterMetrics CreateCompletedMetrics(
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
            status: BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs, analysisRuntimeMs),
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
            runtime: new BenchmarkChapterRuntimeMetrics(pipelineRuntimeMs, analysisRuntimeMs),
            quality: CreateQualityMetrics(
                mismatchCount,
                missingSpeechSec,
                extraSpeechSec,
                rawQcFlags,
                treatedQcFlags),
            metricsFailure: new BenchmarkMetricsFailure(
                kind: RunFailureKind.Timeout,
                message: "metrics timed out",
                operation: "metrics-collection",
                chapterId: chapterId,
                resourcePath: $"{chapterId}.wav"));
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
                durationSec: 12,
                rawSpeechSec: 10,
                treatedSpeechSec: 9.5,
                missingSpeechSec: missingSpeechSec,
                extraSpeechSec: extraSpeechSec,
                mismatchCount: mismatchCount),
            rawQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.7,
                titleBodyGapSec: 1.0,
                tailSilenceSec: 2.3,
                flagCount: rawQcFlags,
                flags: Enumerable.Range(1, rawQcFlags).Select(index => $"RAW_{index}").ToArray()),
            treatedQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.8,
                titleBodyGapSec: 1.2,
                tailSilenceSec: 2.1,
                flagCount: treatedQcFlags,
                flags: Enumerable.Range(1, treatedQcFlags).Select(index => $"TRT_{index}").ToArray()),
            rawLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12,
                samplePeakDbFs: -1.2,
                truePeakDbFs: -1.0,
                overallRmsDbFs: -21.2,
                integratedLufs: -18.1),
            treatedLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12,
                samplePeakDbFs: -1.0,
                truePeakDbFs: -0.8,
                overallRmsDbFs: -20.9,
                integratedLufs: -17.9));
    }

    private static BenchmarkDeterminismContract CreateValidDeterminismContract()
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 0, 0, TimeSpan.Zero),
            verdict: BenchmarkDeterminismVerdict.Valid,
            reasonCodes: [],
            modelProvenance: new BenchmarkModelProvenance(
                state: BenchmarkReadinessState.Ready,
                sourceKind: BenchmarkModelProvenanceKind.PinnedPath,
                requestedModel: "large-v3",
                normalizedModelPath: "/models/large-v3.bin",
                isDeterministic: true,
                summary: "Pinned model",
                guidance: "Model path pinned."),
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "ffmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "ready"),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "mfa",
                state: BenchmarkReadinessState.Ready,
                summary: "ready"),
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            guidance: ["ready"]);
    }

    private static BenchmarkDeterminismContract CreateInvalidDeterminismContract()
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 5, 0, TimeSpan.Zero),
            verdict: BenchmarkDeterminismVerdict.Invalid,
            reasonCodes: [BenchmarkDeterminismReasonCode.AliasOnlyModel],
            modelProvenance: new BenchmarkModelProvenance(
                state: BenchmarkReadinessState.Warning,
                sourceKind: BenchmarkModelProvenanceKind.AliasOnly,
                requestedModel: "large-v3",
                normalizedModelPath: null,
                isDeterministic: false,
                summary: "Alias model",
                guidance: "Use pinned path."),
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "ffmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "ready"),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "mfa",
                state: BenchmarkReadinessState.Ready,
                summary: "ready"),
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            guidance: ["invalid"]);
    }

    private static string ComputeFingerprint(IReadOnlyList<string> chapterSet)
    {
        var canonical = string.Join("\n", chapterSet.Select(chapter => chapter.Trim()));
        var bytes = Encoding.UTF8.GetBytes(canonical);

        using var hash = SHA256.Create();
        var digest = hash.ComputeHash(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ams-benchmark-compare-service", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }
}
