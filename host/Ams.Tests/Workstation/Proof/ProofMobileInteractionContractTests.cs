using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofMobileInteractionContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string WaveformInteropRelativePath = "host/Ams.Workstation.Server/wwwroot/js/waveform-interop.js";
    private const string KeyboardShortcutsRelativePath = "host/Ams.Workstation.Server/wwwroot/js/keyboard-shortcuts.js";

    [Fact]
    public void WaveformInterop_TapSeekGuard_UsesPassivePointerListenersAndScrollSuppressionDiagnostics()
    {
        var source = ReadRepoFile(WaveformInteropRelativePath);

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "const DEFAULT_TAP_SUPPRESSION_MOVE_TOLERANCE_PX = 12;",
            "tap-seek suppression move tolerance default");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "const DEFAULT_TAP_SUPPRESSION_WINDOW_MS = 280;",
            "tap-seek suppression window default");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "tapSuppressionMoveTolerancePx: options.tapSuppressionMoveTolerancePx || DEFAULT_TAP_SUPPRESSION_MOVE_TOLERANCE_PX,",
            "tap-seek suppression tolerance stored in waveform instance metadata");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "tapSuppressionWindowMs: options.tapSuppressionWindowMs || DEFAULT_TAP_SUPPRESSION_WINDOW_MS,",
            "tap-seek suppression window stored in waveform instance metadata");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "attachTapSeekGuard(instance);",
            "registerCallbacks wires tap-seek suppression guard");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "wrapper.addEventListener('pointerdown', onPointerDown, { passive: true });",
            "pointerdown listener is passive for touch scroll friendliness");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "wrapper.addEventListener('pointermove', onPointerMove, { passive: true });",
            "pointermove listener is passive for touch scroll friendliness");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "wrapper.addEventListener('pointerup', clearPointerTracking, { passive: true });",
            "pointerup listener is passive for touch scroll friendliness");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "wrapper.addEventListener('pointercancel', clearPointerTracking, { passive: true });",
            "pointercancel listener is passive for touch scroll friendliness");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "wrapper.addEventListener('click', onClickCapture, { passive: false, capture: true });",
            "click capture listener remains active for suppressing accidental seek after scroll");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "markTapSeekSuppressed('vertical-scroll');",
            "vertical-scroll path suppresses accidental tap-to-seek dispatch");

        AssertContains(
            source,
            WaveformInteropRelativePath,
            "[waveform-interop] tap-seek suppressed",
            "tap-seek suppression diagnostics anchor");
    }

    [Fact]
    public void KeyboardShortcuts_RebindsSingleNonPassiveKeydownListener()
    {
        var source = ReadRepoFile(KeyboardShortcutsRelativePath);

        AssertContains(
            source,
            KeyboardShortcutsRelativePath,
            "const KEYDOWN_LISTENER_OPTIONS = { passive: false };",
            "keydown listener options remain non-passive for preventDefault keyboard shortcuts");

        AssertContains(
            source,
            KeyboardShortcutsRelativePath,
            "if (_handler)",
            "init path guards against duplicate keydown listener registration");

        AssertContains(
            source,
            KeyboardShortcutsRelativePath,
            "document.removeEventListener('keydown', _handler, KEYDOWN_LISTENER_OPTIONS);",
            "init/dispose rebind path removes prior keydown listener before replacing it");

        AssertContains(
            source,
            KeyboardShortcutsRelativePath,
            "document.addEventListener('keydown', _handler, KEYDOWN_LISTENER_OPTIONS);",
            "keydown listener rebind uses shared options contract");
    }

    [Fact]
    public void ChapterReview_BatchGestureValidation_FailClosesSurfaceMismatchesWithDiagnostics()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var methodBody = ExtractMethodBody(source, "private bool TryValidateBatchGestureContext(");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private static bool IsFallbackActionSurface(string sourceSurface)",
            "fallback action surface helper declaration");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "!IsFallbackActionSurface(sourceSurface)",
            "batch gesture validation checks fallback control bypass first");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "!string.Equals(sourceSurface, CurrentMobileActionSurface, StringComparison.Ordinal)",
            "batch gesture validation fail-closes mismatched source surfaces");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "_lastSelectionGestureEvent = $\"{eventName}-ignored-surface-mismatch\";",
            "surface mismatch diagnostic event name anchor");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "expectedSurface={CurrentMobileActionSurface};reason=surface-mismatch",
            "surface mismatch diagnostics include current expected surface");
    }

    [Fact]
    public void ChapterReview_KeyboardNavigation_SkipsNoOpRendersAtBounds()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var methodBody = ExtractMethodBody(source, "public async Task OnNavigateItem(string direction)");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "var nextIndex = direction == \"prev\"",
            "errors-view navigation computes clamped candidate index");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "if (nextIndex == _selectedErrorIndex)",
            "errors-view keyboard navigation exits early on no-op boundary movement");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "var navigationChanged =",
            "playback-view navigation computes state-change predicate before rendering");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "if (!navigationChanged && !shouldSeek)",
            "playback-view keyboard navigation exits early when no seek or selection change occurred");

        AssertContains(
            methodBody,
            ChapterReviewRelativePath,
            "if (shouldRender)",
            "single render gate prevents unconditional rerenders per key event");
    }

    private static string ExtractMethodBody(string source, string methodSignature)
    {
        var signatureIndex = source.IndexOf(methodSignature, StringComparison.Ordinal);
        Assert.True(
            signatureIndex >= 0,
            $"Missing method signature '{methodSignature}' while extracting contract body.");

        var bodyStart = source.IndexOf('{', signatureIndex);
        Assert.True(
            bodyStart >= 0,
            $"Could not locate opening brace for method signature '{methodSignature}'.");

        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            var current = source[index];
            if (current == '{')
            {
                depth++;
            }
            else if (current == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(bodyStart, index - bodyStart + 1);
                }
            }
        }

        throw new XunitException($"Unbalanced braces while extracting method body for signature '{methodSignature}'.");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof mobile interaction source contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof mobile interaction source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof mobile interaction source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
