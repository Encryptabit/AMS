using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofMobileAccessibilityContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string MainLayoutRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor";
    private const string HeaderControlsRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor";
    private const string CrxModalRelativePath = "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor";

    [Fact]
    public void ChapterReview_GestureFallbackControls_AreExplicitAndDiagnosable()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(source, ChapterReviewRelativePath, "<SectionContent SectionName=\"mobile-action-bar\">", "mobile action bar section host");
        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-gesture-fallback-controls=\"mobile-action-buttons\"", "gesture fallback controls contract marker");
        AssertContains(source, ChapterReviewRelativePath, "aria-label=\"Proof gesture fallback controls\"", "gesture fallback region accessibility label");

        AssertContains(source, ChapterReviewRelativePath, "data-ams-proof-selection-fallback-toggle=\"true\"", "selection-mode fallback toggle marker");
        AssertContains(source, ChapterReviewRelativePath, "aria-label=\"@SelectionFallbackToggleAriaLabel\"", "selection fallback toggle accessibility label binding");
        AssertContains(source, ChapterReviewRelativePath, "title=\"@SelectionFallbackToggleTitle\"", "selection fallback toggle title binding");
        AssertContains(source, ChapterReviewRelativePath, "OnClick=\"ToggleSelectionModeFallbackAsync\"", "selection fallback toggle callback");

        AssertContains(source, ChapterReviewRelativePath, "private async Task ToggleSelectionModeFallbackAsync()", "selection fallback toggle handler");
        AssertContains(source, ChapterReviewRelativePath, "private int ResolveFallbackSelectionSentenceId()", "selection fallback sentence resolver");

        AssertContains(source, ChapterReviewRelativePath, "var isFallbackAction = sourceSurface.StartsWith(\"mobile-action-bar-\", StringComparison.Ordinal);", "fallback action source classifier");
        AssertContains(source, ChapterReviewRelativePath, "var trigger = isFallbackAction ? \"export-button\" : \"swipe-right\";", "export trigger distinguishes explicit fallback from gesture");
        AssertContains(source, ChapterReviewRelativePath, "var eventName = isFallbackAction ? \"explicit-export\" : \"swipe-right-export\";", "export event naming distinguishes fallback path");
        AssertContains(source, ChapterReviewRelativePath, "var trigger = isFallbackAction ? \"ignore-button\" : \"swipe-left\";", "ignore trigger distinguishes explicit fallback from gesture");
        AssertContains(source, ChapterReviewRelativePath, "var eventName = isFallbackAction ? \"explicit-ignore\" : \"swipe-left-ignore\";", "ignore event naming distinguishes fallback path");

        AssertContains(source, ChapterReviewRelativePath, "[ProofSelectionMode]", "selection-mode diagnostics anchor remains present");
    }

    [Fact]
    public void ShellUtilityToggles_DeclareExplicitAccessibleLabelsAndTitles()
    {
        var mainLayout = ReadRepoFile(MainLayoutRelativePath);
        var headerControls = ReadRepoFile(HeaderControlsRelativePath);

        AssertContains(mainLayout, MainLayoutRelativePath, "aria-label=\"@(isMobileModuleRailOpen ? \"Close module navigation\" : \"Open module navigation\")\"", "module rail toggle accessibility label expression");
        AssertContains(mainLayout, MainLayoutRelativePath, "title=\"@(isMobileModuleRailOpen ? \"Close module navigation\" : \"Open module navigation\")\"", "module rail toggle title expression");
        AssertContains(mainLayout, MainLayoutRelativePath, "aria-label=\"Close module navigation\"", "module rail close button accessibility label");
        AssertContains(mainLayout, MainLayoutRelativePath, "title=\"Close module navigation\"", "module rail close button title");

        AssertContains(headerControls, HeaderControlsRelativePath, "aria-label=\"@(isMobileOverflowOpen ? \"Close secondary header actions\" : \"Open secondary header actions\")\"", "overflow trigger accessibility label expression");
        AssertContains(headerControls, HeaderControlsRelativePath, "title=\"@(isMobileOverflowOpen ? \"Close secondary header actions\" : \"Open secondary header actions\")\"", "overflow trigger title expression");
        AssertContains(headerControls, HeaderControlsRelativePath, "aria-label=\"Close secondary header actions\"", "overflow panel close button accessibility label");
        AssertContains(headerControls, HeaderControlsRelativePath, "title=\"Close secondary header actions\"", "overflow panel close button title");
    }

    [Fact]
    public void CrxModal_IconOnlyCloseControl_HasAccessibleContractAnchors()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(source, CrxModalRelativePath, "IconName=\"Cancel\"", "icon-only close control anchor");
        AssertContains(source, CrxModalRelativePath, "AriaLabel=\"Close CRX modal\"", "icon-only close control aria label");
        AssertContains(source, CrxModalRelativePath, "Title=\"Close CRX modal\"", "icon-only close control title");
        AssertContains(source, CrxModalRelativePath, "data-ams-crx-modal-control=\"close\"", "icon-only close control diagnostic marker");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof mobile accessibility source contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof mobile accessibility source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof mobile accessibility source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
