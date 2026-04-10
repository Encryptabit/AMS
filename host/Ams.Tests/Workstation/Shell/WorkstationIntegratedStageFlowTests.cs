using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;

namespace Ams.Tests.Workstation.Shell;

public sealed class WorkstationIntegratedStageFlowTests
{
    [Fact]
    public void ResolveStateForPath_PrepProofPolishTraversal_RemainsVisibleWithDeterministicMarkers()
    {
        var chapterDisplayTitle = "Chapter 01 - Integrated Flow";

        var traversal = new[]
        {
            (
                Path: StageRouteCatalog.GetModuleCanonicalPath(
                    StageRouteCatalog.StageIds.Prep,
                    StageRouteCatalog.ModuleIds.PrepPipeline),
                StageId: StageRouteCatalog.StageIds.Prep,
                ModuleId: StageRouteCatalog.ModuleIds.PrepPipeline),
            (
                Path: StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterDisplayTitle),
                StageId: StageRouteCatalog.StageIds.Proof,
                ModuleId: StageRouteCatalog.ModuleIds.ProofEditing),
            (
                Path: StageRouteCatalog.BuildProofChapterCanonicalPath(chapterDisplayTitle),
                StageId: StageRouteCatalog.StageIds.Proof,
                ModuleId: StageRouteCatalog.ModuleIds.ProofEditing),
            (
                Path: "/polish",
                StageId: StageRouteCatalog.StageIds.Polish,
                ModuleId: StageRouteCatalog.ModuleIds.PolishScaffold),
            (
                Path: "/polish/scaffold",
                StageId: StageRouteCatalog.StageIds.Polish,
                ModuleId: StageRouteCatalog.ModuleIds.PolishScaffold),
            (
                Path: "/polish/pickups",
                StageId: StageRouteCatalog.StageIds.Polish,
                ModuleId: StageRouteCatalog.ModuleIds.PolishScaffold),
            (
                Path: "/polish/batch",
                StageId: StageRouteCatalog.StageIds.Polish,
                ModuleId: StageRouteCatalog.ModuleIds.PolishScaffold)
        };

        foreach (var step in traversal)
        {
            var state = StageModuleRail.ResolveStateForPath(step.Path);
            AssertVisibleState(state, step.Path, step.StageId, step.ModuleId);
        }
    }

    [Theory]
    [InlineData("/polish", "/polish", true)]
    [InlineData("/polish/pickups", "/polish/pickups", true)]
    [InlineData("/polish/batch", "/polish/batch", true)]
    [InlineData("/polish/scaffold", "/polish/scaffold", false)]
    public void Resolve_PolishScaffoldAliases_EmitExpectedRouteDiagnostics(
        string path,
        string expectedTemplate,
        bool expectedCompatibilityAlias)
    {
        var routeMatch = StageRouteCatalog.Resolve(path);

        Assert.True(
            routeMatch is not null,
            $"Expected polish scaffold path '{path}' to resolve, but no match was returned.");

        Assert.True(
            string.Equals(routeMatch!.Stage.Id, StageRouteCatalog.StageIds.Polish, StringComparison.OrdinalIgnoreCase)
            && string.Equals(routeMatch.Module.Id, StageRouteCatalog.ModuleIds.PolishScaffold, StringComparison.OrdinalIgnoreCase)
            && string.Equals(routeMatch.MatchedTemplate, expectedTemplate, StringComparison.OrdinalIgnoreCase)
            && routeMatch.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Unexpected polish scaffold route diagnostics for path '{path}'. {routeMatch.DiagnosticContext}");

        Assert.Contains($"template='{expectedTemplate}'", routeMatch.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"compatibility='{expectedCompatibilityAlias}'", routeMatch.DiagnosticContext, StringComparison.OrdinalIgnoreCase);

        var railState = StageModuleRail.ResolveStateForPath(path);
        AssertVisibleState(
            railState,
            path,
            StageRouteCatalog.StageIds.Polish,
            StageRouteCatalog.ModuleIds.PolishScaffold);
    }

    [Fact]
    public void ResolveStateForPath_LegacyPolishRoutesStayOutOfShellContract_WithoutBreakingProofDeepLinks()
    {
        var proofPath = StageRouteCatalog.BuildProofChapterCompatibilityPath("Chapter 02 / Review");
        var proofState = StageModuleRail.ResolveStateForPath(proofPath);
        AssertVisibleState(
            proofState,
            proofPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);

        var legacyPolishPaths = new[]
        {
            "/polish/legacy/pickups",
            "/polish/legacy/batch"
        };

        foreach (var legacyPath in legacyPolishPaths)
        {
            var legacyState = StageModuleRail.ResolveStateForPath(legacyPath);

            Assert.False(
                legacyState.IsVisible,
                $"Expected legacy polish route '{legacyPath}' to remain outside shell navigation, but state was visible.");

            Assert.Equal("none", legacyState.DiagnosticMarker);
            Assert.Contains(legacyPath, legacyState.WarningMessage, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertVisibleState(
        StageModuleRail.StageModuleRailState state,
        string path,
        string expectedStageId,
        string expectedModuleId)
    {
        Assert.True(
            state.IsVisible,
            $"Expected visible stage shell for path '{path}', but received hidden state with warning '{state.WarningMessage}'.");

        Assert.Equal(expectedStageId, state.ActiveStageId, ignoreCase: true);
        Assert.Equal(expectedModuleId, state.ActiveModuleId, ignoreCase: true);
        Assert.Equal($"{expectedStageId}:{expectedModuleId}", state.DiagnosticMarker, ignoreCase: true);

        var activeModules = state.Modules.Where(module => module.IsActive).ToArray();
        Assert.Single(activeModules);
        Assert.Equal(expectedModuleId, activeModules[0].Id, ignoreCase: true);
    }
}
