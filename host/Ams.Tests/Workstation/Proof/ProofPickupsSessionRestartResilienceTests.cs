using Ams.Core.Application.Commands;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;
using Ams.Workstation.Server.Services.Pickups.Pick;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofPickupsSessionRestartResilienceTests
{
    [Fact]
    public async Task SyncToWorkspace_ReloadAndRestart_RehydratesMixedLifecycleSnapshotFromDurableState()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            ProofPickupsSessionSnapshot baseline;
            using (var firstBoot = await SessionBoot.OpenAsync(root, loadPersistedState: false))
            {
                var chapterStem = firstBoot.ActiveChapterStem;
                var staged = CreateReplacement("op-staged", chapterStem, pickupPath, sentenceId: 11, status: ReplacementStatus.Staged, baselineStartSec: 0.10);
                var applied = CreateReplacement("op-applied", chapterStem, pickupPath, sentenceId: 12, status: ReplacementStatus.Applied, baselineStartSec: 0.40);
                var reverted = CreateReplacement("op-reverted", chapterStem, pickupPath, sentenceId: 13, status: ReplacementStatus.Reverted, baselineStartSec: 0.70);
                var failed = CreateReplacement("op-failed", chapterStem, pickupPath, sentenceId: 14, status: ReplacementStatus.Failed, baselineStartSec: 1.00);

                SeedPersistedArtifacts(firstBoot, chapterStem, pickupPath, [staged, applied, reverted, failed]);

                baseline = firstBoot.Service.SyncToWorkspace();
                Assert.Equal(chapterStem, baseline.ActiveChapterStem, ignoreCase: true);
                Assert.Equal(pickupPath, baseline.SourcePath, ignoreCase: true);
                Assert.Equal("op-failed", baseline.LastValidationError);
                Assert.Null(baseline.ArtifactLedgerReadError);
                Assert.Null(baseline.LastError);

                var reloadedService = firstBoot.CreateSessionService();
                var reloaded = reloadedService.SyncToWorkspace();
                AssertSnapshotConverged(baseline, reloaded);

                var committed = await firstBoot.Service.CommitAsync(staged.Id, CancellationToken.None);
                Assert.Equal(staged.Id, committed.LastOperationId);
            }

            using var restartedBoot = await SessionBoot.OpenAsync(root, loadPersistedState: true);
            Assert.True(restartedBoot.Workspace.IsInitialized);
            Assert.True(restartedBoot.Workspace.HasBookIndex);

            var restarted = restartedBoot.Service.SyncToWorkspace();
            AssertSnapshotConverged(baseline, restarted);
            Assert.Null(restarted.LastOperationId);

            Assert.Single(restarted.Staged);
            Assert.Single(restarted.Applied);
            Assert.Single(restarted.Reverted);
            Assert.Single(restarted.Failed);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SyncToWorkspace_Restart_OnlyAppliedAndRevertedQueuesRemainConvergent()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            ProofPickupsSessionSnapshot baseline;
            using (var firstBoot = await SessionBoot.OpenAsync(root, loadPersistedState: false))
            {
                var chapterStem = firstBoot.ActiveChapterStem;
                var applied = CreateReplacement("op-applied-only", chapterStem, pickupPath, sentenceId: 21, status: ReplacementStatus.Applied, baselineStartSec: 0.20);
                var reverted = CreateReplacement("op-reverted-only", chapterStem, pickupPath, sentenceId: 22, status: ReplacementStatus.Reverted, baselineStartSec: 0.60);

                SeedPersistedArtifacts(firstBoot, chapterStem, pickupPath, [applied, reverted]);

                baseline = firstBoot.Service.SyncToWorkspace();
                Assert.Empty(baseline.Staged);
                Assert.Empty(baseline.Failed);
                Assert.Single(baseline.Applied);
                Assert.Single(baseline.Reverted);
            }

            using var restartedBoot = await SessionBoot.OpenAsync(root, loadPersistedState: true);
            var restarted = restartedBoot.Service.SyncToWorkspace();

            AssertSnapshotConverged(baseline, restarted);
            Assert.Empty(restarted.Staged);
            Assert.Empty(restarted.Failed);
            Assert.Single(restarted.Applied);
            Assert.Single(restarted.Reverted);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SyncToWorkspace_RestartWithMalformedProjectState_FailsClosedWithActionableDiagnostics()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            using (var firstBoot = await SessionBoot.OpenAsync(root, loadPersistedState: false))
            {
                firstBoot.Workspace.SetPolishPaths(pickupPath, roomtoneFilePath: null);
                _ = firstBoot.Service.SyncToWorkspace();
            }

            var projectStatePath = Path.Combine(root, ".polish", "project-state.json");
            Assert.True(File.Exists(projectStatePath));
            await File.WriteAllTextAsync(projectStatePath, "{ malformed-json");

            using var restartedBoot = await SessionBoot.OpenAsync(root, loadPersistedState: true);

            var snapshot = restartedBoot.Service.SyncToWorkspace();

            Assert.False(snapshot.HasActiveChapter);
            Assert.Equal(ProofPickupsSessionPhase.Idle, snapshot.Phase);
            Assert.Contains("Select an active chapter", snapshot.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("project-state.json", snapshot.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("failed to load persisted project state", snapshot.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Null(snapshot.SourcePath);
            Assert.Empty(snapshot.Staged);
            Assert.Empty(snapshot.Applied);
            Assert.Empty(snapshot.Reverted);
            Assert.Empty(snapshot.Failed);
            Assert.Empty(snapshot.ArtifactLedgerEntries);
            Assert.Null(snapshot.ArtifactLedgerRevision);
            Assert.Null(snapshot.ArtifactLedgerReadError);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SyncToWorkspace_MalformedLedgerAfterBaseline_PreservesPriorLedgerSnapshotAndQuarantinesFile()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            using var boot = await SessionBoot.OpenAsync(root, loadPersistedState: false);

            var chapterStem = boot.ActiveChapterStem;
            var staged = CreateReplacement("op-ledger-staged", chapterStem, pickupPath, sentenceId: 31, status: ReplacementStatus.Staged, baselineStartSec: 0.10);
            var applied = CreateReplacement("op-ledger-applied", chapterStem, pickupPath, sentenceId: 32, status: ReplacementStatus.Applied, baselineStartSec: 0.45);
            SeedPersistedArtifacts(boot, chapterStem, pickupPath, [staged, applied]);

            var baseline = boot.Service.SyncToWorkspace();
            Assert.NotEmpty(baseline.ArtifactLedgerEntries);

            var ledgerPath = boot.LedgerStore.GetDocumentPath(chapterStem);
            Assert.True(File.Exists(ledgerPath));
            await File.WriteAllTextAsync(ledgerPath, "{ malformed-ledger-json");

            var refreshed = boot.Service.SyncToWorkspace();

            Assert.Equal(baseline.ArtifactLedgerRevision, refreshed.ArtifactLedgerRevision);
            Assert.Equal(
                baseline.ArtifactLedgerEntries.Select(entry => entry.OperationId),
                refreshed.ArtifactLedgerEntries.Select(entry => entry.OperationId));
            Assert.Contains("artifact ledger read failed", refreshed.ArtifactLedgerReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("malformed pickup artifact ledger json", refreshed.ArtifactLedgerReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("artifact-ledger.json", refreshed.ArtifactLedgerReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            Assert.False(File.Exists(ledgerPath));
            var quarantineFiles = Directory.GetFiles(
                Path.GetDirectoryName(ledgerPath)!,
                $"{Path.GetFileName(ledgerPath)}.malformed.*.json");
            Assert.NotEmpty(quarantineFiles);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SyncToWorkspace_RestartReloadsStoredPickMap()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            ProofPickupsSessionSnapshot baseline;
            using (var firstBoot = await SessionBoot.OpenAsync(root, loadPersistedState: false))
            {
                var source = BuildPickSourceReference(pickupPath);
                var document = CreatePickMapDocument(source, firstBoot.ActiveChapterStem, isDraft: false);
                var saved = firstBoot.PickMapStore.Save(source, document, CancellationToken.None);
                Assert.False(saved.IsDraft);

                baseline = firstBoot.Service.SyncToWorkspace();
                Assert.NotNull(baseline.PickMap);
                Assert.Equal(saved.Revision, baseline.PickMapRevision);
                Assert.Equal(1, baseline.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Confirmed]);
                Assert.Null(baseline.PickMapReadError);
            }

            using var restartedBoot = await SessionBoot.OpenAsync(root, loadPersistedState: true);
            var restarted = restartedBoot.Service.SyncToWorkspace();

            Assert.NotNull(restarted.PickMap);
            Assert.Equal(baseline.PickMapRevision, restarted.PickMapRevision);
            Assert.Equal(baseline.PickMap!.Source.Fingerprint, restarted.PickMap!.Source.Fingerprint);
            Assert.Equal(baseline.PickMap.Assignments.Select(item => item.Id), restarted.PickMap.Assignments.Select(item => item.Id));
            Assert.Equal(1, restarted.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Confirmed]);
            Assert.Null(restarted.PickMapReadError);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SyncToWorkspace_MalformedPickMapAfterBaseline_PreservesPriorPickMapAndQuarantinesFile()
    {
        var root = await CreateWorkspaceRootAsync(["chapter-01"]);

        try
        {
            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            using var boot = await SessionBoot.OpenAsync(root, loadPersistedState: false);
            var source = BuildPickSourceReference(pickupPath);
            _ = boot.PickMapStore.Save(
                source,
                CreatePickMapDocument(source, boot.ActiveChapterStem, isDraft: false),
                CancellationToken.None);

            var baseline = boot.Service.SyncToWorkspace();
            Assert.NotNull(baseline.PickMap);

            var pickMapPath = boot.PickMapStore.GetDocumentPath();
            Assert.True(File.Exists(pickMapPath));
            await File.WriteAllTextAsync(pickMapPath, "{ malformed-pick-map-json");

            var refreshed = boot.Service.SyncToWorkspace();

            Assert.Equal(baseline.PickMapRevision, refreshed.PickMapRevision);
            Assert.Equal(baseline.PickMap!.Assignments.Select(item => item.Id), refreshed.PickMap!.Assignments.Select(item => item.Id));
            Assert.Contains("pick-map read failed", refreshed.PickMapReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("malformed pickup pick-map json", refreshed.PickMapReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pick-map.json", refreshed.PickMapReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            Assert.False(File.Exists(pickMapPath));
            var quarantineFiles = Directory.GetFiles(
                Path.GetDirectoryName(pickMapPath)!,
                $"{Path.GetFileName(pickMapPath)}.malformed.*.json");
            Assert.NotEmpty(quarantineFiles);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    private static void SeedPersistedArtifacts(
        SessionBoot boot,
        string chapterStem,
        string pickupPath,
        IReadOnlyList<StagedReplacement> replacements)
    {
        boot.Workspace.SetPolishPaths(pickupPath, roomtoneFilePath: null);

        foreach (var replacement in replacements)
        {
            var staged = replacement with { Status = ReplacementStatus.Staged };
            Assert.True(
                boot.StagingQueue.TryStage(staged, out var validationError),
                validationError ?? $"Failed to stage '{replacement.Id}'.");

            if (replacement.Status != ReplacementStatus.Staged)
            {
                Assert.True(
                    boot.StagingQueue.UpdateStatus(replacement.Id, replacement.Status, syncEditList: false),
                    $"Failed to set status '{replacement.Status}' for '{replacement.Id}'.");
            }
        }

        var source = BuildSourceReference(pickupPath, chapterStem);
        _ = boot.EdlStore.Mutate(
            chapterStem,
            source,
            document =>
            {
                var current = document;
                foreach (var replacement in replacements)
                {
                    var operation = boot.Engine.BuildOperation(
                        replacement,
                        source,
                        MapEdlState(replacement.Status),
                        knownSentenceIds: null,
                        updatedAtUtc: DateTime.UtcNow.AddMilliseconds(replacement.SentenceId));

                    current = boot.Engine.UpsertOperation(current, operation);
                }

                return current;
            },
            CancellationToken.None);

        for (var index = 0; index < replacements.Count; index++)
        {
            var replacement = replacements[index];
            _ = boot.LedgerStore.Append(
                chapterStem,
                new PickupArtifactLedgerEntryDraft(
                    operationId: replacement.Id,
                    transition: MapLedgerTransition(replacement.Status),
                    phase: "rehydrate-seed",
                    edlRevision: index + 1,
                    queueStatus: replacement.Status,
                    edlState: MapEdlState(replacement.Status),
                    rollbackVerdict: replacement.Status == ReplacementStatus.Failed
                        ? PickupArtifactLedgerRollbackVerdict.NotAttempted
                        : PickupArtifactLedgerRollbackVerdict.NotRequired,
                    artifactRefs:
                    [
                        $".polish/edl/{chapterStem}.edl.json",
                        $".polish/edl/{chapterStem}.artifact-ledger.json"
                    ],
                    failureReason: replacement.Status == ReplacementStatus.Failed
                        ? "seeded failure"
                        : null,
                    occurredAtUtc: DateTime.UtcNow.AddSeconds(index + 1)));
        }
    }

    private static PickupEdlOperationState MapEdlState(ReplacementStatus status)
        => status switch
        {
            ReplacementStatus.Staged => PickupEdlOperationState.Staged,
            ReplacementStatus.Applied => PickupEdlOperationState.Applied,
            ReplacementStatus.Reverted => PickupEdlOperationState.Reverted,
            ReplacementStatus.Failed => PickupEdlOperationState.Failed,
            _ => throw new InvalidOperationException($"Unsupported replacement status '{status}' for EDL mapping.")
        };

    private static string MapLedgerTransition(ReplacementStatus status)
        => status switch
        {
            ReplacementStatus.Staged => PickupArtifactLedgerTransitions.CommitAttempt,
            ReplacementStatus.Applied => PickupArtifactLedgerTransitions.CommitSuccess,
            ReplacementStatus.Reverted => PickupArtifactLedgerTransitions.RevertSuccess,
            ReplacementStatus.Failed => PickupArtifactLedgerTransitions.RevertFailure,
            _ => throw new InvalidOperationException($"Unsupported replacement status '{status}' for ledger transition mapping.")
        };

    private static PickupEdlSourceReference BuildSourceReference(string pickupPath, string chapterStem)
    {
        var info = new FileInfo(pickupPath);
        return new PickupEdlSourceReference(
            path: Path.GetFullPath(pickupPath),
            fingerprint: $"fp-{chapterStem}",
            fileSizeBytes: info.Exists ? info.Length : 0,
            modifiedAtUtc: info.Exists ? info.LastWriteTimeUtc : DateTime.UtcNow);
    }

    private static PickupPickMapSourceReference BuildPickSourceReference(string pickupPath)
    {
        var info = new FileInfo(pickupPath);
        return new PickupPickMapSourceReference(
            path: Path.GetFullPath(pickupPath),
            fingerprint: $"pick-fp-{info.Length}-{info.LastWriteTimeUtc.Ticks}",
            fileSizeBytes: info.Exists ? info.Length : 0,
            modifiedAtUtc: info.Exists ? info.LastWriteTimeUtc : DateTime.UtcNow,
            crxTargetsFingerprint: "crx-restart-fp");
    }

    private static PickupPickMapDocument CreatePickMapDocument(
        PickupPickMapSourceReference source,
        string chapterStem,
        bool isDraft)
    {
        var target = new PickupPickMapTargetReference(
            chapterStem: chapterStem,
            chapterName: chapterStem,
            errorNumber: 101,
            sentenceId: 11,
            originalStartSec: 0.10,
            originalEndSec: 0.40);
        var now = DateTime.UtcNow;
        return new PickupPickMapDocument(
            schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
            revision: 0,
            source: source,
            assignments:
            [
                new PickupPickMapAssignment(
                    id: "pick-assignment-001",
                    pickupSegmentId: "segment-001",
                    sourceStartSec: 0.00,
                    sourceEndSec: 0.30,
                    status: PickupPickMapAssignmentStatus.Confirmed,
                    inferredTarget: target,
                    selectedTarget: target,
                    confidence: 1.0,
                    note: null,
                    validationError: null,
                    updatedAtUtc: now)
            ],
            createdAtUtc: now,
            updatedAtUtc: now,
            lastOperationId: "pick-restart-seed",
            lastValidationError: null,
            isDraft: isDraft);
    }

    private static void AssertSnapshotConverged(
        ProofPickupsSessionSnapshot expected,
        ProofPickupsSessionSnapshot actual)
    {
        Assert.Equal(expected.ActiveChapterName, actual.ActiveChapterName);
        Assert.Equal(expected.ActiveChapterStem, actual.ActiveChapterStem, ignoreCase: true);
        AssertPathsEqual(expected.SourcePath, actual.SourcePath);
        Assert.Equal(expected.Phase, actual.Phase);
        Assert.Equal(expected.Progress, actual.Progress);

        Assert.Equal(expected.EdlRevision, actual.EdlRevision);
        Assert.Equal(expected.DeterministicOrderingDiagnostics, actual.DeterministicOrderingDiagnostics);
        Assert.Equal(expected.LastValidationError, actual.LastValidationError);

        Assert.Equal(expected.Staged.Select(item => item.Id), actual.Staged.Select(item => item.Id));
        Assert.Equal(expected.Applied.Select(item => item.Id), actual.Applied.Select(item => item.Id));
        Assert.Equal(expected.Reverted.Select(item => item.Id), actual.Reverted.Select(item => item.Id));
        Assert.Equal(expected.Failed.Select(item => item.Id), actual.Failed.Select(item => item.Id));

        Assert.Equal(expected.ArtifactLedgerRevision, actual.ArtifactLedgerRevision);
        Assert.Equal(
            expected.ArtifactLedgerEntries.Select(entry => (entry.Sequence, entry.OperationId, entry.Transition)),
            actual.ArtifactLedgerEntries.Select(entry => (entry.Sequence, entry.OperationId, entry.Transition)));

        Assert.Equal(expected.ArtifactLedgerReadError, actual.ArtifactLedgerReadError);
        Assert.Equal(expected.PickMapRevision, actual.PickMapRevision);
        Assert.Equal(expected.PickMapReadError, actual.PickMapReadError);
        Assert.Equal(expected.LastPickOperationId, actual.LastPickOperationId);
        Assert.Equal(expected.LastPickValidationError, actual.LastPickValidationError);
        Assert.Equal(expected.PickMap?.Assignments.Select(item => item.Id) ?? Array.Empty<string>(), actual.PickMap?.Assignments.Select(item => item.Id) ?? Array.Empty<string>());
        Assert.Equal(expected.LastError, actual.LastError);
    }

    private static void AssertPathsEqual(string? expected, string? actual)
    {
        if (expected is null || actual is null)
        {
            Assert.Equal(expected, actual);
            return;
        }

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        Assert.True(
            string.Equals(expected, actual, comparison),
            $"Expected path '{expected}' but got '{actual}'.");
    }

    private static StagedReplacement CreateReplacement(
        string id,
        string chapterStem,
        string pickupPath,
        int sentenceId,
        ReplacementStatus status,
        double baselineStartSec)
    {
        return new StagedReplacement(
            Id: id,
            ChapterStem: chapterStem,
            SentenceId: sentenceId,
            OriginalStartSec: baselineStartSec,
            OriginalEndSec: baselineStartSec + 0.20,
            PickupSourcePath: pickupPath,
            PickupStartSec: 0.00,
            PickupEndSec: 0.20,
            CrossfadeDurationSec: 0.05,
            CrossfadeCurve: "hsin",
            StagedAtUtc: DateTime.UtcNow,
            Status: status);
    }

    private static async Task<string> CreateWorkspaceRootAsync(IReadOnlyList<string> chapterStems)
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-proof-pickups-restart-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        foreach (var chapterStem in chapterStems)
        {
            WriteWavStub(Path.Combine(root, $"{chapterStem}.wav"));
        }

        var bookPath = Path.Combine(root, "book.md");
        var markdown = "# Restart Resilience Book\n\n"
            + string.Join("\n\n", chapterStems.Select(chapter => $"## {chapter}\n\nSentence for {chapter}."));
        await File.WriteAllTextAsync(bookPath, markdown);

        var bookIndexPath = Path.Combine(root, "book-index.json");
        await CreateBookIndexAsync(new FileInfo(bookPath), new FileInfo(bookIndexPath));

        return root;
    }

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

    private static void SafeDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best effort cleanup only
        }
    }

    private sealed class SessionBoot : IDisposable
    {
        private SessionBoot(
            string root,
            BlazorWorkspace workspace,
            StagingQueueService stagingQueue,
            PickupEdlStore edlStore,
            PickupArtifactLedgerStore ledgerStore,
            PickupPickMapStore pickMapStore,
            PickupEdlEngine engine)
        {
            Root = root;
            Workspace = workspace;
            StagingQueue = stagingQueue;
            EdlStore = edlStore;
            LedgerStore = ledgerStore;
            PickMapStore = pickMapStore;
            Engine = engine;

            CommitBehavior = static (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0));
            RevertBehavior = static (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0));

            Service = CreateSessionService();
        }

        public string Root { get; }

        public BlazorWorkspace Workspace { get; }

        public StagingQueueService StagingQueue { get; }

        public PickupEdlStore EdlStore { get; }

        public PickupArtifactLedgerStore LedgerStore { get; }

        public PickupPickMapStore PickMapStore { get; }

        public PickupEdlEngine Engine { get; }

        public ProofPickupsSessionService Service { get; private set; }

        public Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> CommitBehavior { get; set; }

        public Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> RevertBehavior { get; set; }

        public string ActiveChapterStem
            => Workspace.CurrentChapterHandle?.Chapter.Descriptor.ChapterId
               ?? throw new InvalidOperationException("No active chapter selected.");

        public static async Task<SessionBoot> OpenAsync(string root, bool loadPersistedState)
        {
            var statePath = Path.Combine(root, ".workstation-state.json");
            var workspace = new BlazorWorkspace(statePath, loadPersistedState: loadPersistedState);

            if (!loadPersistedState)
            {
                Assert.True(workspace.SetWorkingDirectory(root));
                workspace.SetPrecomputePeaksInBackground(false);

                var chapter = Assert.Single(workspace.AvailableChapters.Take(1));
                Assert.True(workspace.SelectChapter(chapter));
            }

            var editList = new EditListService(workspace);
            var stagingQueue = new StagingQueueService(workspace, editList);
            var edlStore = new PickupEdlStore(workspace);
            var ledgerStore = new PickupArtifactLedgerStore(workspace);
            var pickMapStore = new PickupPickMapStore(workspace);
            var engine = new PickupEdlEngine();

            await Task.Yield();
            return new SessionBoot(root, workspace, stagingQueue, edlStore, ledgerStore, pickMapStore, engine);
        }

        public ProofPickupsSessionService CreateSessionService()
        {
            return new ProofPickupsSessionService(
                Workspace,
                new ProofPickupsSessionService.RuntimeHooks(
                    GetCrxEntries: static () => Array.Empty<CrxEntry>(),
                    ImportAssetsAsync: static (_, _, _) => Task.FromResult(((IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(), (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>())),
                    StageReplacement: static (_, _, _, _, _) => throw new InvalidOperationException("StageReplacement hook not configured in restart harness."),
                    UnstageReplacement: replacementId => StagingQueue.Unstage(replacementId),
                    RestageReplacement: replacement =>
                    {
                        var staged = StagingQueue.TryStage(replacement, out var validationError);
                        return (staged, validationError);
                    },
                    GetQueue: chapterStem => StagingQueue.GetQueue(chapterStem),
                    CommitReplacementAsync: (replacementId, ct) => CommitBehavior(replacementId, ct),
                    RevertReplacementAsync: (replacementId, ct) => RevertBehavior(replacementId, ct),
                    ReadEdlDocument: (chapterStem, ct) => EdlStore.TryRead(chapterStem, ct),
                    ReadArtifactLedgerDocument: (chapterStem, ct) => LedgerStore.TryRead(chapterStem, ct),
                    MutateEdlDocument: (chapterStem, source, mutation, ct) => EdlStore.Mutate(chapterStem, source, mutation, ct),
                    TryGetOperation: (document, operationId) => Engine.TryGetOperation(document, operationId),
                    TransitionOperationState: (document, operationId, nextState) =>
                        Engine.TransitionOperationState(document, operationId, nextState, DateTime.UtcNow),
                    BuildOrderingDiagnostics: document => Engine.BuildDeterministicOrderingDiagnostics(document),
                    ReadPickMapDocument: ct => PickMapStore.TryRead(ct),
                    LoadOrCreatePickMap: (source, ct) => PickMapStore.LoadOrCreate(source, ct),
                    SavePickMap: (source, document, ct) => PickMapStore.Save(source, document, ct)));
        }

        public void Dispose()
        {
            Workspace.Dispose();
        }
    }
}
