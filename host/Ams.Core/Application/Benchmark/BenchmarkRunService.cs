using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Runtime.Workspace;
using Ams.Core.Services;

namespace Ams.Core.Application.Benchmark;

public sealed class BenchmarkRunService
{
    private const string GatedStage = "gated";
    private const string InvalidStage = "invalid";
    private const string RunningStage = "running";
    private const string FailedStage = "failed";

    private readonly Func<IWorkspace, PipelineRunOptions, CancellationToken, Task<PipelineChapterResult>> _runPipelineChapterAsync;
    private readonly Func<BenchmarkDeterminismGateRequest, CancellationToken, Task<BenchmarkDeterminismContract>> _evaluateDeterminismAsync;
    private readonly BenchmarkRunArtifactStore _artifactStore;
    private readonly Func<BenchmarkMetricsCollectionRequest, CancellationToken, Task<BenchmarkChapterMetrics>> _collectChapterMetricsAsync;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly Func<string> _runIdFactory;

    public BenchmarkRunService(
        PipelineService pipelineService,
        BenchmarkDeterminismGate determinismGate,
        BenchmarkRunArtifactStore artifactStore,
        IBenchmarkMetricsCollector metricsCollector,
        Func<DateTimeOffset>? utcNow = null,
        Func<string>? runIdFactory = null)
        : this(
            runPipelineChapterAsync: (pipelineService ?? throw new ArgumentNullException(nameof(pipelineService))).RunChapterAsync,
            evaluateDeterminismAsync: (determinismGate ?? throw new ArgumentNullException(nameof(determinismGate))).EvaluateAsync,
            artifactStore,
            collectMetricsAsync: (metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector))).CollectAsync,
            utcNow,
            runIdFactory)
    {
    }

    internal BenchmarkRunService(
        Func<IWorkspace, PipelineRunOptions, CancellationToken, Task<PipelineChapterResult>> runPipelineChapterAsync,
        Func<BenchmarkDeterminismGateRequest, CancellationToken, Task<BenchmarkDeterminismContract>> evaluateDeterminismAsync,
        BenchmarkRunArtifactStore artifactStore,
        Func<BenchmarkMetricsCollectionRequest, CancellationToken, Task<BenchmarkChapterMetrics>>? collectMetricsAsync = null,
        Func<DateTimeOffset>? utcNow = null,
        Func<string>? runIdFactory = null)
    {
        _runPipelineChapterAsync = runPipelineChapterAsync ?? throw new ArgumentNullException(nameof(runPipelineChapterAsync));
        _evaluateDeterminismAsync = evaluateDeterminismAsync ?? throw new ArgumentNullException(nameof(evaluateDeterminismAsync));
        _artifactStore = artifactStore ?? throw new ArgumentNullException(nameof(artifactStore));
        _collectChapterMetricsAsync = collectMetricsAsync ?? ((_, _) => Task.FromResult(BenchmarkChapterMetrics.NotRun));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _runIdFactory = runIdFactory
                        ?? GenerateDefaultRunId;
    }

    public async Task<BenchmarkRunResult> ExecuteAsync(
        IWorkspace workspace,
        BenchmarkRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(request);

        var runId = ResolveRunId(request);
        var startedAtUtc = _utcNow();
        var phaseTransitions = new List<BenchmarkRunPhaseTransition>();
        var chapterOutcomes = new List<BenchmarkRunChapterOutcome>();
        var chapterSet = request.Chapters.Select(chapter => chapter.ChapterId).ToArray();
        var chapterSetFingerprint = ComputeChapterSetFingerprint(chapterSet);

        void Transition(BenchmarkRunPhase phase, string message)
            => phaseTransitions.Add(new BenchmarkRunPhaseTransition(phase, _utcNow(), message));

        if (!TryValidateRequest(workspace, request, out var validationFailure))
        {
            Transition(BenchmarkRunPhase.Failed, validationFailure.Message);
            return BuildResult(
                request,
                runId,
                phase: BenchmarkRunPhase.Failed,
                state: RunState.Failed,
                determinism: null,
                chapterSet,
                chapterSetFingerprint,
                chapterOutcomes,
                manifestFile: null,
                invalidRunFile: null,
                failure: validationFailure,
                phaseTransitions);
        }

        BenchmarkDeterminismContract? determinism = null;

        if (request.Deterministic)
        {
            Transition(BenchmarkRunPhase.Gated, "Evaluating deterministic benchmark gate.");

            try
            {
                determinism = await _evaluateDeterminismAsync(request.CreateGateRequest(), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var gateFailure = MapExceptionToFailure(
                    ex,
                    stage: GatedStage,
                    prefix: "Deterministic benchmark gate failed");

                Transition(BenchmarkRunPhase.Failed, gateFailure.Message);
                return BuildResult(
                    request,
                    runId,
                    phase: BenchmarkRunPhase.Failed,
                    state: RunState.Failed,
                    determinism: null,
                    chapterSet,
                    chapterSetFingerprint,
                    chapterOutcomes,
                    manifestFile: null,
                    invalidRunFile: null,
                    failure: gateFailure,
                    phaseTransitions);
            }

            if (!TryValidateDeterminismContract(determinism, out var malformedReason))
            {
                var malformedGateFailure = new RunFailure(
                    RunFailureKind.Validation,
                    $"Deterministic benchmark gate returned malformed payload: {malformedReason}",
                    GatedStage);

                Transition(BenchmarkRunPhase.Failed, malformedGateFailure.Message);
                return BuildResult(
                    request,
                    runId,
                    phase: BenchmarkRunPhase.Failed,
                    state: RunState.Failed,
                    determinism: null,
                    chapterSet,
                    chapterSetFingerprint,
                    chapterOutcomes,
                    manifestFile: null,
                    invalidRunFile: null,
                    failure: malformedGateFailure,
                    phaseTransitions);
            }

            if (!determinism.IsValid)
            {
                Transition(BenchmarkRunPhase.Invalid, "Deterministic benchmark run rejected before pipeline dispatch.");

                var invalidArtifact = new BenchmarkInvalidRunArtifact(
                    runId,
                    rejectedAtUtc: _utcNow(),
                    deterministic: true,
                    phase: BenchmarkRunPhase.Invalid,
                    determinism,
                    chapterSet,
                    chapterSetFingerprint,
                    phaseTransitions);

                FileInfo invalidArtifactFile;
                try
                {
                    invalidArtifactFile = await _artifactStore
                        .WriteInvalidRunAsync(request.OutputRoot, invalidArtifact, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var artifactFailure = MapExceptionToFailure(
                        ex,
                        stage: InvalidStage,
                        prefix: "Deterministic rejection artifact write failed");

                    Transition(BenchmarkRunPhase.Failed, artifactFailure.Message);
                    return BuildResult(
                        request,
                        runId,
                        phase: BenchmarkRunPhase.Failed,
                        state: RunState.Failed,
                        determinism,
                        chapterSet,
                        chapterSetFingerprint,
                        chapterOutcomes,
                        manifestFile: null,
                        invalidRunFile: null,
                        failure: artifactFailure,
                        phaseTransitions);
                }

                return BuildResult(
                    request,
                    runId,
                    phase: BenchmarkRunPhase.Invalid,
                    state: RunState.Completed,
                    determinism,
                    chapterSet,
                    chapterSetFingerprint,
                    chapterOutcomes,
                    manifestFile: null,
                    invalidRunFile: invalidArtifactFile,
                    failure: null,
                    phaseTransitions);
            }
        }

        Transition(BenchmarkRunPhase.Running, "Dispatching chapter pipeline execution.");

        RunFailure? executionFailure = null;

        foreach (var chapter in request.Chapters)
        {
            var chapterOptions = BuildChapterOptions(request, chapter);
            var pipelineStopwatch = Stopwatch.StartNew();

            try
            {
                var chapterResult = await _runPipelineChapterAsync(workspace, chapterOptions, cancellationToken).ConfigureAwait(false);
                pipelineStopwatch.Stop();

                if (!TryValidatePipelineResult(chapterResult, chapter.ChapterId, out var malformedResultReason))
                {
                    executionFailure = new RunFailure(
                        RunFailureKind.Validation,
                        $"Pipeline chapter result was malformed for '{chapter.ChapterId}': {malformedResultReason}",
                        RunningStage);

                    chapterOutcomes.Add(CreateFailedChapterOutcome(chapter.ChapterId, executionFailure));
                    Transition(BenchmarkRunPhase.Failed, executionFailure.Message);
                    break;
                }

                var outcome = await CreateChapterOutcomeAsync(
                        request,
                        chapter,
                        chapterResult,
                        workspace.RootPath,
                        pipelineStopwatch.ElapsedMilliseconds,
                        cancellationToken)
                    .ConfigureAwait(false);

                chapterOutcomes.Add(outcome);

                if (outcome.State == RunState.Failed)
                {
                    executionFailure = new RunFailure(
                        outcome.Failure?.Kind ?? RunFailureKind.Execution,
                        $"Chapter '{chapter.ChapterId}' failed: {outcome.Failure?.Message ?? outcome.Summary}",
                        RunningStage);

                    Transition(BenchmarkRunPhase.Failed, executionFailure.Message);
                    break;
                }

                if (outcome.State == RunState.Running)
                {
                    executionFailure = new RunFailure(
                        RunFailureKind.Execution,
                        $"Chapter '{chapter.ChapterId}' returned non-terminal state '{RunState.Running}'.",
                        RunningStage);

                    Transition(BenchmarkRunPhase.Failed, executionFailure.Message);
                    break;
                }
            }
            catch (PipelineRunException ex)
            {
                pipelineStopwatch.Stop();

                if (TryValidatePipelineResult(ex.Result, chapter.ChapterId, out _))
                {
                    chapterOutcomes.Add(await CreateChapterOutcomeAsync(
                            request,
                            chapter,
                            ex.Result,
                            workspace.RootPath,
                            pipelineStopwatch.ElapsedMilliseconds,
                            cancellationToken)
                        .ConfigureAwait(false));
                }
                else
                {
                    chapterOutcomes.Add(CreateFailedChapterOutcome(
                        chapter.ChapterId,
                        new RunFailure(RunFailureKind.Validation, "Pipeline failure payload was malformed.", RunningStage)));
                }

                executionFailure = new RunFailure(
                    ex.Failure.Kind,
                    $"Chapter '{chapter.ChapterId}' failed: {ex.Failure.Message}",
                    RunningStage);

                Transition(BenchmarkRunPhase.Failed, executionFailure.Message);
                break;
            }
            catch (Exception ex)
            {
                pipelineStopwatch.Stop();

                executionFailure = MapExceptionToFailure(
                    ex,
                    RunningStage,
                    $"Chapter '{chapter.ChapterId}' execution failed");

                chapterOutcomes.Add(CreateFailedChapterOutcome(chapter.ChapterId, executionFailure));
                Transition(BenchmarkRunPhase.Failed, executionFailure.Message);
                break;
            }
        }

        EnsureAllRequestedChaptersHaveOutcomes(request.Chapters, chapterOutcomes, executionFailure);

        if (executionFailure is null)
        {
            Transition(BenchmarkRunPhase.Completed, "Benchmark run completed.");
        }

        var cachePolicy = determinism?.CachePolicy ?? BenchmarkCachePolicy.FromPipelineOptions(request.PipelineOptions);
        var chunkPolicy = determinism?.ChunkPolicy ?? BenchmarkChunkPolicy.FromPipelineOptions(request.PipelineOptions);
        var finalPhase = executionFailure is null ? BenchmarkRunPhase.Completed : BenchmarkRunPhase.Failed;
        var finalState = executionFailure is null ? RunState.Completed : RunState.Failed;

        BenchmarkRunManifest manifest;
        try
        {
            manifest = new BenchmarkRunManifest(
                runId,
                startedAtUtc,
                completedAtUtc: _utcNow(),
                deterministic: request.Deterministic,
                phase: finalPhase,
                state: finalState,
                determinism,
                cachePolicy,
                chunkPolicy,
                chapterSet,
                chapterSetFingerprint,
                chapterOutcomes,
                executionFailure,
                phaseTransitions,
                metricsPolicy: request.MetricsPolicy);
        }
        catch (Exception ex)
        {
            var manifestValidationFailure = MapExceptionToFailure(
                ex,
                stage: FailedStage,
                prefix: "Benchmark manifest contract validation failed",
                priorFailure: executionFailure);

            Transition(BenchmarkRunPhase.Failed, manifestValidationFailure.Message);

            return BuildResult(
                request,
                runId,
                phase: BenchmarkRunPhase.Failed,
                state: RunState.Failed,
                determinism,
                chapterSet,
                chapterSetFingerprint,
                chapterOutcomes,
                manifestFile: null,
                invalidRunFile: null,
                failure: manifestValidationFailure,
                phaseTransitions);
        }

        FileInfo manifestFile;
        try
        {
            manifestFile = await _artifactStore
                .WriteManifestAsync(request.OutputRoot, manifest, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var artifactFailure = MapExceptionToFailure(
                ex,
                stage: FailedStage,
                prefix: "Benchmark manifest artifact write failed",
                priorFailure: executionFailure);

            Transition(BenchmarkRunPhase.Failed, artifactFailure.Message);

            return BuildResult(
                request,
                runId,
                phase: BenchmarkRunPhase.Failed,
                state: RunState.Failed,
                determinism,
                chapterSet,
                chapterSetFingerprint,
                chapterOutcomes,
                manifestFile: null,
                invalidRunFile: null,
                failure: artifactFailure,
                phaseTransitions);
        }

        return BuildResult(
            request,
            runId,
            phase: finalPhase,
            state: finalState,
            determinism,
            chapterSet,
            chapterSetFingerprint,
            chapterOutcomes,
            manifestFile,
            invalidRunFile: null,
            failure: executionFailure,
            phaseTransitions);
    }

    private static bool TryValidateRequest(
        IWorkspace workspace,
        BenchmarkRunRequest request,
        out RunFailure failure)
    {
        if (string.IsNullOrWhiteSpace(workspace.RootPath))
        {
            failure = new RunFailure(RunFailureKind.Validation, "Workspace root path is required.", GatedStage);
            return false;
        }

        try
        {
            _ = Path.GetFullPath(request.OutputRoot.FullName);
        }
        catch (Exception ex)
        {
            failure = new RunFailure(
                RunFailureKind.Validation,
                $"Benchmark output root path is invalid: {ex.Message}",
                GatedStage);
            return false;
        }

        if (request.Chapters.Count == 0)
        {
            failure = new RunFailure(RunFailureKind.Validation, "Benchmark chapter set cannot be empty.", GatedStage);
            return false;
        }

        var chapterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var chapter in request.Chapters)
        {
            if (chapter is null)
            {
                failure = new RunFailure(RunFailureKind.Validation, "Benchmark chapter entry cannot be null.", GatedStage);
                return false;
            }

            if (!chapterIds.Add(chapter.ChapterId))
            {
                failure = new RunFailure(
                    RunFailureKind.Validation,
                    $"Duplicate benchmark chapter identifier '{chapter.ChapterId}'.",
                    GatedStage);
                return false;
            }

            if (chapter.AudioFile is null)
            {
                failure = new RunFailure(
                    RunFailureKind.Validation,
                    $"Chapter '{chapter.ChapterId}' is missing audio file metadata.",
                    GatedStage);
                return false;
            }
        }

        if (request.PipelineOptions.BookFile is null)
        {
            failure = new RunFailure(RunFailureKind.Validation, "Pipeline options must include BookFile.", GatedStage);
            return false;
        }

        if (request.PipelineOptions.BookIndexFile is null)
        {
            failure = new RunFailure(RunFailureKind.Validation, "Pipeline options must include BookIndexFile.", GatedStage);
            return false;
        }

        failure = null!;
        return true;
    }

    private static bool TryValidateDeterminismContract(BenchmarkDeterminismContract? contract, out string reason)
    {
        if (contract is null)
        {
            reason = "Contract was null.";
            return false;
        }

        if (contract.Verdict == BenchmarkDeterminismVerdict.Valid && contract.ReasonCodes.Count > 0)
        {
            reason = "Valid verdict included rejection reason codes.";
            return false;
        }

        if (contract.Verdict == BenchmarkDeterminismVerdict.Invalid && contract.ReasonCodes.Count == 0)
        {
            reason = "Invalid verdict omitted rejection reason codes.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool TryValidatePipelineResult(PipelineChapterResult? result, string expectedChapterId, out string reason)
    {
        if (result is null)
        {
            reason = "Result was null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(result.ChapterId))
        {
            reason = "Chapter id was blank.";
            return false;
        }

        if (!string.Equals(result.ChapterId, expectedChapterId, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"Expected chapter '{expectedChapterId}' but result was '{result.ChapterId}'.";
            return false;
        }

        if (result.State == RunState.Failed && result.Failure is null)
        {
            reason = "Failed chapter result omitted failure metadata.";
            return false;
        }

        if (result.State != RunState.Failed && result.Failure is not null)
        {
            reason = "Non-failed chapter result included failure metadata.";
            return false;
        }

        foreach (var stage in result.StageResults)
        {
            if (string.IsNullOrWhiteSpace(stage.StageName))
            {
                reason = "Stage result had blank stage name.";
                return false;
            }

            if (stage.State == RunState.Failed && stage.Failure is null)
            {
                reason = $"Stage '{stage.StageName}' failed without failure metadata.";
                return false;
            }

            if (stage.State != RunState.Failed && stage.Failure is not null)
            {
                reason = $"Stage '{stage.StageName}' included failure metadata while state was {stage.State}.";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    private static PipelineRunOptions BuildChapterOptions(BenchmarkRunRequest request, BenchmarkRunChapterRequest chapter)
    {
        return request.PipelineOptions with
        {
            ChapterId = chapter.ChapterId,
            AudioFile = chapter.AudioFile,
            ChapterDirectory = chapter.ChapterDirectory,
            ModuleId = request.ModuleId
        };
    }

    private async Task<BenchmarkRunChapterOutcome> CreateChapterOutcomeAsync(
        BenchmarkRunRequest request,
        BenchmarkRunChapterRequest chapter,
        PipelineChapterResult chapterResult,
        string workspaceRoot,
        long pipelineRuntimeMs,
        CancellationToken cancellationToken)
    {
        var metrics = await CollectChapterMetricsAsync(
                request,
                chapter,
                chapterResult,
                pipelineRuntimeMs,
                cancellationToken)
            .ConfigureAwait(false);

        var outcome = ToChapterOutcome(chapterResult, workspaceRoot);

        return new BenchmarkRunChapterOutcome(
            outcome.ChapterId,
            outcome.State,
            outcome.Summary,
            outcome.Failure,
            outcome.StageSummaries,
            outcome.Artifacts,
            metrics);
    }

    private async Task<BenchmarkChapterMetrics> CollectChapterMetricsAsync(
        BenchmarkRunRequest request,
        BenchmarkRunChapterRequest chapter,
        PipelineChapterResult chapterResult,
        long pipelineRuntimeMs,
        CancellationToken cancellationToken)
    {
        var metricsRequest = BuildMetricsCollectionRequest(request, chapter, chapterResult, pipelineRuntimeMs);
        var analysisStopwatch = Stopwatch.StartNew();

        try
        {
            var collected = await _collectChapterMetricsAsync(metricsRequest, cancellationToken).ConfigureAwait(false);
            return EnsureMetricsRuntimeSplit(collected, pipelineRuntimeMs);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new BenchmarkChapterMetrics(
                BenchmarkMetricsStatus.Failed,
                runtime: new BenchmarkChapterRuntimeMetrics(
                    pipelineRuntimeMs: pipelineRuntimeMs,
                    analysisRuntimeMs: analysisStopwatch.ElapsedMilliseconds),
                quality: null,
                metricsFailure: CreateMetricsCollectionFailure(ex, chapter.ChapterId));
        }
        finally
        {
            analysisStopwatch.Stop();
        }
    }

    private static BenchmarkMetricsCollectionRequest BuildMetricsCollectionRequest(
        BenchmarkRunRequest request,
        BenchmarkRunChapterRequest chapter,
        PipelineChapterResult chapterResult,
        long pipelineRuntimeMs)
    {
        var treatedAudioFile = ResolveArtifactFile(chapterResult, "treated-audio", chapterResult.TreatedAudioFile);
        var hydrateFile = ResolveArtifactFile(chapterResult, "hydrate", chapterResult.HydrateFile);

        return new BenchmarkMetricsCollectionRequest(
            chapterId: chapter.ChapterId,
            rawAudioFile: chapter.AudioFile,
            treatedAudioFile: treatedAudioFile,
            hydrateFile: hydrateFile,
            policy: request.MetricsPolicy,
            pipelineRuntimeMs: pipelineRuntimeMs);
    }

    private static FileInfo ResolveArtifactFile(PipelineChapterResult chapterResult, string artifactName, FileInfo fallback)
    {
        var artifactPath = chapterResult.Artifacts
            .FirstOrDefault(artifact => string.Equals(artifact.Name, artifactName, StringComparison.OrdinalIgnoreCase))
            ?.Path;

        if (string.IsNullOrWhiteSpace(artifactPath))
        {
            return fallback;
        }

        return new FileInfo(artifactPath);
    }

    private static BenchmarkChapterMetrics EnsureMetricsRuntimeSplit(BenchmarkChapterMetrics metrics, long pipelineRuntimeMs)
    {
        if (metrics.Runtime.PipelineRuntimeMs == pipelineRuntimeMs)
        {
            return metrics;
        }

        return new BenchmarkChapterMetrics(
            metrics.Status,
            runtime: new BenchmarkChapterRuntimeMetrics(
                pipelineRuntimeMs: pipelineRuntimeMs,
                analysisRuntimeMs: metrics.Runtime.AnalysisRuntimeMs),
            quality: metrics.Quality,
            metricsFailure: metrics.MetricsFailure);
    }

    private static BenchmarkMetricsFailure CreateMetricsCollectionFailure(Exception exception, string chapterId)
    {
        var kind = exception switch
        {
            TimeoutException => RunFailureKind.Timeout,
            OperationCanceledException => RunFailureKind.Cancelled,
            FileNotFoundException => RunFailureKind.Validation,
            DirectoryNotFoundException => RunFailureKind.Validation,
            InvalidDataException => RunFailureKind.Validation,
            ArgumentException => RunFailureKind.Validation,
            IOException => RunFailureKind.Dependency,
            UnauthorizedAccessException => RunFailureKind.Dependency,
            _ => RunFailureKind.Execution
        };

        return new BenchmarkMetricsFailure(
            kind,
            $"Metrics collection failed: {exception.Message}",
            operation: "metrics-collection",
            chapterId: chapterId);
    }

    private static void EnsureAllRequestedChaptersHaveOutcomes(
        IReadOnlyList<BenchmarkRunChapterRequest> requestedChapters,
        List<BenchmarkRunChapterOutcome> chapterOutcomes,
        RunFailure? executionFailure)
    {
        var representedChapters = new HashSet<string>(
            chapterOutcomes.Select(outcome => outcome.ChapterId),
            StringComparer.OrdinalIgnoreCase);

        foreach (var chapter in requestedChapters)
        {
            if (representedChapters.Contains(chapter.ChapterId))
            {
                continue;
            }

            var summary = executionFailure is null
                ? $"Chapter '{chapter.ChapterId}' was not dispatched."
                : $"Chapter '{chapter.ChapterId}' was not run because a prior chapter terminated benchmark execution.";

            chapterOutcomes.Add(CreatePendingChapterOutcome(chapter.ChapterId, summary));
        }
    }

    private static BenchmarkRunChapterOutcome ToChapterOutcome(PipelineChapterResult result, string workspaceRoot)
    {
        var stageSummaries = result.StageResults
            .Select(stage => new BenchmarkRunStageSummary(
                stage: stage.StageName,
                state: stage.State,
                executed: stage.Executed,
                message: stage.Message,
                failure: stage.Failure))
            .ToArray();

        var artifacts = result.Artifacts
            .Select(artifact => RedactArtifactPath(artifact, workspaceRoot))
            .ToArray();

        var summary = result.State switch
        {
            RunState.Failed => result.Failure?.Message ?? "Pipeline chapter failed.",
            RunState.Running => "Pipeline chapter ended in running state.",
            RunState.Pending => "Pipeline chapter remains pending.",
            _ => "Pipeline chapter completed."
        };

        return new BenchmarkRunChapterOutcome(
            result.ChapterId,
            result.State,
            summary,
            result.Failure,
            stageSummaries,
            artifacts);
    }

    private static BenchmarkRunChapterOutcome CreatePendingChapterOutcome(string chapterId, string summary)
    {
        return new BenchmarkRunChapterOutcome(
            chapterId,
            RunState.Pending,
            summary,
            stageSummaries: [],
            artifacts: []);
    }

    private static BenchmarkRunChapterOutcome CreateFailedChapterOutcome(string chapterId, RunFailure failure)
    {
        return new BenchmarkRunChapterOutcome(
            chapterId,
            RunState.Failed,
            failure.Message,
            failure,
            stageSummaries: [],
            artifacts: []);
    }

    private static RunArtifact RedactArtifactPath(RunArtifact artifact, string workspaceRoot)
    {
        var relativePath = ToWorkspaceRelativePath(workspaceRoot, artifact.Path);
        return new RunArtifact(artifact.Name, artifact.Kind, relativePath, artifact.Exists);
    }

    private static string ToWorkspaceRelativePath(string workspaceRoot, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "unknown";
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch
        {
            return path.Replace('\\', '/');
        }

        if (string.IsNullOrWhiteSpace(workspaceRoot))
        {
            return Path.GetFileName(fullPath);
        }

        var normalizedRoot = Path.GetFullPath(workspaceRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (fullPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            return ".";
        }

        var prefix = normalizedRoot + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetRelativePath(normalizedRoot, fullPath).Replace('\\', '/');
        }

        return Path.GetFileName(fullPath);
    }

    private static string ComputeChapterSetFingerprint(IReadOnlyList<string> chapterSet)
    {
        var canonical = string.Join("\n", chapterSet.Select(chapter => chapter.Trim()));
        var bytes = Encoding.UTF8.GetBytes(canonical);

        using var hash = SHA256.Create();
        var digest = hash.ComputeHash(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    private BenchmarkRunResult BuildResult(
        BenchmarkRunRequest request,
        string runId,
        BenchmarkRunPhase phase,
        RunState state,
        BenchmarkDeterminismContract? determinism,
        IReadOnlyList<string> chapterSet,
        string chapterSetFingerprint,
        IReadOnlyList<BenchmarkRunChapterOutcome> chapterOutcomes,
        FileInfo? manifestFile,
        FileInfo? invalidRunFile,
        RunFailure? failure,
        IReadOnlyList<BenchmarkRunPhaseTransition> phaseTransitions)
    {
        return new BenchmarkRunResult(
            runId,
            request.ModuleId,
            request.Deterministic,
            phase,
            state,
            determinism,
            chapterSet,
            chapterSetFingerprint,
            chapterOutcomes,
            manifestFile,
            invalidRunFile,
            failure,
            phaseTransitions,
            metricsPolicy: request.MetricsPolicy);
    }

    private RunFailure MapExceptionToFailure(
        Exception exception,
        string stage,
        string prefix,
        RunFailure? priorFailure = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var message = string.IsNullOrWhiteSpace(priorFailure?.Message)
            ? $"{prefix}: {exception.Message}"
            : $"{prefix}: {exception.Message}. Prior failure: {priorFailure!.Message}";

        var kind = exception switch
        {
            TimeoutException => RunFailureKind.Timeout,
            OperationCanceledException when exception is OperationCanceledException => RunFailureKind.Cancelled,
            FileNotFoundException => RunFailureKind.Validation,
            DirectoryNotFoundException => RunFailureKind.Validation,
            ArgumentException => RunFailureKind.Validation,
            IOException => RunFailureKind.Dependency,
            UnauthorizedAccessException => RunFailureKind.Dependency,
            _ => RunFailureKind.Execution
        };

        return new RunFailure(kind, message, stage);
    }

    private static string GenerateDefaultRunId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"run-{timestamp}-{suffix}";
    }

    private string ResolveRunId(BenchmarkRunRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RunId))
        {
            return request.RunId;
        }

        return _runIdFactory();
    }
}
