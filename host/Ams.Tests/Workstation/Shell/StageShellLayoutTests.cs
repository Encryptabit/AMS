using System.Reflection;
using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Microsoft.AspNetCore.Components;
using PolishIndexPage = Ams.Workstation.Server.Components.Pages.Polish.Index;
using PrepIndexPage = Ams.Workstation.Server.Components.Pages.Prep.Index;
using ProofIndexPage = Ams.Workstation.Server.Components.Pages.Proof.Index;
using ProofPickupsPage = Ams.Workstation.Server.Components.Pages.Proof.Pickups;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellLayoutTests
{
    private const string HeaderControlsRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor";
    private const string HeaderControlsCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor.css";
    [Theory]
    [InlineData("/prep", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/prep/", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/prep/pipeline", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/editing", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/pickups", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups)]
    [InlineData("/proof/overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview)]
    [InlineData("/proof/patterns", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns)]
    [InlineData("/proof/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/polish", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/scaffold", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/pickups", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    [InlineData("/polish/batch", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)]
    public void ResolveStateForPath_RepresentativeStagePaths_ShowVisibleRailWithActiveModule(
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        var state = StageModuleRail.ResolveStateForPath(path);

        AssertVisibleState(state, path, expectedStageId, expectedModuleId);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/not-found")]
    [InlineData("/api/audio/chapter")]
    [InlineData("/polish/legacy/pickups")]
    [InlineData("/polish/legacy/batch")]
    public void ResolveStateForPath_NonStagePaths_ReturnHiddenFallbackState(string path)
    {
        var state = StageModuleRail.ResolveStateForPath(path);

        Assert.True(
            state.IsVisible is false,
            $"Expected hidden shell rail for path '{path}', but received {DescribeState(state)}.");

        Assert.True(
            string.Equals(state.DiagnosticMarker, "none", StringComparison.Ordinal),
            $"Expected hidden shell diagnostic marker 'none' for path '{path}', but received {DescribeState(state)}.");

        Assert.True(
            state.WarningMessage.Contains(path, StringComparison.OrdinalIgnoreCase),
            $"Hidden shell fallback should mention failing path '{path}', but warning was '{state.WarningMessage}'. Context: {DescribeState(state)}.");
    }

    [Fact]
    public void ResolveStateForPath_UnknownProofSlug_MapsToCompatibilityModuleWithoutThrowing()
    {
        const string path = "/proof/unknown-module";

        var state = StageModuleRail.ResolveStateForPath(path);

        AssertVisibleState(
            state,
            path,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);
    }

    [Fact]
    public void ResolveStateForPath_RapidDeepLinkSwitching_KeepsStageAndModuleSynchronized()
    {
        var expectedByPath = new Dictionary<string, (string StageId, string ModuleId)>
        {
            ["/proof/overview"] = (StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview),
            ["/proof/pickups"] = (StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups),
            ["/polish/batch"] = (StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold),
            ["/prep/pipeline"] = (StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline),
            ["/proof/patterns"] = (StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns),
            ["/polish/pickups"] = (StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold),
            ["/polish/scaffold"] = (StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold)
        };

        for (var iteration = 0; iteration < 25; iteration++)
        {
            foreach (var (path, expected) in expectedByPath)
            {
                var state = StageModuleRail.ResolveStateForPath(path);
                AssertVisibleState(state, path, expected.StageId, expected.ModuleId);
            }
        }
    }

    [Fact]
    public void StageEntryPages_DeclareCanonicalAliasesAndExcludeLegacyPolishPickupRoutes()
    {
        AssertRoutes(
            typeof(PrepIndexPage),
            "/prep",
            "/prep/pipeline");

        AssertRoutes(
            typeof(ProofIndexPage),
            "/proof",
            "/proof/editing");

        AssertRoutes(
            typeof(ProofPickupsPage),
            "/proof/pickups");

        AssertRoutes(
            typeof(PolishIndexPage),
            "/polish",
            "/polish/scaffold",
            "/polish/pickups",
            "/polish/batch");

        var polishDeclaredTemplates = typeof(PolishIndexPage)
            .GetCustomAttributes<RouteAttribute>(inherit: true)
            .Select(route => route.Template)
            .ToArray();

        Assert.DoesNotContain(
            polishDeclaredTemplates,
            template => string.Equals(template, "/polish/legacy/pickups", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            polishDeclaredTemplates,
            template => string.Equals(template, "/polish/legacy/batch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ResolveStateForPath_PolishScaffoldDoesNotAdvertiseBatchingModules()
    {
        var state = StageModuleRail.ResolveStateForPath("/polish/batch");
        AssertVisibleState(
            state,
            "/polish/batch",
            StageRouteCatalog.StageIds.Polish,
            StageRouteCatalog.ModuleIds.PolishScaffold);

        var modulesWithBatching = state.Modules
            .Where(module => module.SupportsBatching)
            .Select(module => module.Id)
            .ToArray();

        Assert.True(
            modulesWithBatching.Length == 0,
            $"Expected no shell-visible batching modules for polish scaffold, but found: {string.Join(", ", modulesWithBatching)}. Context: {DescribeState(state)}.");
    }

    [Fact]
    public void HeaderControls_MobileContract_KeepChapterWorkspaceReachableAndExposeSecondaryControlsViaOverflow()
    {
        var source = ReadRepoFile(HeaderControlsRelativePath);
        var css = ReadRepoFile(HeaderControlsCssRelativePath);

        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-header-control=\"chapter\"", "chapter selector anchor");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-header-control=\"workspace\"", "workspace selector anchor");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-mobile-overflow-state=", "mobile overflow diagnostic state marker");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-mobile-overflow-open=", "mobile overflow trigger diagnostic marker");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-header-control=\"mobile-overflow-trigger\"", "mobile overflow trigger anchor");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-header-control=\"mobile-overflow-panel\"", "mobile overflow panel anchor");
        AssertSourceContains(source, HeaderControlsRelativePath, "data-ams-header-optional=\"mobile-overflow-hidden\"", "mobile-hide optional controls anchor");

        AssertSourceContains(css, HeaderControlsCssRelativePath, "@media (max-width: 768px)", "mobile breakpoint rule");
        AssertSourceContains(css, HeaderControlsCssRelativePath, ".header-field--workspace", "workspace mobile style block");
        AssertSourceContains(css, HeaderControlsCssRelativePath, "flex: 1 1 100%;", "workspace full-width mobile reachability");
        AssertSourceContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-trigger", "mobile overflow trigger style block");
        AssertSourceContains(css, HeaderControlsCssRelativePath, "[data-ams-mobile-overflow-open=\"true\"]", "mobile overflow trigger-open style selector");
        AssertSourceContains(css, HeaderControlsCssRelativePath, ".header-mobile-overflow-overlay", "mobile overflow overlay style block");
        AssertSourceContains(css, HeaderControlsCssRelativePath, ".header-secondary-optional", "secondary controls container selector");
        AssertSourceContains(css, HeaderControlsCssRelativePath, "display: none !important;", "mobile hide rule for inline secondary controls");
    }

    private static void AssertVisibleState(
        StageModuleRail.StageModuleRailState state,
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        Assert.True(
            state.IsVisible,
            $"Expected visible shell rail for path '{path}', but received {DescribeState(state)}.");

        Assert.True(
            string.Equals(state.ActiveStageId, expectedStageId, StringComparison.OrdinalIgnoreCase),
            $"Stage mismatch for path '{path}'. Expected stage='{expectedStageId}', actual stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}'.");

        Assert.True(
            string.Equals(state.ActiveModuleId, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Module mismatch for path '{path}'. Expected module='{expectedModuleId}', actual stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}'.");

        var activeModules = state.Modules.Where(module => module.IsActive).ToArray();
        Assert.True(
            activeModules.Length == 1,
            $"Expected exactly one active module for path '{path}', but found {activeModules.Length}. Modules: {string.Join(", ", state.Modules.Select(module => $"{module.Id}:{module.IsActive}"))}. Context: {DescribeState(state)}.");

        Assert.True(
            string.Equals(activeModules[0].Id, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Active module mismatch for path '{path}'. Expected '{expectedModuleId}', active '{activeModules[0].Id}'. Context: {DescribeState(state)}.");

        var expectedMarker = $"{expectedStageId}:{expectedModuleId}";
        Assert.True(
            string.Equals(state.DiagnosticMarker, expectedMarker, StringComparison.OrdinalIgnoreCase),
            $"Diagnostic marker mismatch for path '{path}'. Expected '{expectedMarker}', actual '{state.DiagnosticMarker}'. Context: {DescribeState(state)}.");
    }

    private static void AssertRoutes(Type componentType, params string[] expectedTemplates)
    {
        var declaredTemplates = componentType
            .GetCustomAttributes<RouteAttribute>(inherit: true)
            .Select(route => route.Template)
            .ToArray();

        foreach (var expectedTemplate in expectedTemplates)
        {
            Assert.True(
                declaredTemplates.Contains(expectedTemplate, StringComparer.OrdinalIgnoreCase),
                $"Missing route template '{expectedTemplate}' on component '{componentType.FullName}'. Declared templates: {string.Join(", ", declaredTemplates)}.");
        }
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

    private static string DescribeState(StageModuleRail.StageModuleRailState state)
        => $"visible='{state.IsVisible}', path='{state.NormalizedPath}', stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}', warning='{state.WarningMessage}'";
}
