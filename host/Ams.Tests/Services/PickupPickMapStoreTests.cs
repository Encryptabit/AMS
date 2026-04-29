using System.IO;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Pick;

namespace Ams.Tests.Services;

public sealed class PickupPickMapStoreTests
{
    private static readonly DateTime FixedUtc = new(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Save_ThenTryRead_RoundTripsCanonicalBatchDocument()
    {
        var root = CreateTempDirectory();
        try
        {
            var source = CreateSource();
            var store = new PickupPickMapStore(() => root);
            var document = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-001",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1.25,
                        segmentEndSec: 2.50,
                        inferredTarget: CreateTarget("chapter-01", errorNumber: 3),
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 3))
                ]);

            var saved = store.Save(source, document);
            var loaded = store.TryRead();

            Assert.NotNull(loaded);
            Assert.Equal("pickup-pick-map/v1", saved.SchemaVersion);
            Assert.Equal(1, saved.Revision);
            Assert.Equal(source.Fingerprint, loaded!.Source.Fingerprint);
            Assert.Equal(source.CrxTargetsFingerprint, loaded.Source.CrxTargetsFingerprint);
            var row = Assert.Single(loaded.Assignments);
            Assert.Equal("seg-001", row.Id);
            Assert.Equal(PickupPickMapAssignmentStatus.Confirmed, row.Status);
            Assert.Equal("chapter-01", row.SelectedTarget?.ChapterStem);
            Assert.True(File.Exists(store.GetDocumentPath()));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void DeterministicOrder_GroupsByChapterThenError_WithUnassignedLast()
    {
        var source = CreateSource();
        var document = CreateDocument(
            source,
            [
                CreateAssignment(
                    id: "seg-late",
                    status: PickupPickMapAssignmentStatus.Confirmed,
                    segmentStartSec: 0,
                    segmentEndSec: 1,
                    selectedTarget: CreateTarget("chapter-10", errorNumber: 2)),
                CreateAssignment(
                    id: "seg-deferred",
                    status: PickupPickMapAssignmentStatus.Deferred,
                    segmentStartSec: 1,
                    segmentEndSec: 2),
                CreateAssignment(
                    id: "seg-middle",
                    status: PickupPickMapAssignmentStatus.Override,
                    segmentStartSec: 2,
                    segmentEndSec: 3,
                    selectedTarget: CreateTarget("chapter-02", errorNumber: 5)),
                CreateAssignment(
                    id: "seg-first",
                    status: PickupPickMapAssignmentStatus.Inferred,
                    segmentStartSec: 3,
                    segmentEndSec: 4,
                    inferredTarget: CreateTarget("chapter-02", errorNumber: 1))
            ]);

        var orderedIds = document.GetDeterministicAssignmentOrder().Select(row => row.Id).ToArray();
        Assert.Equal(["seg-first", "seg-middle", "seg-late", "seg-deferred"], orderedIds);

        var groups = document.GetDeterministicChapterGroups();
        Assert.Equal(["chapter-02", "chapter-10", PickupPickMapDocument.UnassignedChapterGroup], groups.Select(group => group.ChapterStem));
        Assert.Equal([2, 1, 1], groups.Select(group => group.Assignments.Count));
    }

    [Theory]
    [InlineData(PickupPickMapAssignmentStatus.Confirmed)]
    [InlineData(PickupPickMapAssignmentStatus.Override)]
    public void Assignment_RejectsConfirmedOrOverrideWithoutSelectedTarget(PickupPickMapAssignmentStatus status)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateAssignment(
                id: "seg-no-target",
                status: status,
                segmentStartSec: 1,
                segmentEndSec: 2));

        Assert.Contains("selected target", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("seg-no-target", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadOrCreate_StaleSourceFingerprintWithAssignments_FailsClosedWithoutMutation()
    {
        var root = CreateTempDirectory();
        try
        {
            var originalSource = CreateSource(fingerprint: "batch-fp-original");
            var staleSource = CreateSource(fingerprint: "batch-fp-stale");
            var store = new PickupPickMapStore(() => root);
            var document = CreateDocument(
                originalSource,
                [
                    CreateAssignment(
                        id: "seg-stable",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1))
                ]);

            _ = store.Save(originalSource, document);
            var before = File.ReadAllText(store.GetDocumentPath());

            var ex = Assert.Throws<InvalidOperationException>(() => store.LoadOrCreate(staleSource));

            Assert.Contains("fingerprint mismatch", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("batch-fp-original", ex.Message, StringComparison.Ordinal);
            Assert.Contains("batch-fp-stale", ex.Message, StringComparison.Ordinal);
            Assert.Equal(before, File.ReadAllText(store.GetDocumentPath()));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void TryRead_MalformedJson_QuarantinesFileAndReportsPath()
    {
        var root = CreateTempDirectory();
        try
        {
            var store = new PickupPickMapStore(() => root);
            var documentPath = store.GetDocumentPath();
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "{ malformed pick map json");

            var ex = Assert.Throws<InvalidOperationException>(() => store.TryRead());

            Assert.Contains("Malformed pickup pick-map JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(documentPath, ex.Message, StringComparison.Ordinal);
            Assert.Contains("malformed", ex.Message, StringComparison.OrdinalIgnoreCase);
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
    public void Save_AtomicWriteFailureRetriesThenKeepsPreviousDocumentIntact()
    {
        var root = CreateTempDirectory();
        try
        {
            var source = CreateSource();
            var stableStore = new PickupPickMapStore(() => root);
            var stableDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-stable",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1))
                ]);
            _ = stableStore.Save(source, stableDocument);
            var before = File.ReadAllText(stableStore.GetDocumentPath());

            var attempts = 0;
            var failingStore = new PickupPickMapStore(
                workspaceRootResolver: () => root,
                atomicWrite: (_, _, _) =>
                {
                    attempts++;
                    throw new IOException("simulated pick-map write failure");
                });
            var nextDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-new",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 2))
                ]);

            var ex = Assert.Throws<InvalidOperationException>(() => failingStore.Save(source, nextDocument));

            Assert.Equal(2, attempts);
            Assert.Contains("write failed after retry", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(ex.InnerException);
            Assert.Contains("simulated pick-map write failure", ex.InnerException!.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, File.ReadAllText(stableStore.GetDocumentPath()));
            var loaded = stableStore.TryRead();
            Assert.NotNull(loaded);
            Assert.Equal("seg-stable", Assert.Single(loaded!.Assignments).Id);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Save_FirstWriteCorruptsDestinationAndThrows_RetriesWithoutQuarantiningCurrentMap()
    {
        var root = CreateTempDirectory();
        try
        {
            var source = CreateSource();
            var stableStore = new PickupPickMapStore(() => root);
            var stableDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-stable",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1))
                ]);
            _ = stableStore.Save(source, stableDocument);
            var documentPath = stableStore.GetDocumentPath();

            var attempts = 0;
            var retryingStore = new PickupPickMapStore(
                workspaceRootResolver: () => root,
                atomicWrite: (path, json, _) =>
                {
                    attempts++;
                    if (attempts == 1)
                    {
                        File.WriteAllText(path, "{ corrupted by interrupted pick-map write");
                        throw new IOException("simulated interrupted pick-map write after destination corruption");
                    }

                    File.WriteAllText(path, json);
                });
            var nextDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-retry-success",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 2))
                ]);

            var saved = retryingStore.Save(source, nextDocument);
            var loaded = stableStore.TryRead();

            Assert.Equal(2, attempts);
            Assert.Equal(2, saved.Revision);
            Assert.NotNull(loaded);
            Assert.Equal("seg-retry-success", Assert.Single(loaded!.Assignments).Id);
            Assert.Empty(Directory.GetFiles(
                Path.GetDirectoryName(documentPath)!,
                $"{Path.GetFileName(documentPath)}.malformed.*.json"));
            Assert.True(File.Exists(documentPath));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Save_CorruptingWriteFailures_RestorePreviousDocumentWithoutQuarantine()
    {
        var root = CreateTempDirectory();
        try
        {
            var source = CreateSource();
            var stableStore = new PickupPickMapStore(() => root);
            var stableDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-stable",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1))
                ]);
            _ = stableStore.Save(source, stableDocument);
            var documentPath = stableStore.GetDocumentPath();
            var before = File.ReadAllText(documentPath);

            var attempts = 0;
            var failingStore = new PickupPickMapStore(
                workspaceRootResolver: () => root,
                atomicWrite: (path, _, _) =>
                {
                    attempts++;
                    File.WriteAllText(path, $"{{ corrupted by failed pick-map write attempt {attempts}");
                    throw new IOException($"simulated corrupting pick-map write failure {attempts}");
                });
            var nextDocument = CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-never-saved",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 2))
                ]);

            var ex = Assert.Throws<InvalidOperationException>(() => failingStore.Save(source, nextDocument));
            var loaded = stableStore.TryRead();

            Assert.Equal(2, attempts);
            Assert.Contains("write failed after retry", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, File.ReadAllText(documentPath));
            Assert.NotNull(loaded);
            Assert.Equal("seg-stable", Assert.Single(loaded!.Assignments).Id);
            Assert.Empty(Directory.GetFiles(
                Path.GetDirectoryName(documentPath)!,
                $"{Path.GetFileName(documentPath)}.malformed.*.json"));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Document_RejectsDuplicateAssignmentIds_WithSourceDiagnostics()
    {
        var source = CreateSource();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateDocument(
                source,
                [
                    CreateAssignment(
                        id: "seg-duplicate",
                        pickupSegmentId: "segment-a",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1)),
                    CreateAssignment(
                        id: "seg-duplicate",
                        pickupSegmentId: "segment-b",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 2))
                ]));

        Assert.Contains("duplicate assignment id", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("seg-duplicate", ex.Message, StringComparison.Ordinal);
        Assert.Contains(source.Path, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_RejectsDuplicatePickupSegmentIds()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateDocument(
                CreateSource(),
                [
                    CreateAssignment(
                        id: "row-a",
                        pickupSegmentId: "pickup-segment-001",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 1)),
                    CreateAssignment(
                        id: "row-b",
                        pickupSegmentId: "pickup-segment-001",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: CreateTarget("chapter-01", errorNumber: 2))
                ]));

        Assert.Contains("duplicate pickup segment id", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pickup-segment-001", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_RejectsDuplicateSelectedTargets()
    {
        var duplicateTarget = CreateTarget("chapter-01", errorNumber: 7);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateDocument(
                CreateSource(),
                [
                    CreateAssignment(
                        id: "row-a",
                        status: PickupPickMapAssignmentStatus.Confirmed,
                        segmentStartSec: 1,
                        segmentEndSec: 2,
                        selectedTarget: duplicateTarget),
                    CreateAssignment(
                        id: "row-b",
                        status: PickupPickMapAssignmentStatus.Override,
                        segmentStartSec: 3,
                        segmentEndSec: 4,
                        selectedTarget: duplicateTarget)
                ]));

        Assert.Contains("duplicate selected target", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("chapter-01", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("7", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_AllowsEmptyAssignmentsOnlyWhenDraftIsExplicit()
    {
        var source = CreateSource();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateDocument(source, [], isDraft: false));
        Assert.Contains("IsDraft", ex.Message, StringComparison.Ordinal);

        var draft = CreateDocument(source, [], isDraft: true);

        Assert.True(draft.IsDraft);
        Assert.Empty(draft.Assignments);
    }

    [Fact]
    public void SourceReference_RejectsBlankSourcePath()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new PickupPickMapSourceReference(
                path: " ",
                fingerprint: "fp",
                fileSizeBytes: 1,
                modifiedAtUtc: FixedUtc,
                crxTargetsFingerprint: "crx"));

        Assert.Contains("path", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assignment_RejectsUnknownStatusInteger()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateAssignment(
                id: "seg-bad-status",
                status: (PickupPickMapAssignmentStatus)999,
                segmentStartSec: 1,
                segmentEndSec: 2));

        Assert.Contains("unknown status", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("999", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-0.1, 1.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 1.5)]
    public void Assignment_RejectsInvalidSegmentRanges(double sourceStartSec, double sourceEndSec)
    {
        var ex = Assert.ThrowsAny<Exception>(() =>
            CreateAssignment(
                id: "seg-bad-range",
                status: PickupPickMapAssignmentStatus.Deferred,
                segmentStartSec: sourceStartSec,
                segmentEndSec: sourceEndSec));

        Assert.Contains("seg-bad-range", ex.ToString(), StringComparison.Ordinal);
        Assert.Contains("range", ex.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Program_RegistersPickupPickMapStore()
    {
        var repoRoot = FindRepoRoot();
        var programPath = Path.Combine(repoRoot, "host", "Ams.Workstation.Server", "Program.cs");

        var source = File.ReadAllText(programPath);

        Assert.Contains("using Ams.Workstation.Server.Services.Pickups.Pick;", source, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddSingleton<PickupPickMapStore>();", source, StringComparison.Ordinal);
    }

    private static PickupPickMapDocument CreateDocument(
        PickupPickMapSourceReference source,
        IReadOnlyList<PickupPickMapAssignment> assignments,
        int revision = 0,
        bool isDraft = false)
        => new(
            schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
            revision: revision,
            source: source,
            assignments: assignments,
            createdAtUtc: FixedUtc,
            updatedAtUtc: FixedUtc,
            lastOperationId: "op-import-001",
            lastValidationError: null,
            isDraft: isDraft);

    private static PickupPickMapSourceReference CreateSource(
        string fingerprint = "batch-fp-001",
        string crxTargetsFingerprint = "crx-fp-001")
        => new(
            path: "/tmp/pickups.wav",
            fingerprint: fingerprint,
            fileSizeBytes: 1024,
            modifiedAtUtc: FixedUtc,
            crxTargetsFingerprint: crxTargetsFingerprint);

    private static PickupPickMapAssignment CreateAssignment(
        string id,
        PickupPickMapAssignmentStatus status,
        double segmentStartSec,
        double segmentEndSec,
        PickupPickMapTargetReference? inferredTarget = null,
        PickupPickMapTargetReference? selectedTarget = null,
        string? pickupSegmentId = null)
        => new(
            id: id,
            pickupSegmentId: pickupSegmentId ?? id,
            sourceStartSec: segmentStartSec,
            sourceEndSec: segmentEndSec,
            status: status,
            inferredTarget: inferredTarget,
            selectedTarget: selectedTarget,
            confidence: 0.98,
            note: null,
            validationError: null,
            updatedAtUtc: FixedUtc);

    private static PickupPickMapTargetReference CreateTarget(string chapterStem, int errorNumber)
        => new(
            chapterStem: chapterStem,
            chapterName: chapterStem.Replace('-', ' '),
            errorNumber: errorNumber,
            sentenceId: errorNumber * 10,
            originalStartSec: errorNumber,
            originalEndSec: errorNumber + 0.5);

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
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-pick-map-{Guid.NewGuid():N}");
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
