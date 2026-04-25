using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellCompositionContractTests
{
    private const string MainLayoutRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor";
    private const string MainLayoutCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor.css";
    private const string StageModuleRailRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor";
    private const string StageModuleRailCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor.css";
    private const string HeaderControlsRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor";
    private const string HeaderControlsCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor.css";

    [Fact]
    public void MainLayout_UsesThreeRegionShellCompositionWithLeftRailAndOptionalInspectorHost()
    {
        var source = ReadRepoFile(MainLayoutRelativePath);

        AssertContains(source, MainLayoutRelativePath, "data-stage-shell-region=\"left\"", "left shell region marker");
        AssertContains(source, MainLayoutRelativePath, "data-stage-shell-region=\"center\"", "center shell region marker");
        AssertContains(source, MainLayoutRelativePath, "data-stage-shell-region=\"right\"", "right shell region marker");
        AssertContains(source, MainLayoutRelativePath, "<aside class=\"workstation-sidebar @SidebarMobileStateClass\"", "left sidebar shell host");
        AssertContains(source, MainLayoutRelativePath, "<StageModuleRail", "sidebar module rail host");
        AssertContains(source, MainLayoutRelativePath, "<SectionOutlet SectionName=\"module-inspector\" />", "optional inspector outlet host");

        AssertDoesNotContain(source, MainLayoutRelativePath, "<div class=\"workstation-shell-rail\">", "stale horizontal rail-only shell wrapper");
    }

    [Fact]
    public void StageModuleRail_DeclaresModuleNavigationAnchorsAndStageDiagnostics()
    {
        var source = ReadRepoFile(StageModuleRailRelativePath);

        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-nav=\"modules\"", "explicit module navigation section");
        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-stage=\"@_state.ActiveStageId\"", "active stage diagnostic marker");
        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-module=\"@_state.ActiveModuleId\"", "active module diagnostic marker");
        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-marker=\"@_state.DiagnosticMarker\"", "stage-module diagnostic marker");
        AssertContains(source, StageModuleRailRelativePath, "match.Stage.Modules", "catalog-driven module iteration");
        AssertContains(source, StageModuleRailRelativePath, "stage-module-rail__module-link", "module link style hook");

        AssertDoesNotContain(source, StageModuleRailRelativePath, "data-stage-shell-nav=\"stages\"", "legacy stage section should not duplicate header stage nav");
        AssertDoesNotContain(source, StageModuleRailRelativePath, "stage-module-rail__stage-link", "legacy stage-link class should not exist in module rail");
    }

    [Fact]
    public void MainLayoutAndRailStyles_EncodeSidebarShellAndNoLegacyHorizontalRailClass()
    {
        var mainLayoutCss = ReadRepoFile(MainLayoutCssRelativePath);
        var railCss = ReadRepoFile(StageModuleRailCssRelativePath);

        AssertContains(mainLayoutCss, MainLayoutCssRelativePath, ".workstation-shell", "shell container class");
        AssertContains(mainLayoutCss, MainLayoutCssRelativePath, ".workstation-sidebar", "left sidebar class");
        AssertContains(mainLayoutCss, MainLayoutCssRelativePath, ".workstation-inspector", "optional inspector class");
        AssertDoesNotContain(mainLayoutCss, MainLayoutCssRelativePath, ".workstation-shell-rail", "legacy horizontal shell rail class");

        AssertContains(railCss, StageModuleRailCssRelativePath, "flex-direction: column", "vertical sidebar rail layout");
        AssertContains(railCss, StageModuleRailCssRelativePath, ".stage-module-rail__stage-list", "stage list selector");
        AssertContains(railCss, StageModuleRailCssRelativePath, ".stage-module-rail__module-list", "module list selector");
    }

    [Fact]
    public void HeaderControls_PreservesStageTopNavWhileSidebarOwnsModuleLinks()
    {
        var source = ReadRepoFile(HeaderControlsRelativePath);
        var css = ReadRepoFile(HeaderControlsCssRelativePath);

        AssertContains(source, HeaderControlsRelativePath, "<div class=\"header-nav\"", "top-header stage nav row");
        AssertContains(source, HeaderControlsRelativePath, "NavigateToStage(StageRouteCatalog.StageIds.Prep)", "prep stage nav callback");
        AssertContains(source, HeaderControlsRelativePath, "NavigateToStage(StageRouteCatalog.StageIds.Proof)", "proof stage nav callback");
        AssertContains(source, HeaderControlsRelativePath, "NavigateToStage(StageRouteCatalog.StageIds.Polish)", "polish stage nav callback");
        AssertContains(css, HeaderControlsCssRelativePath, ".header-nav", "header stage nav style block");

        AssertDoesNotContain(source, HeaderControlsRelativePath, "data-stage-module-id", "module links should remain in sidebar rail, not header");
    }

    [Fact]
    public void HeaderControls_MobileOverflowSecondaryActions_AreDiagnosableAndAccessible()
    {
        var source = ReadRepoFile(HeaderControlsRelativePath);
        var css = ReadRepoFile(HeaderControlsCssRelativePath);
        var mainLayout = ReadRepoFile(MainLayoutRelativePath);

        AssertContains(source, HeaderControlsRelativePath, "data-ams-mobile-overflow-state=", "mobile overflow state marker");
        AssertContains(source, HeaderControlsRelativePath, "data-ams-mobile-overflow-open=", "mobile overflow trigger-open marker");
        AssertContains(source, HeaderControlsRelativePath, "aria-haspopup=\"dialog\"", "overflow trigger dialog semantics");
        AssertContains(source, HeaderControlsRelativePath, "aria-controls=\"@MobileOverflowPanelId\"", "overflow trigger controls relationship");
        AssertContains(source, HeaderControlsRelativePath, "aria-expanded=\"@(isMobileOverflowOpen ? \"true\" : \"false\")\"", "overflow expanded state expression");
        AssertContains(source, HeaderControlsRelativePath, "aria-label=\"@(isMobileOverflowOpen ? \"Close secondary header actions\" : \"Open secondary header actions\")\"", "overflow trigger accessibility label expression");
        AssertContains(source, HeaderControlsRelativePath, "title=\"@(isMobileOverflowOpen ? \"Close secondary header actions\" : \"Open secondary header actions\")\"", "overflow trigger title expression");
        AssertContains(source, HeaderControlsRelativePath, "aria-label=\"Close secondary header actions\"", "overflow panel close button accessibility label");
        AssertContains(source, HeaderControlsRelativePath, "title=\"Close secondary header actions\"", "overflow panel close button title");
        AssertContains(source, HeaderControlsRelativePath, "id=\"@MobileOverflowPanelId\"", "overflow panel id expression");
        AssertContains(source, HeaderControlsRelativePath, "aria-labelledby=\"@MobileOverflowTitleId\"", "overflow panel labelled-by expression");
        AssertContains(source, HeaderControlsRelativePath, "@ref=\"mobileOverflowPanelRef\"", "overflow panel focus target");
        AssertContains(source, HeaderControlsRelativePath, "OnAfterRenderAsync", "post-render focus handoff for keyboard users");
        AssertContains(source, HeaderControlsRelativePath, "focusMobileOverflowPanel", "focus state tracking");
        AssertContains(source, HeaderControlsRelativePath, "data-ams-header-control=\"directory-actions-mobile\"", "mobile directory action host");

        AssertContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-trigger", "mobile overflow trigger style");
        AssertContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-overlay", "mobile overflow overlay style");
        AssertContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-panel", "mobile overflow panel style");
        AssertContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-panel:focus", "mobile overflow keyboard focus style");
        AssertContains(css, HeaderControlsCssRelativePath, ".header-secondary-optional", "desktop secondary control group selector");
        AssertContains(css, HeaderControlsCssRelativePath, "display: none !important;", "mobile hide rule for inline secondary controls");

        AssertContains(mainLayout, MainLayoutRelativePath, "data-ams-shell-region=\"header-controls\"", "header controls shell region marker");
        AssertContains(mainLayout, MainLayoutRelativePath, "data-ams-mobile-overflow-contract=\"secondary-actions\"", "header overflow contract marker");
        AssertContains(mainLayout, MainLayoutRelativePath, "aria-label=\"@(isMobileModuleRailOpen ? \"Close module navigation\" : \"Open module navigation\")\"", "module rail toggle accessibility label expression");
        AssertContains(mainLayout, MainLayoutRelativePath, "title=\"@(isMobileModuleRailOpen ? \"Close module navigation\" : \"Open module navigation\")\"", "module rail toggle title expression");
        AssertContains(mainLayout, MainLayoutRelativePath, "aria-label=\"Close module navigation\"", "module rail close button accessibility label");
        AssertContains(mainLayout, MainLayoutRelativePath, "title=\"Close module navigation\"", "module rail close button title");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string anchorDescription)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing shell composition anchor '{anchorDescription}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string anchorDescription)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale shell anchor '{anchorDescription}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required shell contract file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read shell contract file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "host", "Ams.sln"))
                && Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing host/Ams.sln.");
    }
}
