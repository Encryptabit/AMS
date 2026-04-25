using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofGestureSelectionContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string TouchGesturesRelativePath = "host/Ams.Workstation.Server/wwwroot/js/touch-gestures.js";
    private const string CrxServiceRelativePath = "host/Ams.Workstation.Server/Services/CrxService.cs";
    private const string ReviewedStatusServiceRelativePath = "host/Ams.Workstation.Server/Services/ReviewedStatusService.cs";

    [Fact]
    public void ChapterReview_LongPressSelectionContract_EntersThenClearsAndResumesPlayback()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "public async Task OnSentenceLongPress(int sentenceId)",
            "JS invokable long-press entry point");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (_isSelectionModeActive)",
            "long-press toggle branch for active selection mode");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "await ClearSelectionModeAndResumeAsync(sentenceId, \"long-press\");",
            "active-mode long-press clears selection and triggers resume flow");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "await EnterSelectionModeAsync(sentenceId, \"long-press\");",
            "inactive-mode long-press enters selection mode");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private async Task EnterSelectionModeAsync(int sentenceId, string trigger)",
            "selection-mode enter helper");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_selectedSentenceIds.Add(resolvedSentenceId);",
            "selection mode seeds pressed sentence");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var autoPaused = await TryPausePlaybackForSelectionModeAsync();",
            "long-press enter auto-pause check");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_resumePlaybackOnSelectionExit = autoPaused;",
            "enter helper records resume guard based on auto-pause result");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_lastSelectionGestureEvent = \"long-press-enter\";",
            "selection diagnostics event for long-press entry");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private async Task ClearSelectionModeAndResumeAsync(int sentenceId, string trigger)",
            "selection-mode clear helper");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_selectedSentenceIds.Clear();",
            "clear helper empties selected sentence set");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_isSelectionModeActive = false;",
            "clear helper exits selection mode");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var resumedPlayback = await TryResumePlaybackAfterSelectionModeAsync();",
            "clear helper runs guarded playback resume path");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_resumePlaybackOnSelectionExit = false;",
            "clear helper resets resume guard after exit");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_lastSelectionGestureEvent = \"long-press-exit\";",
            "selection diagnostics event for long-press exit");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "Long-press again to clear and resume playback.",
            "selection mode affordance text documents clear+resume behavior");
    }

    [Fact]
    public void GestureDispatcher_MultiSelectSwipeContract_CoversPlaybackSwipeLeftIgnorePath()
    {
        var chapterReviewSource = ReadRepoFile(ChapterReviewRelativePath);
        var touchGestureSource = ReadRepoFile(TouchGesturesRelativePath);

        AssertContains(
            touchGestureSource,
            TouchGesturesRelativePath,
            "const playbackRow = target.closest('[data-ams-proof-gesture-surface=\"playback\"] [id^=\"sentence-\"]')",
            "gesture context resolves playback sentence rows");

        AssertContains(
            touchGestureSource,
            TouchGesturesRelativePath,
            "surface: 'playback',",
            "playback gesture context tags surface as playback");

        AssertContains(
            touchGestureSource,
            TouchGesturesRelativePath,
            "void dispatchGesture('OnSelectionSwipeRight', gesture.sentenceId, gesture.surface);",
            "swipe-right dispatch forwards sentence id and source surface");

        AssertContains(
            touchGestureSource,
            TouchGesturesRelativePath,
            "void dispatchGesture('OnSelectionSwipeLeft', gesture.sentenceId, gesture.surface);",
            "swipe-left dispatch forwards sentence id and source surface");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "public Task OnSelectionSwipeRight(int sentenceId, string sourceSurface)",
            "ChapterReview exposes swipe-right JS invokable with source-surface context");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "public Task OnSelectionSwipeLeft(int sentenceId, string sourceSurface)",
            "ChapterReview exposes swipe-left JS invokable with source-surface context");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "=> HandleSelectionSwipeLeftAsync(sentenceId, sourceSurface);",
            "swipe-left invokable routes into ignore path handler");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "const string eventName = \"swipe-left-ignore\";",
            "swipe-left ignore event contract");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "if (!TryValidateBatchGestureContext(eventName, trigger, sentenceId, sourceSurface, out var selectedSentenceIds))",
            "swipe-left path validates batch context with explicit source-surface input");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "surface={sourceSurface};selectedCount={selectedReports.Count};ignoredCount={ignoredPatternCount}",
            "swipe-left ignore diagnostics preserve playback/errors source surface in emitted contract text");
    }

    [Fact]
    public void GestureLayer_PersistenceContract_DoesNotIntroduceNewArtifactSeams()
    {
        var chapterReviewSource = ReadRepoFile(ChapterReviewRelativePath);
        var touchGestureSource = ReadRepoFile(TouchGesturesRelativePath);
        var crxServiceSource = ReadRepoFile(CrxServiceRelativePath);
        var reviewedStatusServiceSource = ReadRepoFile(ReviewedStatusServiceRelativePath);

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "private string GetCrxJsonPath(bool createDir = true)",
            "CRX JSON persistence seam remains service-owned");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "return Path.Combine(crxFolder, $\"{bookName}_CRX.json\");",
            "CRX JSON artifact naming contract remains unchanged");

        AssertContains(
            reviewedStatusServiceSource,
            ReviewedStatusServiceRelativePath,
            "private string GetFilePath() => Path.Combine(BasePath, \"reviewed-status.json\");",
            "reviewed-status persistence seam remains service-owned");

        AssertContains(
            reviewedStatusServiceSource,
            ReviewedStatusServiceRelativePath,
            "private static readonly string BasePath = AmsAppDataPaths.Resolve(\"workstation\");",
            "reviewed-status base path contract remains unchanged");

        AssertDoesNotContain(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "reviewed-status.json",
            "gesture UI wiring must not define reviewed persistence artifacts");

        AssertDoesNotContain(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "_CRX.json",
            "gesture UI wiring must not define CRX JSON persistence artifacts");

        AssertDoesNotContain(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "_CRX.xlsx",
            "gesture UI wiring must not define CRX workbook persistence artifacts");

        AssertDoesNotContain(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "AmsAppDataPaths.Resolve",
            "gesture UI wiring must not own app-data persistence roots");

        AssertDoesNotContain(
            touchGestureSource,
            TouchGesturesRelativePath,
            "localStorage",
            "touch gesture dispatcher must stay persistence-free");

        AssertDoesNotContain(
            touchGestureSource,
            TouchGesturesRelativePath,
            "sessionStorage",
            "touch gesture dispatcher must stay persistence-free");

        AssertDoesNotContain(
            touchGestureSource,
            TouchGesturesRelativePath,
            "reviewed-status.json",
            "touch gesture dispatcher must not introduce reviewed persistence seams");

        AssertDoesNotContain(
            touchGestureSource,
            TouchGesturesRelativePath,
            "_CRX.json",
            "touch gesture dispatcher must not introduce CRX persistence seams");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof gesture selection contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale or forbidden proof gesture selection anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof gesture selection source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof gesture selection source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CODE-STYLE.md"))
                && Directory.Exists(Path.Combine(current.FullName, "host"))
                && Directory.Exists(Path.Combine(current.FullName, ".gsd")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md, host/, and .gsd/.");
    }
}
