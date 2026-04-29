using System.IO;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Fit;

namespace Ams.Tests.Services;

public sealed class PickupFitPlanStoreTests
{
    private static readonly DateTime FixedUtc = new(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Save_ThenTryRead_RoundTripsChapterFitPlanDocument()
    {
        var root = CreateTempDirectory();
        try
        {
            var pickMap = CreatePickMap(
                assignments:
                [
                    CreateAssignment(
                        id: "assign-002",
                        pickupSegmentId: "pickup-segment-002",
                        segmentStartSec: 20,
                        segmentEndSec: 21,
                        target: CreateTarget("chapter-01", errorNumber: 2)),
                    CreateAssignment(
                        id: "assign-001",
                        pickupSegmentId: "pickup-segment-001",
                        segmentStartSec: 10,
                        segmentEndSec: 11,
                        target: CreateTarget("chapter-01", errorNumber: 1))
                ]);
            var store = new PickupFitPlanStore(() => root);
            var initial = PickupFitPlanDocument.CreateInitial("chapter-01", pickMap, "op-fit-create");

            var saved = store.Save("chapter-01", pickMap, initial);
            var loaded = store.TryRead("chapter-01");

            Assert.NotNull(loaded);
            Assert.Equal("pickup-fit-plan/v1", saved.SchemaVersion);
            Assert.Equal(1, saved.Revision);
            Assert.Equal("chapter-01", loaded!.ChapterStem);
            Assert.Equal(pickMap.Source.Fingerprint, loaded.Source.Fingerprint);
            Assert.Equal(pickMap.Revision, loaded.PickMapRevision);
            Assert.Equal(PickupFitPlanDocument.ComputePickAssignmentsFingerprint("chapter-01", pickMap), loaded.PickAssignmentsFingerprint);
            Assert.Equal(["assign-001", "assign-002"], loaded.Items.Select(item => item.PickAssignmentId));
            var first = loaded.Items[0];
            Assert.Equal("fit::assign-001", first.FitItemId);
            Assert.Equal("replacement::assign-001", first.ReplacementId);
            Assert.Equal("pickup-segment-001", first.PickupSegmentId);
            Assert.Equal(PickupFitPlanItemStatus.Draft, first.Status);
            Assert.Equal(10, first.InnerRange.StartSec);
            Assert.Equal(11, first.InnerRange.EndSec);
            Assert.Equal(first.Target.OriginalStartSec, first.OuterRange.StartSec);
            Assert.Equal(first.Target.OriginalEndSec, first.Placement.EndSec);
            Assert.True(File.Exists(Path.Combine(root, ".polish", "pickups", "fit", "chapter-01.fit-plan.json")));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Document_DeterministicOrder_SortsByTargetThenPlacementThenFitItemId()
    {
        var pickMap = CreatePickMap(
            assignments:
            [
                CreateAssignment("late", "segment-late", 30, 31, CreateTarget("chapter-01", errorNumber: 30)),
                CreateAssignment("early-b", "segment-early-b", 20, 21, CreateTarget("chapter-01", errorNumber: 10, sentenceId: 20)),
                CreateAssignment("early-a", "segment-early-a", 10, 11, CreateTarget("chapter-01", errorNumber: 10, sentenceId: 10))
            ]);
        var initial = PickupFitPlanDocument.CreateInitial("chapter-01", pickMap);
        var reversedItems = initial.Items.Reverse().ToArray();

        var document = CreateFitDocument(pickMap, reversedItems);

        Assert.Equal(["early-a", "early-b", "late"], document.Items.Select(item => item.PickAssignmentId));
        Assert.Equal(["early-a", "early-b", "late"], document.GetDeterministicItemOrder().Select(item => item.PickAssignmentId));
    }

    [Fact]
    public void Document_RejectsDuplicateFitItemIds()
    {
        var pickMap = CreatePickMap();
        var first = CreateFitItem(pickAssignmentId: "assign-001", fitItemId: "fit-duplicate");
        var duplicate = CreateFitItem(
            pickAssignmentId: "assign-002",
            pickupSegmentId: "pickup-segment-002",
            fitItemId: "fit-duplicate",
            replacementId: "replacement-002",
            target: CreateTarget("chapter-01", errorNumber: 2));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateFitDocument(pickMap, [first, duplicate]));

        Assert.Contains("duplicate fit item id", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fit-duplicate", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_RejectsDuplicatePickAssignmentIds()
    {
        var pickMap = CreatePickMap();
        var first = CreateFitItem(pickAssignmentId: "assign-duplicate", fitItemId: "fit-a");
        var duplicate = CreateFitItem(
            pickAssignmentId: "assign-duplicate",
            pickupSegmentId: "pickup-segment-002",
            fitItemId: "fit-b",
            replacementId: "replacement-b",
            target: CreateTarget("chapter-01", errorNumber: 2));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateFitDocument(pickMap, [first, duplicate]));

        Assert.Contains("duplicate pick assignment id", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("assign-duplicate", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-0.1, 1.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 1.5)]
    public void Range_RejectsInvalidDurations(double startSec, double endSec)
    {
        var ex = Assert.ThrowsAny<Exception>(() => new PickupFitPlanRange(startSec, endSec));

        Assert.Contains("range", ex.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Item_RejectsPlacementOutsideOuterRange()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateFitItem(
                outerRange: new PickupFitPlanRange(10, 12),
                placement: new PickupFitPlanRange(12.5, 13)));

        Assert.Contains("outside outer range", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_RejectsAcceptedItemWithoutPreviewEvidence()
    {
        var pickMap = CreatePickMap();
        var accepted = CreateFitItem(
            status: PickupFitPlanItemStatus.Accepted,
            previewEvidence: null,
            acceptance: new PickupFitAcceptanceState(
                isAccepted: true,
                acceptedPreviewVersion: 1,
                acceptedAtUtc: FixedUtc,
                acceptedBy: "operator"));

        var ex = Assert.Throws<InvalidOperationException>(() => CreateFitDocument(pickMap, [accepted]));

        Assert.Contains("accepted", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("preview evidence", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Document_RejectsCommitReadyItemWithStalePreviewEvidence()
    {
        var pickMap = CreatePickMap(revision: 7);
        var stalePreview = CreatePreviewEvidence(
            pickMapRevision: 6,
            pickAssignmentsFingerprint: "stale-assignment-fingerprint");
        var commitReady = CreateFitItem(
            status: PickupFitPlanItemStatus.CommitReady,
            previewEvidence: stalePreview,
            acceptance: new PickupFitAcceptanceState(
                isAccepted: true,
                acceptedPreviewVersion: stalePreview.PreviewVersion,
                acceptedAtUtc: FixedUtc,
                acceptedBy: "operator"),
            commit: new PickupFitCommitState(
                status: PickupFitCommitStatus.Ready,
                operationId: "op-ready-001",
                edlRevision: null,
                ledgerSequence: null,
                committedAtUtc: null,
                error: null));

        var ex = Assert.Throws<InvalidOperationException>(() => CreateFitDocument(pickMap, [commitReady]));

        Assert.Contains("stale preview evidence", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stale-assignment-fingerprint", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_AllowsEmptyItemsOnlyWhenDraftIsExplicit()
    {
        var pickMap = CreatePickMap();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateFitDocument(pickMap, [], isDraft: false));
        Assert.Contains("IsDraft", ex.Message, StringComparison.Ordinal);

        var draft = CreateFitDocument(pickMap, [], isDraft: true);
        Assert.True(draft.IsDraft);
        Assert.Empty(draft.Items);
    }

    [Fact]
    public void Save_StalePickAssignmentFingerprint_FailsClosedWithoutMutation()
    {
        var root = CreateTempDirectory();
        try
        {
            var originalPickMap = CreatePickMap(revision: 7);
            var stalePickMap = CreatePickMap(
                revision: 8,
                assignments:
                [
                    CreateAssignment(
                        id: "assign-001",
                        pickupSegmentId: "pickup-segment-001",
                        segmentStartSec: 10,
                        segmentEndSec: 12,
                        target: CreateTarget("chapter-01", errorNumber: 1))
                ]);
            var store = new PickupFitPlanStore(() => root);
            var original = PickupFitPlanDocument.CreateInitial("chapter-01", originalPickMap);
            _ = store.Save("chapter-01", originalPickMap, original);
            var before = File.ReadAllText(store.GetDocumentPath("chapter-01"));

            var ex = Assert.Throws<InvalidOperationException>(() => store.LoadOrCreate("chapter-01", stalePickMap));

            Assert.Contains("canonical Pick truth mismatch", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("chapter-01", ex.Message, StringComparison.Ordinal);
            Assert.Contains("requestedPickRevision='8'", ex.Message, StringComparison.Ordinal);
            Assert.Equal(before, File.ReadAllText(store.GetDocumentPath("chapter-01")));
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void Save_DocumentWithUnknownCanonicalAssignment_FailsClosed()
    {
        var root = CreateTempDirectory();
        try
        {
            var pickMap = CreatePickMap();
            var store = new PickupFitPlanStore(() => root);
            var badItem = CreateFitItem(pickAssignmentId: "missing-assignment");
            var badDocument = CreateFitDocument(pickMap, [badItem]);

            var ex = Assert.Throws<InvalidOperationException>(() => store.Save("chapter-01", pickMap, badDocument));

            Assert.Contains("unknown Pick assignment", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("missing-assignment", ex.Message, StringComparison.Ordinal);
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
            var store = new PickupFitPlanStore(() => root);
            var documentPath = store.GetDocumentPath("chapter-01");
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "{ malformed fit plan json");

            var ex = Assert.Throws<InvalidOperationException>(() => store.TryRead("chapter-01"));

            Assert.Contains("Malformed pickup fit-plan JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(documentPath, ex.Message, StringComparison.Ordinal);
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
    public void Save_CorruptingWriteFailures_RestorePreviousDocumentWithoutQuarantine()
    {
        var root = CreateTempDirectory();
        try
        {
            var pickMap = CreatePickMap();
            var stableStore = new PickupFitPlanStore(() => root);
            var stableDocument = PickupFitPlanDocument.CreateInitial("chapter-01", pickMap);
            _ = stableStore.Save("chapter-01", pickMap, stableDocument);
            var documentPath = stableStore.GetDocumentPath("chapter-01");
            var before = File.ReadAllText(documentPath);

            var attempts = 0;
            var failingStore = new PickupFitPlanStore(
                workspaceRootResolver: () => root,
                atomicWrite: (path, _, _) =>
                {
                    attempts++;
                    File.WriteAllText(path, $"{{ corrupted by failed fit-plan write attempt {attempts}");
                    throw new IOException($"simulated corrupting fit-plan write failure {attempts}");
                });
            var mutatedItem = CreateFitItem(status: PickupFitPlanItemStatus.Fitted);
            var mutatedDocument = CreateFitDocument(pickMap, [mutatedItem]);

            var ex = Assert.Throws<InvalidOperationException>(() => failingStore.Save("chapter-01", pickMap, mutatedDocument));
            var loaded = stableStore.TryRead("chapter-01");

            Assert.Equal(2, attempts);
            Assert.Contains("write failed after retry", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, File.ReadAllText(documentPath));
            Assert.NotNull(loaded);
            Assert.Equal("assign-001", Assert.Single(loaded!.Items).PickAssignmentId);
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
    public void Program_RegistersPickupFitPlanStoreAndDocumentPath()
    {
        var root = CreateTempDirectory();
        try
        {
            var store = new PickupFitPlanStore(() => root);
            Assert.Equal(
                Path.Combine(root, ".polish", "pickups", "fit", "chapter-01.fit-plan.json"),
                store.GetDocumentPath("chapter-01"));

            var repoRoot = FindRepoRoot();
            var programPath = Path.Combine(repoRoot, "host", "Ams.Workstation.Server", "Program.cs");
            var source = File.ReadAllText(programPath);

            Assert.Contains("using Ams.Workstation.Server.Services.Pickups.Fit;", source, StringComparison.Ordinal);
            Assert.Contains("builder.Services.AddSingleton<PickupFitPlanStore>();", source, StringComparison.Ordinal);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    private static PickupFitPlanDocument CreateFitDocument(
        PickupPickMapDocument pickMap,
        IReadOnlyList<PickupFitPlanItem>? items = null,
        bool isDraft = true,
        string chapterStem = "chapter-01",
        int? pickMapRevision = null,
        string? pickAssignmentsFingerprint = null)
    {
        var initial = PickupFitPlanDocument.CreateInitial(chapterStem, pickMap);
        return new PickupFitPlanDocument(
            schemaVersion: PickupFitPlanDocument.CurrentSchemaVersion,
            chapterStem: chapterStem,
            revision: 0,
            source: pickMap.Source,
            pickMapRevision: pickMapRevision ?? pickMap.Revision,
            pickAssignmentsFingerprint: pickAssignmentsFingerprint ?? PickupFitPlanDocument.ComputePickAssignmentsFingerprint(chapterStem, pickMap),
            items: items ?? initial.Items,
            createdAtUtc: FixedUtc,
            updatedAtUtc: FixedUtc,
            lastOperationId: "op-fit-001",
            lastValidationError: null,
            isDraft: isDraft);
    }

    private static PickupFitPlanItem CreateFitItem(
        string pickAssignmentId = "assign-001",
        string pickupSegmentId = "pickup-segment-001",
        string fitItemId = "fit::assign-001",
        string replacementId = "replacement::assign-001",
        PickupPickMapTargetReference? target = null,
        PickupFitPlanRange? outerRange = null,
        PickupFitPlanRange? innerRange = null,
        PickupFitPlanRange? placement = null,
        PickupFitPlanItemStatus status = PickupFitPlanItemStatus.Draft,
        PickupFitPreviewEvidence? previewEvidence = null,
        PickupFitAcceptanceState? acceptance = null,
        PickupFitCommitState? commit = null)
    {
        var resolvedTarget = target ?? CreateTarget("chapter-01", errorNumber: 1);
        var resolvedOuter = outerRange ?? new PickupFitPlanRange(resolvedTarget.OriginalStartSec, resolvedTarget.OriginalEndSec);
        return new PickupFitPlanItem(
            fitItemId: fitItemId,
            replacementId: replacementId,
            pickAssignmentId: pickAssignmentId,
            pickupSegmentId: pickupSegmentId,
            target: resolvedTarget,
            outerRange: resolvedOuter,
            innerRange: innerRange ?? new PickupFitPlanRange(10, 11),
            placement: placement ?? resolvedOuter,
            transitionPolicy: PickupFitTransitionPolicy.Default,
            status: status,
            previewEvidence: previewEvidence,
            acceptance: acceptance ?? PickupFitAcceptanceState.None,
            commit: commit ?? PickupFitCommitState.NotReady,
            validationError: null,
            commitError: null,
            updatedAtUtc: FixedUtc);
    }

    private static PickupFitPreviewEvidence CreatePreviewEvidence(
        int pickMapRevision,
        string pickAssignmentsFingerprint,
        int previewVersion = 1)
        => new(
            previewVersion: previewVersion,
            pickMapRevision: pickMapRevision,
            pickAssignmentsFingerprint: pickAssignmentsFingerprint,
            previewArtifactRef: ".polish/previews/chapter-01/preview-001.wav",
            renderedDurationSec: 2.5,
            generatedAtUtc: FixedUtc,
            previousFitItemId: null,
            nextFitItemId: null);

    private static PickupPickMapDocument CreatePickMap(
        int revision = 7,
        IReadOnlyList<PickupPickMapAssignment>? assignments = null,
        string sourceFingerprint = "batch-fp-001",
        string crxTargetsFingerprint = "crx-fp-001")
    {
        var source = new PickupPickMapSourceReference(
            path: "/tmp/pickups.wav",
            fingerprint: sourceFingerprint,
            fileSizeBytes: 1024,
            modifiedAtUtc: FixedUtc,
            crxTargetsFingerprint: crxTargetsFingerprint);

        return new PickupPickMapDocument(
            schemaVersion: PickupPickMapDocument.CurrentSchemaVersion,
            revision: revision,
            source: source,
            assignments: assignments ??
            [
                CreateAssignment(
                    id: "assign-001",
                    pickupSegmentId: "pickup-segment-001",
                    segmentStartSec: 10,
                    segmentEndSec: 11,
                    target: CreateTarget("chapter-01", errorNumber: 1))
            ],
            createdAtUtc: FixedUtc,
            updatedAtUtc: FixedUtc,
            lastOperationId: "op-pick-001",
            lastValidationError: null,
            isDraft: false);
    }

    private static PickupPickMapAssignment CreateAssignment(
        string id,
        string pickupSegmentId,
        double segmentStartSec,
        double segmentEndSec,
        PickupPickMapTargetReference target)
        => new(
            id: id,
            pickupSegmentId: pickupSegmentId,
            sourceStartSec: segmentStartSec,
            sourceEndSec: segmentEndSec,
            status: PickupPickMapAssignmentStatus.Confirmed,
            inferredTarget: target,
            selectedTarget: target,
            confidence: 0.99,
            note: null,
            validationError: null,
            updatedAtUtc: FixedUtc);

    private static PickupPickMapTargetReference CreateTarget(
        string chapterStem,
        int errorNumber,
        int? sentenceId = null)
        => new(
            chapterStem: chapterStem,
            chapterName: chapterStem.Replace('-', ' '),
            errorNumber: errorNumber,
            sentenceId: sentenceId ?? errorNumber * 10,
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
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-fit-plan-{Guid.NewGuid():N}");
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
