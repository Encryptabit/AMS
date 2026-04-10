using Ams.Core.Application.Commands;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Pipeline;
using Ams.Core.Application.Runs;
using Ams.Core.Asr;
using Ams.Core.Common;
using Ams.Core.Runtime.Book;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Interfaces;

namespace Ams.Workstation.Server.Services.Prep;

public enum PrepRunPhase
{
    Idle,
    BuildingBookIndex,
    RefreshingWorkspace,
    RunningPipeline,
    RefreshingChapterHandle,
    Completed,
    Failed,
    Cancelled
}

public sealed class PrepRunSession : IDisposable
{
    public const double ExplicitAverageWordsPerMinute = 200.0;

    public event Action? StateChanged;

    internal const string BookIndexStage = "book_index";
    internal const string WorkspaceReloadStage = "workspace_reload";
    internal const string RuntimeReadinessStage = "runtime_readiness";
    internal const string ChapterReloadStage = "chapter_reload";

    private readonly BlazorWorkspace _workspace;
    private readonly BuildBookIndexCommand _buildBookIndex;
    private readonly PipelineService _pipelineService;
    private readonly IPrepRuntimeReadinessProbe _runtimeReadinessProbe;
    private readonly List<RunProgressUpdate> _progressUpdates = [];

    private CancellationTokenSource? _runCts;
    private bool _disposed;

