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
        AssertContains(source, MainLayoutRelativePath, "<aside class=\"workstation-sidebar\"", "left sidebar shell host");
        AssertContains(source, MainLayoutRelativePath, "<StageModuleRail />", "sidebar module rail host");
        AssertContains(source, MainLayoutRelativePath, "<SectionOutlet SectionName=\"module-inspector\" />", "optional inspector outlet host");

        AssertDoesNotContain(source, MainLayoutRelativePath, "<div class=\"workstation-shell-rail\">", "stale horizontal rail-only shell wrapper");
    }

    [Fact]
    public void StageModuleRail_DeclaresStageAndModuleNavigationAnchors()
    {
        var source = ReadRepoFile(StageModuleRailRelativePath);

        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-nav=\"stages\"", "explicit stage navigation section");
        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-nav=\"modules\"", "explicit module navigation section");
        AssertContains(source, StageModuleRailRelativePath, "StageRouteCatalog.Stages", "catalog-driven stage iteration");
        AssertContains(source, StageModuleRailRelativePath, "stage-module-rail__stage-link", "stage link style hook");
        AssertContains(source, StageModuleRailRelativePath, "stage-module-rail__module-link", "module link style hook");
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
    public void StageModuleRail_PreservesFailClosedFallbackAnchorsForResolveFailures()
    {
        var source = ReadRepoFile(StageModuleRailRelativePath);

        AssertContains(source, StageModuleRailRelativePath, "StageModuleRailState.Hidden(", "hidden-state fallback constructor");
        AssertContains(source, StageModuleRailRelativePath, "No stage-module rail mapping for path", "unknown route fallback warning");
        AssertContains(source, StageModuleRailRelativePath, "Route resolved without active module for", "resolved-without-active-module fallback warning");
        AssertContains(source, StageModuleRailRelativePath, "data-stage-shell-warning=\"@_state.WarningMessage\"", "hidden-state warning marker");
    }

    [Fact]
    public void StageModuleRailStyles_PreserveNarrowViewportOverflowGuardrails()
    {
        var railCss = ReadRepoFile(StageModuleRailCssRelativePath);

        AssertContains(railCss, StageModuleRailCssRelativePath, "@media (max-width: 960px)", "narrow viewport rail media query");
        AssertContains(railCss, StageModuleRailCssRelativePath, "overflow-x: auto", "horizontal rail overflow behavior");
        AssertContains(railCss, StageModuleRailCssRelativePath, "min-width: 11rem", "minimum section width to avoid clipped labels");
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
