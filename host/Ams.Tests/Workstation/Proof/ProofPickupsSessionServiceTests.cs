using Ams.Core.Application.Commands;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofPickupsSessionServiceTests
{
    [Fact]
    public async Task CommitAsync_EmptyReplacementId_FailsClosedWithoutRuntimeCall()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var staged = harness.CreateReplacement(
            id: "op-empty-id",
            chapterStem: harness.ActiveChapterStem,
            sentenceId: 11,
            status: ReplacementStatus.Staged);
        harness.SetQueue(harness.ActiveChapterStem, [staged]);

        _ = harness.Service.SyncToWorkspace();

        var result = await harness.Service.CommitAsync(string.Empty, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, result.Phase);
        Assert.Contains("replacement id is empty", result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.CommitCallCount);
        Assert.Single(result.Staged);
        Assert.Empty(result.Applied);
    }

    [Fact]
    public async Task CommitAsync_StaleChapterSwitch_IsRejectedBeforeRuntimeCall()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01", "chapter-02"]);

        _ = harness.Service.SyncToWorkspace();
        var originalChapterStem = harness.ActiveChapterStem;

        harness.SelectChapterByIndex(1);
        var switchedChapterStem = harness.ActiveChapterStem;

        var result = await harness.Service.CommitAsync("op-stale", CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, result.Phase);
        Assert.Contains(originalChapterStem, result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(switchedChapterStem, result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Reload pickups before commit", result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.CommitCallCount);
    }

    [Fact]
    public async Task CommitAsync_MalformedRuntimePayload_FailsAndPreservesLifecycleQueues()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var staged = harness.CreateReplacement(
            id: "op-malformed-commit",
            chapterStem: harness.ActiveChapterStem,
            sentenceId: 12,
            status: ReplacementStatus.Staged);
        harness.SetQueue(harness.ActiveChapterStem, [staged]);

        harness.CommitBehavior = (_, _) => Task.FromResult((HasResult: false, TimingDeltaSec: 0.125));

        _ = harness.Service.SyncToWorkspace();

        var result = await harness.Service.CommitAsync(staged.Id, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, result.Phase);
        Assert.Contains("malformed runtime payload", result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, harness.CommitCallCount);
        Assert.Single(result.Staged);
        Assert.Empty(result.Applied);
    }

    [Fact]
    public async Task CommitAsync_Success_RefreshesAppliedQueueAndLedgerDiagnostics()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var chapterStem = harness.ActiveChapterStem;
        var staged = harness.CreateReplacement(
            id: "op-commit-success",
            chapterStem: chapterStem,
            sentenceId: 13,
            status: ReplacementStatus.Staged);
        harness.SetQueue(chapterStem, [staged]);
        harness.SetEdlDocument(
            chapterStem,
            harness.CreateEdlDocument(chapterStem, staged, PickupEdlOperationState.Staged, revision: 1));

        harness.CommitBehavior = (replacementId, _) =>
        {
            harness.UpdateQueueStatus(chapterStem, replacementId, ReplacementStatus.Applied);

            var applied = staged with { Status = ReplacementStatus.Applied };
            harness.SetEdlDocument(
                chapterStem,
                harness.CreateEdlDocument(chapterStem, applied, PickupEdlOperationState.Applied, revision: 2));

            harness.SetLedgerDocument(
                chapterStem,
                harness.CreateLedgerDocument(
                    chapterStem,
                    [
                        harness.CreateLedgerEntry(
                            sequence: 1,
                            operationId: replacementId,
                            transition: PickupArtifactLedgerTransitions.CommitSuccess,
                            phase: "apply",
                            edlRevision: 2,
                            queueStatus: ReplacementStatus.Applied,
                            edlState: PickupEdlOperationState.Applied,
                            rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired)
                    ],
                    revision: 1));

            return Task.FromResult((HasResult: true, TimingDeltaSec: 0.200));
        };

        var result = await harness.Service.CommitAsync(staged.Id, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, result.Phase);
        Assert.Null(result.LastError);
        Assert.Equal(staged.Id, result.LastOperationId);
        Assert.Equal(1, harness.CommitCallCount);

        Assert.Empty(result.Staged);
        var appliedItem = Assert.Single(result.Applied);
        Assert.Equal(staged.Id, appliedItem.Id);

        Assert.Equal(1, result.ArtifactLedgerRevision);
        var ledgerEntry = Assert.Single(result.ArtifactLedgerEntries);
        Assert.Equal(PickupArtifactLedgerTransitions.CommitSuccess, ledgerEntry.Transition);
        Assert.Null(result.ArtifactLedgerReadError);
    }

    [Fact]
    public async Task RevertAsync_OperationOutsideAppliedQueue_FailsWithoutRuntimeCall()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var reverted = harness.CreateReplacement(
            id: "op-revert-replay",
            chapterStem: harness.ActiveChapterStem,
            sentenceId: 14,
            status: ReplacementStatus.Reverted);
        harness.SetQueue(harness.ActiveChapterStem, [reverted]);

        _ = harness.Service.SyncToWorkspace();

        var result = await harness.Service.RevertAsync(reverted.Id, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, result.Phase);
        Assert.Contains("not in applied queue", result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, harness.RevertCallCount);
        Assert.Empty(result.Applied);
        Assert.Single(result.Reverted);
    }

    [Fact]
    public async Task RevertAsync_RuntimeFailure_RefreshesFailedQueueAndLedgerEvidence()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var chapterStem = harness.ActiveChapterStem;
        var applied = harness.CreateReplacement(
            id: "op-revert-failure",
            chapterStem: chapterStem,
            sentenceId: 15,
            status: ReplacementStatus.Applied);
        harness.SetQueue(chapterStem, [applied]);
        harness.SetEdlDocument(
            chapterStem,
            harness.CreateEdlDocument(chapterStem, applied, PickupEdlOperationState.Applied, revision: 2));

        harness.RevertBehavior = (replacementId, _) =>
        {
            harness.UpdateQueueStatus(chapterStem, replacementId, ReplacementStatus.Failed);

            var failed = applied with { Status = ReplacementStatus.Failed };
            harness.SetEdlDocument(
                chapterStem,
                harness.CreateEdlDocument(chapterStem, failed, PickupEdlOperationState.Failed, revision: 3));

            harness.SetLedgerDocument(
                chapterStem,
                harness.CreateLedgerDocument(
                    chapterStem,
                    [
                        harness.CreateLedgerEntry(
                            sequence: 1,
                            operationId: replacementId,
                            transition: PickupArtifactLedgerTransitions.RevertFailure,
                            phase: "revert-failure",
                            edlRevision: 3,
                            queueStatus: ReplacementStatus.Failed,
                            edlState: PickupEdlOperationState.Failed,
                            rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotAttempted,
                            failureReason: "undo drift")
                    ],
                    revision: 1));

            throw new InvalidOperationException("undo drift");
        };

        var result = await harness.Service.RevertAsync(applied.Id, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, result.Phase);
        Assert.Contains("Revert failed", result.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(applied.Id, result.LastError ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal(1, harness.RevertCallCount);

        var failedItem = Assert.Single(result.Failed);
        Assert.Equal(applied.Id, failedItem.Id);

        var ledgerEntry = Assert.Single(result.ArtifactLedgerEntries);
        Assert.Equal(PickupArtifactLedgerTransitions.RevertFailure, ledgerEntry.Transition);
        Assert.Contains("undo drift", ledgerEntry.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SyncToWorkspace_LedgerReadFailure_PreservesLastLedgerSnapshotWithReadError()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var chapterStem = harness.ActiveChapterStem;
        var staged = harness.CreateReplacement(
            id: "op-ledger-stable",
            chapterStem: chapterStem,
            sentenceId: 16,
            status: ReplacementStatus.Staged);
        harness.SetQueue(chapterStem, [staged]);

        harness.SetLedgerDocument(
            chapterStem,
            harness.CreateLedgerDocument(
                chapterStem,
                [
                    harness.CreateLedgerEntry(
                        sequence: 1,
                        operationId: staged.Id,
                        transition: PickupArtifactLedgerTransitions.CommitAttempt,
                        phase: "apply",
                        edlRevision: 1,
                        queueStatus: ReplacementStatus.Staged,
                        edlState: PickupEdlOperationState.Staged,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired)
                ],
                revision: 1));

        var baseline = harness.Service.SyncToWorkspace();
        Assert.Equal(1, baseline.ArtifactLedgerRevision);
        Assert.Single(baseline.ArtifactLedgerEntries);

        harness.LedgerReadBehavior = (_, _) => throw new InvalidOperationException("simulated ledger read failure");

        var refreshed = harness.Service.SyncToWorkspace();

        Assert.Equal(1, refreshed.ArtifactLedgerRevision);
        Assert.Single(refreshed.ArtifactLedgerEntries);
        Assert.Contains("ledger read failed", refreshed.ArtifactLedgerReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("simulated ledger read failure", refreshed.ArtifactLedgerReadError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SyncToWorkspace_EdlReadFailure_PreservesLastEdlSnapshotWithReadError()
    {
        using var harness = await SessionHarness.CreateAsync(["chapter-01"]);

        var chapterStem = harness.ActiveChapterStem;
        var failed = harness.CreateReplacement(
            id: "op-edl-failed",
            chapterStem: chapterStem,
            sentenceId: 17,
            status: ReplacementStatus.Failed);
        harness.SetQueue(chapterStem, [failed]);

        harness.SetEdlDocument(
            chapterStem,
            harness.CreateEdlDocument(
                chapterStem,
                failed,
                PickupEdlOperationState.Failed,
                revision: 4));

        var baseline = harness.Service.SyncToWorkspace();
        Assert.Equal(4, baseline.EdlRevision);
        Assert.Equal("op-edl-failed", baseline.LastValidationError);
        Assert.NotNull(baseline.DeterministicOrderingDiagnostics);

        harness.EdlReadBehavior = (_, _) => throw new InvalidOperationException("simulated edl read failure");

        var refreshed = harness.Service.SyncToWorkspace();

        Assert.Equal(4, refreshed.EdlRevision);
        Assert.Equal(baseline.DeterministicOrderingDiagnostics, refreshed.DeterministicOrderingDiagnostics);
        Assert.Equal("op-edl-failed", refreshed.LastValidationError);
        Assert.Contains("edl read failed", refreshed.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("simulated edl read failure", refreshed.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Single(refreshed.Failed);
    }

    private sealed class SessionHarness : IDisposable
    {
        private readonly string _root;
        private readonly Dictionary<string, List<StagedReplacement>> _queues =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PickupEdlDocument?> _edlByChapter =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PickupArtifactLedgerDocument?> _ledgerByChapter =
            new(StringComparer.OrdinalIgnoreCase);

        private SessionHarness(string root, BlazorWorkspace workspace, PickupEdlEngine engine, ProofPickupsSessionService service)
        {
            _root = root;
            Workspace = workspace;
            Engine = engine;
            Service = service;

            CommitBehavior = (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0));
            RevertBehavior = (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0));
            EdlReadBehavior = (chapterStem, _) => _edlByChapter.TryGetValue(chapterStem, out var document)
                ? document
                : null;
            LedgerReadBehavior = (chapterStem, _) => _ledgerByChapter.TryGetValue(chapterStem, out var document)
                ? document
                : null;
        }

        public BlazorWorkspace Workspace { get; }

        public PickupEdlEngine Engine { get; }

        public ProofPickupsSessionService Service { get; }

        public int CommitCallCount { get; private set; }

        public int RevertCallCount { get; private set; }

        public Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> CommitBehavior { get; set; }

        public Func<string, CancellationToken, Task<(bool HasResult, double TimingDeltaSec)>> RevertBehavior { get; set; }

        public Func<string, CancellationToken, PickupEdlDocument?> EdlReadBehavior { get; set; }

        public Func<string, CancellationToken, PickupArtifactLedgerDocument?> LedgerReadBehavior { get; set; }

        public string ActiveChapterStem
            => Workspace.CurrentChapterHandle?.Chapter.Descriptor.ChapterId
               ?? throw new InvalidOperationException("No active chapter handle.");

        public static async Task<SessionHarness> CreateAsync(IReadOnlyList<string> chapterStems)
        {
            var root = Path.Combine(Path.GetTempPath(), $"ams-proof-pickups-session-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);

            foreach (var chapterStem in chapterStems)
            {
                WriteWavStub(Path.Combine(root, $"{chapterStem}.wav"));
            }

            var pickupPath = Path.Combine(root, ".pickups", "session.wav");
            WriteWavStub(pickupPath);

            var bookPath = Path.Combine(root, "book.md");
            var markdown = "# Test Book\n\n"
                + string.Join("\n\n", chapterStems.Select(chapter => $"## {chapter}\n\nSentence for {chapter}."));
            await File.WriteAllTextAsync(bookPath, markdown);

            var bookIndexPath = Path.Combine(root, "book-index.json");
            await CreateBookIndexAsync(new FileInfo(bookPath), new FileInfo(bookIndexPath));

            var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
            Assert.True(workspace.SetWorkingDirectory(root));
            workspace.SetPrecomputePeaksInBackground(false);

            var firstChapter = Assert.Single(workspace.AvailableChapters.Take(1));
            Assert.True(workspace.SelectChapter(firstChapter));

            SessionHarness? harness = null;
            harness = new SessionHarness(
                root,
                workspace,
                new PickupEdlEngine(),
                new ProofPickupsSessionService(
                    workspace,
                    new ProofPickupsSessionService.RuntimeHooks(
                        GetCrxEntries: static () => Array.Empty<CrxEntry>(),
                        ImportAssetsAsync: static (_, _, _) => Task.FromResult(((IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(), (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>())),
                        StageReplacement: static (_, _, _, _, _) => throw new InvalidOperationException("StageReplacement hook not configured for this test."),
                        UnstageReplacement: static _ => false,
                        RestageReplacement: static _ => (false, "RestageReplacement hook not configured for this test."),
                        GetQueue: chapterStem => harness!.GetQueue(chapterStem),
                        CommitReplacementAsync: (replacementId, ct) => harness!.InvokeCommitAsync(replacementId, ct),
                        RevertReplacementAsync: (replacementId, ct) => harness!.InvokeRevertAsync(replacementId, ct),
                        ReadEdlDocument: (chapterStem, ct) => harness!.EdlReadBehavior(chapterStem, ct),
                        ReadArtifactLedgerDocument: (chapterStem, ct) => harness!.LedgerReadBehavior(chapterStem, ct),
                        MutateEdlDocument: (chapterStem, source, mutation, _) => harness!.MutateEdlDocument(chapterStem, source, mutation),
                        TryGetOperation: (document, operationId) => harness!.Engine.TryGetOperation(document, operationId),
                        TransitionOperationState: (document, operationId, nextState) =>
                            harness!.Engine.TransitionOperationState(document, operationId, nextState, DateTime.UtcNow),
                        BuildOrderingDiagnostics: document => harness!.Engine.BuildDeterministicOrderingDiagnostics(document))));

            return harness;
        }

        public void SelectChapterByIndex(int index)
        {
            var chapterName = Workspace.AvailableChapters[index];
            Assert.True(Workspace.SelectChapter(chapterName));
        }

        public StagedReplacement CreateReplacement(
            string id,
            string chapterStem,
            int sentenceId,
            ReplacementStatus status)
        {
            return new StagedReplacement(
                Id: id,
                ChapterStem: chapterStem,
                SentenceId: sentenceId,
                OriginalStartSec: 0.10 + sentenceId / 1000.0,
                OriginalEndSec: 0.30 + sentenceId / 1000.0,
                PickupSourcePath: Path.Combine(_root, ".pickups", "session.wav"),
                PickupStartSec: 0.00,
                PickupEndSec: 0.20,
                CrossfadeDurationSec: 0.05,
                CrossfadeCurve: "hsin",
                StagedAtUtc: DateTime.UtcNow,
                Status: status);
        }

        public PickupEdlDocument CreateEdlDocument(
            string chapterStem,
            StagedReplacement replacement,
            PickupEdlOperationState state,
            int revision)
        {
            var source = new PickupEdlSourceReference(
                path: Path.GetFullPath(replacement.PickupSourcePath),
                fingerprint: $"fp-{chapterStem}",
                fileSizeBytes: 8,
                modifiedAtUtc: DateTime.UtcNow);

            var operation = Engine.BuildOperation(
                replacement,
                source,
                state,
                knownSentenceIds: null,
                updatedAtUtc: DateTime.UtcNow);

            return new PickupEdlDocument(
                schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
                chapterStem: chapterStem,
                revision: revision,
                source: source,
                operations: [operation]);
        }

        public PickupArtifactLedgerEntry CreateLedgerEntry(
            long sequence,
            string operationId,
            string transition,
            string phase,
            int edlRevision,
            ReplacementStatus queueStatus,
            PickupEdlOperationState edlState,
            PickupArtifactLedgerRollbackVerdict rollbackVerdict,
            string? failureReason = null)
        {
            return new PickupArtifactLedgerEntry(
                sequence: sequence,
                operationId: operationId,
                transition: transition,
                phase: phase,
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
        }

        public PickupArtifactLedgerDocument CreateLedgerDocument(
            string chapterStem,
            IReadOnlyList<PickupArtifactLedgerEntry> entries,
            int revision)
        {
            return new PickupArtifactLedgerDocument(
                schemaVersion: PickupArtifactLedgerDocument.CurrentSchemaVersion,
                chapterStem: chapterStem,
                revision: revision,
                lastSequence: entries.Count == 0 ? 0 : entries[^1].Sequence,
                entries: entries);
        }

        public void SetQueue(string chapterStem, IReadOnlyList<StagedReplacement> items)
        {
            _queues[chapterStem] = items.ToList();
        }

        public void UpdateQueueStatus(string chapterStem, string replacementId, ReplacementStatus status)
        {
            if (!_queues.TryGetValue(chapterStem, out var queue))
            {
                return;
            }

            var index = queue.FindIndex(item => string.Equals(item.Id, replacementId, StringComparison.Ordinal));
            if (index >= 0)
            {
                queue[index] = queue[index] with { Status = status };
            }
        }

        public void SetEdlDocument(string chapterStem, PickupEdlDocument? document)
        {
            _edlByChapter[chapterStem] = document;
        }

        public void SetLedgerDocument(string chapterStem, PickupArtifactLedgerDocument? document)
        {
            _ledgerByChapter[chapterStem] = document;
        }

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
        {
            if (_queues.TryGetValue(chapterStem, out var queue))
            {
                return queue;
            }

            return Array.Empty<StagedReplacement>();
        }

        private PickupEdlDocument? GetEdlDocument(string chapterStem)
        {
            if (_edlByChapter.TryGetValue(chapterStem, out var document))
            {
                return document;
            }

            return null;
        }

        private PickupEdlDocument MutateEdlDocument(
            string chapterStem,
            PickupEdlSourceReference source,
            Func<PickupEdlDocument, PickupEdlDocument> mutation)
        {
            var current = GetEdlDocument(chapterStem)
                ?? new PickupEdlDocument(
                    schemaVersion: PickupEdlDocument.CurrentSchemaVersion,
                    chapterStem: chapterStem,
                    revision: 0,
                    source: source,
                    operations: []);

            var next = mutation(current);
            SetEdlDocument(chapterStem, next);
            return next;
        }

        private Task<(bool HasResult, double TimingDeltaSec)> InvokeCommitAsync(string replacementId, CancellationToken ct)
        {
            CommitCallCount++;
            return CommitBehavior(replacementId, ct);
        }

        private Task<(bool HasResult, double TimingDeltaSec)> InvokeRevertAsync(string replacementId, CancellationToken ct)
        {
            RevertCallCount++;
            return RevertBehavior(replacementId, ct);
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
    }
}
