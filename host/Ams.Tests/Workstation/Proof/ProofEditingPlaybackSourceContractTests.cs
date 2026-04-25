using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofEditingPlaybackSourceContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string ChapterReviewCssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor.css";
    private const string SentenceListRelativePath = "host/Ams.Workstation.Server/Components/Shared/SentenceList.razor";
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
    public void ChapterReview_SelectionMode_LongPressIsWiredToSentenceRows_AndSelectionIndicatorUsesValidBorderToken()
    {
        var chapterReviewSource = ReadRepoFile(ChapterReviewRelativePath);
        var sentenceListSource = ReadRepoFile(SentenceListRelativePath);
        var chapterReviewCss = ReadRepoFile(ChapterReviewCssRelativePath);

        AssertContains(
            chapterReviewSource,
            ChapterReviewRelativePath,
            "OnSentenceLongPress=\"OnSentenceLongPress\"",
            "ChapterReview wires sentence list long-press callback to selection state machine");

        AssertContains(
            sentenceListSource,
            SentenceListRelativePath,
            "public EventCallback<int> OnSentenceLongPress { get; set; }",
            "SentenceList exposes long-press callback for sentence rows");

        AssertContains(
            sentenceListSource,
            SentenceListRelativePath,
            "@onpointerdown=\"@(args => HandleSentencePointerDown(sentence.Id, args))\"",
            "SentenceList row template arms long-press timer on pointer-down");

        AssertContains(
            sentenceListSource,
            SentenceListRelativePath,
            "if (_longPressTriggeredSentenceIds.Remove(sentence.Id))",
            "SentenceList suppresses click toggle after long-press gesture fires");

        AssertContains(
            chapterReviewCss,
            ChapterReviewCssRelativePath,
            "border: 1px solid var(--bit-clr-brd-pri);",
            "selection indicator border uses valid Bit token");

        AssertDoesNotContain(
            chapterReviewCss,
            ChapterReviewCssRelativePath,
            "var(--bit-clr-bdr-pri)",
            "selection indicator no longer uses typo bit border token");
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
