using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofEditingPlaybackSourceContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string ChapterReviewCssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor.css";
    private const string MobileActionBarRelativePath = "host/Ams.Workstation.Server/Components/Layout/MobileActionBar.razor";
    private const string CrxModalRelativePath = "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor";
    private const string AudioControllerRelativePath = "host/Ams.Workstation.Server/Controllers/AudioController.cs";

    [Fact]
    public void ChapterReview_PlaybackHelpers_TargetCorrectedAwareEndpoints()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected\";",
            "corrected waveform playback endpoint");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected/peaks?pxPerSec={PeakPxPerSec}\";",
            "corrected waveform peaks endpoint");

        AssertDoesNotContain(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}\";",
            "stale generic chapter playback endpoint literal");

        AssertDoesNotContain(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/peaks?pxPerSec={PeakPxPerSec}\";",
            "stale generic chapter peaks endpoint literal");
    }

    [Fact]
    public void ChapterReview_PlaybackHelpers_KeepEscapingAndBlankChapterGuards()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (!string.IsNullOrWhiteSpace(ChapterName))",
            "audio URL blank-chapter guard");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (string.IsNullOrWhiteSpace(ChapterName))",
            "peaks URL blank-chapter guard");

        var escapedChapterUsageCount = CountOccurrences(source, "Uri.EscapeDataString(ChapterName)");
        Assert.True(
            escapedChapterUsageCount >= 2,
            $"Expected corrected playback helpers to URI-escape chapter names in '{ChapterReviewRelativePath}'. Found {escapedChapterUsageCount} occurrence(s) of Uri.EscapeDataString(ChapterName).");
    }

    [Fact]
    public void CrxModal_SubmitPath_CommitsAndValidatesPendingRangeInputs()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(
            source,
            CrxModalRelativePath,
            "private bool TryCommitPendingRangeInputs()",
            "submit helper that commits active raw range text before request creation");

        AssertContains(
            source,
            CrxModalRelativePath,
            "if (!TryCommitPendingRangeInputs())",
            "submit path guard that fail-closes when pending start/end text is invalid");

        AssertContains(
            source,
            CrxModalRelativePath,
            "if (_isVisible && !_submitting)",
            "submit async gate delegates validation to submit helper instead of stale pre-check");

        AssertDoesNotContain(
            source,
            CrxModalRelativePath,
            "if (_isVisible && !_submitting && !HasRangeValidationError)",
            "stale async pre-check that could skip active range text validation");
    }

    [Fact]
    public void ChapterReview_CrxExportComposition_UsesMinMaxRangeAndSharedSingleBatchFlow()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private static CrxExportComposition ComposeCrxExportComposition(IReadOnlyList<CrxExportSeed> exportSeeds)",
            "shared CRX export composition helper for single and multi-sentence paths");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var batchStart = startCandidates.Count > 0 ? startCandidates.Min() : double.NaN;",
            "batch export start bound composes from minimum selected start time");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var batchEnd = endCandidates.Count > 0 ? endCandidates.Max() : double.NaN;",
            "batch export end bound composes from maximum selected end time");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var composition = ComposeCrxExportComposition(exportSeeds);",
            "selection swipe-right export path uses shared composition helper");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var composition = ComposeCrxExportComposition(new List<CrxExportSeed> { exportSeed });",
            "single sentence CRX path uses shared composition helper");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "composition.RequiresRangeConfirmation);",
            "modal handoff carries explicit range confirmation requirement for fallback ranges");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "[ProofCrxExport]",
            "CRX export diagnostics anchor emits composed range/cardinality metadata");
    }

    [Fact]
    public void ChapterReview_PlaybackNavigation_UsesStableSentenceCursorForKeyboardStepping()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private int ResolvePlaybackNavigationIndex()",
            "playback keyboard navigation cursor resolver");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var currentIndex = ResolvePlaybackNavigationIndex();",
            "playback keyboard navigation uses cursor resolver");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_playbackNavigationIndex = currentIndex;",
            "playback keyboard navigation persists cursor index");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var shouldSeek = _waveformPlayer != null && Math.Abs(_currentTime - targetSentence.StartTime) > 0.005;",
            "playback keyboard navigation avoids redundant seek requests when sentence times match");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "_pendingKeyboardSeekTime = targetSentence.StartTime;",
            "playback keyboard navigation tracks in-flight seek target");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "wavesurfer may emit an intermediate seeking callback at the pre-seek time.",
            "playback seek handler tolerates transient pre-seek callback ordering");
    }

    [Fact]
    public void ChapterReview_PlaybackErrorAlert_IsGatedToActiveAudioPlayback()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "var isPlaying = _waveformPlayer?.IsPlaying == true;",
            "playback alert gating reads waveform play state");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (!isPlaying)",
            "paused playback gate prevents time-sync cursor snapback");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "Keep manual keyboard selection stable while paused.",
            "paused playback gate documents manual navigation stability");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (_currentView == \"playback\"",
            "playback alert is emitted only in playback view");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "Manual keyboard seeking should stay silent.",
            "playback alert contract notes silent manual navigation");
    }

    [Fact]
    public void ChapterReview_MobileActionBar_UsesCrossNavDefaultsAndSelectionAwareBatchActions()
    {
        var chapterReviewSource = ReadRepoFile(ChapterReviewRelativePath);
        var mobileActionBarSource = ReadRepoFile(MobileActionBarRelativePath);

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "<SectionContent SectionName=\"mobile-action-bar\">",
            "ChapterReview provides mobile action bar section content");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "<MobileActionBar CurrentView=\"@_currentView\"",
            "ChapterReview renders MobileActionBar with current view binding");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "IsBatchActionsEnabled=\"@CanRunSelectionBatchActions\"",
            "mobile action bar binds batch action enablement to selection context");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "private bool CanRunSelectionBatchActions => _isSelectionModeActive && _selectedSentenceIds.Count > 0;",
            "selection-aware enablement guard for export/ignore mobile actions");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "private void ApplyCrossNav(string direction, string trigger)",
            "cross-nav helper centralizes keyboard and mobile action behavior");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "ApplyCrossNav(direction, \"keyboard-shortcut\");",
            "keyboard cross-nav path delegates to shared helper");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "ApplyCrossNav(\"playback-to-errors\", \"mobile-action-errors\");",
            "mobile errors action reuses playback-to-errors cross-nav semantics");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "ApplyCrossNav(\"errors-to-playback\", \"mobile-action-playback\");",
            "mobile playback action reuses errors-to-playback cross-nav semantics");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "private int ResolveCrossNavErrorIndex()",
            "cross-nav fallback computes deterministic default error selection index");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "TryResolveCrossNavPlaybackSentence(preferredSentenceId, out var playbackSentence)",
            "cross-nav fallback auto-selects a playback sentence when switching from errors");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "return HandleSelectionSwipeRightAsync(anchorSentenceId, $\"mobile-action-bar-{CurrentMobileActionSurface}\");",
            "mobile export action reuses swipe-right batch semantics");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "return HandleSelectionSwipeLeftAsync(anchorSentenceId, $\"mobile-action-bar-{CurrentMobileActionSurface}\");",
            "mobile ignore action reuses swipe-left batch semantics");

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "await JS.InvokeVoidAsync(\"eval\", $\"document.getElementById('{MobileModuleRailToggleElementId}')?.click();\");",
            "mobile modules action forwards to module rail toggle control");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"errors\"",
            "mobile action bar errors button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"playback\"",
            "mobile action bar playback button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"export\"",
            "mobile action bar export button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"ignore\"",
            "mobile action bar ignore button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"reviewed\"",
            "mobile action bar reviewed button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action=\"modules\"",
            "mobile action bar modules button anchor");

        AssertContains(
            mobileActionBarSource,
            MobileActionBarRelativePath,
            "data-ams-proof-mobile-action-bar-selection-count=\"@SelectedCount\"",
            "mobile action bar selection-count diagnostic marker");
    }

    [Fact]
    public void ChapterReview_MobileCssContracts_DeclareTouchActionAndSafeAreaGuards()
    {
        var css = ReadRepoFile(ChapterReviewCssRelativePath);

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            "touch-action: manipulation;",
            "proof surface touch-action manipulation baseline");

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            ".proof-errors-shell",
            "proof errors shell selector for mobile gesture region");

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            "touch-action: pan-y;",
            "proof swipe region pan-y touch-action guard");

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            "padding-bottom: calc(9rem + env(safe-area-inset-bottom));",
            "proof safe-area bottom spacing for mobile action bar");

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            ".proof-review ::deep input",
            "proof mobile iOS zoom guard selector");

        AssertContains(
            css,
            ChapterReviewCssRelativePath,
            "font-size: 16px;",
            "proof mobile input font-size guard");
    }

    [Fact]
    public void CrxModal_MobileLayoutContract_DeclaresResponsiveSafeAreaAndInputGuards()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(
            source,
            CrxModalRelativePath,
            "data-ams-crx-modal-state=",
            "CRX modal state diagnostic marker");

        AssertContains(
            source,
            CrxModalRelativePath,
            "data-ams-crx-modal-layout=\"responsive\"",
            "CRX modal responsive layout contract marker");

        AssertContains(
            source,
            CrxModalRelativePath,
            "aria-label=\"Add to CRX\"",
            "CRX modal dialog accessibility label");

        AssertContains(
            source,
            CrxModalRelativePath,
            "z-index: 1300; /* Above shell mobile action bar host (z-index: 1150). */",
            "CRX modal overlay stacks above mobile action bar shell chrome");

        AssertContains(
            source,
            CrxModalRelativePath,
            "max-height: min(92dvh, 48rem);",
            "CRX modal desktop max-height clamp");

        AssertContains(
            source,
            CrxModalRelativePath,
            "@@media (max-width: 768px)",
            "CRX modal mobile breakpoint rule");

        AssertContains(
            source,
            CrxModalRelativePath,
            "max-height: calc(100dvh - env(safe-area-inset-top) - env(safe-area-inset-bottom) - 0.7rem);",
            "CRX modal mobile safe-area max-height clamp");

        AssertContains(
            source,
            CrxModalRelativePath,
            "padding-bottom: calc(0.85rem + env(safe-area-inset-bottom));",
            "CRX modal footer safe-area padding");

        AssertContains(
            source,
            CrxModalRelativePath,
            "min-height: 44px;",
            "CRX modal touch-target minimum size");
    }

    [Fact]
    public void ChapterReview_BatchIgnore_RoutesSwipeAndExplicitActionsThroughSharedExecutor()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "OnIgnoreSentence=\"HandleIgnoreSentenceActionAsync\"",
            "errors view explicit ignore callback routes through selection-aware handler");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private Task HandleIgnoreSentenceActionAsync(SentenceReport sentence)",
            "selection-aware explicit ignore helper declaration");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private async Task ExecuteSelectionBatchIgnoreAsync(",
            "shared batch ignore executor declaration");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return ExecuteSelectionBatchIgnoreAsync(",
            "swipe-left and explicit ignore wrappers delegate to shared batch ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"swipe-left\"",
            "swipe-left trigger preserved while delegating to shared batch ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"ignore-button\"",
            "errors view explicit ignore trigger delegates to shared batch ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"ignore-shortcut\"",
            "keyboard explicit ignore trigger delegates to shared batch ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "eventName: \"explicit-ignore\"",
            "explicit ignore paths share explicit batch-ignore event contract");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "const string selectionPolicy = \"retain\";",
            "batch ignore uses deterministic selection retention policy");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "[ProofBatchIgnore]",
            "batch ignore diagnostics anchor emits selected/ignored/failure counts");
    }

    [Fact]
    public void AudioController_CorrectedEndpoints_UseSharedResolutionPath()
    {
        var source = ReadRepoFile(AudioControllerRelativePath);

        const string resolverCall = "TryResolveCorrectedPlayback(chapterName, out var resolved, out var failureResult)";
        var resolverCallCount = CountOccurrences(source, resolverCall);

        Assert.True(
            resolverCallCount == 2,
            $"Expected corrected audio and peaks endpoints to share resolver call '{resolverCall}' exactly twice in '{AudioControllerRelativePath}', but found {resolverCallCount} occurrence(s).");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "private bool TryResolveCorrectedPlayback(",
            "shared corrected playback resolver declaration");
    }

    [Fact]
    public void AudioController_CorrectedResolver_DeclaresDeterministicFallbackAndFailClosedGuards()
    {
        var source = ReadRepoFile(AudioControllerRelativePath);

        AssertContains(
            source,
            AudioControllerRelativePath,
            "var requestedChapter = Uri.UnescapeDataString(chapterName ?? string.Empty).Trim();",
            "decoded chapter token normalization");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "if (string.IsNullOrWhiteSpace(requestedChapter))",
            "blank chapter fail-closed guard");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "if (!MatchesRequestedChapter(requestedChapter, activeChapter, descriptor))",
            "chapter mismatch fail-closed guard");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "new[] { (\"corrected\", audio.Corrected), (\"treated\", audio.Treated), (\"current\", currentContext) }",
            "deterministic corrected→treated→current fallback order");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "(checked corrected, treated, current)",
            "explicit deterministic fallback diagnostics");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "private static bool MatchesRequestedChapter(",
            "chapter identity matcher for reserved slug/alias compatibility");
    }

    private static int CountOccurrences(string source, string token)
    {
        var count = 0;
        var index = 0;
        while (true)
        {
            index = source.IndexOf(token, index, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            index += token.Length;
        }
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof editing playback source contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale proof editing playback source anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof editing playback source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof editing playback source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
