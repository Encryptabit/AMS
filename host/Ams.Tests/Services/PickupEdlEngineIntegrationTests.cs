using System.IO;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Tests.Services;

public sealed class PickupEdlEngineIntegrationTests
{
    [Fact]
    public void StoreAndEngine_UpsertAndTransitions_IncrementRevisionAndProduceDeterministicOrders()
    {
        var root = CreateTempDirectory();
        using var workspace = CreateWorkspace(root);

        try
        {
            var chapterStem = "chapter-10";
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var sourceCache = new PickupSourceBufferCache(_ => throw new InvalidOperationException("decode not expected"));
            var source = sourceCache.DescribeSource(sourcePath);
            var engine = new PickupEdlEngine();
            var store = new PickupEdlStore(workspace);

            var opA = engine.BuildOperation(
                CreateReplacement("op-a", chapterStem, sentenceId: 11, originalStartSec: 5, originalEndSec: 6, pickupStartSec: 0, pickupEndSec: 1),
                source,
                PickupEdlOperationState.Staged,
                knownSentenceIds: new HashSet<int> { 11, 12 });

            var doc1 = store.Mutate(chapterStem, source, document => engine.UpsertOperation(document, opA));
            Assert.Equal(1, doc1.Revision);
            Assert.Single(doc1.Operations);

            var doc2 = store.Mutate(
                chapterStem,
                source,
                document => engine.TransitionOperationState(document, "op-a", PickupEdlOperationState.Applied));
            Assert.Equal(2, doc2.Revision);

            var opB = engine.BuildOperation(
                CreateReplacement("op-b", chapterStem, sentenceId: 12, originalStartSec: 20, originalEndSec: 21.5, pickupStartSec: 2, pickupEndSec: 3),
                source,
                PickupEdlOperationState.Applied,
                knownSentenceIds: new HashSet<int> { 11, 12 });

            var doc3 = store.Mutate(chapterStem, source, document => engine.UpsertOperation(document, opB));
            Assert.Equal(3, doc3.Revision);

            var rebuildOrder = engine.BuildDeterministicRebuildOrder(doc3).Select(op => op.Id).ToArray();
            var projectionOrder = engine.BuildAppliedProjectionEdits(doc3).Select(edit => edit.Id).ToArray();

            Assert.Equal(new[] { "op-b", "op-a" }, rebuildOrder);
            Assert.Equal(new[] { "op-a", "op-b" }, projectionOrder);
            Assert.Contains("chapter=chapter-10", engine.BuildDeterministicOrderingDiagnostics(doc3), StringComparison.Ordinal);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void StoreAndEngine_TransitionsPreserveExplicitFitDurationAndMetadata()
    {
        var root = CreateTempDirectory();
        using var workspace = CreateWorkspace(root);

        try
        {
            var chapterStem = "chapter-10-fit";
            var sourcePath = CreateTempSourceFile(root, "pickup-fit.wav");
            var sourceCache = new PickupSourceBufferCache(_ => throw new InvalidOperationException("decode not expected"));
            var source = sourceCache.DescribeSource(sourcePath);
            var engine = new PickupEdlEngine();
            var store = new PickupEdlStore(workspace);
            var metadata = new PickupEdlFitMetadata(
                fitItemId: "fit::pick-fit-001",
                pickAssignmentId: "pick-fit-001",
                pickupSegmentId: "segment-fit-001",
                previewVersion: 2,
                pickMapRevision: 5,
                pickAssignmentsFingerprint: "fp-fit-picks");

            var op = engine.BuildOperation(
                CreateReplacement(
                    "op-fit",
                    chapterStem,
                    sentenceId: 31,
                    originalStartSec: 5,
                    originalEndSec: 7,
                    pickupStartSec: 1,
                    pickupEndSec: 2),
                source,
                PickupEdlOperationState.Staged,
                knownSentenceIds: new HashSet<int> { 31 },
                updatedAtUtc: new DateTime(2026, 01, 10, 0, 0, 0, DateTimeKind.Utc),
                explicitReplacementDurationSec: 2.75,
                fitMetadata: metadata);

            _ = store.Mutate(chapterStem, source, document => engine.UpsertOperation(document, op));
            var transitioned = store.Mutate(
                chapterStem,
                source,
                document => engine.TransitionOperationState(document, "op-fit", PickupEdlOperationState.Applied));

            var persisted = store.TryRead(chapterStem, CancellationToken.None);

            Assert.NotNull(persisted);
            var actual = Assert.Single(persisted!.Operations);
            Assert.Equal(PickupEdlOperationState.Applied, actual.State);
            Assert.Equal(2.75, actual.ExplicitReplacementDurationSec);
            Assert.Equal(2.75, actual.ReplacementDurationSec);
            Assert.NotNull(actual.FitMetadata);
            Assert.Equal("fit::pick-fit-001", actual.FitMetadata!.FitItemId);
            Assert.Equal("pick-fit-001", actual.FitMetadata.PickAssignmentId);
            Assert.Equal("segment-fit-001", actual.FitMetadata.PickupSegmentId);

            var projectionEdit = Assert.Single(engine.BuildAppliedProjectionEdits(transitioned));
            Assert.Equal(2.75, projectionEdit.ReplacementDurationSec);
            Assert.Contains("replacementDurations=[op-fit:2.750000s]", engine.BuildDeterministicOrderingDiagnostics(transitioned), StringComparison.Ordinal);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void TransitionOperationState_AppliedToApplied_ThrowsIllegalTransitionDiagnostics()
    {
        var engine = new PickupEdlEngine();
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickup.wav",
            fingerprint: "fp-illegal-applied",
            fileSizeBytes: 456,
            modifiedAtUtc: DateTime.UtcNow);

        var op = engine.BuildOperation(
            CreateReplacement("op-illegal", "chapter-10", sentenceId: 14, originalStartSec: 10, originalEndSec: 11, pickupStartSec: 0, pickupEndSec: 1),
            source,
            PickupEdlOperationState.Applied,
            knownSentenceIds: new HashSet<int> { 14 });

        var document = new PickupEdlDocument(
            PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: "chapter-10",
            revision: 2,
            source,
            operations: [op]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            engine.TransitionOperationState(document, "op-illegal", PickupEdlOperationState.Applied));

        Assert.Contains("illegal transition", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("chapter-10", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("op-illegal", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Applied", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TransitionOperationState_RevertedToApplied_ThrowsIllegalTransitionDiagnostics()
    {
        var engine = new PickupEdlEngine();
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickup.wav",
            fingerprint: "fp-illegal-reverted",
            fileSizeBytes: 789,
            modifiedAtUtc: DateTime.UtcNow);

        var op = engine.BuildOperation(
            CreateReplacement("op-reverted", "chapter-10", sentenceId: 15, originalStartSec: 12, originalEndSec: 13, pickupStartSec: 0, pickupEndSec: 1),
            source,
            PickupEdlOperationState.Reverted,
            knownSentenceIds: new HashSet<int> { 15 });

        var document = new PickupEdlDocument(
            PickupEdlDocument.CurrentSchemaVersion,
            chapterStem: "chapter-10",
            revision: 3,
            source,
            operations: [op]);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            engine.TransitionOperationState(document, "op-reverted", PickupEdlOperationState.Applied));

        Assert.Contains("illegal transition", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Reverted", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Applied", ex.Message, StringComparison.Ordinal);
        Assert.Contains("op-reverted", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildOperation_UnknownSentence_ThrowsWithChapterAndSentenceDiagnostics()
    {
        var engine = new PickupEdlEngine();
        var source = new PickupEdlSourceReference(
            path: "/tmp/pickup.wav",
            fingerprint: "fp-unknown-sentence",
            fileSizeBytes: 123,
            modifiedAtUtc: DateTime.UtcNow);

        var replacement = CreateReplacement(
            id: "op-unknown",
            chapterStem: "chapter-11",
            sentenceId: 99,
            originalStartSec: 1,
            originalEndSec: 2,
            pickupStartSec: 0,
            pickupEndSec: 1);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            engine.BuildOperation(
                replacement,
                source,
                PickupEdlOperationState.Staged,
                knownSentenceIds: new HashSet<int> { 1, 2, 3 }));

        Assert.Contains("op-unknown", ex.Message, StringComparison.Ordinal);
        Assert.Contains("chapter-11", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unknown sentence", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadOrCreate_CorruptedEdl_QuarantinesMalformedFile()
    {
        var root = CreateTempDirectory();
        using var workspace = CreateWorkspace(root);

        try
        {
            var chapterStem = "chapter-12";
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var sourceCache = new PickupSourceBufferCache(_ => throw new InvalidOperationException("decode not expected"));
            var source = sourceCache.DescribeSource(sourcePath);
            var store = new PickupEdlStore(workspace);

            var documentPath = store.GetDocumentPath(chapterStem);
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "{ malformed json");

            var ex = Assert.Throws<InvalidOperationException>(() => store.LoadOrCreate(chapterStem, source));

            Assert.Contains("Malformed pickup EDL JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(documentPath));

            var quarantineFiles = Directory.GetFiles(
                Path.GetDirectoryName(documentPath)!,
                $"{Path.GetFileName(documentPath)}.malformed.*.json");
            Assert.Single(quarantineFiles);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Mutate_AtomicWriteFailure_KeepsPreviousDocumentIntact()
    {
        var root = CreateTempDirectory();
        try
        {
            var chapterStem = "chapter-13";
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var sourceCache = new PickupSourceBufferCache(_ => throw new InvalidOperationException("decode not expected"));
            var source = sourceCache.DescribeSource(sourcePath);
            var engine = new PickupEdlEngine();

            var stableStore = new PickupEdlStore(() => root);
            var opA = engine.BuildOperation(
                CreateReplacement("op-a", chapterStem, sentenceId: 21, originalStartSec: 1, originalEndSec: 2, pickupStartSec: 0, pickupEndSec: 1),
                source,
                PickupEdlOperationState.Staged,
                knownSentenceIds: new HashSet<int> { 21, 22 });
            _ = stableStore.Mutate(chapterStem, source, doc => engine.UpsertOperation(doc, opA));

            var documentPath = stableStore.GetDocumentPath(chapterStem);
            var before = File.ReadAllText(documentPath);

            var failingStore = new PickupEdlStore(
                workspaceRootResolver: () => root,
                atomicWrite: (_, _, _) => throw new IOException("simulated atomic write failure"));

            var opB = engine.BuildOperation(
                CreateReplacement("op-b", chapterStem, sentenceId: 22, originalStartSec: 4, originalEndSec: 5, pickupStartSec: 2, pickupEndSec: 3),
                source,
                PickupEdlOperationState.Staged,
                knownSentenceIds: new HashSet<int> { 21, 22 });

            var ex = Assert.Throws<IOException>(() =>
                failingStore.Mutate(chapterStem, source, doc => engine.UpsertOperation(doc, opB)));
            Assert.Contains("simulated atomic write failure", ex.Message, StringComparison.OrdinalIgnoreCase);

            var after = File.ReadAllText(documentPath);
            Assert.Equal(before, after);

            var persisted = stableStore.LoadOrCreate(chapterStem, source);
            Assert.Equal(1, persisted.Revision);
            Assert.Single(persisted.Operations);
            Assert.Equal("op-a", persisted.Operations[0].Id);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    private static BlazorWorkspace CreateWorkspace(string root)
    {
        var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
        Assert.True(workspace.SetWorkingDirectory(root));
        return workspace;
    }

    private static StagedReplacement CreateReplacement(
        string id,
        string chapterStem,
        int sentenceId,
        double originalStartSec,
        double originalEndSec,
        double pickupStartSec,
        double pickupEndSec)
    {
        return new StagedReplacement(
            Id: id,
            ChapterStem: chapterStem,
            SentenceId: sentenceId,
            OriginalStartSec: originalStartSec,
            OriginalEndSec: originalEndSec,
            PickupSourcePath: "/tmp/pickup.wav",
            PickupStartSec: pickupStartSec,
            PickupEndSec: pickupEndSec,
            CrossfadeDurationSec: 0.07,
            CrossfadeCurve: "hsin",
            StagedAtUtc: DateTime.UtcNow,
            Status: ReplacementStatus.Staged);
    }

    private static string CreateTempSourceFile(string root, string fileName)
    {
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 });
        return path;
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-edl-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
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
}
