using System.Reflection;
using Ams.Core.Application.Commands;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Documents;
using Ams.Workstation.Server.Components.Pages.Proof;
using Ams.Workstation.Server.Components.Shared;
using Ams.Workstation.Server.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofGestureSelectionContractTests
{

    [Fact]
    public async Task LongPress_EntersSelection_AndSecondLongPressExitsSelectionBeforeFurtherIgnoreDispatch()
    {
        using var harness = await GestureHarness.CreateAsync();

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeLeft(harness.PrimarySentenceId, "playback"));

        // Must wait past duplicate dispatch guard to exercise real second long-press exit behavior.
        await Task.Delay(350);
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));

        InvokeComponentStateTransition(() => harness.Component.OnSelectionSentenceTap(harness.SecondarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeLeft(harness.SecondarySentenceId, "playback"));

        Assert.True(
            harness.IgnoredPatternsService.IsIgnored(harness.PrimaryPatternKey),
            "Expected playback swipe-left to ignore selected sentence while long-press selection mode is active.");

        Assert.False(
            harness.IgnoredPatternsService.IsIgnored(harness.SecondaryPatternKey),
            "Expected second long-press to clear selection mode so later tap+swipe cannot ignore unselected sentence.");
    }

    [Fact]
    public async Task SwipeLeft_FromPlaybackSurface_DispatchesIgnorePathForSelectedSentence()
    {
        using var harness = await GestureHarness.CreateAsync();

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeLeft(harness.PrimarySentenceId, "playback"));

        Assert.True(
            harness.IgnoredPatternsService.IsIgnored(harness.PrimaryPatternKey),
            "Expected playback-surface swipe-left to route through batch ignore and persist ignored pattern state.");
    }

    [Fact]
    public async Task SwipeLeft_IgnoresGestureFromSentenceOutsideSelection()
    {
        using var harness = await GestureHarness.CreateAsync();

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeLeft(harness.SecondarySentenceId, "playback"));

        Assert.False(
            harness.IgnoredPatternsService.IsIgnored(harness.PrimaryPatternKey),
            "Expected swipe-left to no-op when gesture sentence is outside active selection context.");

        Assert.False(
            harness.IgnoredPatternsService.IsIgnored(harness.SecondaryPatternKey),
            "Expected swipe-left to no-op for unselected gesture-origin sentence.");
    }

    [Fact]
    public async Task SwipeRight_FromPlaybackSurface_UsesExportDispatchWithoutCreatingNewPersistenceArtifacts()
    {
        using var harness = await GestureHarness.CreateAsync();

        var baselineWorkspaceFiles = SnapshotFiles(harness.RootPath);

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeRight(harness.PrimarySentenceId, "playback"));

        Assert.True(
            harness.CrxModalProbe.IsVisible,
            "Expected swipe-right gesture path to dispatch through CRX modal export flow.");

        Assert.Equal(harness.PrimarySentenceId, harness.CrxModalProbe.SentenceId);
        Assert.Equal(harness.ChapterName, harness.CrxModalProbe.ChapterName);
        Assert.Equal(0.0, harness.CrxModalProbe.StartTime, precision: 3);
        Assert.Equal(0.8, harness.CrxModalProbe.EndTime, precision: 3);

        Assert.False(
            harness.IgnoredPatternsService.IsIgnored(harness.PrimaryPatternKey),
            "Expected swipe-right export path to avoid mutating ignore persistence state.");

        Assert.Empty(harness.CrxService.GetEntries());
        Assert.Empty(harness.ReviewedStatusService.GetAll());
        Assert.False(Directory.Exists(Path.Combine(harness.RootPath, "CRX")));

        var workspaceFilesAfterGestures = SnapshotFiles(harness.RootPath);
        Assert.Equal(baselineWorkspaceFiles, workspaceFilesAfterGestures);
    }

    [Fact]
    public async Task SwipeRight_MultiSelection_ComposesMergedRangeAndBatchContext()
    {
        using var harness = await GestureHarness.CreateAsync();

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        InvokeComponentStateTransition(() => harness.Component.OnSelectionSentenceTap(harness.SecondarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeRight(harness.PrimarySentenceId, "playback"));

        Assert.True(harness.CrxModalProbe.IsVisible);
        Assert.Equal(harness.PrimarySentenceId, harness.CrxModalProbe.SentenceId);
        Assert.Equal(0.0, harness.CrxModalProbe.StartTime, precision: 3);
        Assert.Equal(1.6, harness.CrxModalProbe.EndTime, precision: 3);
        Assert.False(harness.CrxModalProbe.RequiresRangeConfirmation);
        Assert.Contains("Batch export sentences", harness.CrxModalProbe.Excerpt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SwipeRight_InvalidMergedRange_SeedsFallbackWindowAndRequiresExplicitConfirmation()
    {
        using var harness = await GestureHarness.CreateAsync(
            primaryStartTime: 0.4,
            primaryEndTime: 0.4,
            secondaryStartTime: 0.4,
            secondaryEndTime: 0.4);

        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSentenceLongPress(harness.PrimarySentenceId));
        InvokeComponentStateTransition(() => harness.Component.OnSelectionSentenceTap(harness.SecondarySentenceId));
        await InvokeComponentStateTransitionAsync(() => harness.Component.OnSelectionSwipeRight(harness.PrimarySentenceId, "playback"));

        Assert.True(harness.CrxModalProbe.IsVisible);
        Assert.True(harness.CrxModalProbe.RequiresRangeConfirmation);
        Assert.Equal(0.15, harness.CrxModalProbe.StartTime, precision: 2);
        Assert.Equal(1.15, harness.CrxModalProbe.EndTime, precision: 2);
        Assert.Contains("Batch export sentences", harness.CrxModalProbe.Excerpt, StringComparison.Ordinal);
    }

    private static async Task InvokeComponentStateTransitionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("render handle is not yet assigned", StringComparison.OrdinalIgnoreCase))
        {
            // These tests intentionally exercise component behavior without bootstrapping a renderer.
            // State transitions and service side effects complete before this framework lifecycle guard throws.
        }
    }

    private static void InvokeComponentStateTransition(Action action)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("render handle is not yet assigned", StringComparison.OrdinalIgnoreCase))
        {
            // Same lifecycle guard as async helper.
        }
    }

    private static HashSet<string> SnapshotFiles(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return Directory
            .EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(rootPath, path))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class GestureHarness : IDisposable
    {
        private GestureHarness(
            string rootPath,
            string chapterName,
            int primarySentenceId,
            int secondarySentenceId,
            string primaryPatternKey,
            string secondaryPatternKey,
            BlazorWorkspace workspace,
            ReviewedStatusService reviewedStatusService,
            IgnoredPatternsService ignoredPatternsService,
            CrxService crxService,
            TestableChapterReview component,
            CrxModalProbe crxModalProbe)
        {
            RootPath = rootPath;
            ChapterName = chapterName;
            PrimarySentenceId = primarySentenceId;
            SecondarySentenceId = secondarySentenceId;
            PrimaryPatternKey = primaryPatternKey;
            SecondaryPatternKey = secondaryPatternKey;
            Workspace = workspace;
            ReviewedStatusService = reviewedStatusService;
            IgnoredPatternsService = ignoredPatternsService;
            CrxService = crxService;
            Component = component;
            CrxModalProbe = crxModalProbe;

        }

        public string RootPath { get; }

        public string ChapterName { get; }

        public int PrimarySentenceId { get; }

        public int SecondarySentenceId { get; }

        public string PrimaryPatternKey { get; }

        public string SecondaryPatternKey { get; }

        public BlazorWorkspace Workspace { get; }

        public ReviewedStatusService ReviewedStatusService { get; }

        public IgnoredPatternsService IgnoredPatternsService { get; }

        public CrxService CrxService { get; }

        public TestableChapterReview Component { get; }

        public CrxModalProbe CrxModalProbe { get; }

        public static async Task<GestureHarness> CreateAsync(
            double primaryStartTime = 0.0,
            double primaryEndTime = 0.8,
            double secondaryStartTime = 0.8,
            double secondaryEndTime = 1.6)
        {
            var root = Path.Combine(Path.GetTempPath(), $"ams-proof-gesture-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);

            var chapterStem = "chapter-01";
            var sentencePrefix = Path.GetFileName(root);
            WriteWavStub(Path.Combine(root, $"{chapterStem}.wav"));

            var bookPath = Path.Combine(root, "book.md");
            await File.WriteAllTextAsync(bookPath, "# Gesture Contract Test Book\n\n## chapter-01\n\nFirst sentence. Second sentence.");

            var bookIndexPath = Path.Combine(root, "book-index.json");
            await CreateBookIndexAsync(new FileInfo(bookPath), new FileInfo(bookIndexPath));

            var workspace = new BlazorWorkspace(Path.Combine(root, ".workstation-state.json"), loadPersistedState: false);
            Assert.True(workspace.SetWorkingDirectory(root));
            workspace.SetPrecomputePeaksInBackground(false);

            var chapterName = Assert.Single(workspace.AvailableChapters);
            Assert.True(workspace.SelectChapter(chapterName));

            var primarySentenceId = 101;
            var secondarySentenceId = 202;
            var primaryBookToken = $"book-{sentencePrefix}-primary";
            var primaryScriptToken = $"script-{sentencePrefix}-primary";
            var secondaryBookToken = $"book-{sentencePrefix}-secondary";
            var secondaryScriptToken = $"script-{sentencePrefix}-secondary";

            var primaryPatternKey = ErrorPatternService.BuildKey("sub", primaryBookToken, primaryScriptToken);
            var secondaryPatternKey = ErrorPatternService.BuildKey("sub", secondaryBookToken, secondaryScriptToken);

            var hydrate = CreateHydratedTranscript(
                root,
                chapterStem,
                sentencePrefix,
                primarySentenceId,
                secondarySentenceId,
                primaryBookToken,
                primaryScriptToken,
                secondaryBookToken,
                secondaryScriptToken,
                primaryStartTime,
                primaryEndTime,
                secondaryStartTime,
                secondaryEndTime);

            WorkspaceSeedHydratedTranscript(workspace, hydrate);

            var chapterDataService = new ChapterDataService(workspace);
            var proofReportService = new ProofReportService();
            var errorPatternService = new ErrorPatternService(workspace);
            var reviewedStatusBasePath = Path.Combine(root, ".test-appdata", "workstation");
            var reviewedStatusService = new ReviewedStatusService(workspace, reviewedStatusBasePath);
            var ignoredPatternsService = new IgnoredPatternsService(workspace);
            var toastService = new ToastService();
            var audioExportService = new AudioExportService(workspace);
            var crxService = new CrxService(workspace, audioExportService);

            var component = new TestableChapterReview();
            component.ConfigureForTest(
                workspace,
                chapterDataService,
                proofReportService,
                errorPatternService,
                reviewedStatusService,
                audioExportService,
                ignoredPatternsService,
                toastService,
                crxService,
                new TestNavigationManager(),
                new NoopJsRuntime(),
                Uri.EscapeDataString(chapterName));

            var crxModalProbe = new CrxModalProbe();
            component.SetCrxModalForTest(crxModalProbe);

            await component.RunOnParametersSetAsyncForTest();

            // Ensure deterministic clean start for this book scope.
            ignoredPatternsService.ResetCurrentBook();
            reviewedStatusService.ResetCurrentBook();

            return new GestureHarness(
                root,
                chapterName,
                primarySentenceId,
                secondarySentenceId,
                primaryPatternKey,
                secondaryPatternKey,
                workspace,
                reviewedStatusService,
                ignoredPatternsService,
                crxService,
                component,
                crxModalProbe);
        }

        public void Dispose()
        {
            try
            {
                IgnoredPatternsService.SetIgnored(PrimaryPatternKey, ignored: false);
                IgnoredPatternsService.SetIgnored(SecondaryPatternKey, ignored: false);
                ReviewedStatusService.ResetCurrentBook();
            }
            catch
            {
                // Best-effort shared appdata cleanup only.
            }

            Workspace.Dispose();

            try
            {
                if (Directory.Exists(RootPath))
                {
                    Directory.Delete(RootPath, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp directory cleanup.
            }
        }

        private static async Task CreateBookIndexAsync(FileInfo bookFile, FileInfo outputFile)
        {
            var documentService = new DocumentService(pronunciationProvider: null, cache: new NullBookCache());
            var command = new BuildBookIndexCommand(documentService);
            await command.ExecuteAsync(new BuildBookIndexRequest(bookFile, outputFile));
        }

        private static void WorkspaceSeedHydratedTranscript(BlazorWorkspace workspace, HydratedTranscript hydrate)
        {
            var handle = workspace.CurrentChapterHandle;
            Assert.NotNull(handle);

            handle!.Chapter.Documents.HydratedTranscript = hydrate;
        }

        private static HydratedTranscript CreateHydratedTranscript(
            string root,
            string chapterStem,
            string sentencePrefix,
            int primarySentenceId,
            int secondarySentenceId,
            string primaryBookToken,
            string primaryScriptToken,
            string secondaryBookToken,
            string secondaryScriptToken,
            double primaryStartTime,
            double primaryEndTime,
            double secondaryStartTime,
            double secondaryEndTime)
        {
            static HydratedDiff BuildSubDiff(string bookToken, string scriptToken)
                => new(
                    [
                        new HydratedDiffOp("delete", [bookToken]),
                        new HydratedDiffOp("insert", [scriptToken])
                    ],
                    new HydratedDiffStats(ReferenceTokens: 1, HypothesisTokens: 1, Matches: 0, Insertions: 1, Deletions: 1));

            var words = new List<HydratedWord>
            {
                new(BookIdx: 0, AsrIdx: 0, BookWord: primaryBookToken, AsrWord: primaryScriptToken, Op: "sub", Reason: "gesture-test", Score: 0)
                {
                    StartSec = primaryStartTime,
                    EndSec = primaryEndTime,
                    DurationSec = Math.Max(0, primaryEndTime - primaryStartTime)
                },
                new(BookIdx: 1, AsrIdx: 1, BookWord: secondaryBookToken, AsrWord: secondaryScriptToken, Op: "sub", Reason: "gesture-test", Score: 0)
                {
                    StartSec = secondaryStartTime,
                    EndSec = secondaryEndTime,
                    DurationSec = Math.Max(0, secondaryEndTime - secondaryStartTime)
                }
            };

            var sentences = new List<HydratedSentence>
            {
                new(
                    Id: primarySentenceId,
                    BookRange: new HydratedRange(0, 1),
                    ScriptRange: new HydratedScriptRange(0, 1),
                    BookText: $"{sentencePrefix} primary sentence",
                    ScriptText: $"{sentencePrefix} primary script",
                    Metrics: new SentenceMetrics(Wer: 1.0, Cer: 1.0, SpanWer: 1.0, MissingRuns: 1, ExtraRuns: 1),
                    Status: "error",
                    Timing: new TimingRange(primaryStartTime, primaryEndTime),
                    Diff: BuildSubDiff(primaryBookToken, primaryScriptToken)),
                new(
                    Id: secondarySentenceId,
                    BookRange: new HydratedRange(1, 2),
                    ScriptRange: new HydratedScriptRange(1, 2),
                    BookText: $"{sentencePrefix} secondary sentence",
                    ScriptText: $"{sentencePrefix} secondary script",
                    Metrics: new SentenceMetrics(Wer: 1.0, Cer: 1.0, SpanWer: 1.0, MissingRuns: 1, ExtraRuns: 1),
                    Status: "error",
                    Timing: new TimingRange(secondaryStartTime, secondaryEndTime),
                    Diff: BuildSubDiff(secondaryBookToken, secondaryScriptToken))
            };

            var paragraphs = new List<HydratedParagraph>
            {
                new(
                    Id: 1,
                    BookRange: new HydratedRange(0, 2),
                    SentenceIds: [primarySentenceId, secondarySentenceId],
                    BookText: "Gesture test paragraph",
                    Metrics: new ParagraphMetrics(Wer: 1.0, Cer: 1.0, Coverage: 0.0),
                    Status: "error",
                    Diff: null)
            };

            return new HydratedTranscript(
                AudioPath: Path.Combine(root, $"{chapterStem}.wav"),
                ScriptPath: Path.Combine(root, "book.md"),
                BookIndexPath: Path.Combine(root, "book-index.json"),
                CreatedAtUtc: DateTime.UtcNow,
                NormalizationVersion: "gesture-test",
                Words: words,
                Sentences: sentences,
                Paragraphs: paragraphs);
        }

        private static void WriteWavStub(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, [0x52, 0x49, 0x46, 0x46]);
        }

        private sealed class NullBookCache : IBookCache
        {
            public Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default)
            {
                _ = sourceFile;
                _ = cancellationToken;
                return Task.FromResult<BookIndex?>(null);
            }

            public Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
            {
                _ = bookIndex;
                _ = cancellationToken;
                return Task.FromResult(true);
            }

            public Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default)
            {
                _ = sourceFile;
                _ = cancellationToken;
                return Task.FromResult(true);
            }

            public Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default)
            {
                _ = bookIndex;
                _ = cancellationToken;
                return Task.FromResult(true);
            }

            public Task ClearAsync(CancellationToken cancellationToken = default)
            {
                _ = cancellationToken;
                return Task.CompletedTask;
            }
        }
    }

    private sealed class TestableChapterReview : ChapterReview
    {
        public void ConfigureForTest(
            BlazorWorkspace workspace,
            ChapterDataService chapterDataService,
            ProofReportService proofReportService,
            ErrorPatternService errorPatternService,
            ReviewedStatusService reviewedStatusService,
            AudioExportService audioExportService,
            IgnoredPatternsService ignoredPatternsService,
            ToastService toastService,
            CrxService crxService,
            NavigationManager navigation,
            IJSRuntime jsRuntime,
            string chapterName)
        {
            SetMember(this, "Workspace", workspace);
            SetMember(this, "ChapterDataService", chapterDataService);
            SetMember(this, "ProofReportService", proofReportService);
            SetMember(this, "ErrorPatternService", errorPatternService);
            SetMember(this, "ReviewedStatusService", reviewedStatusService);
            SetMember(this, "AudioExportService", audioExportService);
            SetMember(this, "IgnoredPatternsService", ignoredPatternsService);
            SetMember(this, "ToastService", toastService);
            SetMember(this, "CrxService", crxService);
            SetMember(this, "Navigation", navigation);
            SetMember(this, "JS", jsRuntime);
            SetMember(this, "ChapterName", chapterName);
        }

        public void SetCrxModalForTest(CrxModal modal)
            => SetMember(this, "_crxModal", modal);

        private static void SetMember(object target, string memberName, object? value)
        {
            const BindingFlags SearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var currentType = target.GetType(); currentType is not null; currentType = currentType.BaseType)
            {
                var property = currentType.GetProperty(memberName, SearchFlags);
                if (property is not null)
                {
                    property.SetValue(target, value);
                    return;
                }

                var field = currentType.GetField(memberName, SearchFlags);
                if (field is not null)
                {
                    field.SetValue(target, value);
                    return;
                }
            }

            throw new Xunit.Sdk.XunitException(
                $"Unable to configure test dependency member '{memberName}' on {target.GetType().FullName}.");
        }

        public Task RunOnParametersSetAsyncForTest() => base.OnParametersSetAsync();
    }

    private sealed class CrxModalProbe : CrxModal
    {
        public bool IsVisible => ReadPrivateField<bool>("_isVisible");

        public string ChapterName => ReadPrivateField<string>("_chapterName");

        public double StartTime => ReadPrivateField<double>("_startTime");

        public double EndTime => ReadPrivateField<double>("_endTime");

        public int SentenceId => ReadPrivateField<int>("_sentenceId");

        public string Excerpt => ReadPrivateField<string>("_excerpt");

        public bool RequiresRangeConfirmation => ReadPrivateField<bool>("_requiresRangeConfirmation");

        private T ReadPrivateField<T>(string fieldName)
        {
            var field = typeof(CrxModal).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new Xunit.Sdk.XunitException($"Missing private field '{fieldName}' on {typeof(CrxModal).FullName}.");

            var value = field.GetValue(this);
            if (value is T typed)
            {
                return typed;
            }

            throw new Xunit.Sdk.XunitException(
                $"Unable to read private field '{fieldName}' as {typeof(T).Name}; actual value type was '{value?.GetType().Name ?? "null"}'.");
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/proof");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = ToAbsoluteUri(uri).ToString();
        }
    }

    private sealed class NoopJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            _ = identifier;
            _ = cancellationToken;
            _ = args;
            throw new NotSupportedException("JS runtime is not used in ProofGestureSelectionContractTests.");
        }
    }
}
