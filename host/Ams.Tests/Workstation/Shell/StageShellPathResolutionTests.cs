using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellPathResolutionTests
{
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
}