    public PrepRunSession(
        BlazorWorkspace workspace,
        BuildBookIndexCommand buildBookIndex,
        PipelineService pipelineService,
        IPrepRuntimeReadinessProbe runtimeReadinessProbe)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _buildBookIndex = buildBookIndex ?? throw new ArgumentNullException(nameof(buildBookIndex));
        _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));
        _runtimeReadinessProbe = runtimeReadinessProbe ?? throw new ArgumentNullException(nameof(runtimeReadinessProbe));
    }

    public PrepRunPhase CurrentPhase { get; private set; } = PrepRunPhase.Idle;

    public string? CurrentStage { get; private set; }

    public string? LastCompletedStage { get; private set; }

    public string? LastMessage { get; private set; }

    public RunProgressUpdate? CurrentProgress { get; private set; }

    public RunFailure? LastFailure { get; private set; }

    public RunFailureKind? LastFailureKind => LastFailure?.Kind;

    public string? LastFailureStage => LastFailure?.Stage;

    public bool WasCancelled { get; private set; }

    public bool IsRunning => _runCts is not null;

    public bool IsCancellationRequested => _runCts?.IsCancellationRequested ?? false;

    public BuildBookIndexResult? LastBuildBookIndexResult { get; private set; }

    public PipelineChapterResult? LastPipelineResult { get; private set; }

    public PrepRuntimeReadinessSnapshot? LastRuntimeReadiness { get; private set; }

    public IReadOnlyList<RunProgressUpdate> ProgressUpdates => _progressUpdates;

    public IReadOnlyList<RunArtifact> LastArtifacts { get; private set; } = [];

    public bool LastWorkspaceReloaded { get; private set; }

    public bool LastChapterHandleReloaded { get; private set; }

    public string? LastChapterId { get; private set; }

    internal BuildBookIndexRequest? LastBuildRequest { get; private set; }

    internal PipelineRunOptions? LastPipelineOptions { get; private set; }

    internal PrepPipelineRunRequest? LastPipelineRequest { get; private set; }

    internal IReadOnlyList<string> LastPipelineOptionWarnings { get; private set; } = [];

    public bool Cancel()
    {
        if (_runCts is null)
        {
            return false;
        }

        _runCts.Cancel();
        NotifyStateChanged();
        return true;
    }

    public async Task<bool> BuildBookIndexAsync(string manuscriptPath)
    {
        ThrowIfDisposed();
        using var runScope = BeginRun();
        if (runScope is null)
        {
            return false;
        }

        CurrentPhase = PrepRunPhase.BuildingBookIndex;
        CurrentStage = BookIndexStage;

        try
        {
            LastBuildRequest = CreateBuildBookIndexRequest(manuscriptPath);
            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.BuildBookIndex,
                RunState.Pending,
                "Queued",
                progress: 0d,
                stage: BookIndexStage));
            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.BuildBookIndex,
                RunState.Running,
                "Building book index",
                progress: 0d,
                stage: BookIndexStage));

            var result = await _buildBookIndex.ExecuteAsync(LastBuildRequest, runScope.Token).ConfigureAwait(false);
            LastBuildBookIndexResult = result;
            LastArtifacts = result.Artifacts;

            LastBuildRequest.OutputFile.Refresh();
            if (!LastBuildRequest.OutputFile.Exists)
            {
                return Fail(
                    ModuleIds.BuildBookIndex,
                    new RunFailure(
                        RunFailureKind.Execution,
                        $"Book index output is missing: {LastBuildRequest.OutputFile.FullName}",
                        BookIndexStage),
                    result.Artifacts);
            }

            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.BuildBookIndex,
                RunState.Completed,
                $"Book index prepared ({result.CacheDisposition})",
                progress: 0.85d,
                stage: BookIndexStage,
                artifacts: result.Artifacts));

            CurrentPhase = PrepRunPhase.RefreshingWorkspace;
            CurrentStage = WorkspaceReloadStage;
            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.BuildBookIndex,
                RunState.Running,
                "Refreshing workspace",
                progress: 0.95d,
                stage: WorkspaceReloadStage,
                artifacts: result.Artifacts));

            LastWorkspaceReloaded = _workspace.RefreshWorkspaceFromDisk("prep-build-index");
            if (!LastWorkspaceReloaded)
            {
                return Fail(
                    ModuleIds.BuildBookIndex,
                    new RunFailure(
                        RunFailureKind.Execution,
                        "Workspace reload failed after building book index.",
                        WorkspaceReloadStage),
                    result.Artifacts);
            }

            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.BuildBookIndex,
                RunState.Completed,
                "Workspace refreshed",
                progress: 1d,
                stage: WorkspaceReloadStage,
                artifacts: result.Artifacts));

            CurrentPhase = PrepRunPhase.Completed;
            return true;
        }
        catch (BuildBookIndexCommandException ex)
        {
            return Fail(ModuleIds.BuildBookIndex, ex.Failure, ex.Artifacts);
        }
        catch (OperationCanceledException)
        {
            return Cancelled(ModuleIds.BuildBookIndex, BookIndexStage);
        }
        catch (Exception ex)
        {
            return Fail(ModuleIds.BuildBookIndex, MapFailure(CurrentStage ?? BookIndexStage, ex), LastArtifacts);
        }
    }

    public Task<bool> RunChapterPrepAsync(string chapterDisplayTitle, PipelineStage endStage = PipelineStage.Mfa)
        => RunChapterPrepAsync(chapterDisplayTitle, PrepPipelineRunRequest.Default with { EndStage = endStage });

    public async Task<bool> RunChapterPrepAsync(string chapterDisplayTitle, PrepPipelineRunRequest? request = null)
    {
        ThrowIfDisposed();
        using var runScope = BeginRun();
        if (runScope is null)
        {
            return false;
        }

        CurrentPhase = PrepRunPhase.RunningPipeline;

        try
        {
            var (normalizedRequest, warnings) = NormalizePipelineRunRequest(request);
            var pipelineOptions = CreatePipelineRunOptions(chapterDisplayTitle, normalizedRequest) with
            {
                Progress = new Progress<RunProgressUpdate>(AppendProgress)
            };

            LastPipelineRequest = normalizedRequest;
            LastPipelineOptionWarnings = warnings;
            LastPipelineOptions = pipelineOptions;

            LastChapterId = LastPipelineOptions.ChapterId;
            CurrentStage = RuntimeReadinessStage;
            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.PipelineRun,
                RunState.Running,
                "Capturing runtime readiness",
                progress: 0d,
                stage: RuntimeReadinessStage,
                itemId: LastChapterId));

            var runtimeReadiness = await CaptureRuntimeReadinessSnapshotAsync(
                    LastPipelineRequest,
                    chapterDisplayTitle,
                    LastChapterId,
                    runScope.Token)
                .ConfigureAwait(false);
            LastRuntimeReadiness = runtimeReadiness;

            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.PipelineRun,
                RunState.Completed,
                runtimeReadiness.IsReady
                    ? "Runtime readiness captured"
                    : "Runtime readiness captured with warnings",
                progress: 0.02d,
                stage: RuntimeReadinessStage,
                itemId: LastChapterId));

            CurrentStage = PipelineRunContractStageName;
            var result = await _pipelineService.RunChapterAsync(_workspace, LastPipelineOptions, runScope.Token)
                .ConfigureAwait(false);

            LastPipelineResult = result;
            LastArtifacts = result.Artifacts;

            CurrentPhase = PrepRunPhase.RefreshingChapterHandle;
            CurrentStage = ChapterReloadStage;
            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.PipelineRun,
                RunState.Running,
                "Refreshing chapter handle",
                progress: 1d,
                stage: ChapterReloadStage,
                artifacts: result.Artifacts,
                itemId: result.ChapterId));

            LastChapterHandleReloaded = _workspace.RefreshChapterHandle(result.ChapterId, "prep-pipeline");
            if (!LastChapterHandleReloaded)
            {
                return Fail(
                    ModuleIds.PipelineRun,
                    new RunFailure(
                        RunFailureKind.Execution,
                        $"Chapter handle reload failed for '{result.ChapterId}'.",
                        ChapterReloadStage),
                    result.Artifacts,
                    itemId: result.ChapterId);
            }

            AppendProgress(RunProgressUpdate.CreateStatus(
                ModuleIds.PipelineRun,
                RunState.Completed,
                "Chapter handle refreshed",
                progress: 1d,
                stage: ChapterReloadStage,
                artifacts: result.Artifacts,
                itemId: result.ChapterId));

            CurrentPhase = PrepRunPhase.Completed;
            return true;
        }
        catch (PipelineRunException ex)
        {
            LastPipelineResult = ex.Result;
            LastArtifacts = ex.Result.Artifacts;
            RecordTerminalState(ex.Failure, cancelled: false);
            _ = RefreshChapterHandleAfterPipelineFailure(ex.Result.ChapterId, "prep-pipeline-failed");
            NotifyStateChanged();
            return false;
        }
        catch (OperationCanceledException)
        {
            var chapterId = LastPipelineOptions?.ChapterId ?? LastChapterId;
            _ = RefreshChapterHandleAfterPipelineFailure(chapterId, "prep-pipeline-cancelled");
            return Cancelled(ModuleIds.PipelineRun, CurrentStage ?? PipelineRunContractStageName, chapterId);
        }
        catch (Exception ex)
        {
            var chapterId = LastPipelineOptions?.ChapterId ?? LastChapterId;
            _ = RefreshChapterHandleAfterPipelineFailure(chapterId, "prep-pipeline-failed");
            return Fail(
                ModuleIds.PipelineRun,
                MapFailure(CurrentStage ?? PipelineRunContractStageName, ex),
                LastArtifacts,
                itemId: chapterId);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _runCts?.Cancel();
        _runCts?.Dispose();
        _runCts = null;
    }

    private RunScope? BeginRun()
    {
        if (_runCts is not null)
        {
            return FailToBeginRun();
        }

        ResetRunState();
        _runCts = new CancellationTokenSource();
        return new RunScope(this, _runCts);
    }

    private RunScope? FailToBeginRun()
    {
        LastFailure = new RunFailure(RunFailureKind.Validation, "A Prep run is already in progress.", "prep");
        LastMessage = LastFailure.Message;
        CurrentPhase = PrepRunPhase.Failed;
        CurrentStage = LastFailure.Stage;
        NotifyStateChanged();
        return null;
    }

    private void ResetRunState()
    {
        _progressUpdates.Clear();
        CurrentProgress = null;
        CurrentPhase = PrepRunPhase.Idle;
        CurrentStage = null;
        LastCompletedStage = null;
        LastMessage = null;
        LastFailure = null;
        WasCancelled = false;
        LastBuildBookIndexResult = null;
        LastPipelineResult = null;
        LastArtifacts = [];
        LastWorkspaceReloaded = false;
        LastChapterHandleReloaded = false;
        LastChapterId = null;
        LastBuildRequest = null;
        NotifyStateChanged();
    }

    private async Task<PrepRuntimeReadinessSnapshot> CaptureRuntimeReadinessSnapshotAsync(
        PrepPipelineRunRequest request,
        string chapterDisplayTitle,
        string chapterId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var snapshot = await _runtimeReadinessProbe
            .CaptureAsync(request, chapterDisplayTitle, chapterId, cancellationToken)
            .ConfigureAwait(false);

        if (snapshot is null)
        {
            throw new InvalidOperationException("Runtime readiness probe returned null snapshot data.");
        }

        if (snapshot.ModelProvenance is null
            || snapshot.Ffmpeg is null
            || snapshot.Mfa is null)
        {
            throw new InvalidOperationException("Runtime readiness probe returned malformed snapshot components.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.ModelProvenance.Summary)
            || string.IsNullOrWhiteSpace(snapshot.ModelProvenance.Guidance)
            || string.IsNullOrWhiteSpace(snapshot.Ffmpeg.Summary)
            || string.IsNullOrWhiteSpace(snapshot.Mfa.Summary))
        {
            throw new InvalidOperationException("Runtime readiness probe returned malformed summary data.");
        }

        return snapshot;
    }

    private BuildBookIndexRequest CreateBuildBookIndexRequest(string manuscriptPath)
    {
        var rootPath = RequireWorkspaceRoot();
        var normalizedPath = AmsPathResolver.NormalizeOptionalPath(manuscriptPath);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            throw new InvalidOperationException("An explicit manuscript path is required when book-index.json is missing.");
        }

        var bookFile = new FileInfo(normalizedPath);
        if (!bookFile.Exists)
        {
            throw new FileNotFoundException($"Manuscript file not found: {bookFile.FullName}", bookFile.FullName);
        }

        var outputFile = new FileInfo(Path.Combine(rootPath, "book-index.json"));
        return new BuildBookIndexRequest(
            AmsPathResolver.NormalizeFile(bookFile),
            outputFile,
            new BookIndexOptions
            {
                AverageWpm = ExplicitAverageWordsPerMinute
            },
            BookIndexCacheMode.PreferCache);
    }

    private PipelineRunOptions CreatePipelineRunOptions(string chapterDisplayTitle, PrepPipelineRunRequest request)
    {
        var rootPath = RequireWorkspaceRoot();
        if (!_workspace.HasBookIndex)
        {
            throw new InvalidOperationException("book-index.json is required before chapter prep can run.");
        }

        if (!_workspace.IsInitialized)
        {
            throw new InvalidOperationException("Workspace book context is not loaded. Refresh the workspace before chapter prep.");
        }

        if (string.IsNullOrWhiteSpace(chapterDisplayTitle))
        {
            throw new ArgumentException("A chapter display title is required.", nameof(chapterDisplayTitle));
        }

        var chapterStem = _workspace.GetStemForChapter(chapterDisplayTitle);
        if (string.IsNullOrWhiteSpace(chapterStem))
        {
            throw new InvalidOperationException($"Unknown chapter display title: {chapterDisplayTitle}");
        }

        var sourceFile = _workspace.Book.Documents.BookIndex?.SourceFile;
        if (string.IsNullOrWhiteSpace(sourceFile))
        {
            throw new InvalidOperationException("The loaded book-index does not include a manuscript source path.");
        }

        var bookFile = AmsPathResolver.NormalizeFile(new FileInfo(sourceFile));
        if (!bookFile.Exists)
        {
            throw new FileNotFoundException($"Book source file not found: {bookFile.FullName}", bookFile.FullName);
        }

        var bookIndexFile = new FileInfo(Path.Combine(rootPath, "book-index.json"));
        bookIndexFile.Refresh();
        if (!bookIndexFile.Exists)
        {
            throw new FileNotFoundException($"Book index not found: {bookIndexFile.FullName}", bookIndexFile.FullName);
        }

        var audioFile = new FileInfo(Path.Combine(rootPath, $"{chapterStem}.wav"));
        audioFile.Refresh();
        if (!audioFile.Exists)
        {
            throw new FileNotFoundException(
                $"Audio file not found for chapter '{chapterDisplayTitle}': {audioFile.FullName}",
                audioFile.FullName);
        }

        var chapterDirectory = new DirectoryInfo(Path.Combine(rootPath, chapterStem));
        var chunkPolicy = request.Chunking.Policy;

        return new PipelineRunOptions
        {
            ModuleId = ModuleIds.PipelineRun,
            BookFile = bookFile,
            BookIndexFile = bookIndexFile,
            AudioFile = audioFile,
            ChapterDirectory = chapterDirectory,
            ChapterId = chapterStem,
            AverageWordsPerMinute = ExplicitAverageWordsPerMinute,
            Force = request.Force,
            ForceIndex = request.ForceIndex,
            EndStage = request.EndStage,
            TranscriptOptions = new GenerateTranscriptOptions
            {
                Engine = request.Asr.Engine,
                Model = request.Asr.Model,
                Language = request.Asr.Language ?? GenerateTranscriptOptions.Default.Language,
                EnableWordTimestamps = request.Asr.EnableWordTimestamps,
                EnableFlashAttention = request.Asr.EnableFlashAttention,
                EnableDtwTimestamps = request.Asr.EnableDtwTimestamps,
                DisablePrompt = request.Asr.DisablePrompt,
                DisableChunkPlan = request.Chunking.DisableChunkPlan
            },
            ChunkPlanningPolicy = BuildChunkPlanningPolicy(chunkPolicy),
            DisableChunkPlan = request.Chunking.DisableChunkPlan,
            DisableChunkedMfa = request.Chunking.DisableChunkedMfa,
            MfaOptions = new RunMfaOptions
            {
                BeamProfile = request.Mfa.BeamProfile,
                Beam = request.Mfa.Beam,
                RetryBeam = request.Mfa.RetryBeam,
                DisableChunkedMfa = request.Chunking.DisableChunkedMfa,
                RequireAsrChunkAudio = request.Chunking.RequireAsrChunkAudio
            }
        };
    }

    private static (PrepPipelineRunRequest Request, IReadOnlyList<string> Warnings) NormalizePipelineRunRequest(PrepPipelineRunRequest? request)
    {
        var incoming = request ?? PrepPipelineRunRequest.Default;
        var warnings = new List<string>();

        var asr = incoming.Asr ?? PrepPipelineAsrRequest.Default;
        var mfa = incoming.Mfa ?? PrepPipelineMfaRequest.Default;
        var chunking = incoming.Chunking ?? PrepPipelineChunkRequest.Default;
        var chunkPolicy = chunking.Policy ?? PrepPipelineChunkPolicyRequest.Default;

        var endStage = NormalizeEndStage(incoming.EndStage, warnings);
        var model = NormalizeOptionalText(asr.Model, "ASR model", warnings);
        var language = NormalizeLanguage(asr.Language, warnings);

        var enableDtw = asr.EnableDtwTimestamps;
        if (asr.Engine == AsrEngine.WhisperX && enableDtw)
        {
            enableDtw = false;
            warnings.Add("WhisperX ignores DTW timestamps; DTW was disabled for this run.");
        }

        var enableFlashAttention = asr.EnableFlashAttention;
        if (asr.Engine == AsrEngine.WhisperX && enableFlashAttention)
        {
            enableFlashAttention = false;
            warnings.Add("WhisperX ignores FlashAttention; FlashAttention was disabled for this run.");
        }

        var beam = NormalizePositiveInt(mfa.Beam, "MFA beam", warnings);
        var retryBeam = NormalizePositiveInt(mfa.RetryBeam, "MFA retry beam", warnings);
        if (beam is int beamValue && retryBeam is int retryBeamValue && retryBeamValue < beamValue)
        {
            retryBeam = beamValue;
            warnings.Add("MFA retry beam cannot be lower than MFA beam; retry beam was raised to match the beam value.");
        }

        var requireAsrChunkAudio = chunking.RequireAsrChunkAudio;
        if (chunking.DisableChunkedMfa && requireAsrChunkAudio)
        {
            requireAsrChunkAudio = false;
            warnings.Add("Require ASR chunk audio was disabled because chunked MFA is disabled.");
        }

        var normalizedPolicy = NormalizeChunkPolicy(chunkPolicy, chunking.DisableChunkPlan, warnings);

        return (incoming with
        {
            EndStage = endStage,
            Asr = asr with
            {
                Model = model,
                Language = language,
                EnableDtwTimestamps = enableDtw,
                EnableFlashAttention = enableFlashAttention
            },
            Mfa = mfa with
            {
                Beam = beam,
                RetryBeam = retryBeam
            },
            Chunking = chunking with
            {
                RequireAsrChunkAudio = requireAsrChunkAudio,
                Policy = normalizedPolicy
            }
        }, warnings);
    }

    private static ChunkPlanningPolicy? BuildChunkPlanningPolicy(PrepPipelineChunkPolicyRequest policy)
    {
        if (!HasChunkPolicyOverrides(policy))
        {
            return null;
        }

        var defaults = ChunkPlanningPolicy.Default;

        return new ChunkPlanningPolicy
        {
            SilenceThresholdDb = policy.SilenceThresholdDb ?? defaults.SilenceThresholdDb,
            MinSilenceDuration = policy.MinSilenceDurationMs is double minSilenceDurationMs
                ? TimeSpan.FromMilliseconds(minSilenceDurationMs)
                : defaults.MinSilenceDuration,
            MinChunkDuration = policy.MinChunkDurationSec is double minChunkDurationSec
                ? TimeSpan.FromSeconds(minChunkDurationSec)
                : defaults.MinChunkDuration,
            MaxChunkDuration = policy.MaxChunkDurationSec is double maxChunkDurationSec
                ? TimeSpan.FromSeconds(maxChunkDurationSec)
                : defaults.MaxChunkDuration
        };
    }

    private static PrepPipelineChunkPolicyRequest NormalizeChunkPolicy(
        PrepPipelineChunkPolicyRequest policy,
        bool disableChunkPlan,
        List<string> warnings)
    {
        if (disableChunkPlan)
        {
            if (HasChunkPolicyOverrides(policy))
            {
                warnings.Add("Chunk policy overrides were ignored because chunk planning is disabled.");
            }

            return PrepPipelineChunkPolicyRequest.Default;
        }

        var normalized = policy with
        {
            SilenceThresholdDb = NormalizeFiniteDouble(policy.SilenceThresholdDb, "Chunk silence threshold", warnings),
            MinSilenceDurationMs = NormalizePositiveDouble(policy.MinSilenceDurationMs, "Chunk minimum silence duration", warnings),
            MinChunkDurationSec = NormalizePositiveDouble(policy.MinChunkDurationSec, "Chunk minimum duration", warnings),
            MaxChunkDurationSec = NormalizePositiveDouble(policy.MaxChunkDurationSec, "Chunk maximum duration", warnings)
        };

        if (normalized.MinChunkDurationSec is double minChunkDurationSec
            && normalized.MaxChunkDurationSec is double maxChunkDurationSec
            && maxChunkDurationSec < minChunkDurationSec)
        {
            normalized = normalized with { MaxChunkDurationSec = minChunkDurationSec };
            warnings.Add("Chunk maximum duration cannot be lower than minimum duration; maximum duration was raised to match the minimum.");
        }

        return normalized;
    }

    private static PipelineStage NormalizeEndStage(PipelineStage endStage, List<string> warnings)
    {
        if (endStage < PipelineStage.BookIndex)
        {
            warnings.Add($"Pipeline end stage '{endStage}' is below '{PipelineStage.BookIndex}'; defaulting to '{PipelineStage.BookIndex}'.");
            return PipelineStage.BookIndex;
        }

        if (endStage > PipelineStage.Mfa)
        {
            warnings.Add($"Pipeline end stage '{endStage}' is above '{PipelineStage.Mfa}'; defaulting to '{PipelineStage.Mfa}'.");
            return PipelineStage.Mfa;
        }

        return endStage;
    }

    private static string? NormalizeOptionalText(string? value, string fieldName, List<string> warnings)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        warnings.Add($"{fieldName} was blank and has been ignored.");
        return null;
    }

    private static string NormalizeLanguage(string? value, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (value is not null)
            {
                warnings.Add($"ASR language was blank; defaulting to '{GenerateTranscriptOptions.Default.Language}'.");
            }

            return GenerateTranscriptOptions.Default.Language;
        }

        return value.Trim();
    }

    private static int? NormalizePositiveInt(int? value, string fieldName, List<string> warnings)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (value.Value >= 1)
        {
            return value;
        }

        warnings.Add($"{fieldName} must be >= 1; value '{value.Value}' was clamped to 1.");
        return 1;
    }

    private static double? NormalizePositiveDouble(double? value, string fieldName, List<string> warnings)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            warnings.Add($"{fieldName} must be a finite number; value '{value.Value}' was ignored.");
            return null;
        }

        if (value.Value > 0d)
        {
            return value;
        }

        warnings.Add($"{fieldName} must be > 0; value '{value.Value}' was ignored.");
        return null;
    }

    private static double? NormalizeFiniteDouble(double? value, string fieldName, List<string> warnings)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (!double.IsNaN(value.Value) && !double.IsInfinity(value.Value))
        {
            return value;
        }

        warnings.Add($"{fieldName} must be finite; value '{value.Value}' was ignored.");
        return null;
    }

    private static bool HasChunkPolicyOverrides(PrepPipelineChunkPolicyRequest policy)
        => policy.SilenceThresholdDb.HasValue
           || policy.MinSilenceDurationMs.HasValue
           || policy.MinChunkDurationSec.HasValue
           || policy.MaxChunkDurationSec.HasValue;

    private bool RefreshChapterHandleAfterPipelineFailure(string? chapterId, string reason)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return false;
        }

        var previousPhase = CurrentPhase;
        var previousStage = CurrentStage;
        CurrentPhase = PrepRunPhase.RefreshingChapterHandle;
        CurrentStage = ChapterReloadStage;
        LastChapterHandleReloaded = _workspace.RefreshChapterHandle(chapterId, reason);
        CurrentPhase = previousPhase;
        CurrentStage = previousStage;
        return LastChapterHandleReloaded;
    }

    private void AppendProgress(RunProgressUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        _progressUpdates.Add(update);
        CurrentProgress = update;
        LastMessage = update.Message;
        CurrentStage = update.Stage ?? CurrentStage;

        if (update.Artifacts.Count > 0)
        {
            LastArtifacts = update.Artifacts;
        }

        if (update.State == RunState.Completed && !string.IsNullOrWhiteSpace(update.Stage))
        {
            LastCompletedStage = update.Stage;
        }

        if (update.State == RunState.Failed && update.Failure is not null)
        {
            RecordTerminalState(update.Failure, update.Failure.Kind == RunFailureKind.Cancelled);
        }

        NotifyStateChanged();
    }

    private bool Fail(
        ModuleId moduleId,
        RunFailure failure,
        IReadOnlyList<RunArtifact>? artifacts,
        string? itemId = null)
    {
        ArgumentNullException.ThrowIfNull(moduleId);
        ArgumentNullException.ThrowIfNull(failure);

        LastArtifacts = artifacts?.ToArray() ?? [];
        RecordTerminalState(failure, cancelled: false);
        AppendProgress(RunProgressUpdate.CreateFailure(moduleId, failure, artifacts: artifacts, itemId: itemId));
        return false;
    }

    private bool Cancelled(ModuleId moduleId, string stage, string? itemId = null)
    {
        var failure = LastFailure?.Kind == RunFailureKind.Cancelled && CurrentProgress?.Failure is not null
            ? CurrentProgress.Failure
            : new RunFailure(RunFailureKind.Cancelled, "Prep run cancelled.", stage);

        RecordTerminalState(failure, cancelled: true);
        if (CurrentProgress?.State != RunState.Failed || CurrentProgress.Failure is null)
        {
            AppendProgress(RunProgressUpdate.CreateFailure(moduleId, failure, artifacts: LastArtifacts, itemId: itemId));
        }

        return false;
    }

    private void RecordTerminalState(RunFailure failure, bool cancelled)
    {
        LastFailure = failure;
        LastMessage = failure.Message;
        CurrentStage = failure.Stage;
        WasCancelled = cancelled;
        CurrentPhase = cancelled ? PrepRunPhase.Cancelled : PrepRunPhase.Failed;
    }

    private string RequireWorkspaceRoot()
    {
        if (string.IsNullOrWhiteSpace(_workspace.WorkingDirectory))
        {
            throw new InvalidOperationException("Workspace not initialized. Set a working directory first.");
        }

        return _workspace.RootPath;
    }

    private static RunFailure MapFailure(string stage, Exception exception)
    {
        return exception switch
        {
            TimeoutException => new RunFailure(RunFailureKind.Timeout, exception.Message, stage),
            FileNotFoundException => new RunFailure(RunFailureKind.Validation, exception.Message, stage),
            DirectoryNotFoundException => new RunFailure(RunFailureKind.Validation, exception.Message, stage),
            ArgumentException => new RunFailure(RunFailureKind.Validation, exception.Message, stage),
            InvalidOperationException => new RunFailure(RunFailureKind.Validation, exception.Message, stage),
            IOException => new RunFailure(RunFailureKind.Dependency, exception.Message, stage),
            _ => new RunFailure(RunFailureKind.Execution, exception.Message, stage)
        };
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void EndRun(CancellationTokenSource source)
    {
        if (!ReferenceEquals(_runCts, source))
        {
            return;
        }

        _runCts.Dispose();
        _runCts = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private const string PipelineRunContractStageName = "pipeline";

    private sealed class RunScope : IDisposable
    {
        private readonly PrepRunSession _owner;
        private readonly CancellationTokenSource _source;
        private bool _disposed;

        public RunScope(PrepRunSession owner, CancellationTokenSource source)
        {
            _owner = owner;
            _source = source;
        }

        public CancellationToken Token => _source.Token;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.EndRun(_source);
        }
    }
}
