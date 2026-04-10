using System.Reflection;
using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Microsoft.AspNetCore.Components;
using PolishIndexPage = Ams.Workstation.Server.Components.Pages.Polish.Index;
using PolishLegacyBatchPage = Ams.Workstation.Server.Components.Pages.Polish.BatchEditor;
using PolishLegacyPickupsPage = Ams.Workstation.Server.Components.Pages.Polish.PickupSubstitution;
using PrepIndexPage = Ams.Workstation.Server.Components.Pages.Prep.Index;
using ProofIndexPage = Ams.Workstation.Server.Components.Pages.Proof.Index;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellLayoutTests
{
    [Theory]
    [InlineData("/prep", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/prep/", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/prep/pipeline", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline)]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
    [InlineData("/proof/editing", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing)]
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
    public void StageEntryPages_DeclareCanonicalAliasesWithoutRemovingLegacyRoutes()
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
            typeof(PolishIndexPage),
            "/polish",
            "/polish/scaffold",
            "/polish/pickups",
            "/polish/batch");

        AssertRoutes(
            typeof(PolishLegacyPickupsPage),
            "/polish/legacy/pickups");

        AssertRoutes(
            typeof(PolishLegacyBatchPage),
            "/polish/legacy/batch");
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

    private static string DescribeState(StageModuleRail.StageModuleRailState state)
        => $"visible='{state.IsVisible}', path='{state.NormalizedPath}', stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}', warning='{state.WarningMessage}'";
}
