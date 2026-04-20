using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;

namespace Ams.Tests.Application.Benchmark;

public sealed class BenchmarkRunServiceTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public async Task ExecuteAsync_DeterministicRejection_WritesInvalidArtifact_AndSkipsPipelineDispatch()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;
        var service = CreateService(
            runPipelineAsync: (_, _, _) =>
            {
                pipelineCalls++;
                return Task.FromResult(CreatePipelineResult(root, "chapter-01"));
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateInvalidDeterminismContract(
                BenchmarkDeterminismReasonCode.MissingExplicitModel)),
            runIdFactory: () => "reject-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Invalid, result.Phase);
        Assert.Equal(RunState.Completed, result.State);
        Assert.Null(result.ManifestFile);
        Assert.NotNull(result.InvalidRunFile);
        Assert.True(result.InvalidRunFile!.Exists);
        Assert.Equal(0, pipelineCalls);

        var persisted = BenchmarkInvalidRunArtifact.Deserialize(await File.ReadAllTextAsync(result.InvalidRunFile.FullName));
        Assert.Equal(BenchmarkDeterminismVerdict.Invalid, persisted.Determinism.Verdict);
        Assert.Contains(BenchmarkDeterminismReasonCode.MissingExplicitModel, persisted.Determinism.ReasonCodes);
        Assert.Equal(["chapter-01"], persisted.ChapterSet);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptedDeterministicRun_WritesManifestWithRequiredMetadata()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;
        var service = CreateService(
            runPipelineAsync: (_, options, _) =>
            {
                pipelineCalls++;
                return Task.FromResult(CreatePipelineResult(root, options.ChapterId));
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            runIdFactory: () => "accept-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Completed, result.Phase);
        Assert.Equal(RunState.Completed, result.State);
        Assert.NotNull(result.ManifestFile);
        Assert.True(result.ManifestFile!.Exists);
        Assert.Equal(2, pipelineCalls);

        var manifest = BenchmarkRunManifest.Deserialize(await File.ReadAllTextAsync(result.ManifestFile.FullName));
        Assert.True(manifest.Deterministic);
        Assert.NotNull(manifest.Determinism);
        Assert.Equal(BenchmarkModelProvenanceKind.PinnedPath, manifest.Determinism!.ModelProvenance.SourceKind);
        Assert.Equal(BenchmarkReadinessState.Ready, manifest.Determinism.Ffmpeg.State);
        Assert.Equal(BenchmarkReadinessState.Ready, manifest.Determinism.Mfa.State);
        Assert.True(manifest.CachePolicy.ForcePipelineRebuild);
        Assert.True(manifest.ChunkPolicy.DisableChunkPlan);
        Assert.Equal(new[] { "chapter-01", "chapter-02" }, manifest.ChapterSet);
        Assert.Equal(2, manifest.ChapterOutcomes.Count);
        Assert.All(
            manifest.ChapterOutcomes.SelectMany(outcome => outcome.Artifacts),
            artifact => Assert.False(Path.IsPathRooted(artifact.Path)));
    }

    [Fact]
    public async Task ExecuteAsync_ChapterTimeout_PersistsFailedManifestWithPartialOutcomes()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var service = CreateService(
            runPipelineAsync: (_, options, _) =>
            {
                if (string.Equals(options.ChapterId, "chapter-02", StringComparison.OrdinalIgnoreCase))
                {
                    throw new TimeoutException("pipeline timed out");
                }

                return Task.FromResult(CreatePipelineResult(root, options.ChapterId));
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            runIdFactory: () => "timeout-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Timeout, result.Failure!.Kind);
        Assert.Equal("running", result.Failure.Stage);
        Assert.NotNull(result.ManifestFile);

        var manifest = BenchmarkRunManifest.Deserialize(await File.ReadAllTextAsync(result.ManifestFile!.FullName));
        Assert.Equal(BenchmarkRunPhase.Failed, manifest.Phase);
        Assert.Equal(RunState.Failed, manifest.State);
        Assert.Equal(2, manifest.ChapterOutcomes.Count);
        Assert.Equal("chapter-01", manifest.ChapterOutcomes[0].ChapterId);
        Assert.Equal(RunState.Completed, manifest.ChapterOutcomes[0].State);
        Assert.Equal("chapter-02", manifest.ChapterOutcomes[1].ChapterId);
        Assert.Equal(RunState.Failed, manifest.ChapterOutcomes[1].State);
        Assert.Equal(RunFailureKind.Timeout, manifest.ChapterOutcomes[1].Failure!.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_MalformedGatePayload_FailsClosedBeforePipelineDispatch()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;
        var service = CreateService(
            runPipelineAsync: (_, _, _) =>
            {
                pipelineCalls++;
                return Task.FromResult(CreatePipelineResult(root, "chapter-01"));
            },
            evaluateGateAsync: (_, _) => Task.FromResult<BenchmarkDeterminismContract>(null!),
            runIdFactory: () => "gate-malformed-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Validation, result.Failure!.Kind);
        Assert.Equal("gated", result.Failure.Stage);
        Assert.Equal(0, pipelineCalls);
        Assert.Null(result.ManifestFile);
        Assert.Null(result.InvalidRunFile);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateChapterIds_ReturnsValidationFailureAndSkipsGateAndPipeline()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;
        var gateCalls = 0;

        var service = CreateService(
            runPipelineAsync: (_, _, _) =>
            {
                pipelineCalls++;
                return Task.FromResult(CreatePipelineResult(root, "chapter-01"));
            },
            evaluateGateAsync: (_, _) =>
            {
                gateCalls++;
                return Task.FromResult(CreateValidDeterminismContract());
            },
            runIdFactory: () => "duplicate-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01-alt.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Validation, result.Failure!.Kind);
        Assert.Equal("gated", result.Failure.Stage);
        Assert.Equal(0, gateCalls);
        Assert.Equal(0, pipelineCalls);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidArtifactWriteFailure_ReturnsExplicitFailure()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var artifactStore = new BenchmarkRunArtifactStore(
            writeAllText: (_, _) => throw new IOException("disk full"));

        var service = CreateService(
            runPipelineAsync: (_, _, _) => Task.FromResult(CreatePipelineResult(root, "chapter-01")),
            evaluateGateAsync: (_, _) => Task.FromResult(CreateInvalidDeterminismContract(
                BenchmarkDeterminismReasonCode.MissingModelFile)),
            artifactStore: artifactStore,
            runIdFactory: () => "artifact-fail-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Dependency, result.Failure!.Kind);
        Assert.Equal("invalid", result.Failure.Stage);
        Assert.Contains("artifact", result.Failure.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.ManifestFile);
        Assert.Null(result.InvalidRunFile);
    }

    [Fact]
    public async Task ExecuteAsync_ZeroSuccessfulChapters_StillPersistsManifestArtifact()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var service = CreateService(
            runPipelineAsync: (_, options, _) =>
            {
                var failure = new RunFailure(RunFailureKind.Execution, "hydrate crashed", "hydrate");
                var failedResult = CreatePipelineResult(root, options.ChapterId, RunState.Failed, failure);
                throw new PipelineRunException(failedResult);
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            runIdFactory: () => "zero-success-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-99", new FileInfo(Path.Combine(root, "audio", "chapter-99.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.ManifestFile);
        Assert.True(result.ManifestFile!.Exists);

        var manifest = BenchmarkRunManifest.Deserialize(await File.ReadAllTextAsync(result.ManifestFile.FullName));
        Assert.Single(manifest.ChapterOutcomes);
        Assert.All(manifest.ChapterOutcomes, outcome => Assert.Equal(RunState.Failed, outcome.State));
    }

    [Fact]
    public async Task ExecuteAsync_FirstChapterFailure_SurfacesPendingOutcomesForRemainingChapters()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;

        var service = CreateService(
            runPipelineAsync: (_, options, _) =>
            {
                pipelineCalls++;

                if (string.Equals(options.ChapterId, "chapter-01", StringComparison.OrdinalIgnoreCase))
                {
                    var failure = new RunFailure(RunFailureKind.Execution, "hydrate crashed", "hydrate");
                    var failedResult = CreatePipelineResult(root, options.ChapterId, RunState.Failed, failure);
                    throw new PipelineRunException(failedResult);
                }

                return Task.FromResult(CreatePipelineResult(root, options.ChapterId));
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            collectMetricsAsync: (metricsRequest, _) => Task.FromResult(
                new BenchmarkChapterMetrics(
                    BenchmarkMetricsStatus.Failed,
                    runtime: new BenchmarkChapterRuntimeMetrics(metricsRequest.PipelineRuntimeMs, analysisRuntimeMs: 9),
                    quality: null,
                    metricsFailure: new BenchmarkMetricsFailure(
                        RunFailureKind.Execution,
                        "metrics unavailable",
                        "metrics-collection",
                        metricsRequest.ChapterId))),
            runIdFactory: () => "pending-remaining-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav"))),
                new BenchmarkRunChapterRequest("chapter-03", new FileInfo(Path.Combine(root, "audio", "chapter-03.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.Equal(1, pipelineCalls);
        Assert.Equal(3, result.ChapterOutcomes.Count);

        Assert.Collection(
            result.ChapterOutcomes,
            first =>
            {
                Assert.Equal("chapter-01", first.ChapterId);
                Assert.Equal(RunState.Failed, first.State);
                Assert.Equal(BenchmarkMetricsStatus.Failed, first.Metrics.Status);
            },
            second =>
            {
                Assert.Equal("chapter-02", second.ChapterId);
                Assert.Equal(RunState.Pending, second.State);
                Assert.Equal(BenchmarkMetricsStatus.NotRun, second.Metrics.Status);
            },
            third =>
            {
                Assert.Equal("chapter-03", third.ChapterId);
                Assert.Equal(RunState.Pending, third.State);
                Assert.Equal(BenchmarkMetricsStatus.NotRun, third.Metrics.Status);
            });

        Assert.Equal(2, result.AggregateMetrics.ChapterStates.Pending);
        Assert.Equal(1, result.AggregateMetrics.ChapterStates.Failed);
        Assert.Equal(2, result.AggregateMetrics.MetricsStates.NotRun);
    }

    [Fact]
    public async Task ExecuteAsync_RunningChapterState_IsPreservedAndRemainingChaptersMarkedPending()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var service = CreateService(
            runPipelineAsync: (_, options, _) => Task.FromResult(
                string.Equals(options.ChapterId, "chapter-01", StringComparison.OrdinalIgnoreCase)
                    ? CreateRunningPipelineResult(root, options.ChapterId)
                    : CreatePipelineResult(root, options.ChapterId)),
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            collectMetricsAsync: (metricsRequest, _) => Task.FromResult(
                CreateCompletedMetrics(metricsRequest.PipelineRuntimeMs, analysisRuntimeMs: 12)),
            runIdFactory: () => "running-state-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Contains("non-terminal state", result.Failure!.Message, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(2, result.ChapterOutcomes.Count);
        Assert.Equal(RunState.Running, result.ChapterOutcomes[0].State);
        Assert.Equal(BenchmarkMetricsStatus.Completed, result.ChapterOutcomes[0].Metrics.Status);
        Assert.Equal(RunState.Pending, result.ChapterOutcomes[1].State);
        Assert.Equal(BenchmarkMetricsStatus.NotRun, result.ChapterOutcomes[1].Metrics.Status);

        Assert.Equal(1, result.AggregateMetrics.ChapterStates.Running);
        Assert.Equal(1, result.AggregateMetrics.ChapterStates.Pending);
    }

    [Fact]
    public async Task ExecuteAsync_MetricsCollectorTimeout_AttachesMetricsFailureAndKeepsManifestDurable()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var service = CreateService(
            runPipelineAsync: (_, options, _) => Task.FromResult(CreatePipelineResult(root, options.ChapterId)),
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            collectMetricsAsync: (_, _) => throw new TimeoutException("metrics timeout"),
            runIdFactory: () => "metrics-timeout-001");

        var request = CreateRequest(
            root,
            deterministic: false,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Completed, result.Phase);
        Assert.Equal(RunState.Completed, result.State);
        Assert.NotNull(result.ManifestFile);

        var outcome = Assert.Single(result.ChapterOutcomes);
        Assert.Equal(BenchmarkMetricsStatus.Failed, outcome.Metrics.Status);
        Assert.NotNull(outcome.Metrics.MetricsFailure);
        Assert.Equal(RunFailureKind.Timeout, outcome.Metrics.MetricsFailure!.Kind);
        Assert.Equal("metrics-collection", outcome.Metrics.MetricsFailure.Operation);

        Assert.Equal(1, result.AggregateMetrics.MetricsStates.Failed);

        var manifest = BenchmarkRunManifest.Deserialize(await File.ReadAllTextAsync(result.ManifestFile!.FullName));
        Assert.Single(manifest.ChapterOutcomes);
        Assert.Equal(BenchmarkMetricsStatus.Failed, manifest.ChapterOutcomes[0].Metrics.Status);
        Assert.Equal(1, manifest.AggregateMetrics.MetricsStates.Failed);
    }

    [Fact]
    public async Task ExecuteAsync_NullPipelineResult_FailsValidationAndLeavesRemainingChaptersPending()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var pipelineCalls = 0;

        var service = CreateService(
            runPipelineAsync: (_, _, _) =>
            {
                pipelineCalls++;
                return Task.FromResult<PipelineChapterResult>(null!);
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            runIdFactory: () => "null-result-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Failed, result.Phase);
        Assert.Equal(RunState.Failed, result.State);
        Assert.NotNull(result.Failure);
        Assert.Equal(RunFailureKind.Validation, result.Failure!.Kind);
        Assert.Equal(1, pipelineCalls);

        Assert.Equal(2, result.ChapterOutcomes.Count);
        Assert.Equal(RunState.Failed, result.ChapterOutcomes[0].State);
        Assert.Equal(RunState.Pending, result.ChapterOutcomes[1].State);
    }

    [Fact]
    public async Task ExecuteAsync_RuntimeSplitAndRollups_AreDerivedFromChapterMetrics()
    {
        var root = CreateTempDirectory();
        using var workspace = new TestWorkspace(root);

        var service = CreateService(
            runPipelineAsync: async (_, options, cancellationToken) =>
            {
                await Task.Delay(20, cancellationToken).ConfigureAwait(false);
                return CreatePipelineResult(root, options.ChapterId);
            },
            evaluateGateAsync: (_, _) => Task.FromResult(CreateValidDeterminismContract()),
            collectMetricsAsync: (metricsRequest, _) =>
            {
                var isFirstChapter = string.Equals(metricsRequest.ChapterId, "chapter-01", StringComparison.OrdinalIgnoreCase);
                var analysisRuntimeMs = isFirstChapter ? 17L : 23L;
                var mismatchCount = isFirstChapter ? 2 : 1;
                var rawFlags = isFirstChapter ? 1 : 0;
                var treatedFlags = isFirstChapter ? 0 : 2;

                return Task.FromResult(CreateCompletedMetrics(
                    metricsRequest.PipelineRuntimeMs,
                    analysisRuntimeMs,
                    mismatchCount,
                    rawFlags,
                    treatedFlags));
            },
            runIdFactory: () => "runtime-rollup-001");

        var request = CreateRequest(
            root,
            deterministic: true,
            chapters:
            [
                new BenchmarkRunChapterRequest("chapter-01", new FileInfo(Path.Combine(root, "audio", "chapter-01.wav"))),
                new BenchmarkRunChapterRequest("chapter-02", new FileInfo(Path.Combine(root, "audio", "chapter-02.wav")))
            ]);

        var result = await service.ExecuteAsync(workspace, request);

        Assert.Equal(BenchmarkRunPhase.Completed, result.Phase);
        Assert.Equal(RunState.Completed, result.State);
        Assert.Equal(2, result.ChapterOutcomes.Count);

        Assert.All(
            result.ChapterOutcomes,
            outcome =>
            {
                Assert.Equal(BenchmarkMetricsStatus.Completed, outcome.Metrics.Status);
                Assert.True(outcome.Metrics.Runtime.PipelineRuntimeMs.HasValue);
                Assert.True(outcome.Metrics.Runtime.PipelineRuntimeMs!.Value > 0);
            });

        var expectedPipelineRuntime = result.ChapterOutcomes.Sum(outcome => outcome.Metrics.Runtime.PipelineRuntimeMs ?? 0L);
        var expectedAnalysisRuntime = result.ChapterOutcomes.Sum(outcome => outcome.Metrics.Runtime.AnalysisRuntimeMs ?? 0L);

        Assert.Equal(expectedPipelineRuntime, result.AggregateMetrics.TotalPipelineRuntimeMs);
        Assert.Equal(expectedAnalysisRuntime, result.AggregateMetrics.TotalAnalysisRuntimeMs);
        Assert.Equal(3, result.AggregateMetrics.TotalMismatchCount);
        Assert.Equal(3, result.AggregateMetrics.TotalQcFlags);
        Assert.Equal(2, result.AggregateMetrics.ChapterStates.Completed);
        Assert.Equal(2, result.AggregateMetrics.MetricsStates.Completed);
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
                // Best-effort cleanup for test temp directories.
            }
        }
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ams-benchmark-run-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);
        return path;
    }

    private static BenchmarkRunService CreateService(
        Func<IWorkspace, PipelineRunOptions, CancellationToken, Task<PipelineChapterResult>> runPipelineAsync,
        Func<BenchmarkDeterminismGateRequest, CancellationToken, Task<BenchmarkDeterminismContract>> evaluateGateAsync,
        Func<BenchmarkMetricsCollectionRequest, CancellationToken, Task<BenchmarkChapterMetrics>>? collectMetricsAsync = null,
        BenchmarkRunArtifactStore? artifactStore = null,
        Func<string>? runIdFactory = null)
    {
        return new BenchmarkRunService(
            runPipelineAsync,
            evaluateGateAsync,
            artifactStore ?? new BenchmarkRunArtifactStore(),
            collectMetricsAsync: collectMetricsAsync,
            utcNow: () => new DateTimeOffset(2026, 4, 15, 21, 30, 0, TimeSpan.Zero),
            runIdFactory: runIdFactory);
    }

    private static BenchmarkRunRequest CreateRequest(
        string root,
        bool deterministic,
        IReadOnlyList<BenchmarkRunChapterRequest> chapters)
    {
        var outputRoot = new DirectoryInfo(Path.Combine(root, "benchmark-runs"));

        var options = new PipelineRunOptions
        {
            BookFile = new FileInfo(Path.Combine(root, "book.md")),
            BookIndexFile = new FileInfo(Path.Combine(root, "book-index.json")),
            AudioFile = new FileInfo(Path.Combine(root, "placeholder.wav")),
            ChapterId = "placeholder",
            Force = true,
            ForceIndex = false,
            DisableChunkPlan = true,
            DisableChunkedMfa = true
        };

        return new BenchmarkRunRequest(
            deterministic,
            requestedModel: Path.Combine(root, "models", "ggml-large-v3.bin"),
            options,
            chapters,
            outputRoot,
            moduleId: ModuleIds.BenchmarkRun);
    }

    private static BenchmarkDeterminismContract CreateValidDeterminismContract()
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 0, 0, TimeSpan.Zero),
            verdict: BenchmarkDeterminismVerdict.Valid,
            reasonCodes: [],
            modelProvenance: new BenchmarkModelProvenance(
                BenchmarkReadinessState.Ready,
                BenchmarkModelProvenanceKind.PinnedPath,
                requestedModel: "/models/ggml-large-v3.bin",
                normalizedModelPath: "/models/ggml-large-v3.bin",
                isDeterministic: true,
                summary: "Pinned model path is valid.",
                guidance: "Proceed."),
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA ready."),
            cachePolicy: new BenchmarkCachePolicy(forcePipelineRebuild: true, forceBookIndexRebuild: false),
            chunkPolicy: new BenchmarkChunkPolicy(
                disableChunkPlan: true,
                disableChunkedMfa: true,
                usedDefaultPlanningPolicy: false,
                silenceThresholdDb: -42,
                minSilenceDurationMs: 450,
                minChunkDurationSec: 8,
                maxChunkDurationSec: 25),
            guidance:
            [
                "Deterministic gate passed."
            ]);
    }

    private static BenchmarkDeterminismContract CreateInvalidDeterminismContract(
        params BenchmarkDeterminismReasonCode[] reasonCodes)
    {
        return new BenchmarkDeterminismContract(
            evaluatedAtUtc: new DateTimeOffset(2026, 4, 15, 21, 0, 0, TimeSpan.Zero),
            verdict: BenchmarkDeterminismVerdict.Invalid,
            reasonCodes,
            modelProvenance: new BenchmarkModelProvenance(
                BenchmarkReadinessState.Warning,
                BenchmarkModelProvenanceKind.MissingModelFile,
                requestedModel: "/models/missing.bin",
                normalizedModelPath: "/models/missing.bin",
                isDeterministic: false,
                summary: "Pinned model path missing.",
                guidance: "Fix model path."),
            ffmpeg: new BenchmarkDependencyReadiness(
                dependency: "FFmpeg",
                state: BenchmarkReadinessState.Ready,
                summary: "FFmpeg ready."),
            mfa: new BenchmarkDependencyReadiness(
                dependency: "MFA",
                state: BenchmarkReadinessState.Ready,
                summary: "MFA ready."),
            cachePolicy: BenchmarkCachePolicy.Default,
            chunkPolicy: BenchmarkChunkPolicy.Default,
            guidance:
            [
                "Deterministic gate rejected run."
            ]);
    }

    private static PipelineChapterResult CreatePipelineResult(
        string root,
        string chapterId,
        RunState state = RunState.Completed,
        RunFailure? failure = null)
    {
        var chapterRoot = Path.Combine(root, chapterId);
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));
        var asrFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.asr.json"));
        var anchorFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.anchors.json"));
        var transcriptFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.tx.json"));
        var hydrateFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.hydrate.json"));
        var textGridFile = new FileInfo(Path.Combine(chapterRoot, "alignment", "mfa", $"{chapterId}.TextGrid"));
        var treatedFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.treated.wav"));

        var artifacts = new[]
        {
            new RunArtifact("hydrate", RunArtifactKind.Output, hydrateFile.FullName, exists: state == RunState.Completed),
            new RunArtifact("text-grid", RunArtifactKind.Output, textGridFile.FullName, exists: state == RunState.Completed)
        };

        var stageResults = state == RunState.Completed
            ? new[]
            {
                new PipelineStageResult(PipelineStage.Hydrate, RunState.Completed, executed: true, message: "Hydrate complete")
            }
            : new[]
            {
                new PipelineStageResult(
                    PipelineStage.Hydrate,
                    RunState.Failed,
                    executed: false,
                    message: failure?.Message ?? "Hydrate failed",
                    failure: failure ?? new RunFailure(RunFailureKind.Execution, "Hydrate failed", "hydrate"))
            };

        return new PipelineChapterResult(
            chapterId,
            bookIndexBuilt: false,
            asrRan: state == RunState.Completed,
            anchorsRan: state == RunState.Completed,
            transcriptRan: state == RunState.Completed,
            hydrateRan: state == RunState.Completed,
            mfaRan: false,
            bookIndexFile,
            asrFile,
            anchorFile,
            transcriptFile,
            hydrateFile,
            textGridFile,
            treatedFile,
            state: state,
            failure: failure,
            artifacts: artifacts,
            stageResults: stageResults);
    }

    private static PipelineChapterResult CreateRunningPipelineResult(string root, string chapterId)
    {
        var chapterRoot = Path.Combine(root, chapterId);
        var bookIndexFile = new FileInfo(Path.Combine(root, "book-index.json"));
        var asrFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.asr.json"));
        var anchorFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.anchors.json"));
        var transcriptFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.tx.json"));
        var hydrateFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.align.hydrate.json"));
        var textGridFile = new FileInfo(Path.Combine(chapterRoot, "alignment", "mfa", $"{chapterId}.TextGrid"));
        var treatedFile = new FileInfo(Path.Combine(chapterRoot, $"{chapterId}.treated.wav"));

        var artifacts = new[]
        {
            new RunArtifact("hydrate", RunArtifactKind.Output, hydrateFile.FullName, exists: true),
            new RunArtifact("treated-audio", RunArtifactKind.Output, treatedFile.FullName, exists: true)
        };

        var stageResults = new[]
        {
            new PipelineStageResult(PipelineStage.Hydrate, RunState.Completed, executed: true, message: "Hydrate complete"),
            new PipelineStageResult(PipelineStage.Mfa, RunState.Running, executed: true, message: "Promptless ASR recovery requested")
        };

        return new PipelineChapterResult(
            chapterId,
            bookIndexBuilt: false,
            asrRan: true,
            anchorsRan: true,
            transcriptRan: true,
            hydrateRan: true,
            mfaRan: false,
            bookIndexFile,
            asrFile,
            anchorFile,
            transcriptFile,
            hydrateFile,
            textGridFile,
            treatedFile,
            state: RunState.Running,
            failure: null,
            artifacts: artifacts,
            stageResults: stageResults);
    }

    private static BenchmarkChapterMetrics CreateCompletedMetrics(
        long? pipelineRuntimeMs,
        long analysisRuntimeMs,
        int mismatchCount = 1,
        int rawQcFlags = 0,
        int treatedQcFlags = 0)
    {
        var quality = new BenchmarkChapterQualityMetrics(
            integrity: new BenchmarkAudioIntegrityMetrics(
                durationSec: 12.5,
                rawSpeechSec: 10.3,
                treatedSpeechSec: 9.9,
                missingSpeechSec: 0.2,
                extraSpeechSec: 0.1,
                mismatchCount: mismatchCount),
            rawQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.7,
                titleBodyGapSec: 1.1,
                tailSilenceSec: 2.3,
                flagCount: rawQcFlags,
                flags: Enumerable.Range(1, rawQcFlags).Select(index => $"RAW_FLAG_{index}").ToArray()),
            treatedQc: new BenchmarkQcMetrics(
                headSilenceSec: 0.8,
                titleBodyGapSec: 1.2,
                tailSilenceSec: 2.1,
                flagCount: treatedQcFlags,
                flags: Enumerable.Range(1, treatedQcFlags).Select(index => $"TREATED_FLAG_{index}").ToArray()),
            rawLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.5,
                samplePeakDbFs: -1.1,
                truePeakDbFs: -0.9,
                overallRmsDbFs: -21.2,
                integratedLufs: -18.0),
            treatedLoudness: new BenchmarkLoudnessMetrics(
                durationSec: 12.4,
                samplePeakDbFs: -1.0,
                truePeakDbFs: -0.8,
                overallRmsDbFs: -20.8,
                integratedLufs: -17.6));

        return new BenchmarkChapterMetrics(
            BenchmarkMetricsStatus.Completed,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: analysisRuntimeMs),
            quality: quality,
            metricsFailure: null);
    }

    private sealed class TestWorkspace : IWorkspace, IDisposable
    {
        private readonly BookManager _manager;

        public TestWorkspace(string rootPath)
        {
            RootPath = rootPath;
            _manager = new BookManager(
                new[] { new BookDescriptor("benchmark-test", rootPath, Array.Empty<ChapterDescriptor>()) },
                FileArtifactResolver.Instance);
        }

        public string RootPath { get; }

        public BookContext Book => _manager.Current;

        public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var chapterId = string.IsNullOrWhiteSpace(options.ChapterId) ? "chapter-01" : options.ChapterId;
            var chapterDirectory = options.ChapterDirectory ?? new DirectoryInfo(Path.Combine(RootPath, chapterId));
            var bookIndexFile = options.BookIndexFile ?? new FileInfo(Path.Combine(RootPath, "book-index.json"));

            return Book.Chapters.CreateContext(
                bookIndexFile,
                options.AsrFile,
                options.TranscriptFile,
                options.HydrateFile,
                options.AudioFile,
                chapterDirectory,
                chapterId,
                options.ReloadBookIndex);
        }

        public void Dispose()
        {
            _manager.DeallocateAll();
        }
    }
}
