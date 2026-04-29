using Ams.Core.Application.Commands;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Tests.Services;

public sealed class PickupCommitRevertReliabilityTests
{
    [Fact]
    public async Task TryRollbackPickupTransition_AppliedToStaged_RestoresQueueAndProjection()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: false);

        var replacement = CreateReplacement(
            id: "op-single-rollback",
            chapterStem: runtime.ChapterStem,
            pickupPath: runtime.PickupPath,
            sentenceId: 11,
            status: ReplacementStatus.Staged);

        Assert.True(runtime.StagingQueue.TryStage(replacement, out var stageError), stageError ?? "stage failed");
        var source = runtime.SourceBufferCache.DescribeSource(runtime.PickupPath);
        var appliedOperation = BuildOperation(runtime, replacement, source, PickupEdlOperationState.Applied);

        _ = runtime.Store.Mutate(
            runtime.ChapterStem,
            source,
            document => runtime.Engine.UpsertOperation(document, appliedOperation),
            CancellationToken.None);

        Assert.True(runtime.StagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Applied, syncEditList: false));
        runtime.EditListService.Add(appliedOperation.ToChapterEdit());

        var rollbackSucceeded = runtime.PolishService.TryRollbackPickupTransition(
            runtime.ChapterStem,
            source,
            replacement.Id,
            rollbackEdlState: PickupEdlOperationState.Staged,
            rollbackQueueStatus: ReplacementStatus.Staged,
            phase: "test-single",
            trigger: new InvalidOperationException("simulated rebuild failure"));

        Assert.True(rollbackSucceeded);

        var queueItem = Assert.Single(runtime.StagingQueue.GetQueue(runtime.ChapterStem), item => item.Id == replacement.Id);
        Assert.Equal(ReplacementStatus.Staged, queueItem.Status);

        var documentAfter = runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(documentAfter);

        var opAfter = runtime.Engine.TryGetOperation(documentAfter!, replacement.Id);
        Assert.NotNull(opAfter);
        Assert.Equal(PickupEdlOperationState.Staged, opAfter!.State);

        var pickupEdits = runtime.EditListService
            .GetEdits(runtime.ChapterStem)
            .Where(edit => edit.Operation == EditOperation.PickupReplace)
            .ToArray();
        Assert.Empty(pickupEdits);

        var ledger = runtime.ArtifactLedgerStore.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(ledger);

        var rollbackEntry = Assert.Single(
            ledger!.Entries,
            entry =>
                string.Equals(entry.OperationId, replacement.Id, StringComparison.Ordinal) &&
                string.Equals(entry.Transition, PickupArtifactLedgerTransitions.RollbackSucceeded, StringComparison.Ordinal));
        Assert.Equal(PickupArtifactLedgerRollbackVerdict.Succeeded, rollbackEntry.RollbackVerdict);
    }

    [Fact]
    public async Task TryRollbackBatchPickupTransitions_RestoresAllAppliedOpsToStaged()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: false);

        var first = CreateReplacement(
            id: "op-batch-a",
            chapterStem: runtime.ChapterStem,
            pickupPath: runtime.PickupPath,
            sentenceId: 21,
            status: ReplacementStatus.Staged);

        var second = CreateReplacement(
            id: "op-batch-b",
            chapterStem: runtime.ChapterStem,
            pickupPath: runtime.PickupPath,
            sentenceId: 22,
            status: ReplacementStatus.Staged) with
        {
            OriginalStartSec = 0.40,
            OriginalEndSec = 0.60,
            PickupStartSec = 0.20,
            PickupEndSec = 0.40
        };

        Assert.True(runtime.StagingQueue.TryStage(first, out var firstError), firstError ?? "first stage failed");
        Assert.True(runtime.StagingQueue.TryStage(second, out var secondError), secondError ?? "second stage failed");

        var source = runtime.SourceBufferCache.DescribeSource(runtime.PickupPath);
        var firstApplied = BuildOperation(runtime, first, source, PickupEdlOperationState.Applied);
        var secondApplied = BuildOperation(runtime, second, source, PickupEdlOperationState.Applied);

        _ = runtime.Store.Mutate(
            runtime.ChapterStem,
            source,
            document => runtime.Engine.UpsertOperation(document, firstApplied),
            CancellationToken.None);

        _ = runtime.Store.Mutate(
            runtime.ChapterStem,
            source,
            document => runtime.Engine.UpsertOperation(document, secondApplied),
            CancellationToken.None);

        Assert.True(runtime.StagingQueue.UpdateStatus(first.Id, ReplacementStatus.Applied, syncEditList: false));
        Assert.True(runtime.StagingQueue.UpdateStatus(second.Id, ReplacementStatus.Applied, syncEditList: false));
        runtime.EditListService.Add(firstApplied.ToChapterEdit());
        runtime.EditListService.Add(secondApplied.ToChapterEdit());

        var rollbackSucceeded = runtime.PolishService.TryRollbackBatchPickupTransitions(
            runtime.ChapterStem,
            [
                (first, source),
                (second, source)
            ],
            rollbackEdlState: PickupEdlOperationState.Staged,
            rollbackQueueStatus: ReplacementStatus.Staged,
            phase: "test-batch",
            trigger: new InvalidOperationException("simulated persist failure"));

        Assert.True(rollbackSucceeded);

        var queueById = runtime.StagingQueue
            .GetQueue(runtime.ChapterStem)
            .Where(item => item.Id == first.Id || item.Id == second.Id)
            .ToDictionary(item => item.Id, StringComparer.Ordinal);

        Assert.Equal(ReplacementStatus.Staged, queueById[first.Id].Status);
        Assert.Equal(ReplacementStatus.Staged, queueById[second.Id].Status);

        var documentAfter = runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(documentAfter);

        var opA = runtime.Engine.TryGetOperation(documentAfter!, first.Id);
        var opB = runtime.Engine.TryGetOperation(documentAfter!, second.Id);
        Assert.NotNull(opA);
        Assert.NotNull(opB);
        Assert.Equal(PickupEdlOperationState.Staged, opA!.State);
        Assert.Equal(PickupEdlOperationState.Staged, opB!.State);

        var pickupEdits = runtime.EditListService
            .GetEdits(runtime.ChapterStem)
            .Where(edit => edit.Operation == EditOperation.PickupReplace)
            .ToArray();
        Assert.Empty(pickupEdits);
    }

    [Fact]
    public async Task TryRestorePickupOperationSnapshot_RevertFailure_RestoresAppliedStateAndProjection()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: false);

        var replacement = CreateReplacement(
            id: "op-revert-rollback",
            chapterStem: runtime.ChapterStem,
            pickupPath: runtime.PickupPath,
            sentenceId: 31,
            status: ReplacementStatus.Reverted);

        Assert.True(runtime.StagingQueue.TryStage(replacement with { Status = ReplacementStatus.Staged }, out var stageError), stageError ?? "stage failed");
        Assert.True(runtime.StagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Reverted, syncEditList: false));

        var source = runtime.SourceBufferCache.DescribeSource(runtime.PickupPath);
        var revertedOperation = BuildOperation(runtime, replacement, source, PickupEdlOperationState.Reverted);
        var appliedSnapshot = BuildOperation(runtime, replacement, source, PickupEdlOperationState.Applied);

        _ = runtime.Store.Mutate(
            runtime.ChapterStem,
            source,
            document => runtime.Engine.UpsertOperation(document, revertedOperation),
            CancellationToken.None);

        var rollbackSucceeded = runtime.PolishService.TryRestorePickupOperationSnapshot(
            runtime.ChapterStem,
            source,
            appliedSnapshot,
            rollbackQueueStatus: ReplacementStatus.Applied,
            phase: "test-revert",
            trigger: new InvalidOperationException("simulated corrected persist failure"));

        Assert.True(rollbackSucceeded);

        var queueItem = Assert.Single(runtime.StagingQueue.GetQueue(runtime.ChapterStem), item => item.Id == replacement.Id);
        Assert.Equal(ReplacementStatus.Applied, queueItem.Status);

        var documentAfter = runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(documentAfter);

        var opAfter = runtime.Engine.TryGetOperation(documentAfter!, replacement.Id);
        Assert.NotNull(opAfter);
        Assert.Equal(PickupEdlOperationState.Applied, opAfter!.State);

        var pickupEdits = runtime.EditListService
            .GetEdits(runtime.ChapterStem)
            .Where(edit => edit.Operation == EditOperation.PickupReplace)
            .ToArray();
        Assert.Single(pickupEdits);
        Assert.Equal(replacement.Id, pickupEdits[0].Id);
    }

    [Fact]
    public async Task RevertReplacementAsync_MissingUndoRecord_FailsClosedWithoutStateMutation()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: true);

        var replacement = CreateReplacement(
            id: "op-revert-missing-undo",
            chapterStem: runtime.ChapterStem,
            pickupPath: runtime.PickupPath,
            sentenceId: 41,
            status: ReplacementStatus.Staged);

        Assert.True(runtime.StagingQueue.TryStage(replacement, out var stageError), stageError ?? "stage failed");
        Assert.True(runtime.StagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Applied, syncEditList: false));

        var source = runtime.SourceBufferCache.DescribeSource(runtime.PickupPath);
        var appliedOperation = BuildOperation(runtime, replacement, source, PickupEdlOperationState.Applied);

        _ = runtime.Store.Mutate(
            runtime.ChapterStem,
            source,
            document => runtime.Engine.UpsertOperation(document, appliedOperation),
            CancellationToken.None);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.PolishService.RevertReplacementAsync(replacement.Id, CancellationToken.None));

        Assert.Contains("undo record is missing or malformed", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(runtime.ChapterStem, ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(replacement.Id, ex.Message, StringComparison.Ordinal);

        var queueItem = Assert.Single(runtime.StagingQueue.GetQueue(runtime.ChapterStem), item => item.Id == replacement.Id);
        Assert.Equal(ReplacementStatus.Applied, queueItem.Status);

        var documentAfter = runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(documentAfter);

        var operationAfter = runtime.Engine.TryGetOperation(documentAfter!, replacement.Id);
        Assert.NotNull(operationAfter);
        Assert.Equal(PickupEdlOperationState.Applied, operationAfter!.State);

        var ledger = runtime.ArtifactLedgerStore.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(ledger);

        var failureEntry = Assert.Single(
            ledger!.Entries,
            entry =>
                string.Equals(entry.OperationId, replacement.Id, StringComparison.Ordinal) &&
                string.Equals(entry.Transition, PickupArtifactLedgerTransitions.RevertFailure, StringComparison.Ordinal));
        Assert.Equal(PickupArtifactLedgerRollbackVerdict.NotAttempted, failureEntry.RollbackVerdict);
        Assert.Contains("undo record is missing or malformed", failureEntry.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommitFitReplacementAsync_RendersCompositeAndPreservesEdlLedgerRebuildPath()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: true, withValidAudio: true);
        var fitPlan = CreateAcceptedFitPlan(runtime);
        var fitItem = Assert.Single(fitPlan.Items);

        var result = await runtime.PolishService.CommitFitReplacementAsync(
            fitPlan,
            fitItem,
            runtime.RoomtonePath,
            CancellationToken.None);

        Assert.Equal(fitItem.ReplacementId, result.OperationId);
        Assert.Equal(2.3, result.RenderedReplacementDurationSec, precision: 3);
        Assert.Equal(0.3, result.TimingDeltaSec, precision: 3);

        var documentAfter = runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(documentAfter);
        var operation = Assert.Single(documentAfter!.Operations);
        Assert.Equal(fitItem.ReplacementId, operation.Id);
        Assert.Equal(PickupEdlOperationState.Applied, operation.State);
        Assert.Equal(fitItem.OuterRange.StartSec, operation.BaselineStartSec, precision: 6);
        Assert.Equal(fitItem.OuterRange.EndSec, operation.BaselineEndSec, precision: 6);
        Assert.Equal(2.3, operation.ExplicitReplacementDurationSec!.Value, precision: 3);
        Assert.NotNull(operation.FitMetadata);
        Assert.Equal(fitItem.FitItemId, operation.FitMetadata!.FitItemId);
        Assert.Equal(fitItem.PickAssignmentId, operation.FitMetadata.PickAssignmentId);
        Assert.Equal(fitItem.PickupSegmentId, operation.FitMetadata.PickupSegmentId);

        var edit = Assert.Single(runtime.EditListService.GetEdits(runtime.ChapterStem), e => e.Operation == EditOperation.PickupReplace);
        Assert.Equal(fitItem.ReplacementId, edit.Id);
        Assert.Equal(2.3, edit.ReplacementDurationSec, precision: 3);

        var undoRecord = runtime.UndoService.GetUndoRecord(fitItem.ReplacementId);
        Assert.NotNull(undoRecord);
        Assert.Equal(2.0, undoRecord!.OriginalDurationSec, precision: 3);
        Assert.Equal(2.3, undoRecord.ReplacementDurationSec, precision: 3);
        Assert.False(string.IsNullOrWhiteSpace(undoRecord.ReplacementSegmentPath));
        Assert.True(File.Exists(undoRecord.ReplacementSegmentPath));

        var ledger = runtime.ArtifactLedgerStore.TryRead(runtime.ChapterStem, CancellationToken.None);
        Assert.NotNull(ledger);
        Assert.Contains(
            ledger!.Entries,
            entry =>
                string.Equals(entry.OperationId, fitItem.ReplacementId, StringComparison.Ordinal) &&
                string.Equals(entry.Transition, PickupArtifactLedgerTransitions.CommitSuccess, StringComparison.Ordinal));

        var correctedRoot = runtime.Workspace.CurrentChapterHandle!.Chapter.Descriptor.RootPath;
        var correctedPath = Path.Combine(correctedRoot, $"{runtime.ChapterStem}.corrected.wav");
        Assert.True(File.Exists(correctedPath));
        var corrected = AudioProcessor.Decode(correctedPath);
        Assert.Equal(result.ResultBuffer.Length, corrected.Length);
    }

    [Fact]
    public async Task CommitFitReplacementAsync_MissingRoomtone_FailsBeforeEdlOrQueueMutation()
    {
        using var runtime = await CreateRuntimeAsync(withActiveChapter: true, withValidAudio: true);
        var fitPlan = CreateAcceptedFitPlan(runtime);
        var fitItem = Assert.Single(fitPlan.Items);
        var missingRoomtonePath = Path.Combine(runtime.Root, "missing-roomtone.wav");

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            runtime.PolishService.CommitFitReplacementAsync(
                fitPlan,
                fitItem,
                missingRoomtonePath,
                CancellationToken.None));

        Assert.Contains("roomtone file is missing", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(runtime.Store.TryRead(runtime.ChapterStem, CancellationToken.None));
        Assert.Empty(runtime.StagingQueue.GetQueue(runtime.ChapterStem));

        var correctedRoot = runtime.Workspace.CurrentChapterHandle!.Chapter.Descriptor.RootPath;
        Assert.False(File.Exists(Path.Combine(correctedRoot, $"{runtime.ChapterStem}.corrected.wav")));
    }

    private static PickupEdlOperation BuildOperation(
        RuntimeFixture runtime,
        StagedReplacement replacement,
        PickupEdlSourceReference source,
        PickupEdlOperationState state)
    {
        return runtime.Engine.BuildOperation(
            replacement,
            source,
            state,
            knownSentenceIds: null,
            updatedAtUtc: DateTime.UtcNow);
    }

    private static async Task<RuntimeFixture> CreateRuntimeAsync(bool withActiveChapter, bool withValidAudio = false)
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-reliability-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        var chapterStem = "chapter-01";
        var pickupPath = Path.Combine(root, ".pickups", "pickup.wav");
        var roomtonePath = Path.Combine(root, "roomtone.wav");
        if (withValidAudio)
        {
            WriteTestWav(pickupPath, durationSec: 4.0, seed: 0.25f);
            WriteTestWav(roomtonePath, durationSec: 0.25, seed: -0.05f);
        }
        else
        {
            WriteWavStub(pickupPath);
        }

        var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);

        string runtimeChapterStem;
        if (withActiveChapter)
        {
            var chapterWavPath = Path.Combine(root, $"{chapterStem}.wav");
            var bookPath = Path.Combine(root, "book.md");
            var bookIndexPath = Path.Combine(root, "book-index.json");

            if (withValidAudio)
            {
                WriteTestWav(chapterWavPath, durationSec: 4.0, seed: 0.5f);
            }
            else
            {
                WriteWavStub(chapterWavPath);
            }
            await File.WriteAllTextAsync(bookPath, "# Test Book\n\n## chapter-01\n\nHello world.");
            await CreateBookIndexAsync(new FileInfo(bookPath), new FileInfo(bookIndexPath));

            Assert.True(workspace.SetWorkingDirectory(root));
            workspace.SetPrecomputePeaksInBackground(false);

            var chapterDisplay = workspace.AvailableChapters.Single(chapter => string.Equals(chapter, chapterStem, StringComparison.OrdinalIgnoreCase));
            Assert.True(workspace.SelectChapter(chapterDisplay));
            runtimeChapterStem = workspace.CurrentChapterHandle?.Chapter.Descriptor.ChapterId
                ?? throw new InvalidOperationException("Failed to resolve active chapter.");
            if (withValidAudio)
            {
                var selectedRoot = workspace.CurrentChapterHandle!.Chapter.Descriptor.RootPath;
                WriteTestWav(Path.Combine(selectedRoot, $"{runtimeChapterStem}.treated.wav"), durationSec: 4.0, seed: 0.5f);
            }
        }
        else
        {
            Assert.True(workspace.SetWorkingDirectory(root));
            runtimeChapterStem = chapterStem;
        }

        var editList = new EditListService(workspace);
        var stagingQueue = new StagingQueueService(workspace, editList);
        var undoService = new UndoService(workspace);
        var sourceBufferCache = new PickupSourceBufferCache();
        var store = new PickupEdlStore(workspace);
        var artifactLedgerStore = new PickupArtifactLedgerStore(workspace);
        var engine = new PickupEdlEngine();
        var pickupMatching = new PickupMatchingService(workspace, new PickupMfaRefinementService(workspace));

        var polishService = new PolishService(
            workspace,
            stagingQueue,
            undoService,
            pickupMatching,
            new PreviewBufferService(),
            editList,
            store,
            artifactLedgerStore,
            engine,
            sourceBufferCache);

        return new RuntimeFixture(
            root,
            runtimeChapterStem,
            pickupPath,
            roomtonePath,
            workspace,
            polishService,
            stagingQueue,
            editList,
            undoService,
            sourceBufferCache,
            store,
            artifactLedgerStore,
            engine);
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

    private static void WriteTestWav(string path, double durationSec, float seed)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        const int sampleRate = 16_000;
        var length = Math.Max(1, (int)Math.Round(durationSec * sampleRate));
        var buffer = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: length);
        for (var i = 0; i < length; i++)
        {
            buffer[0, i] = seed + (i % 256) / 10_000f;
        }

        AudioProcessor.EncodeWav(path, buffer);
    }

    private static PickupFitPlanDocument CreateAcceptedFitPlan(RuntimeFixture runtime)
    {
        var source = runtime.SourceBufferCache.DescribeSource(runtime.PickupPath);
        var fitSource = new PickupPickMapSourceReference(
            path: source.Path,
            fingerprint: source.Fingerprint,
            fileSizeBytes: source.FileSizeBytes,
            modifiedAtUtc: source.ModifiedAtUtc,
            crxTargetsFingerprint: "crx-fit-runtime-fp");
        const int pickMapRevision = 3;
        const string assignmentFingerprint = "fit-assignment-fingerprint";
        var preview = new PickupFitPreviewEvidence(
            previewVersion: 2,
            pickMapRevision: pickMapRevision,
            pickAssignmentsFingerprint: assignmentFingerprint,
            previewArtifactRef: ".polish/pickups/preview/chapter-01/replacement.wav",
            renderedDurationSec: 2.3,
            generatedAtUtc: DateTime.UtcNow);
        var accepted = new PickupFitAcceptanceState(
            isAccepted: true,
            acceptedPreviewVersion: preview.PreviewVersion,
            acceptedAtUtc: DateTime.UtcNow,
            acceptedBy: "test");
        var target = new PickupPickMapTargetReference(
            chapterStem: runtime.ChapterStem,
            chapterName: "Chapter 01",
            errorNumber: 7,
            sentenceId: 11,
            originalStartSec: 0.5,
            originalEndSec: 2.5);
        var item = new PickupFitPlanItem(
            fitItemId: "fit::assignment-runtime",
            replacementId: "replacement::assignment-runtime",
            pickAssignmentId: "assignment-runtime",
            pickupSegmentId: "segment-runtime",
            target: target,
            outerRange: new PickupFitPlanRange(0.5, 2.5),
            innerRange: new PickupFitPlanRange(1.0, 2.0),
            placement: new PickupFitPlanRange(1.0, 2.0),
            transitionPolicy: new PickupFitTransitionPolicy(
                roomtoneBeforeSec: 0.1,
                roomtoneAfterSec: 0.2,
                crossfadeDurationSec: 0.05,
                crossfadeCurve: "hsin",
                roomtoneAssetId: "test-roomtone"),
            status: PickupFitPlanItemStatus.Accepted,
            previewEvidence: preview,
            acceptance: accepted,
            commit: PickupFitCommitState.NotReady,
            validationError: null,
            commitError: null,
            updatedAtUtc: DateTime.UtcNow);

        return new PickupFitPlanDocument(
            schemaVersion: PickupFitPlanDocument.CurrentSchemaVersion,
            chapterStem: runtime.ChapterStem,
            revision: 4,
            source: fitSource,
            pickMapRevision: pickMapRevision,
            pickAssignmentsFingerprint: assignmentFingerprint,
            items: [item],
            createdAtUtc: DateTime.UtcNow.AddMinutes(-5),
            updatedAtUtc: DateTime.UtcNow,
            lastOperationId: null,
            lastValidationError: null,
            isDraft: false);
    }

    private static StagedReplacement CreateReplacement(
        string id,
        string chapterStem,
        string pickupPath,
        int sentenceId,
        ReplacementStatus status)
    {
        return new StagedReplacement(
            Id: id,
            ChapterStem: chapterStem,
            SentenceId: sentenceId,
            OriginalStartSec: 0.10,
            OriginalEndSec: 0.30,
            PickupSourcePath: pickupPath,
            PickupStartSec: 0.00,
            PickupEndSec: 0.20,
            CrossfadeDurationSec: 0.05,
            CrossfadeCurve: "hsin",
            StagedAtUtc: DateTime.UtcNow,
            Status: status);
    }

    private sealed class RuntimeFixture : IDisposable
    {
        public RuntimeFixture(
            string root,
            string chapterStem,
            string pickupPath,
            string roomtonePath,
            BlazorWorkspace workspace,
            PolishService polishService,
            StagingQueueService stagingQueue,
            EditListService editListService,
            UndoService undoService,
            PickupSourceBufferCache sourceBufferCache,
            PickupEdlStore store,
            PickupArtifactLedgerStore artifactLedgerStore,
            PickupEdlEngine engine)
        {
            Root = root;
            ChapterStem = chapterStem;
            PickupPath = pickupPath;
            RoomtonePath = roomtonePath;
            Workspace = workspace;
            PolishService = polishService;
            StagingQueue = stagingQueue;
            EditListService = editListService;
            UndoService = undoService;
            SourceBufferCache = sourceBufferCache;
            Store = store;
            ArtifactLedgerStore = artifactLedgerStore;
            Engine = engine;
        }

        public string Root { get; }

        public string ChapterStem { get; }

        public string PickupPath { get; }

        public string RoomtonePath { get; }

        public BlazorWorkspace Workspace { get; }

        public PolishService PolishService { get; }

        public StagingQueueService StagingQueue { get; }

        public EditListService EditListService { get; }

        public UndoService UndoService { get; }

        public PickupSourceBufferCache SourceBufferCache { get; }

        public PickupEdlStore Store { get; }

        public PickupArtifactLedgerStore ArtifactLedgerStore { get; }

        public PickupEdlEngine Engine { get; }

        public void Dispose()
        {
            Workspace.Dispose();

            try
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, recursive: true);
                }
            }
            catch
            {
                // best effort cleanup only
            }
        }
    }
}
