using Ams.Core.Application.Commands;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;
using Ams.Workstation.Server.Services.Pickups.Fit;
using Ams.Workstation.Server.Services.Pickups.Pick;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofPickupsChapterFitServiceTests
{
    [Fact]
    public async Task LoadOrCreateFitPlanAsync_ConfirmedPickMap_CreatesStableRowsAndReloadsFromStore()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 202, sentenceId: 21));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var late = targets.Single(target => target.ErrorNumber == 202);
            var early = targets.Single(target => target.ErrorNumber == 101);
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[
                    harness.CreateAsset("seg-202", sourcePath, late, 0.70, 1.10),
                    harness.CreateAsset("seg-101", sourcePath, early, 0.10, 0.40)
                ],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        var confirmed = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var reloaded = harness.CreateSessionService().SyncToWorkspace(CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, snapshot.Phase);
        Assert.Null(snapshot.LastError);
        Assert.NotNull(snapshot.FitPlan);
        Assert.Equal(1, snapshot.FitPlanRevision);
        Assert.StartsWith("fit-load-or-create-", snapshot.LastFitOperationId, StringComparison.Ordinal);
        Assert.Null(snapshot.LastFitValidationError);
        Assert.Equal(2, snapshot.FitAssignmentCountsByStatus[PickupFitPlanItemStatus.Draft]);
        Assert.Equal(["seg-101", "seg-202"], snapshot.FitPlan!.Items.Select(item => item.PickAssignmentId));
        Assert.Equal(["seg-101", "seg-202"], reloaded.FitPlan!.Items.Select(item => item.PickAssignmentId));
        Assert.Equal(snapshot.FitPlan.PickAssignmentsFingerprint, reloaded.FitPlan.PickAssignmentsFingerprint);
        Assert.Equal(confirmed.PickMapRevision, snapshot.FitPlan.PickMapRevision);
        Assert.Equal(0, harness.StageCallCount);
        Assert.True(File.Exists(harness.FitPlanStore.GetDocumentPath(harness.ActiveChapterStem)));
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_StaleEmptyStoredPlan_PersistsRegeneratedRows()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        var confirmed = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var oldPickMap = confirmed.PickMap!;
        var now = DateTime.UtcNow;
        var emptyPlan = new PickupFitPlanDocument(
            schemaVersion: PickupFitPlanDocument.CurrentSchemaVersion,
            chapterStem: harness.ActiveChapterStem,
            revision: 0,
            source: oldPickMap.Source,
            pickMapRevision: oldPickMap.Revision,
            pickAssignmentsFingerprint: PickupFitPlanDocument.ComputePickAssignmentsFingerprint(harness.ActiveChapterStem, oldPickMap),
            items: Array.Empty<PickupFitPlanItem>(),
            createdAtUtc: now,
            updatedAtUtc: now,
            lastOperationId: "fit-old-empty",
            lastValidationError: null,
            isDraft: true);
        var savedEmpty = harness.FitPlanStore.Save(harness.ActiveChapterStem, oldPickMap, emptyPlan, CancellationToken.None);
        Assert.Empty(savedEmpty.Items);

        var overridden = await harness.Service.SetPickAssignmentTargetAsync(
            assignmentId: "seg-101",
            expectedRevision: confirmed.PickMapRevision!.Value,
            chapterStem: harness.ActiveChapterStem,
            errorNumber: 101,
            note: "refresh pick fingerprint",
            ct: CancellationToken.None);
        var reconfirmed = await harness.Service.ConfirmPickMapAsync(overridden.PickMapRevision!.Value, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var reloaded = harness.CreateSessionService().SyncToWorkspace(CancellationToken.None);

        var item = Assert.Single(snapshot.FitPlan!.Items);
        Assert.Equal("seg-101", item.PickAssignmentId);
        Assert.Equal(["seg-101"], reloaded.FitPlan!.Items.Select(fit => fit.PickAssignmentId));
        Assert.Equal(reconfirmed.PickMapRevision, reloaded.FitPlan.PickMapRevision);
        Assert.Equal(
            PickupFitPlanDocument.ComputePickAssignmentsFingerprint(harness.ActiveChapterStem, reconfirmed.PickMap!),
            reloaded.FitPlan.PickAssignmentsFingerprint);
        Assert.StartsWith("fit-load-or-create-", reloaded.FitPlan.LastOperationId, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_DraftPickMap_FailsClosedWithoutFitMutation()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        _ = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, snapshot.Phase);
        Assert.Null(snapshot.FitPlan);
        Assert.Null(snapshot.FitPlanRevision);
        Assert.Contains("draft Pick map", snapshot.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(harness.FitPlanStore.GetDocumentPath(harness.ActiveChapterStem)));
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_OverrideTarget_UsesEffectiveTargetForSelectedChapter()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-02", errorNumber: 202, sentenceId: 21));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var chapterOne = targets.Single(target => target.ErrorNumber == 101);
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, chapterOne, 0.00, 0.35)],
                Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-extra", sourcePath, 0.55, 0.95)]));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        var overridden = await harness.Service.SetPickAssignmentTargetAsync(
            assignmentId: "seg-extra",
            expectedRevision: imported.PickMapRevision!.Value,
            chapterStem: "chapter-02",
            errorNumber: 202,
            note: "manual chapter-two fit",
            ct: CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(overridden.PickMapRevision!.Value, CancellationToken.None);
        harness.SelectChapterByStem("chapter-02");

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        var item = Assert.Single(snapshot.FitPlan!.Items);
        Assert.Equal("seg-extra", item.PickAssignmentId);
        Assert.Equal("seg-extra", item.PickupSegmentId);
        Assert.Equal("chapter-02", item.Target.ChapterStem);
        Assert.Equal(202, item.Target.ErrorNumber);
        Assert.Equal(0.55, item.InnerRange.StartSec, precision: 3);
        Assert.Equal(0.95, item.InnerRange.EndSec, precision: 3);
    }

    [Theory]
    [InlineData(PickupPickMapAssignmentStatus.Rejected)]
    [InlineData(PickupPickMapAssignmentStatus.Deferred)]
    [InlineData(PickupPickMapAssignmentStatus.NotApplicable)]
    public async Task LoadOrCreateFitPlanAsync_TerminalDispositionRows_AreOmittedFromFitRows(
        PickupPickMapAssignmentStatus disposition)
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.00, 0.35)],
                Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-terminal", sourcePath, 0.55, 0.95)]));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        var dispositioned = await harness.Service.SetPickAssignmentDispositionAsync(
            assignmentId: "seg-terminal",
            expectedRevision: imported.PickMapRevision!.Value,
            disposition: disposition,
            note: "not needed for fit",
            ct: CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(dispositioned.PickMapRevision!.Value, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        var item = Assert.Single(snapshot.FitPlan!.Items);
        Assert.Equal("seg-101", item.PickAssignmentId);
        Assert.DoesNotContain(snapshot.FitPlan.Items, fit => fit.PickAssignmentId == "seg-terminal");
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_NoActiveChapterTargetRows_FailsClosed()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-02", errorNumber: 202, sentenceId: 21));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var chapterTwo = targets.Single(target => target.ErrorNumber == 202);
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-202", sourcePath, chapterTwo, 0.20, 0.60)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        Assert.Equal("chapter-01", snapshot.ActiveChapterStem);
        Assert.Equal(ProofPickupsSessionPhase.Failed, snapshot.Phase);
        Assert.Null(snapshot.FitPlan);
        Assert.Contains("No Fit-ready Pick assignments", snapshot.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_StalePickSource_FailsBeforeFitMutation()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        harness.AppendToPickupSource();

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, snapshot.Phase);
        Assert.Null(snapshot.FitPlan);
        Assert.Contains("source is stale", snapshot.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(harness.FitPlanStore.GetDocumentPath(harness.ActiveChapterStem)));
    }

    [Fact]
    public async Task LoadOrCreateFitPlanAsync_SaveFailure_SurfacesFitDiagnosticsWithoutStaging()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };
        harness.SaveFitPlanBehavior = static (_, _, _, _) => throw new IOException("simulated fit-plan save failure");

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);

        var snapshot = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, snapshot.Phase);
        Assert.Null(snapshot.FitPlan);
        Assert.Contains("simulated fit-plan save failure", snapshot.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.StageCallCount);
    }

    [Fact]
    public async Task FitPreviewAcceptCommitAsync_Success_PersistsContextAcceptanceAndCommitState()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 202, sentenceId: 21));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 303, sentenceId: 31));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var ordered = targets.OrderBy(target => target.SentenceId).ToArray();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[
                    harness.CreateAsset("seg-101", sourcePath, ordered[0], 0.10, 0.40),
                    harness.CreateAsset("seg-202", sourcePath, ordered[1], 0.50, 0.90),
                    harness.CreateAsset("seg-303", sourcePath, ordered[2], 1.00, 1.30)
                ],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };
        harness.GenerateFitPreviewBehavior = static (_, _, _, _) => Task.FromResult(new FitReplacementPreviewResult(
            ResultBuffer: new AudioBuffer(channels: 1, sampleRate: 1_000, length: 1_250),
            PreviewVersion: 42,
            RenderedDurationSec: 1.250,
            ChapterStartSec: 0.10,
            ChapterEndSec: 1.70));

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var middle = loaded.FitPlan!.Items.Single(item => item.Target.SentenceId == 21);

        var previewed = await harness.Service.GenerateFitPreviewAsync(
            middle.FitItemId,
            loaded.FitPlanRevision!.Value,
            CancellationToken.None);
        var previewedItem = previewed.FitPlan!.Items.Single(item => item.FitItemId == middle.FitItemId);

        Assert.Equal(ProofPickupsSessionPhase.Completed, previewed.Phase);
        Assert.Equal(1, harness.FitPreviewCallCount);
        Assert.Equal(PickupFitPlanItemStatus.Previewed, previewedItem.Status);
        Assert.NotNull(previewedItem.PreviewEvidence);
        Assert.Equal(42, previewedItem.PreviewEvidence!.PreviewVersion);
        Assert.NotNull(previewedItem.PreviewEvidence.FitStateFingerprint);
        Assert.Equal(["previous", "current", "next"], previewedItem.PreviewEvidence.SentenceContexts.Select(context => context.Role));
        Assert.False(previewedItem.Acceptance.IsAccepted);
        Assert.Equal(PickupFitCommitStatus.NotReady, previewedItem.Commit.Status);

        var accepted = await harness.Service.AcceptFitPreviewAsync(
            middle.FitItemId,
            previewed.FitPlanRevision!.Value,
            previewedItem.PreviewEvidence.PreviewVersion,
            acceptedBy: "operator-1",
            ct: CancellationToken.None);
        var acceptedItem = accepted.FitPlan!.Items.Single(item => item.FitItemId == middle.FitItemId);

        Assert.Equal(PickupFitPlanItemStatus.CommitReady, acceptedItem.Status);
        Assert.True(acceptedItem.Acceptance.IsAccepted);
        Assert.Equal(42, acceptedItem.Acceptance.AcceptedPreviewVersion);
        Assert.Equal(PickupFitCommitStatus.Ready, acceptedItem.Commit.Status);
        Assert.Equal(middle.ReplacementId, acceptedItem.Commit.OperationId);

        harness.CommitFitBehavior = (fitPlan, fitItem, _, _) =>
        {
            harness.SetArtifactLedgerDocument(
                fitPlan.ChapterStem,
                harness.CreateLedgerDocument(
                    fitPlan.ChapterStem,
                    [harness.CreateLedgerEntry(1, fitItem.ReplacementId, PickupArtifactLedgerTransitions.CommitSuccess, edlRevision: 7)],
                    revision: 3));

            return Task.FromResult(new FitReplacementCommitResult(
                ResultBuffer: new AudioBuffer(channels: 1, sampleRate: 1_000, length: 1_000),
                TimingDeltaSec: 0.100,
                OperationId: fitItem.ReplacementId,
                RenderedReplacementDurationSec: 0.500,
                EdlRevision: 7));
        };

        var committed = await harness.Service.CommitFitAsync(
            middle.FitItemId,
            accepted.FitPlanRevision!.Value,
            CancellationToken.None);
        var committedItem = committed.FitPlan!.Items.Single(item => item.FitItemId == middle.FitItemId);

        Assert.Equal(ProofPickupsSessionPhase.Completed, committed.Phase);
        Assert.Equal(1, harness.FitCommitCallCount);
        Assert.Equal(PickupFitPlanItemStatus.Committed, committedItem.Status);
        Assert.Equal(PickupFitCommitStatus.Committed, committedItem.Commit.Status);
        Assert.Equal(middle.ReplacementId, committedItem.Commit.OperationId);
        Assert.Equal(7, committedItem.Commit.EdlRevision);
        Assert.Equal(1, committedItem.Commit.LedgerSequence);
        Assert.Equal(middle.ReplacementId, committed.LastOperationId);
        Assert.Equal(3, committed.ArtifactLedgerRevision);
        Assert.Single(committed.ArtifactLedgerEntries);
        Assert.Null(committed.LastFitValidationError);
    }

    [Fact]
    public async Task SetFitInnerBoundaryAsync_AfterPreview_ClearsAcceptanceAndBlocksOldPreviewAccept()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };
        harness.GenerateFitPreviewBehavior = static (_, _, _, _) => Task.FromResult(new FitReplacementPreviewResult(
            ResultBuffer: new AudioBuffer(channels: 1, sampleRate: 1_000, length: 500),
            PreviewVersion: 5,
            RenderedDurationSec: 0.500,
            ChapterStartSec: 0.10,
            ChapterEndSec: 0.50));

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var item = Assert.Single(loaded.FitPlan!.Items);
        var previewed = await harness.Service.GenerateFitPreviewAsync(item.FitItemId, loaded.FitPlanRevision!.Value, CancellationToken.None);
        var previewVersion = Assert.Single(previewed.FitPlan!.Items).PreviewEvidence!.PreviewVersion;

        var changed = await harness.Service.SetFitInnerBoundaryAsync(
            item.FitItemId,
            previewed.FitPlanRevision!.Value,
            startSec: 0.12,
            endSec: 0.38,
            ct: CancellationToken.None);
        var changedItem = Assert.Single(changed.FitPlan!.Items);

        Assert.Equal(PickupFitPlanItemStatus.Fitted, changedItem.Status);
        Assert.Null(changedItem.PreviewEvidence);
        Assert.False(changedItem.Acceptance.IsAccepted);
        Assert.Equal(PickupFitCommitStatus.NotReady, changedItem.Commit.Status);

        var rejected = await harness.Service.AcceptFitPreviewAsync(
            item.FitItemId,
            changed.FitPlanRevision!.Value,
            previewVersion,
            ct: CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, rejected.Phase);
        Assert.Contains("no preview evidence", rejected.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, harness.FitPreviewCallCount);
    }

    [Fact]
    public async Task CommitFitAsync_RuntimeFailure_PersistsFailedStateAndStableReplacementId()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };
        harness.CommitFitBehavior = static (_, _, _, _) => throw new InvalidOperationException("simulated fit runtime failure");

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var item = Assert.Single(loaded.FitPlan!.Items);
        var previewed = await harness.Service.GenerateFitPreviewAsync(item.FitItemId, loaded.FitPlanRevision!.Value, CancellationToken.None);
        var previewedItem = Assert.Single(previewed.FitPlan!.Items);
        var accepted = await harness.Service.AcceptFitPreviewAsync(
            item.FitItemId,
            previewed.FitPlanRevision!.Value,
            previewedItem.PreviewEvidence!.PreviewVersion,
            ct: CancellationToken.None);

        var failed = await harness.Service.CommitFitAsync(
            item.FitItemId,
            accepted.FitPlanRevision!.Value,
            CancellationToken.None);
        var failedItem = Assert.Single(failed.FitPlan!.Items);

        Assert.Equal(ProofPickupsSessionPhase.Failed, failed.Phase);
        Assert.Equal(1, harness.FitCommitCallCount);
        Assert.Equal(PickupFitPlanItemStatus.Failed, failedItem.Status);
        Assert.Equal(PickupFitCommitStatus.Failed, failedItem.Commit.Status);
        Assert.Equal(item.ReplacementId, failedItem.Commit.OperationId);
        Assert.Equal(item.ReplacementId, failedItem.ReplacementId);
        Assert.Contains("simulated fit runtime failure", failed.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("simulated fit runtime failure", failedItem.CommitError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommitFitAsync_RuntimeCancellationAfterStart_PersistsFailedStateAndRefreshesDiagnostics()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        using var cts = new CancellationTokenSource();
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };
        harness.CommitFitBehavior = (fitPlan, fitItem, _, _) =>
        {
            harness.SetArtifactLedgerDocument(
                fitPlan.ChapterStem,
                harness.CreateLedgerDocument(
                    fitPlan.ChapterStem,
                    [
                        harness.CreateLedgerEntry(
                            sequence: 1,
                            operationId: fitItem.ReplacementId,
                            transition: PickupArtifactLedgerTransitions.CommitCancelled,
                            edlRevision: 9,
                            queueStatus: ReplacementStatus.Failed,
                            edlState: PickupEdlOperationState.Failed,
                            rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Succeeded,
                            failureReason: "runtime cancellation after rollback")
                    ],
                    revision: 5));

            cts.Cancel();
            throw new OperationCanceledException("runtime cancellation after start", cts.Token);
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var item = Assert.Single(loaded.FitPlan!.Items);
        var previewed = await harness.Service.GenerateFitPreviewAsync(item.FitItemId, loaded.FitPlanRevision!.Value, CancellationToken.None);
        var previewedItem = Assert.Single(previewed.FitPlan!.Items);
        var accepted = await harness.Service.AcceptFitPreviewAsync(
            item.FitItemId,
            previewed.FitPlanRevision!.Value,
            previewedItem.PreviewEvidence!.PreviewVersion,
            ct: CancellationToken.None);

        var failed = await harness.Service.CommitFitAsync(
            item.FitItemId,
            accepted.FitPlanRevision!.Value,
            cts.Token);
        var failedItem = Assert.Single(failed.FitPlan!.Items);

        Assert.Equal(ProofPickupsSessionPhase.Failed, failed.Phase);
        Assert.Equal(1, harness.FitCommitCallCount);
        Assert.Equal(PickupFitPlanItemStatus.Failed, failedItem.Status);
        Assert.Equal(PickupFitCommitStatus.Failed, failedItem.Commit.Status);
        Assert.Equal(item.ReplacementId, failedItem.Commit.OperationId);
        Assert.Equal(item.ReplacementId, failed.LastOperationId);
        Assert.Equal(5, failed.ArtifactLedgerRevision);
        var ledgerEntry = Assert.Single(failed.ArtifactLedgerEntries);
        Assert.Equal(PickupArtifactLedgerTransitions.CommitCancelled, ledgerEntry.Transition);
        Assert.Contains("runtime cancellation", ledgerEntry.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cancelled after start", failed.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cancelled after start", failedItem.CommitError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommitFitAsync_UnacceptedItem_FailsClosedWithoutRuntimeCall()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var item = Assert.Single(loaded.FitPlan!.Items);

        var rejected = await harness.Service.CommitFitAsync(
            item.FitItemId,
            loaded.FitPlanRevision!.Value,
            CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, rejected.Phase);
        Assert.Contains("accepted preview evidence is required", rejected.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.FitCommitCallCount);
        Assert.Equal(PickupFitPlanItemStatus.Draft, Assert.Single(rejected.FitPlan!.Items).Status);
    }

    [Fact]
    public async Task CommitFitAsync_StaleChapterSwitch_IsRejectedBeforeRuntimeCall()
    {
        using var harness = await ChapterFitHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var target = targets.Single(target => target.ChapterStem == "chapter-01");
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, target, 0.10, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        _ = await harness.Service.ConfirmPickMapAsync(imported.PickMapRevision!.Value, CancellationToken.None);
        var loaded = await harness.Service.LoadOrCreateFitPlanAsync(CancellationToken.None);
        var item = Assert.Single(loaded.FitPlan!.Items);
        harness.SelectChapterByStem("chapter-02");

        var rejected = await harness.Service.CommitFitAsync(
            item.FitItemId,
            loaded.FitPlanRevision!.Value,
            CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, rejected.Phase);
        Assert.Contains("Active chapter changed", rejected.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Reload pickups before Fit commit", rejected.LastFitValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.FitCommitCallCount);
    }

    private sealed class ChapterFitHarness : IDisposable
    {
        private readonly string _root;
        private readonly Dictionary<string, List<StagedReplacement>> _queues = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PickupArtifactLedgerDocument?> _ledgerByChapter = new(StringComparer.OrdinalIgnoreCase);

        private ChapterFitHarness(string root, BlazorWorkspace workspace, PickupPickMapStore pickMapStore, PickupFitPlanStore fitPlanStore)
        {
            _root = root;
            Workspace = workspace;
            PickMapStore = pickMapStore;
            FitPlanStore = fitPlanStore;
            PickupPath = Path.Combine(root, ".pickups", "session.wav");
            ImportPickAssetsBehavior = static (_, _, _) => Task.FromResult(((IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(), (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
            ReadFitPlanBehavior = (chapterStem, ct) => FitPlanStore.TryRead(chapterStem, ct);
            LoadOrCreateFitPlanBehavior = (chapterStem, pickMap, ct) => FitPlanStore.LoadOrCreate(chapterStem, pickMap, ct);
            SaveFitPlanBehavior = (chapterStem, pickMap, document, ct) => FitPlanStore.Save(chapterStem, pickMap, document, ct);
            GenerateFitPreviewBehavior = static (_, _, _, _) => Task.FromResult(new FitReplacementPreviewResult(
                ResultBuffer: new AudioBuffer(channels: 1, sampleRate: 1_000, length: 100),
                PreviewVersion: 1,
                RenderedDurationSec: 0.100,
                ChapterStartSec: 0,
                ChapterEndSec: 0.100));
            CommitFitBehavior = static (_, fitItem, _, _) => Task.FromResult(new FitReplacementCommitResult(
                ResultBuffer: new AudioBuffer(channels: 1, sampleRate: 1_000, length: 100),
                TimingDeltaSec: 0,
                OperationId: fitItem.ReplacementId,
                RenderedReplacementDurationSec: fitItem.InnerRange.DurationSec,
                EdlRevision: 1));
            Service = CreateSessionService();
        }

        public BlazorWorkspace Workspace { get; }

        public PickupPickMapStore PickMapStore { get; }

        public PickupFitPlanStore FitPlanStore { get; }

        public ProofPickupsSessionService Service { get; }

        public string PickupPath { get; }

        public List<CrxEntry> CrxEntries { get; } = [];

        public int StageCallCount { get; private set; }

        public int FitPreviewCallCount { get; private set; }

        public int FitCommitCallCount { get; private set; }

        public Func<string, IReadOnlyList<CrxPickupTarget>, CancellationToken, Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)>> ImportPickAssetsBehavior { get; set; }

        public Func<string, CancellationToken, PickupFitPlanDocument?> ReadFitPlanBehavior { get; set; }

        public Func<string, PickupPickMapDocument, CancellationToken, PickupFitPlanDocument> LoadOrCreateFitPlanBehavior { get; set; }

        public Func<string, PickupPickMapDocument, PickupFitPlanDocument, CancellationToken, PickupFitPlanDocument> SaveFitPlanBehavior { get; set; }

        public Func<PickupFitPlanDocument, PickupFitPlanItem, string?, CancellationToken, Task<FitReplacementPreviewResult>> GenerateFitPreviewBehavior { get; set; }

        public Func<PickupFitPlanDocument, PickupFitPlanItem, string?, CancellationToken, Task<FitReplacementCommitResult>> CommitFitBehavior { get; set; }

        public string ActiveChapterStem
            => Workspace.CurrentChapterHandle?.Chapter.Descriptor.ChapterId
               ?? throw new InvalidOperationException("No active chapter selected.");

        public static async Task<ChapterFitHarness> CreateAsync(IReadOnlyList<string> chapterStems)
        {
            var root = Path.Combine(Path.GetTempPath(), $"ams-proof-pickups-chapter-fit-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);

            foreach (var chapterStem in chapterStems)
            {
                WriteWavStub(Path.Combine(root, $"{chapterStem}.wav"));
            }

            WriteWavStub(Path.Combine(root, ".pickups", "session.wav"));

            var bookPath = Path.Combine(root, "book.md");
            var markdown = "# Test Book\n\n"
                + string.Join("\n\n", chapterStems.Select(chapter => $"## {chapter}\n\nSentence for {chapter}."));
            await File.WriteAllTextAsync(bookPath, markdown);

            var bookIndexPath = Path.Combine(root, "book-index.json");
            await CreateBookIndexAsync(new FileInfo(bookPath), new FileInfo(bookIndexPath));

            var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
            Assert.True(workspace.SetWorkingDirectory(root));
            workspace.SetPrecomputePeaksInBackground(false);

            foreach (var chapterName in workspace.AvailableChapters.ToArray())
            {
                Assert.True(workspace.SelectChapter(chapterName));
                var chapterStem = workspace.CurrentChapterHandle!.Chapter.Descriptor.ChapterId;
                workspace.CurrentChapterHandle.Chapter.Documents.HydratedTranscript = CreateHydratedTranscript(root, chapterStem);
            }

            Assert.True(workspace.SelectChapter(workspace.AvailableChapters[0]));

            var pickMapStore = new PickupPickMapStore(workspace);
            var fitPlanStore = new PickupFitPlanStore(workspace);
            return new ChapterFitHarness(root, workspace, pickMapStore, fitPlanStore);
        }

        public ProofPickupsSessionService CreateSessionService()
            => new(
                Workspace,
                new ProofPickupsSessionService.RuntimeHooks(
                    GetCrxEntries: () => CrxEntries,
                    ImportAssetsAsync: (sourcePath, targets, ct) => ImportPickAssetsBehavior(sourcePath, targets, ct),
                    StageReplacement: (chapterStem, match, sourcePath, originalStartSec, originalEndSec) =>
                    {
                        StageCallCount++;
                        throw new InvalidOperationException("StageReplacement must not be called during Fit plan tests.");
                    },
                    UnstageReplacement: static _ => false,
                    RestageReplacement: static _ => (false, "not configured"),
                    GetQueue: GetQueue,
                    CommitReplacementAsync: static (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0)),
                    RevertReplacementAsync: static (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0)),
                    ReadEdlDocument: static (_, _) => null,
                    ReadArtifactLedgerDocument: (chapterStem, _) => _ledgerByChapter.TryGetValue(chapterStem, out var ledger) ? ledger : null,
                    MutateEdlDocument: static (_, _, _, _) => throw new InvalidOperationException("EDL mutation must not be called during Fit plan tests."),
                    TryGetOperation: static (_, _) => null,
                    TransitionOperationState: static (document, _, _) => document,
                    BuildOrderingDiagnostics: static _ => string.Empty,
                    ImportPickAssetsAsync: (sourcePath, targets, ct) => ImportPickAssetsBehavior(sourcePath, targets, ct),
                    ReadPickMapDocument: ct => PickMapStore.TryRead(ct),
                    LoadOrCreatePickMap: (source, ct) => PickMapStore.LoadOrCreate(source, ct),
                    SavePickMap: (source, document, ct) => PickMapStore.Save(source, document, ct),
                    ReadFitPlanDocument: (chapterStem, ct) => ReadFitPlanBehavior(chapterStem, ct),
                    LoadOrCreateFitPlan: (chapterStem, pickMap, ct) => LoadOrCreateFitPlanBehavior(chapterStem, pickMap, ct),
                    SaveFitPlan: (chapterStem, pickMap, document, ct) => SaveFitPlanBehavior(chapterStem, pickMap, document, ct),
                    GenerateFitPreviewAsync: InvokeFitPreviewAsync,
                    CommitFitReplacementAsync: InvokeFitCommitAsync));

        public void SelectChapterByStem(string chapterStem)
        {
            var chapterName = Workspace.AvailableChapters.Single(chapter =>
                string.Equals(Workspace.GetStemForChapter(chapter), chapterStem, StringComparison.OrdinalIgnoreCase));
            Assert.True(Workspace.SelectChapter(chapterName));
        }

        public CrxEntry CreateCrxEntry(string chapterName, int errorNumber, int sentenceId)
            => new(
                ErrorNumber: errorNumber,
                Chapter: chapterName,
                Timecode: "00:00:01",
                ErrorType: "MR",
                Comments: string.Empty,
                SentenceId: sentenceId,
                StartTime: sentenceId switch
                {
                    21 => 0.70,
                    31 => 1.30,
                    _ => 0.10
                },
                EndTime: sentenceId switch
                {
                    21 => 1.10,
                    31 => 1.70,
                    _ => 0.50
                },
                AudioFile: string.Empty,
                CreatedAt: DateTime.UtcNow,
                ShouldBe: $"replacement {errorNumber}",
                ReadAs: null);

        public PickupAsset CreateAsset(string id, string sourcePath, CrxPickupTarget target, double startSec, double endSec)
            => new(
                Id: id,
                SourceType: PickupSourceType.SessionSegment,
                SourceFilePath: sourcePath,
                TrimStartSec: startSec,
                TrimEndSec: endSec,
                TranscribedText: $"pickup {id}",
                Confidence: 0.99,
                MatchedErrorNumber: target.ErrorNumber,
                MatchedSentenceId: target.SentenceId,
                MatchedChapterStem: target.ChapterStem,
                ImportedAtUtc: DateTime.UtcNow);

        public PickupAsset CreateUnmatchedAsset(string id, string sourcePath, double startSec, double endSec)
            => new(
                Id: id,
                SourceType: PickupSourceType.SessionSegment,
                SourceFilePath: sourcePath,
                TrimStartSec: startSec,
                TrimEndSec: endSec,
                TranscribedText: $"pickup {id}",
                Confidence: 0.20,
                MatchedErrorNumber: null,
                MatchedSentenceId: null,
                MatchedChapterStem: null,
                ImportedAtUtc: DateTime.UtcNow);

        public void AppendToPickupSource()
        {
            File.AppendAllBytes(PickupPath, [0x00, 0x01, 0x02, 0x03]);
            File.SetLastWriteTimeUtc(PickupPath, DateTime.UtcNow.AddSeconds(5));
        }

        public void SetArtifactLedgerDocument(string chapterStem, PickupArtifactLedgerDocument document)
        {
            _ledgerByChapter[chapterStem] = document;
        }

        public PickupArtifactLedgerDocument CreateLedgerDocument(
            string chapterStem,
            IReadOnlyList<PickupArtifactLedgerEntry> entries,
            int revision)
            => new(
                schemaVersion: PickupArtifactLedgerDocument.CurrentSchemaVersion,
                chapterStem: chapterStem,
                revision: revision,
                lastSequence: entries.Count == 0 ? 0 : entries[^1].Sequence,
                entries: entries);

        public PickupArtifactLedgerEntry CreateLedgerEntry(
            long sequence,
            string operationId,
            string transition,
            int edlRevision,
            ReplacementStatus queueStatus = ReplacementStatus.Applied,
            PickupEdlOperationState edlState = PickupEdlOperationState.Applied,
            PickupArtifactLedgerRollbackVerdict rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotRequired,
            string? failureReason = null)
            => new(
                sequence: sequence,
                operationId: operationId,
                transition: transition,
                phase: "fit-apply",
                edlRevision: edlRevision,
                queueStatus: queueStatus,
                edlState: edlState,
                rollbackVerdict: rollbackVerdict,
                artifactRefs:
                [
                    $".polish/edl/{ActiveChapterStem}.edl.json",
                    $".polish/edl/{ActiveChapterStem}.artifact-ledger.json"
                ],
                failureReason: failureReason,
                occurredAtUtc: DateTime.UtcNow);

        public void Dispose()
        {
            Workspace.Dispose();
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }
            }
            catch
            {
                // best effort cleanup only
            }
        }

        private IReadOnlyList<StagedReplacement> GetQueue(string chapterStem)
            => _queues.TryGetValue(chapterStem, out var queue)
                ? queue
                : Array.Empty<StagedReplacement>();

        private Task<FitReplacementPreviewResult> InvokeFitPreviewAsync(
            PickupFitPlanDocument fitPlan,
            PickupFitPlanItem fitItem,
            string? roomtonePath,
            CancellationToken ct)
        {
            FitPreviewCallCount++;
            return GenerateFitPreviewBehavior(fitPlan, fitItem, roomtonePath, ct);
        }

        private Task<FitReplacementCommitResult> InvokeFitCommitAsync(
            PickupFitPlanDocument fitPlan,
            PickupFitPlanItem fitItem,
            string? roomtonePath,
            CancellationToken ct)
        {
            FitCommitCallCount++;
            return CommitFitBehavior(fitPlan, fitItem, roomtonePath, ct);
        }

        private static HydratedTranscript CreateHydratedTranscript(string root, string chapterStem)
        {
            var words = new List<HydratedWord>
            {
                new(BookIdx: 0, AsrIdx: 0, BookWord: "book", AsrWord: "script", Op: "sub", Reason: "fit-test", Score: 0)
                {
                    StartSec = 0.10,
                    EndSec = 0.50,
                    DurationSec = 0.40
                },
                new(BookIdx: 1, AsrIdx: 1, BookWord: "book", AsrWord: "script", Op: "sub", Reason: "fit-test", Score: 0)
                {
                    StartSec = 0.70,
                    EndSec = 1.10,
                    DurationSec = 0.40
                },
                new(BookIdx: 2, AsrIdx: 2, BookWord: "book", AsrWord: "script", Op: "sub", Reason: "fit-test", Score: 0)
                {
                    StartSec = 1.30,
                    EndSec = 1.70,
                    DurationSec = 0.40
                }
            };

            var sentences = new List<HydratedSentence>
            {
                CreateSentence(11, 0, 1, $"{chapterStem} sentence 11", 0.10, 0.50),
                CreateSentence(21, 1, 2, $"{chapterStem} sentence 21", 0.70, 1.10),
                CreateSentence(31, 2, 3, $"{chapterStem} sentence 31", 1.30, 1.70)
            };

            var paragraphs = new List<HydratedParagraph>
            {
                new(
                    Id: 1,
                    BookRange: new HydratedRange(0, 3),
                    SentenceIds: [11, 21, 31],
                    BookText: $"{chapterStem} paragraph",
                    Metrics: new ParagraphMetrics(Wer: 0, Cer: 0, Coverage: 1),
                    Status: "ok",
                    Diff: null)
            };

            return new HydratedTranscript(
                AudioPath: Path.Combine(root, $"{chapterStem}.wav"),
                ScriptPath: Path.Combine(root, "book.md"),
                BookIndexPath: Path.Combine(root, "book-index.json"),
                CreatedAtUtc: DateTime.UtcNow,
                NormalizationVersion: "fit-test",
                Words: words,
                Sentences: sentences,
                Paragraphs: paragraphs);
        }

        private static HydratedSentence CreateSentence(int id, int startWord, int endWord, string text, double startSec, double endSec)
            => new(
                Id: id,
                BookRange: new HydratedRange(startWord, endWord),
                ScriptRange: new HydratedScriptRange(startWord, endWord),
                BookText: text,
                ScriptText: text,
                Metrics: new SentenceMetrics(Wer: 0, Cer: 0, SpanWer: 0, MissingRuns: 0, ExtraRuns: 0),
                Status: "ok",
                Timing: new TimingRange(startSec, endSec),
                Diff: null);

        private static async Task CreateBookIndexAsync(FileInfo bookFile, FileInfo outputFile)
        {
            var command = new BuildBookIndexCommand(new DocumentService(pronunciationProvider: null, cache: null));
            await command.ExecuteAsync(new BuildBookIndexRequest(bookFile, outputFile));
        }

        private static void WriteWavStub(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00]);
        }
    }
}
