using Ams.Core.Application.Commands;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;
using Ams.Workstation.Server.Services.Pickups.Edl;
using Ams.Workstation.Server.Services.Pickups.Pick;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofPickupsBatchPickServiceTests
{
    [Fact]
    public async Task ImportPickMapAsync_MultiChapterCandidates_PersistsDraftGroupedByChapterWithoutMutatingQueues()
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-02", errorNumber: 202, sentenceId: 21));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var ordered = targets.OrderBy(target => target.ErrorNumber).ToArray();
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[
                    harness.CreateAsset("seg-101", sourcePath, ordered[0], 0.00, 0.50),
                    harness.CreateAsset("seg-202", sourcePath, ordered[1], 0.70, 1.10)
                ],
                Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        };

        var snapshot = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, snapshot.Phase);
        Assert.Null(snapshot.LastError);
        Assert.NotNull(snapshot.LastPickOperationId);
        Assert.NotNull(snapshot.PickMap);
        Assert.True(snapshot.PickMap!.IsDraft);
        Assert.Equal(1, snapshot.PickMapRevision);
        Assert.Equal(harness.PickupPath, snapshot.PickMap.Source.Path, ignoreCase: true);
        Assert.Equal(2, snapshot.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Inferred]);
        Assert.Equal(1, snapshot.PickAssignmentCountsByChapter["chapter-01"]);
        Assert.Equal(1, snapshot.PickAssignmentCountsByChapter["chapter-02"]);
        Assert.Empty(snapshot.Staged);
        Assert.Empty(snapshot.Applied);
        Assert.Equal(0, harness.StageCallCount);
        Assert.Equal(0, harness.CommitCallCount);
    }

    [Fact]
    public async Task SetPickAssignmentTargetAsync_UnmatchedSegment_CreatesOverrideThenConfirmSavesCanonical()
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-02", errorNumber: 202, sentenceId: 21));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var first = targets.Single(target => target.ErrorNumber == 101);
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, first, 0.00, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-extra", sourcePath, 0.55, 0.95)]));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        Assert.Contains("missing 1 target", imported.LastPickValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, imported.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Inferred]);
        Assert.Equal(1, imported.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Unresolved]);

        var overridden = await harness.Service.SetPickAssignmentTargetAsync(
            assignmentId: "seg-extra",
            expectedRevision: imported.PickMapRevision!.Value,
            chapterStem: "chapter-02",
            errorNumber: 202,
            note: "manual chapter two assignment",
            ct: CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, overridden.Phase);
        Assert.Null(overridden.LastError);
        Assert.Equal(1, overridden.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Inferred]);
        Assert.Equal(1, overridden.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Override]);
        Assert.Equal(2, overridden.PickAssignmentCountsByChapter.Count);

        var confirmed = await harness.Service.ConfirmPickMapAsync(
            expectedRevision: overridden.PickMapRevision!.Value,
            ct: CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, confirmed.Phase);
        Assert.Null(confirmed.LastError);
        Assert.NotNull(confirmed.PickMap);
        Assert.False(confirmed.PickMap!.IsDraft);
        Assert.Equal(1, confirmed.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Confirmed]);
        Assert.Equal(1, confirmed.PickAssignmentCountsByStatus[PickupPickMapAssignmentStatus.Override]);
        Assert.Null(confirmed.LastPickValidationError);
    }

    [Fact]
    public async Task SetPickAssignmentDispositionAsync_WhenTargetStillMissing_PersistsValidationErrorInDraftMap()
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01", "chapter-02"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-02", errorNumber: 202, sentenceId: 21));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) =>
        {
            var first = targets.Single(target => target.ErrorNumber == 101);
            return Task.FromResult((
                Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, first, 0.00, 0.40)],
                Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-extra", sourcePath, 0.55, 0.95)]));
        };

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);

        var updated = await harness.Service.SetPickAssignmentDispositionAsync(
            assignmentId: "seg-extra",
            expectedRevision: imported.PickMapRevision!.Value,
            disposition: PickupPickMapAssignmentStatus.Rejected,
            note: "not part of this batch",
            ct: CancellationToken.None);
        var reloaded = harness.Service.SyncToWorkspace(CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, updated.Phase);
        Assert.Contains("missing 1 target", updated.LastPickValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("chapter-02#202", updated.PickMap!.LastValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(updated.PickMap!.LastValidationError, reloaded.LastPickValidationError);
        Assert.Equal(PickupPickMapAssignmentStatus.Rejected, updated.PickMap.Assignments.Single(item => item.Id == "seg-extra").Status);
    }

    [Theory]
    [InlineData(PickupPickMapAssignmentStatus.Rejected)]
    [InlineData(PickupPickMapAssignmentStatus.Deferred)]
    [InlineData(PickupPickMapAssignmentStatus.NotApplicable)]
    public async Task SetPickAssignmentDispositionAsync_UnmatchedSegment_AllowsExplicitTerminalDisposition(
        PickupPickMapAssignmentStatus disposition)
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, _, _) => Task.FromResult((
            Matched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(),
            Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-extra", sourcePath, 0.10, 0.40)]));

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);

        var updated = await harness.Service.SetPickAssignmentDispositionAsync(
            assignmentId: "seg-extra",
            expectedRevision: imported.PickMapRevision!.Value,
            disposition: disposition,
            note: "operator disposition",
            ct: CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Completed, updated.Phase);
        Assert.Null(updated.LastError);
        Assert.Equal(1, updated.PickAssignmentCountsByStatus[disposition]);
        var assignment = Assert.Single(updated.PickMap!.Assignments);
        Assert.Equal(disposition, assignment.Status);
        Assert.Null(assignment.SelectedTarget);
        Assert.Equal("operator disposition", assignment.Note);
    }

    [Fact]
    public async Task SetPickAssignmentTargetAsync_StaleRevisionFailsClosedAndPreservesPriorMap()
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, _, _) => Task.FromResult((
            Matched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(),
            Unmatched: (IReadOnlyList<PickupAsset>)[harness.CreateUnmatchedAsset("seg-extra", sourcePath, 0.10, 0.40)]));

        var imported = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        var baselineRevision = imported.PickMapRevision!.Value;

        var failed = await harness.Service.SetPickAssignmentTargetAsync(
            assignmentId: "seg-extra",
            expectedRevision: baselineRevision - 1,
            chapterStem: "chapter-01",
            errorNumber: 101,
            note: null,
            ct: CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, failed.Phase);
        Assert.Contains("stale Pick map revision", failed.LastPickValidationError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stale Pick map revision", failed.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(baselineRevision, failed.PickMapRevision);
        Assert.Equal(PickupPickMapAssignmentStatus.Unresolved, Assert.Single(failed.PickMap!.Assignments).Status);
    }

    [Fact]
    public async Task ImportPickMapAsync_DuplicateCrxRowsFailBeforeOverwritingPriorMap()
    {
        using var harness = await BatchPickHarness.CreateAsync(["chapter-01"]);
        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));
        harness.ImportPickAssetsBehavior = (sourcePath, targets, _) => Task.FromResult((
            Matched: (IReadOnlyList<PickupAsset>)[harness.CreateAsset("seg-101", sourcePath, targets.Single(), 0.00, 0.30)],
            Unmatched: (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));

        var baseline = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);
        Assert.NotNull(baseline.PickMap);

        harness.CrxEntries.Add(harness.CreateCrxEntry("chapter-01", errorNumber: 101, sentenceId: 11));

        var failed = await harness.Service.ImportPickMapAsync(harness.PickupPath, CancellationToken.None);

        Assert.Equal(ProofPickupsSessionPhase.Failed, failed.Phase);
        Assert.Contains("duplicate CRX error number", failed.LastError ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(baseline.PickMapRevision, failed.PickMapRevision);
        Assert.Equal(baseline.PickMap!.Assignments.Select(item => item.Id), failed.PickMap!.Assignments.Select(item => item.Id));
    }

    private sealed class BatchPickHarness : IDisposable
    {
        private readonly string _root;
        private readonly Dictionary<string, List<StagedReplacement>> _queues = new(StringComparer.OrdinalIgnoreCase);

        private BatchPickHarness(string root, BlazorWorkspace workspace, PickupPickMapStore pickMapStore, ProofPickupsSessionService service)
        {
            _root = root;
            Workspace = workspace;
            PickMapStore = pickMapStore;
            Service = service;
            PickupPath = Path.Combine(root, ".pickups", "session.wav");
            ImportPickAssetsBehavior = static (_, _, _) => Task.FromResult(((IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>(), (IReadOnlyList<PickupAsset>)Array.Empty<PickupAsset>()));
        }

        public BlazorWorkspace Workspace { get; }

        public PickupPickMapStore PickMapStore { get; }

        public ProofPickupsSessionService Service { get; }

        public string PickupPath { get; }

        public List<CrxEntry> CrxEntries { get; } = [];

        public int StageCallCount { get; private set; }

        public int CommitCallCount { get; private set; }

        public Func<string, IReadOnlyList<CrxPickupTarget>, CancellationToken, Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)>> ImportPickAssetsBehavior { get; set; }

        public static async Task<BatchPickHarness> CreateAsync(IReadOnlyList<string> chapterStems)
        {
            var root = Path.Combine(Path.GetTempPath(), $"ams-proof-pickups-batch-pick-{Guid.NewGuid():N}");
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
            BatchPickHarness? harness = null;
            harness = new BatchPickHarness(
                root,
                workspace,
                pickMapStore,
                new ProofPickupsSessionService(
                    workspace,
                    new ProofPickupsSessionService.RuntimeHooks(
                        GetCrxEntries: () => harness!.CrxEntries,
                        ImportAssetsAsync: (sourcePath, targets, ct) => harness!.ImportPickAssetsBehavior(sourcePath, targets, ct),
                        StageReplacement: (chapterStem, match, sourcePath, originalStartSec, originalEndSec) =>
                        {
                            harness!.StageCallCount++;
                            throw new InvalidOperationException("StageReplacement must not be called during Pick tests.");
                        },
                        UnstageReplacement: static _ => false,
                        RestageReplacement: static _ => (false, "not configured"),
                        GetQueue: chapterStem => harness!.GetQueue(chapterStem),
                        CommitReplacementAsync: (replacementId, _) =>
                        {
                            harness!.CommitCallCount++;
                            return Task.FromResult((HasResult: true, TimingDeltaSec: 0.0));
                        },
                        RevertReplacementAsync: static (_, _) => Task.FromResult((HasResult: true, TimingDeltaSec: 0.0)),
                        ReadEdlDocument: static (_, _) => null,
                        ReadArtifactLedgerDocument: static (_, _) => null,
                        MutateEdlDocument: static (_, _, _, _) => throw new InvalidOperationException("EDL mutation must not be called during Pick tests."),
                        TryGetOperation: static (_, _) => null,
                        TransitionOperationState: static (document, _, _) => document,
                        BuildOrderingDiagnostics: static _ => string.Empty,
                        ImportPickAssetsAsync: (sourcePath, targets, ct) => harness!.ImportPickAssetsBehavior(sourcePath, targets, ct),
                        ReadPickMapDocument: ct => pickMapStore.TryRead(ct),
                        LoadOrCreatePickMap: (source, ct) => pickMapStore.LoadOrCreate(source, ct),
                        SavePickMap: (source, document, ct) => pickMapStore.Save(source, document, ct))));

            return harness;
        }

        public CrxEntry CreateCrxEntry(string chapterName, int errorNumber, int sentenceId)
            => new(
                ErrorNumber: errorNumber,
                Chapter: chapterName,
                Timecode: "00:00:01",
                ErrorType: "MR",
                Comments: string.Empty,
                SentenceId: sentenceId,
                StartTime: 0.10,
                EndTime: 0.50,
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
                new(BookIdx: 0, AsrIdx: 0, BookWord: "book", AsrWord: "script", Op: "sub", Reason: "pick-test", Score: 0)
                {
                    StartSec = 0.10,
                    EndSec = 0.50,
                    DurationSec = 0.40
                },
                new(BookIdx: 1, AsrIdx: 1, BookWord: "book", AsrWord: "script", Op: "sub", Reason: "pick-test", Score: 0)
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
                NormalizationVersion: "pick-test",
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
