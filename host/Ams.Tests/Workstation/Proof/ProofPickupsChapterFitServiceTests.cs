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

    private sealed class ChapterFitHarness : IDisposable
    {
        private readonly string _root;
        private readonly Dictionary<string, List<StagedReplacement>> _queues = new(StringComparer.OrdinalIgnoreCase);

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
            Service = CreateSessionService();
        }

        public BlazorWorkspace Workspace { get; }

        public PickupPickMapStore PickMapStore { get; }

        public PickupFitPlanStore FitPlanStore { get; }

        public ProofPickupsSessionService Service { get; }

        public string PickupPath { get; }

        public List<CrxEntry> CrxEntries { get; } = [];

        public int StageCallCount { get; private set; }

        public Func<string, IReadOnlyList<CrxPickupTarget>, CancellationToken, Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)>> ImportPickAssetsBehavior { get; set; }

        public Func<string, CancellationToken, PickupFitPlanDocument?> ReadFitPlanBehavior { get; set; }

        public Func<string, PickupPickMapDocument, CancellationToken, PickupFitPlanDocument> LoadOrCreateFitPlanBehavior { get; set; }

        public Func<string, PickupPickMapDocument, PickupFitPlanDocument, CancellationToken, PickupFitPlanDocument> SaveFitPlanBehavior { get; set; }

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
                    ReadArtifactLedgerDocument: static (_, _) => null,
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
                    SaveFitPlan: (chapterStem, pickMap, document, ct) => SaveFitPlanBehavior(chapterStem, pickMap, document, ct)));

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
                StartTime: sentenceId == 21 ? 0.70 : 0.10,
                EndTime: sentenceId == 21 ? 1.10 : 0.50,
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
                }
            };

            var sentences = new List<HydratedSentence>
            {
                CreateSentence(11, 0, 1, $"{chapterStem} sentence 11", 0.10, 0.50),
                CreateSentence(21, 1, 2, $"{chapterStem} sentence 21", 0.70, 1.10)
            };

            var paragraphs = new List<HydratedParagraph>
            {
                new(
                    Id: 1,
                    BookRange: new HydratedRange(0, 2),
                    SentenceIds: [11, 21],
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
