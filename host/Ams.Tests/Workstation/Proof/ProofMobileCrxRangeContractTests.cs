using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofMobileCrxRangeContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string CrxModalRelativePath = "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor";

    [Fact]
    public void CrxModal_UsesAbsoluteRangeControls_WithStableStartEndAnchors()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-controls=\"true\"", "range controls wrapper seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-start-control=\"true\"", "start bound control seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-end-control=\"true\"", "end bound control seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-start-input=\"true\"", "start input seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-end-input=\"true\"", "end input seam");
        AssertContains(source, CrxModalRelativePath, "Drag up/down in each time field to nudge that bound.", "drag-adjust affordance text");

        AssertDoesNotContain(source, CrxModalRelativePath, "Tail Padding", "legacy tail-padding label");
        AssertDoesNotContain(source, CrxModalRelativePath, "type=\"range\"", "legacy tail-padding slider control");
    }

    [Fact]
    public void CrxModal_RangeValidation_StaysFailClosedForPreviewAndSubmit()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-validation=\"true\"", "range validation diagnostics seam");
        AssertContains(source, CrxModalRelativePath, "private bool HasRangeValidationError => !string.IsNullOrWhiteSpace(_rangeValidationMessage);", "range validation state helper");
        AssertContains(source, CrxModalRelativePath, "private bool HasPendingRangeConfirmation => _requiresRangeConfirmation && !_isRangeConfirmed;", "fallback range confirmation state helper");
        AssertContains(source, CrxModalRelativePath, "if (HasRangeValidationError)", "fail-closed preview guard");
        AssertContains(source, CrxModalRelativePath, "_audioUrl = string.Empty;", "audio preview cleared on invalid ranges");
        AssertContains(source, CrxModalRelativePath, "_peaksUrl = null;", "waveform peaks cleared on invalid ranges");
        AssertContains(source, CrxModalRelativePath, "OnClick=\"Submit\" IsEnabled=\"@(!_submitting && !HasRangeValidationError && !HasPendingRangeConfirmation)\"", "submit button disabled while range invalid or fallback confirmation pending");
        AssertContains(source, CrxModalRelativePath, "Start must be before End. Adjust either bound to create a positive export range.", "negative-duration validation message");
        AssertContains(source, CrxModalRelativePath, "Range is zero-duration. Adjust Start earlier or End later before submitting this CRX entry.", "zero-duration validation message");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-confirm-required=\"true\"", "fallback confirmation prompt seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-confirm-checkbox=\"true\"", "fallback range confirmation checkbox seam");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-range-confirm-status=\"pending\"", "fallback confirmation pending status seam");
    }

    [Fact]
    public void CrxModal_SubmitContract_PreservesAbsoluteBoundsWithZeroPaddingRequestShape()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(source, CrxModalRelativePath, "Start: _startTime,", "submit request start bound");
        AssertContains(source, CrxModalRelativePath, "End: _endTime,", "submit request end bound");
        AssertContains(source, CrxModalRelativePath, "PaddingMs: 0,", "submit request keeps service contract with explicit zero padding");
    }

    [Fact]
    public void ChapterReview_MobilePlayback_ExposesSentenceFirstCompactLayoutSeams()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-mobile-contract=\"sentence-first\"", "sentence-first playback shell seam");
        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-mobile-waveform=\"compact\"", "compact waveform seam");
        AssertContains(source, ChapterReviewRelativePath, "<div class=\"proof-playback-sentences\" data-ams-proof-gesture-surface=\"playback\">", "sentence list wrapper seam");
    }

    [Fact]
    public void ChapterReview_AlertSettings_DefaultCollapsedWithExplicitToggleAndPanelSeams()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(source, ChapterReviewRelativePath, "private string PlaybackAlertSettingsState => _isPlaybackAlertSettingsExpanded ? \"expanded\" : \"collapsed\";", "alert settings state seam");
        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-alert-settings-state=\"@PlaybackAlertSettingsState\"", "alert settings state diagnostics anchor");
        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-alert-settings-toggle=\"true\"", "alert settings toggle seam");
        AssertContains(source, ChapterReviewRelativePath, "@(_isPlaybackAlertSettingsExpanded ? \"Hide alert settings\" : \"Alert settings\")", "toggle text seam");
        AssertContains(source, ChapterReviewRelativePath, "@if (_isPlaybackAlertSettingsExpanded)", "alert settings conditional render seam");
        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-alert-settings-panel=\"true\"", "alert settings panel seam");
        AssertContains(source, ChapterReviewRelativePath, "_isPlaybackAlertSettingsExpanded = false;", "chapter reset defaults alert settings to collapsed");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof mobile CRX contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale proof mobile CRX contract anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof mobile contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof mobile contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
