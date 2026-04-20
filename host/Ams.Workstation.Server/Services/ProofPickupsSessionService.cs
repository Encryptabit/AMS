using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Workstation.Server.Services;

public enum ProofPickupsSessionPhase
{
    Idle,
    ResolvingTargets,
    Importing,
    Staging,
    Unstaging,
    Committing,
    Reverting,
    Completed,
    Failed,
    Cancelled
}

public sealed record ProofPickupsSessionSnapshot(
    string? ActiveChapterName,
    string? ActiveChapterStem,
    string? SourcePath,
    ProofPickupsSessionPhase Phase,
    double Progress,
    string? LastError,
    string? LastOperationId,
    string? LastValidationError,
    int? EdlRevision,
    string? DeterministicOrderingDiagnostics,
    IReadOnlyList<PickupAsset> Matched,
    IReadOnlyList<PickupAsset> Unmatched,
    IReadOnlyList<StagedReplacement> Staged,
    IReadOnlyList<StagedReplacement> Applied,
    IReadOnlyList<StagedReplacement> Reverted,
    IReadOnlyList<StagedReplacement> Failed,
    IReadOnlyList<CrxPickupTarget> Targets,
    int? ArtifactLedgerRevision,
    IReadOnlyList<PickupArtifactLedgerEntry> ArtifactLedgerEntries,
    string? ArtifactLedgerReadError)
{
    public static ProofPickupsSessionSnapshot Empty { get; } = new(
        ActiveChapterName: null,
        ActiveChapterStem: null,
        SourcePath: null,
        Phase: ProofPickupsSessionPhase.Idle,
        Progress: 0d,
        LastError: null,
        LastOperationId: null,
        LastValidationError: null,
        EdlRevision: null,
        DeterministicOrderingDiagnostics: null,
        Matched: Array.Empty<PickupAsset>(),
        Unmatched: Array.Empty<PickupAsset>(),
        Staged: Array.Empty<StagedReplacement>(),
        Applied: Array.Empty<StagedReplacement>(),
        Reverted: Array.Empty<StagedReplacement>(),
        Failed: Array.Empty<StagedReplacement>(),
        Targets: Array.Empty<CrxPickupTarget>(),
        ArtifactLedgerRevision: null,
        ArtifactLedgerEntries: Array.Empty<PickupArtifactLedgerEntry>(),
        ArtifactLedgerReadError: null);

    public bool HasActiveChapter => !string.IsNullOrWhiteSpace(ActiveChapterStem);

    public bool IsBusy => Phase is ProofPickupsSessionPhase.ResolvingTargets
        or ProofPickupsSessionPhase.Importing
        or ProofPickupsSessionPhase.Staging
        or ProofPickupsSessionPhase.Unstaging
        or ProofPickupsSessionPhase.Committing
        or ProofPickupsSessionPhase.Reverting;
}

public sealed class ProofPickupsSessionService
{
    private readonly BlazorWorkspace _workspace;
    private readonly RuntimeHooks _hooks;

    private ProofPickupsSessionSnapshot _snapshot = ProofPickupsSessionSnapshot.Empty;

    public ProofPickupsSessionService(
        BlazorWorkspace workspace,
        CrxService crxService,
        PickupAssetService pickupAssetService,
        PolishService polishService,
        StagingQueueService stagingQueueService,
        PickupEdlStore pickupEdlStore,
        PickupArtifactLedgerStore pickupArtifactLedgerStore,
        PickupEdlEngine pickupEdlEngine)
        : this(
            workspace,
            new RuntimeHooks(
                GetCrxEntries: () => crxService.GetEntries(),
                ImportAssetsAsync: (sourcePath, targets, ct) => pickupAssetService.ImportAsync(sourcePath, targets, ct),
                StageReplacement: (chapterStem, match, sourcePath, originalStartSec, originalEndSec) =>
                    polishService.StageReplacement(chapterStem, match, sourcePath, originalStartSec, originalEndSec),
                UnstageReplacement: replacementId => stagingQueueService.Unstage(replacementId),
                RestageReplacement: replacement =>
                {
                    var staged = stagingQueueService.TryStage(replacement, out var validationError);
                    return (staged, validationError);
                },
                GetQueue: chapterStem => stagingQueueService.GetQueue(chapterStem),
                CommitReplacementAsync: async (replacementId, ct) =>
                {
                    var result = await polishService.ApplyReplacementAsync(replacementId, ct).ConfigureAwait(false);
                    return (result.ResultBuffer is not null, result.TimingDeltaSec);
                },
                RevertReplacementAsync: async (replacementId, ct) =>
                {
                    var result = await polishService.RevertReplacementAsync(replacementId, ct).ConfigureAwait(false);
                    return (result.ResultBuffer is not null, result.TimingDeltaSec);
                },
                ReadEdlDocument: (chapterStem, ct) => pickupEdlStore.TryRead(chapterStem, ct),
                ReadArtifactLedgerDocument: (chapterStem, ct) => pickupArtifactLedgerStore.TryRead(chapterStem, ct),
                MutateEdlDocument: (chapterStem, source, mutation, ct) => pickupEdlStore.Mutate(chapterStem, source, mutation, ct),
                TryGetOperation: (document, operationId) => pickupEdlEngine.TryGetOperation(document, operationId),
                TransitionOperationState: (document, operationId, nextState) =>
                    pickupEdlEngine.TransitionOperationState(document, operationId, nextState, DateTime.UtcNow),
                BuildOrderingDiagnostics: document => pickupEdlEngine.BuildDeterministicOrderingDiagnostics(document)))
    {
    }

