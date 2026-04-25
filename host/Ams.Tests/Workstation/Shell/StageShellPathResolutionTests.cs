using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellPathResolutionTests
{
    private const string MainLayoutRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor";
    private const string MainLayoutCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor.css";
    private const string StageModuleRailRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor";
    private const string StageModuleRailCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor.css";

    [Theory]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/pickups", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups)]
    [InlineData("/proof/overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview)]
    [InlineData("/proof/patterns", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns)]
    [InlineData("/proof/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/editing/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/polish", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/scaffold", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/pickups", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/batch", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    public void AssertRouteContract_CompatibilityAndCanonicalPaths_ResolveExpectedStageModule(
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        var match = AssertRouteContract(path, expectedStageId, expectedModuleId);

        Assert.True(
            string.Equals(match.Stage.Id, expectedStageId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Unexpected route contract for path '{path}'. Expected stage='{expectedStageId}', module='{expectedModuleId}'. Actual: {match.DiagnosticContext}.");
    }

    [Fact]
    public void AssertRouteContract_DiagnosticsIncludePathAndResolvedContext_OnMismatch()
    {
        var exception = Assert.ThrowsAny<XunitException>(() =>
            AssertRouteContract(
                path: "/proof/overview",
                expectedStageId: StageRouteCatalog.StageIds.Proof,
                expectedModuleId: StageRouteCatalog.ModuleIds.ProofEditing));

        Assert.Contains("/proof/overview", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Expected stage='proof'", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("module='proof-editing'", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Resolved stage='proof'", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("module='proof-overview'", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AssertRouteContract_DiagnosticsIncludePathAndResolvedPlaceholders_OnUnresolvedPath()
    {
        var exception = Assert.ThrowsAny<XunitException>(() =>
            AssertRouteContract(
                path: "proof/overview",
                expectedStageId: StageRouteCatalog.StageIds.Proof,
                expectedModuleId: StageRouteCatalog.ModuleIds.ProofOverview));

        Assert.Contains("proof/overview", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Resolved stage='(none)'", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("module='(none)'", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/PrOoF/Overview?sort=asc#top", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview)]
    [InlineData("/PrOoF/PiCkUpS?sort=asc#top", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups)]
    [InlineData("/proof//", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/pickups/", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups)]
    [InlineData("/proof/editing//", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/%2F", true, StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/polish//", true, StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/batch/", true, StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("", false, "", "")]
    [InlineData("   ", false, "", "")]
    [InlineData("proof/overview", false, "", "")]
    [InlineData("/unknown/path", false, "", "")]
    [InlineData("/polish/unknown", false, "", "")]
    [InlineData("/polish/legacy/pickups", false, "", "")]
    [InlineData("/polish/legacy/batch", false, "", "")]
    public void ResolveStateForPath_NormalizesMalformedInputs_WithoutThrowing(
        string path,
        bool expectedVisible,
        string expectedStageId,
        string expectedModuleId)
    {
        StageModuleRail.StageModuleRailState? state = null;
        var exception = Record.Exception(() => state = StageModuleRail.ResolveStateForPath(path));

        Assert.Null(exception);
        Assert.NotNull(state);

        Assert.True(
            state!.IsVisible == expectedVisible,
            $"Unexpected shell visibility for path '{path}'. Expected visible='{expectedVisible}', resolved visible='{state.IsVisible}', stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', warning='{state.WarningMessage}'.");

        if (expectedVisible)
        {
            Assert.True(
                string.Equals(state.ActiveStageId, expectedStageId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(state.ActiveModuleId, expectedModuleId, StringComparison.OrdinalIgnoreCase),
                $"Normalized path '{path}' resolved to unexpected stage/module. Expected stage='{expectedStageId}', module='{expectedModuleId}'. Resolved stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}'.");
        }
        else
        {
            var expectedPathFragment = string.IsNullOrWhiteSpace(path) ? StageRouteCatalog.RootPath : path.Trim();

            Assert.True(
                state.WarningMessage.Contains(expectedPathFragment, StringComparison.OrdinalIgnoreCase)
                || state.WarningMessage.Contains(StageRouteCatalog.RootPath, StringComparison.OrdinalIgnoreCase),
                $"Hidden-state warning for path '{path}' should include normalized path context. Warning='{state.WarningMessage}'.");
        }
    }

    [Fact]
    public void MobileModuleRailContract_ToggleDiagnosticsAndHideByDefaultAnchors_ArePresent()
    {
        var layout = ReadRepoFile(MainLayoutRelativePath);
        var layoutCss = ReadRepoFile(MainLayoutCssRelativePath);
        var moduleRail = ReadRepoFile(StageModuleRailRelativePath);
        var moduleRailCss = ReadRepoFile(StageModuleRailCssRelativePath);

        AssertSourceContains(layout, MainLayoutRelativePath, "data-ams-mobile-module-rail-state=", "layout-level rail open/closed diagnostic marker");
        AssertSourceContains(layout, MainLayoutRelativePath, "data-ams-header-control=\"module-rail-toggle\"", "mobile module-rail toggle anchor");
        AssertSourceContains(layout, MainLayoutRelativePath, "data-ams-mobile-module-rail-open=", "mobile module-rail toggle-state marker");
        AssertSourceContains(layout, MainLayoutRelativePath, "data-ams-mobile-module-rail=", "sidebar rail state marker");
        AssertSourceContains(layout, MainLayoutRelativePath, "data-ams-mobile-module-rail-overlay=\"visible\"", "drawer overlay diagnostic marker");
        AssertSourceContains(layout, MainLayoutRelativePath, "<div class=\"workstation-module-rail-overlay\"", "drawer overlay uses non-focusable container");
        AssertSourceContains(layout, MainLayoutRelativePath, "role=\"presentation\"", "drawer overlay presentational semantics");
        AssertSourceContains(layout, MainLayoutRelativePath, "aria-hidden=\"true\"", "drawer overlay hidden from accessibility tree");
        AssertSourceContains(layout, MainLayoutRelativePath, "role=\"@(isMobileModuleRailOpen ? \"dialog\" : null)\"", "drawer dialog semantics");
        AssertSourceContains(layout, MainLayoutRelativePath, "aria-modal=\"@(isMobileModuleRailOpen ? \"true\" : null)\"", "drawer modal semantics");
        AssertSourceContains(layout, MainLayoutRelativePath, "@onkeydown=\"HandleMobileModuleRailKeyDown\"", "drawer keydown handler");
        AssertSourceContains(layout, MainLayoutRelativePath, "if (string.Equals(e.Key, \"Escape\"", "escape key close handling");
        AssertSourceContains(layout, MainLayoutRelativePath, "focusMobileModuleRailPanel", "panel focus handoff flag");
        AssertSourceContains(layout, MainLayoutRelativePath, "await mobileModuleRailPanelRef.FocusAsync();", "panel focus handoff call");
        AssertSourceContains(layout, MainLayoutRelativePath, "inert=\"@(isMobileModuleRailOpen ? \"inert\" : null)\"", "background inert toggle while drawer open");
        AssertSourceContains(layout, MainLayoutRelativePath, "HandleLocationChanged", "route-change handler used to auto-close mobile drawer");
        AssertSourceContains(layout, MainLayoutRelativePath, "isMobileModuleRailOpen = false;", "mobile drawer close assignment");

        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, "@media (max-width: 768px)", "mobile breakpoint for rail drawer behavior");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-toggle", "mobile module-rail toggle style block");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, ".workstation-sidebar.workstation-sidebar--mobile-open", "mobile-open sidebar selector");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, "transform: translateX(-106%);", "default hidden mobile drawer transform");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, ".workstation-sidebar:focus", "mobile drawer focus outline style");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-panel-header", "mobile drawer panel header style");
        AssertSourceContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-overlay", "mobile drawer overlay style block");

        AssertSourceContains(moduleRail, StageModuleRailRelativePath, "data-ams-mobile-module-rail-contract=\"collapsible\"", "collapsible module-rail contract marker");
        AssertSourceContains(moduleRail, StageModuleRailRelativePath, "data-ams-mobile-module-rail-link=\"module\"", "module-selection anchor");
        AssertSourceContains(moduleRail, StageModuleRailRelativePath, "public EventCallback OnModuleInvoked", "module-invoked callback contract");
        AssertSourceContains(moduleRail, StageModuleRailRelativePath, "OnClick=\"HandleModuleInvoked\"", "module click callback hook");
        AssertSourceContains(moduleRailCss, StageModuleRailCssRelativePath, "min-height: 44px;", "touch-target minimum size");
    }

    private static StageRouteMatch AssertRouteContract(
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        var match = StageRouteCatalog.Resolve(path);

        var resolvedStageId = match?.Stage.Id ?? "(none)";
        var resolvedModuleId = match?.Module.Id ?? "(none)";
        var resolvedTemplate = match?.MatchedTemplate ?? "(none)";
        var resolvedNormalizedPath = match?.NormalizedPath ?? "(none)";
        var resolvedCompatibility = match?.IsCompatibilityAlias.ToString() ?? "(none)";

        Assert.True(
            match is not null
            && string.Equals(match.Stage.Id, expectedStageId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Path resolution contract failure for path '{path}'. Expected stage='{expectedStageId}', module='{expectedModuleId}'. Resolved stage='{resolvedStageId}', module='{resolvedModuleId}', normalized='{resolvedNormalizedPath}', template='{resolvedTemplate}', compatibility='{resolvedCompatibility}'.");

        return match!;
    }

    private static void AssertSourceContains(string source, string relativePath, string anchor, string anchorDescription)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing shell/mobile anchor '{anchorDescription}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        Assert.True(
            File.Exists(fullPath),
            $"Required shell/mobile contract file is missing: relative='{relativePath}', full='{fullPath}'.");

        return File.ReadAllText(fullPath);
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
