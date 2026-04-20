using System.IO;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Tests.Services;

public sealed class PickupArtifactLedgerTests
{
    [Fact]
    public void Append_FirstEntryInEmptyLedger_CreatesChapterScopedDocument()
    {
        var root = CreateTempDirectory();
        try
        {
            var chapterStem = "chapter-01";
            var store = new PickupArtifactLedgerStore(() => root);

            var appended = store.Append(
                chapterStem,
                BuildDraft(
                    chapterStem,
                    operationId: "op-01",
                    transition: PickupArtifactLedgerTransitions.CommitAttempt,
                    phase: "apply",
                    edlRevision: 0,
                    queueStatus: ReplacementStatus.Staged,
                    edlState: PickupEdlOperationState.Staged,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));

            Assert.Equal(1, appended.Revision);
            Assert.Equal(1, appended.LastSequence);

            var entry = Assert.Single(appended.Entries);
            Assert.Equal(1, entry.Sequence);
            Assert.Equal("op-01", entry.OperationId);
            Assert.Equal(PickupArtifactLedgerTransitions.CommitAttempt, entry.Transition);
            Assert.Equal("apply", entry.Phase);
            Assert.Equal(0, entry.EdlRevision);
            Assert.Equal(ReplacementStatus.Staged, entry.QueueStatus);
            Assert.Equal(PickupEdlOperationState.Staged, entry.EdlState);
            Assert.Equal(PickupArtifactLedgerRollbackVerdict.NotRequired, entry.RollbackVerdict);
            Assert.Equal(
                new[]
                {
                    ".polish/edl/chapter-01.artifact-ledger.json",
                    ".polish/edl/chapter-01.edl.json"
                },
                entry.ArtifactRefs);

            Assert.True(File.Exists(store.GetDocumentPath(chapterStem)));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Append_ManyEntries_MaintainsDeterministicSequenceOrder()
    {
        var root = CreateTempDirectory();
        try
        {
            var chapterStem = "chapter-many";
            var store = new PickupArtifactLedgerStore(() => root);

            PickupArtifactLedgerDocument? current = null;
            for (var i = 1; i <= 25; i++)
            {
                current = store.Append(
                    chapterStem,
                    BuildDraft(
                        chapterStem,
                        operationId: $"op-{i:D2}",
                        transition: i % 2 == 0
                            ? PickupArtifactLedgerTransitions.CommitSuccess
                            : PickupArtifactLedgerTransitions.CommitAttempt,
                        phase: "batch-apply",
                        edlRevision: i,
                        queueStatus: i % 2 == 0 ? ReplacementStatus.Applied : ReplacementStatus.Staged,
                        edlState: i % 2 == 0 ? PickupEdlOperationState.Applied : PickupEdlOperationState.Staged,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));
            }

            Assert.NotNull(current);
            Assert.Equal(25, current!.Revision);
            Assert.Equal(25, current.LastSequence);
            Assert.Equal(25, current.Entries.Count);

            var sequences = current.GetDeterministicOrder().Select(entry => entry.Sequence).ToArray();
            Assert.Equal(Enumerable.Range(1, 25).Select(i => (long)i), sequences);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Append_ChapterIsolation_MaintainsIndependentDocuments()
    {
        var root = CreateTempDirectory();
        try
        {
            var store = new PickupArtifactLedgerStore(() => root);

            _ = store.Append(
                "chapter-a",
                BuildDraft(
                    "chapter-a",
                    operationId: "op-a-01",
                    transition: PickupArtifactLedgerTransitions.CommitAttempt,
                    phase: "apply",
                    edlRevision: 0,
                    queueStatus: ReplacementStatus.Staged,
                    edlState: PickupEdlOperationState.Staged,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));

            _ = store.Append(
                "chapter-b",
                BuildDraft(
                    "chapter-b",
                    operationId: "op-b-01",
                    transition: PickupArtifactLedgerTransitions.CommitAttempt,
                    phase: "apply",
                    edlRevision: 0,
                    queueStatus: ReplacementStatus.Staged,
                    edlState: PickupEdlOperationState.Staged,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));

            var chapterADoc = store.Append(
                "chapter-a",
                BuildDraft(
                    "chapter-a",
                    operationId: "op-a-02",
                    transition: PickupArtifactLedgerTransitions.CommitSuccess,
                    phase: "apply",
                    edlRevision: 2,
                    queueStatus: ReplacementStatus.Applied,
                    edlState: PickupEdlOperationState.Applied,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));

            var chapterBDoc = store.TryRead("chapter-b", CancellationToken.None);
            Assert.NotNull(chapterBDoc);

            Assert.Equal(2, chapterADoc.Revision);
            Assert.Equal(2, chapterADoc.LastSequence);
            Assert.Equal(1, chapterBDoc!.Revision);
            Assert.Equal(1, chapterBDoc.LastSequence);

            Assert.Equal("op-a-02", chapterADoc.Entries[^1].OperationId);
            Assert.Equal("op-b-01", Assert.Single(chapterBDoc.Entries).OperationId);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void LoadOrCreate_CorruptedLedger_QuarantinesMalformedFile()
    {
        var root = CreateTempDirectory();
        try
        {
            var chapterStem = "chapter-corrupt";
            var store = new PickupArtifactLedgerStore(() => root);
            var documentPath = store.GetDocumentPath(chapterStem);
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "{ malformed json");

            var ex = Assert.Throws<InvalidOperationException>(() => store.LoadOrCreate(chapterStem));

            Assert.Contains("Malformed pickup artifact ledger JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
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
    public void Append_AtomicWriteFailure_KeepsPreviousDocumentIntact()
    {
        var root = CreateTempDirectory();
        try
        {
            var chapterStem = "chapter-atomic";
            var stableStore = new PickupArtifactLedgerStore(() => root);

            _ = stableStore.Append(
                chapterStem,
                BuildDraft(
                    chapterStem,
                    operationId: "op-stable",
                    transition: PickupArtifactLedgerTransitions.CommitAttempt,
                    phase: "apply",
                    edlRevision: 1,
                    queueStatus: ReplacementStatus.Staged,
                    edlState: PickupEdlOperationState.Staged,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));

            var documentPath = stableStore.GetDocumentPath(chapterStem);
            var before = File.ReadAllText(documentPath);

            var failingStore = new PickupArtifactLedgerStore(
                workspaceRootResolver: () => root,
                atomicWrite: (_, _, _) => throw new IOException("simulated artifact ledger write failure"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                failingStore.Append(
                    chapterStem,
                    BuildDraft(
                        chapterStem,
                        operationId: "op-fail",
                        transition: PickupArtifactLedgerTransitions.CommitSuccess,
                        phase: "apply",
                        edlRevision: 2,
                        queueStatus: ReplacementStatus.Applied,
                        edlState: PickupEdlOperationState.Applied,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired)));

            Assert.Contains("write failed after retry", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(ex.InnerException);
            Assert.Contains("simulated artifact ledger write failure", ex.InnerException!.Message, StringComparison.OrdinalIgnoreCase);

            var after = File.ReadAllText(documentPath);
            Assert.Equal(before, after);

            var persisted = stableStore.TryRead(chapterStem, CancellationToken.None);
            Assert.NotNull(persisted);
            Assert.Equal(1, persisted!.Revision);
            Assert.Single(persisted.Entries);
            Assert.Equal("op-stable", persisted.Entries[0].OperationId);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void EntryDraft_MalformedInputs_AreRejected()
    {
        var chapterStem = "chapter-malformed";

        var nullOpEx = Assert.Throws<ArgumentNullException>(() =>
            BuildDraft(
                chapterStem,
                operationId: null!,
                transition: PickupArtifactLedgerTransitions.CommitAttempt,
                phase: "apply",
                edlRevision: 0,
                queueStatus: ReplacementStatus.Staged,
                edlState: PickupEdlOperationState.Staged,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));
        Assert.Contains("operationId", nullOpEx.Message, StringComparison.OrdinalIgnoreCase);

        var transitionEx = Assert.Throws<ArgumentException>(() =>
            BuildDraft(
                chapterStem,
                operationId: "op-01",
                transition: "unknown.transition",
                phase: "apply",
                edlRevision: 0,
                queueStatus: ReplacementStatus.Staged,
                edlState: PickupEdlOperationState.Staged,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired));
        Assert.Contains("unknown transition", transitionEx.Message, StringComparison.OrdinalIgnoreCase);

        var refsEx = Assert.Throws<ArgumentException>(() =>
            BuildDraft(
                chapterStem,
                operationId: "op-02",
                transition: PickupArtifactLedgerTransitions.CommitAttempt,
                phase: "apply",
                edlRevision: 0,
                queueStatus: ReplacementStatus.Staged,
                edlState: PickupEdlOperationState.Staged,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                artifactRefs: []));
        Assert.Contains("missing artifact refs", refsEx.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Program_RegistersPickupArtifactLedgerStore()
    {
        var repoRoot = FindRepoRoot();
        var programPath = Path.Combine(repoRoot, "host", "Ams.Workstation.Server", "Program.cs");

        var source = File.ReadAllText(programPath);
        Assert.Contains("builder.Services.AddSingleton<PickupArtifactLedgerStore>();", source, StringComparison.Ordinal);
    }

    private static PickupArtifactLedgerEntryDraft BuildDraft(
        string chapterStem,
        string operationId,
        string transition,
        string phase,
        int edlRevision,
        ReplacementStatus queueStatus,
        PickupEdlOperationState edlState,
        PickupArtifactLedgerRollbackVerdict rollbackVerdict,
        IReadOnlyList<string>? artifactRefs = null,
        string? failureReason = null)
    {
        return new PickupArtifactLedgerEntryDraft(
            operationId: operationId,
            transition: transition,
            phase: phase,
            edlRevision: edlRevision,
            queueStatus: queueStatus,
            edlState: edlState,
            rollbackVerdict: rollbackVerdict,
            artifactRefs: artifactRefs
                ??
                [
                    $".polish/edl/{chapterStem}.edl.json",
                    $".polish/edl/{chapterStem}.artifact-ledger.json"
                ],
            failureReason: failureReason,
            occurredAtUtc: DateTime.UtcNow);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CODE-STYLE.md")) &&
                Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md.");
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-artifact-ledger-{Guid.NewGuid():N}");
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