    internal ProofPickupsSessionService(BlazorWorkspace workspace, RuntimeHooks hooks)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
    }

    public ProofPickupsSessionSnapshot Snapshot => _snapshot;

    public ProofPickupsSessionSnapshot SyncToWorkspace(CancellationToken ct = default)
    {
        if (!TryResolveActiveChapter(out var chapterName, out var chapterStem, out var chapterError))
        {
            _snapshot = _snapshot with
            {
                ActiveChapterName = null,
                ActiveChapterStem = null,
                SourcePath = null,
                Matched = Array.Empty<PickupAsset>(),
                Unmatched = Array.Empty<PickupAsset>(),
                Staged = Array.Empty<StagedReplacement>(),
                Applied = Array.Empty<StagedReplacement>(),
                Reverted = Array.Empty<StagedReplacement>(),
                Failed = Array.Empty<StagedReplacement>(),
                Targets = Array.Empty<CrxPickupTarget>(),
                EdlRevision = null,
                DeterministicOrderingDiagnostics = null,
                LastValidationError = null,
                ArtifactLedgerRevision = null,
                ArtifactLedgerEntries = Array.Empty<PickupArtifactLedgerEntry>(),
                ArtifactLedgerReadError = null,
                LastError = chapterError,
                LastOperationId = null,
                Phase = ProofPickupsSessionPhase.Idle,
                Progress = 0d
            };

            return _snapshot;
        }

        var chapterChanged = !string.Equals(_snapshot.ActiveChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase);

        var (staged, applied, reverted, failed) = GetLifecycleQueues(chapterStem);
        var (revision, orderingDiagnostics, lastValidationError, edlReadError) = ReadEdlDiagnostics(
            chapterStem,
            ct,
            chapterChanged ? null : _snapshot.EdlRevision,
            chapterChanged ? null : _snapshot.DeterministicOrderingDiagnostics,
            chapterChanged ? null : _snapshot.LastValidationError);
        var (ledgerRevision, ledgerEntries, ledgerReadError) = ReadLedgerDiagnostics(
            chapterStem,
            ct,
            chapterChanged ? null : _snapshot.ArtifactLedgerRevision,
            chapterChanged ? Array.Empty<PickupArtifactLedgerEntry>() : _snapshot.ArtifactLedgerEntries);

        _snapshot = _snapshot with
        {
            ActiveChapterName = chapterName,
            ActiveChapterStem = chapterStem,
            SourcePath = chapterChanged
                ? AmsPathResolver.NormalizeOptionalPath(_workspace.PickupSessionPath)
                : _snapshot.SourcePath,
            Matched = chapterChanged ? Array.Empty<PickupAsset>() : _snapshot.Matched,
            Unmatched = chapterChanged ? Array.Empty<PickupAsset>() : _snapshot.Unmatched,
            Targets = chapterChanged ? Array.Empty<CrxPickupTarget>() : _snapshot.Targets,
            Staged = staged,
            Applied = applied,
            Reverted = reverted,
            Failed = failed,
            EdlRevision = revision,
            DeterministicOrderingDiagnostics = orderingDiagnostics,
            LastValidationError = lastValidationError,
            ArtifactLedgerRevision = ledgerRevision,
            ArtifactLedgerEntries = ledgerEntries,
            ArtifactLedgerReadError = ledgerReadError,
            LastError = edlReadError ?? (chapterChanged ? null : _snapshot.LastError),
            LastOperationId = chapterChanged ? null : _snapshot.LastOperationId,
            Phase = chapterChanged ? ProofPickupsSessionPhase.Idle : _snapshot.Phase,
            Progress = chapterChanged ? 0d : _snapshot.Progress
        };

        return _snapshot;
    }

    public async Task<ProofPickupsSessionSnapshot> ImportAsync(string sourcePath, CancellationToken ct = default)
    {
        var synced = SyncToWorkspace(ct);
        if (!synced.HasActiveChapter)
        {
            return _snapshot;
        }

        var chapterName = synced.ActiveChapterName!;
        var chapterStem = synced.ActiveChapterStem!;
        var baseline = synced;

        sourcePath = AmsPathResolver.NormalizeOptionalPath(sourcePath) ?? sourcePath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Pickup source path is required.");
        }

        if (Directory.Exists(sourcePath))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Directory pickup sources are not supported. Provide one stitched pickup WAV file.");
        }

        if (!File.Exists(sourcePath))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Pickup source file is missing: '{sourcePath}'.");
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.ResolvingTargets,
            Progress = 0.15d,
            LastError = null,
            LastOperationId = null
        };

        List<CrxPickupTarget> targets;
        try
        {
            targets = BuildCrxTargets(chapterName, chapterStem);
        }
        catch (Exception ex)
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                ex.Message);
        }

        if (targets.Count == 0)
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"No CRX targets were resolved for chapter '{chapterName}'. Import is blocked.");
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.Importing,
            Progress = 0.55d,
            Targets = targets,
            LastError = null,
            LastOperationId = null
        };

        try
        {
            var (matched, unmatched) = await _hooks
                .ImportAssetsAsync(sourcePath, targets, ct)
                .ConfigureAwait(false);

            var (staged, applied, reverted, failed) = GetLifecycleQueues(chapterStem);
            var (revision, orderingDiagnostics, lastValidationError, edlReadError) = ReadEdlDiagnostics(
                chapterStem,
                ct,
                baseline.EdlRevision,
                baseline.DeterministicOrderingDiagnostics,
                baseline.LastValidationError);
            var (ledgerRevision, ledgerEntries, ledgerReadError) = ReadLedgerDiagnostics(
                chapterStem,
                ct,
                baseline.ArtifactLedgerRevision,
                baseline.ArtifactLedgerEntries);

            _workspace.SetPolishPaths(sourcePath, _workspace.RoomtoneFilePath);

            _snapshot = _snapshot with
            {
                SourcePath = sourcePath,
                Matched = matched.ToArray(),
                Unmatched = unmatched.ToArray(),
                Staged = staged,
                Applied = applied,
                Reverted = reverted,
                Failed = failed,
                EdlRevision = revision,
                DeterministicOrderingDiagnostics = orderingDiagnostics,
                LastValidationError = lastValidationError,
                ArtifactLedgerRevision = ledgerRevision,
                ArtifactLedgerEntries = ledgerEntries,
                ArtifactLedgerReadError = ledgerReadError,
                Phase = ProofPickupsSessionPhase.Completed,
                Progress = 1d,
                LastError = edlReadError,
                LastOperationId = null
            };

            return _snapshot;
        }
        catch (OperationCanceledException)
        {
            _snapshot = baseline with
            {
                Phase = ProofPickupsSessionPhase.Cancelled,
                LastError = "Pickup import cancelled. Prior queue state preserved.",
                LastOperationId = null
            };

            return _snapshot;
        }
        catch (Exception ex)
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Pickup import failed for chapter '{chapterStem}': {ex.Message}");
        }
    }

    public Task<ProofPickupsSessionSnapshot> StageAsync(string assetId, CancellationToken ct = default)
    {
        var previousChapterStem = _snapshot.ActiveChapterStem;
        var synced = SyncToWorkspace(ct);

        if (!synced.HasActiveChapter)
        {
            return Task.FromResult(_snapshot);
        }

        var chapterName = synced.ActiveChapterName!;
        var chapterStem = synced.ActiveChapterStem!;
        var baseline = synced;

        if (!string.IsNullOrWhiteSpace(previousChapterStem)
            && !string.Equals(previousChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Active chapter changed from '{previousChapterStem}' to '{chapterStem}'. Reload pickups before staging."));
        }

        if (string.IsNullOrWhiteSpace(assetId))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Cannot stage pickup: asset id is empty."));
        }

        var asset = baseline.Matched.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, assetId, StringComparison.Ordinal));
        if (asset is null)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot stage pickup '{assetId}': asset is not present in matched queue."));
        }

        var target = ResolveTargetForAsset(baseline.Targets, asset);
        if (target is null)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot stage pickup '{assetId}': no CRX target mapping exists for asset '{assetId}'."));
        }

        if (!string.Equals(target.ChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot stage pickup '{assetId}': CRX target stem '{target.ChapterStem}' does not match active chapter '{chapterStem}'."));
        }

        if (baseline.Staged.Any(item => item.SentenceId == target.SentenceId && item.Status == ReplacementStatus.Staged))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Sentence {target.SentenceId} is already staged. Unstage existing op before restaging."));
        }

        if (target.OriginalEndSec <= target.OriginalStartSec)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot stage pickup '{assetId}': target sentence {target.SentenceId} has invalid original timing range."));
        }

        var sourcePath = ResolveSourcePathForAsset(baseline.SourcePath, asset);
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot stage pickup '{assetId}': source file is missing ('{sourcePath}')."));
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.Staging,
            Progress = 0.7d,
            LastError = null,
            LastOperationId = assetId
        };

        try
        {
            var staged = _hooks.StageReplacement(
                chapterStem,
                BuildPickupMatch(asset, target),
                sourcePath,
                target.OriginalStartSec,
                target.OriginalEndSec);

            var (stagedQueue, appliedQueue, revertedQueue, failedQueue) = GetLifecycleQueues(chapterStem);
            var (revision, orderingDiagnostics, lastValidationError, edlReadError) = ReadEdlDiagnostics(
                chapterStem,
                ct,
                baseline.EdlRevision,
                baseline.DeterministicOrderingDiagnostics,
                baseline.LastValidationError);
            var (ledgerRevision, ledgerEntries, ledgerReadError) = ReadLedgerDiagnostics(
                chapterStem,
                ct,
                baseline.ArtifactLedgerRevision,
                baseline.ArtifactLedgerEntries);

            _snapshot = _snapshot with
            {
                Staged = stagedQueue,
                Applied = appliedQueue,
                Reverted = revertedQueue,
                Failed = failedQueue,
                EdlRevision = revision,
                DeterministicOrderingDiagnostics = orderingDiagnostics,
                LastValidationError = lastValidationError,
                ArtifactLedgerRevision = ledgerRevision,
                ArtifactLedgerEntries = ledgerEntries,
                ArtifactLedgerReadError = ledgerReadError,
                Phase = ProofPickupsSessionPhase.Completed,
                Progress = 1d,
                LastError = edlReadError,
                LastOperationId = staged.Id
            };

            return Task.FromResult(_snapshot);
        }
        catch (Exception ex)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Stage failed for op '{assetId}' in chapter '{chapterStem}': {ex.Message}",
                operationId: assetId));
        }
    }

    public Task<ProofPickupsSessionSnapshot> UnstageAsync(string replacementId, CancellationToken ct = default)
    {
        var previousChapterStem = _snapshot.ActiveChapterStem;
        var synced = SyncToWorkspace(ct);

        if (!synced.HasActiveChapter)
        {
            return Task.FromResult(_snapshot);
        }

        var chapterName = synced.ActiveChapterName!;
        var chapterStem = synced.ActiveChapterStem!;
        var baseline = synced;

        if (!string.IsNullOrWhiteSpace(previousChapterStem)
            && !string.Equals(previousChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Active chapter changed from '{previousChapterStem}' to '{chapterStem}'. Reload pickups before unstaging."));
        }

        if (string.IsNullOrWhiteSpace(replacementId))
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Cannot unstage pickup: replacement id is empty."));
        }

        var stagedItem = baseline.Staged.FirstOrDefault(item =>
            string.Equals(item.Id, replacementId, StringComparison.Ordinal));
        if (stagedItem is null)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot unstage pickup '{replacementId}': op is not in staged queue.",
                operationId: replacementId));
        }

        var document = _hooks.ReadEdlDocument(chapterStem, ct);
        if (document is null)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot unstage pickup '{replacementId}': chapter EDL document is missing.",
                operationId: replacementId));
        }

        var operation = _hooks.TryGetOperation(document, replacementId);
        if (operation is null)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot unstage pickup '{replacementId}': EDL op does not exist.",
                operationId: replacementId));
        }

        if (operation.State != PickupEdlOperationState.Staged)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot unstage pickup '{replacementId}': invalid op transition {operation.State} -> {PickupEdlOperationState.Reverted}.",
                operationId: replacementId));
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.Unstaging,
            Progress = 0.75d,
            LastError = null,
            LastOperationId = replacementId
        };

        try
        {
            if (!_hooks.UnstageReplacement(replacementId))
            {
                return Task.FromResult(FailAndPreserve(
                    baseline,
                    chapterName,
                    chapterStem,
                    $"Cannot unstage pickup '{replacementId}': staging queue update rejected.",
                    operationId: replacementId));
            }

            try
            {
                _ = _hooks.MutateEdlDocument(
                    chapterStem,
                    document.Source,
                    current => _hooks.TransitionOperationState(
                        current,
                        replacementId,
                        PickupEdlOperationState.Reverted),
                    ct);
            }
            catch (Exception ex)
            {
                var rollback = _hooks.RestageReplacement(stagedItem);
                var rollbackStatus = rollback.Success
                    ? "staging queue restored"
                    : $"staging queue restore failed ({rollback.Error ?? "unknown"})";

                return Task.FromResult(FailAndPreserve(
                    baseline,
                    chapterName,
                    chapterStem,
                    $"Cannot unstage pickup '{replacementId}': EDL transition failed ({ex.Message}); {rollbackStatus}.",
                    operationId: replacementId));
            }

            var (stagedQueue, appliedQueue, revertedQueue, failedQueue) = GetLifecycleQueues(chapterStem);
            var (revision, orderingDiagnostics, lastValidationError, edlReadError) = ReadEdlDiagnostics(
                chapterStem,
                ct,
                baseline.EdlRevision,
                baseline.DeterministicOrderingDiagnostics,
                baseline.LastValidationError);
            var (ledgerRevision, ledgerEntries, ledgerReadError) = ReadLedgerDiagnostics(
                chapterStem,
                ct,
                baseline.ArtifactLedgerRevision,
                baseline.ArtifactLedgerEntries);

            _snapshot = _snapshot with
            {
                Staged = stagedQueue,
                Applied = appliedQueue,
                Reverted = revertedQueue,
                Failed = failedQueue,
                EdlRevision = revision,
                DeterministicOrderingDiagnostics = orderingDiagnostics,
                LastValidationError = lastValidationError,
                ArtifactLedgerRevision = ledgerRevision,
                ArtifactLedgerEntries = ledgerEntries,
                ArtifactLedgerReadError = ledgerReadError,
                Phase = ProofPickupsSessionPhase.Completed,
                Progress = 1d,
                LastError = edlReadError,
                LastOperationId = replacementId
            };

            return Task.FromResult(_snapshot);
        }
        catch (Exception ex)
        {
            return Task.FromResult(FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Unstage failed for op '{replacementId}' in chapter '{chapterStem}': {ex.Message}",
                operationId: replacementId));
        }
    }

    public async Task<ProofPickupsSessionSnapshot> CommitAsync(string replacementId, CancellationToken ct = default)
    {
        var previousChapterStem = _snapshot.ActiveChapterStem;
        var synced = SyncToWorkspace(ct);

        if (!synced.HasActiveChapter)
        {
            return _snapshot;
        }

        var chapterName = synced.ActiveChapterName!;
        var chapterStem = synced.ActiveChapterStem!;
        var baseline = synced;

        if (!string.IsNullOrWhiteSpace(previousChapterStem)
            && !string.Equals(previousChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Active chapter changed from '{previousChapterStem}' to '{chapterStem}'. Reload pickups before commit.");
        }

        if (string.IsNullOrWhiteSpace(replacementId))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Cannot commit pickup: replacement id is empty.");
        }

        var stagedItem = baseline.Staged.FirstOrDefault(item =>
            string.Equals(item.Id, replacementId, StringComparison.Ordinal));
        if (stagedItem is null)
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot commit pickup '{replacementId}': op is not in staged queue.",
                operationId: replacementId);
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.Committing,
            Progress = 0.82d,
            LastError = null,
            LastOperationId = replacementId
        };

        try
        {
            var runtimeResult = await _hooks
                .CommitReplacementAsync(replacementId, ct)
                .ConfigureAwait(false);

            if (!runtimeResult.HasResult || !double.IsFinite(runtimeResult.TimingDeltaSec))
            {
                throw new InvalidOperationException(
                    $"Pickup commit returned malformed runtime payload for chapter '{chapterStem}', op '{replacementId}'.");
            }

            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Completed,
                lastError: null,
                operationId: replacementId,
                progress: 1d,
                ct);
        }
        catch (OperationCanceledException)
        {
            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Cancelled,
                lastError: "Pickup commit cancelled. Prior queue state preserved.",
                operationId: replacementId,
                progress: baseline.Progress,
                ct);
        }
        catch (Exception ex)
        {
            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Failed,
                lastError: $"Commit failed for op '{replacementId}' in chapter '{chapterStem}': {ex.Message}",
                operationId: replacementId,
                progress: baseline.Progress,
                ct);
        }
    }

    public async Task<ProofPickupsSessionSnapshot> RevertAsync(string replacementId, CancellationToken ct = default)
    {
        var previousChapterStem = _snapshot.ActiveChapterStem;
        var synced = SyncToWorkspace(ct);

        if (!synced.HasActiveChapter)
        {
            return _snapshot;
        }

        var chapterName = synced.ActiveChapterName!;
        var chapterStem = synced.ActiveChapterStem!;
        var baseline = synced;

        if (!string.IsNullOrWhiteSpace(previousChapterStem)
            && !string.Equals(previousChapterStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Active chapter changed from '{previousChapterStem}' to '{chapterStem}'. Reload pickups before revert.");
        }

        if (string.IsNullOrWhiteSpace(replacementId))
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                "Cannot revert pickup: replacement id is empty.");
        }

        var appliedItem = baseline.Applied.FirstOrDefault(item =>
            string.Equals(item.Id, replacementId, StringComparison.Ordinal));
        if (appliedItem is null)
        {
            return FailAndPreserve(
                baseline,
                chapterName,
                chapterStem,
                $"Cannot revert pickup '{replacementId}': op is not in applied queue.",
                operationId: replacementId);
        }

        _snapshot = _snapshot with
        {
            Phase = ProofPickupsSessionPhase.Reverting,
            Progress = 0.86d,
            LastError = null,
            LastOperationId = replacementId
        };

        try
        {
            var runtimeResult = await _hooks
                .RevertReplacementAsync(replacementId, ct)
                .ConfigureAwait(false);

            if (!runtimeResult.HasResult || !double.IsFinite(runtimeResult.TimingDeltaSec))
            {
                throw new InvalidOperationException(
                    $"Pickup revert returned malformed runtime payload for chapter '{chapterStem}', op '{replacementId}'.");
            }

            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Completed,
                lastError: null,
                operationId: replacementId,
                progress: 1d,
                ct);
        }
        catch (OperationCanceledException)
        {
            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Cancelled,
                lastError: "Pickup revert cancelled. Prior queue state preserved.",
                operationId: replacementId,
                progress: baseline.Progress,
                ct);
        }
        catch (Exception ex)
        {
            return RefreshSnapshotFromRuntime(
                baseline,
                chapterName,
                chapterStem,
                phase: ProofPickupsSessionPhase.Failed,
                lastError: $"Revert failed for op '{replacementId}' in chapter '{chapterStem}': {ex.Message}",
                operationId: replacementId,
                progress: baseline.Progress,
                ct);
        }
    }

    private ProofPickupsSessionSnapshot RefreshSnapshotFromRuntime(
        ProofPickupsSessionSnapshot baseline,
        string chapterName,
        string chapterStem,
        ProofPickupsSessionPhase phase,
        string? lastError,
        string? operationId,
        double progress,
        CancellationToken ct)
    {
        var (staged, applied, reverted, failed) = GetLifecycleQueues(chapterStem);
        var (revision, orderingDiagnostics, lastValidationError, edlReadError) = ReadEdlDiagnostics(
            chapterStem,
            ct,
            baseline.EdlRevision,
            baseline.DeterministicOrderingDiagnostics,
            baseline.LastValidationError);
        var (ledgerRevision, ledgerEntries, ledgerReadError) = ReadLedgerDiagnostics(
            chapterStem,
            ct,
            baseline.ArtifactLedgerRevision,
            baseline.ArtifactLedgerEntries);

        _snapshot = baseline with
        {
            ActiveChapterName = chapterName,
            ActiveChapterStem = chapterStem,
            Staged = staged,
            Applied = applied,
            Reverted = reverted,
            Failed = failed,
            EdlRevision = revision,
            DeterministicOrderingDiagnostics = orderingDiagnostics,
            LastValidationError = lastValidationError,
            ArtifactLedgerRevision = ledgerRevision,
            ArtifactLedgerEntries = ledgerEntries,
            ArtifactLedgerReadError = ledgerReadError,
            Phase = phase,
            Progress = progress,
            LastError = lastError ?? edlReadError,
            LastOperationId = operationId ?? baseline.LastOperationId
        };

        return _snapshot;
    }

    private ProofPickupsSessionSnapshot FailAndPreserve(
        ProofPickupsSessionSnapshot baseline,
        string chapterName,
        string chapterStem,
        string message,
        string? operationId = null)
    {
        _snapshot = baseline with
        {
            ActiveChapterName = chapterName,
            ActiveChapterStem = chapterStem,
            Phase = ProofPickupsSessionPhase.Failed,
            Progress = baseline.Progress,
            LastError = message,
            LastOperationId = operationId ?? baseline.LastOperationId
        };

        return _snapshot;
    }

    private (int? Revision, string? OrderingDiagnostics, string? LastValidationError, string? ReadError) ReadEdlDiagnostics(
        string chapterStem,
        CancellationToken ct,
        int? fallbackRevision,
        string? fallbackOrderingDiagnostics,
        string? fallbackLastValidationError)
    {
        try
        {
            var document = _hooks.ReadEdlDocument(chapterStem, ct);
            if (document is null)
            {
                return (null, null, null, null);
            }

            var lastValidationError = document.Operations
                .Where(operation => operation.State == PickupEdlOperationState.Failed)
                .OrderByDescending(operation => operation.UpdatedAtUtc)
                .Select(operation => operation.Id)
                .FirstOrDefault();

            return (
                document.Revision,
                _hooks.BuildOrderingDiagnostics(document),
                lastValidationError,
                null);
        }
        catch (Exception ex)
        {
            return (
                fallbackRevision,
                fallbackOrderingDiagnostics,
                fallbackLastValidationError,
                $"EDL read failed for chapter '{chapterStem}': {ex.Message}");
        }
    }

    private (int? Revision, IReadOnlyList<PickupArtifactLedgerEntry> Entries, string? ReadError) ReadLedgerDiagnostics(
        string chapterStem,
        CancellationToken ct,
        int? fallbackRevision,
        IReadOnlyList<PickupArtifactLedgerEntry> fallbackEntries)
    {
        try
        {
            var document = _hooks.ReadArtifactLedgerDocument(chapterStem, ct);
            if (document is null)
            {
                return (null, Array.Empty<PickupArtifactLedgerEntry>(), null);
            }

            return (
                document.Revision,
                document.GetDeterministicOrder(),
                null);
        }
        catch (Exception ex)
        {
            var preservedEntries = fallbackEntries?.ToArray() ?? Array.Empty<PickupArtifactLedgerEntry>();
            return (
                fallbackRevision,
                preservedEntries,
                $"Artifact ledger read failed for chapter '{chapterStem}': {ex.Message}");
        }
    }

    private (
        IReadOnlyList<StagedReplacement> Staged,
        IReadOnlyList<StagedReplacement> Applied,
        IReadOnlyList<StagedReplacement> Reverted,
        IReadOnlyList<StagedReplacement> Failed) GetLifecycleQueues(string chapterStem)
    {
        var ordered = _hooks.GetQueue(chapterStem)
            .OrderBy(item => item.OriginalStartSec)
            .ThenBy(item => item.SentenceId)
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .ToArray();

        return (
            ordered.Where(item => item.Status == ReplacementStatus.Staged).ToArray(),
            ordered.Where(item => item.Status == ReplacementStatus.Applied).ToArray(),
            ordered.Where(item => item.Status == ReplacementStatus.Reverted).ToArray(),
            ordered.Where(item => item.Status == ReplacementStatus.Failed).ToArray());
    }

    private static string ResolveSourcePathForAsset(string? sessionSourcePath, PickupAsset asset)
    {
        if (!string.IsNullOrWhiteSpace(asset.SourceFilePath))
        {
            return asset.SourceFilePath;
        }

        return sessionSourcePath ?? string.Empty;
    }

    private static PickupMatch BuildPickupMatch(PickupAsset asset, CrxPickupTarget target)
    {
        return new PickupMatch(
            SentenceId: target.SentenceId,
            PickupStartSec: asset.TrimStartSec,
            PickupEndSec: asset.TrimEndSec,
            Confidence: asset.Confidence,
            RecognizedText: string.Empty,
            ErrorNumber: target.ErrorNumber,
            IsLowConfidence: false);
    }

    private static CrxPickupTarget? ResolveTargetForAsset(
        IReadOnlyList<CrxPickupTarget> targets,
        PickupAsset asset)
    {
        if (asset.MatchedErrorNumber is int errorNumber)
        {
            var byError = targets.FirstOrDefault(target => target.ErrorNumber == errorNumber);
            if (byError is not null)
            {
                return byError;
            }
        }

        if (asset.MatchedSentenceId is int sentenceId)
        {
            var bySentence = targets.FirstOrDefault(target => target.SentenceId == sentenceId);
            if (bySentence is not null)
            {
                return bySentence;
            }
        }

        return null;
    }

    private bool TryResolveActiveChapter(
        out string chapterName,
        out string chapterStem,
        out string error)
    {
        chapterName = string.Empty;
        chapterStem = string.Empty;
        error = string.Empty;

        if (!_workspace.IsInitialized || !_workspace.HasBookIndex)
        {
            var persistedStateError = _workspace.LastWorkspaceStateLoadError;
            error = string.IsNullOrWhiteSpace(persistedStateError)
                ? "Workspace and book index are required before pickup actions."
                : $"Workspace and book index are required before pickup actions. {persistedStateError}";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_workspace.CurrentChapterName))
        {
            var projectStateError = _workspace.LastProjectStateLoadError;
            error = string.IsNullOrWhiteSpace(projectStateError)
                ? "Select an active chapter before pickup import or staging."
                : $"Select an active chapter before pickup import or staging. {projectStateError}";
            return false;
        }

        chapterName = _workspace.CurrentChapterName.Trim();
        chapterStem = _workspace.GetStemForChapter(chapterName) ?? chapterName;

        if (string.IsNullOrWhiteSpace(chapterStem))
        {
            error = $"Unable to resolve chapter stem for '{chapterName}'.";
            return false;
        }

        return true;
    }

    private List<CrxPickupTarget> BuildCrxTargets(string chapterName, string chapterStem)
    {
        if (!_workspace.TryGetHydratedTranscript(chapterName, out var hydrated) || hydrated is null)
        {
            throw new InvalidOperationException(
                $"Hydrated transcript is required for chapter '{chapterName}' before pickup import.");
        }

        var chapterEntries = _hooks.GetCrxEntries()
            .Where(entry => ChapterMatches(entry.Chapter, chapterName))
            .OrderBy(entry => entry.ErrorNumber)
            .ToList();

        if (chapterEntries.Count == 0)
        {
            throw new InvalidOperationException(
                $"No CRX targets found for chapter '{chapterName}'.");
        }

        var seenErrors = new HashSet<int>();
        var targets = new List<CrxPickupTarget>(chapterEntries.Count);

        for (var index = 0; index < chapterEntries.Count; index++)
        {
            var row = chapterEntries[index];
            var rowContext = BuildRowContext(index, row);

            if (row.ErrorNumber <= 0)
            {
                throw new InvalidOperationException(
                    $"Malformed CRX mapping at {rowContext}: error number must be greater than zero.");
            }

            if (!seenErrors.Add(row.ErrorNumber))
            {
                throw new InvalidOperationException(
                    $"Malformed CRX mapping at {rowContext}: duplicate CRX error number '{row.ErrorNumber}'.");
            }

            var sentence = ResolveSentenceFromCrxEntry(row, hydrated.Sentences);
            if (sentence is null)
            {
                throw new InvalidOperationException(
                    $"Malformed CRX mapping at {rowContext}: could not resolve sentence target.");
            }

            if (sentence.Timing is null || sentence.Timing.EndSec <= sentence.Timing.StartSec)
            {
                throw new InvalidOperationException(
                    $"Malformed CRX mapping at {rowContext}: sentence '{sentence.Id}' has invalid timing.");
            }

            var shouldBeText = row.ShouldBe
                ?? row.ReadAs
                ?? CrxCommentParser.TryParseShouldBe(row.Comments)
                ?? CrxCommentParser.TryParseReadAs(row.Comments)
                ?? sentence.BookText
                ?? string.Empty;

            targets.Add(new CrxPickupTarget(
                ErrorNumber: row.ErrorNumber,
                ChapterStem: chapterStem,
                ChapterName: chapterName,
                SentenceId: sentence.Id,
                ShouldBeText: shouldBeText,
                BookText: sentence.BookText ?? string.Empty,
                OriginalStartSec: sentence.Timing.StartSec,
                OriginalEndSec: sentence.Timing.EndSec));
        }

        return targets;
    }

    private static string BuildRowContext(int index, CrxEntry row)
    {
        var errorNumber = row.ErrorNumber <= 0 ? "n/a" : row.ErrorNumber.ToString();
        return $"row {index + 1} (error #{errorNumber})";
    }

    private static bool ChapterMatches(string? crxChapter, string? currentChapter)
        => CrxChapterMatcher.Matches(crxChapter, currentChapter);

    private static HydratedSentence? ResolveSentenceFromCrxEntry(
        CrxEntry entry,
        IReadOnlyList<HydratedSentence> sentences)
    {
        if (sentences.Count == 0)
        {
            return null;
        }

        if (entry.SentenceId > 0)
        {
            var direct = sentences.FirstOrDefault(sentence => sentence.Id == entry.SentenceId);
            if (direct is not null)
            {
                return direct;
            }
        }

        var timeSec = entry.StartTime > 0 ? entry.StartTime : TryParseTimecode(entry.Timecode);
        if (timeSec.HasValue)
        {
            var nearest = sentences
                .Where(sentence => sentence.Timing is not null)
                .OrderBy(sentence => DistanceToSentenceCenter(sentence, timeSec.Value))
                .FirstOrDefault();

            if (nearest is not null)
            {
                return nearest;
            }
        }

        return null;
    }

    private static double? TryParseTimecode(string? timecode)
    {
        if (string.IsNullOrWhiteSpace(timecode))
        {
            return null;
        }

        return TimeSpan.TryParse(timecode, out var parsed)
            ? parsed.TotalSeconds
            : null;
    }

    private static double DistanceToSentenceCenter(HydratedSentence sentence, double timeSec)
    {
        if (sentence.Timing is null)
        {
            return double.MaxValue;
        }

        var center = (sentence.Timing.StartSec + sentence.Timing.EndSec) / 2.0;
        return Math.Abs(center - timeSec);
    }

    internal sealed record RuntimeHooks(
        Func<IReadOnlyList<CrxEntry>> GetCrxEntries,
        Func<string, IReadOnlyList<CrxPickupTarget>, CancellationToken, Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)>> ImportAssetsAsync,
        Func<string, PickupMatch, string, double, double, StagedReplacement> StageReplacement,
        Func<string, bool> UnstageReplacement,
        Func<StagedReplacement, (bool Success, string? Error)> RestageReplacement,
        Func<string, IReadOnlyList<StagedReplacement>> GetQueue,
        Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> CommitReplacementAsync,
        Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> RevertReplacementAsync,
        Func<string, CancellationToken, PickupEdlDocument?> ReadEdlDocument,
        Func<string, CancellationToken, PickupArtifactLedgerDocument?> ReadArtifactLedgerDocument,
        Func<string, PickupEdlSourceReference, Func<PickupEdlDocument, PickupEdlDocument>, CancellationToken, PickupEdlDocument> MutateEdlDocument,
        Func<PickupEdlDocument, string, PickupEdlOperation?> TryGetOperation,
        Func<PickupEdlDocument, string, PickupEdlOperationState, PickupEdlDocument> TransitionOperationState,
        Func<PickupEdlDocument, string> BuildOrderingDiagnostics);
}
