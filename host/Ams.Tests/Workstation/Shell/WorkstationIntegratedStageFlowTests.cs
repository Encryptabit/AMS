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
                Path: "/proof/pickups",
                StageId: StageRouteCatalog.StageIds.Proof,
                ModuleId: StageRouteCatalog.ModuleIds.ProofPickups),
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

    [Theory]
    [InlineData("/proof/pickups", "/proof/pickups", true)]
    [InlineData("/proof/pickups/", "/proof/pickups", true)]
    [InlineData("/PrOoF/PiCkUpS?source=legacy#now", "/proof/pickups", true)]
    public void Resolve_ProofPickupsPaths_EmitExpectedRouteDiagnostics(
        string path,
        string expectedTemplate,
        bool expectedCompatibilityAlias)
    {
        var routeMatch = StageRouteCatalog.Resolve(path);

        Assert.True(
            routeMatch is not null,
            $"Expected proof pickups path '{path}' to resolve, but no match was returned.");

        Assert.True(
            string.Equals(routeMatch!.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(routeMatch.Module.Id, StageRouteCatalog.ModuleIds.ProofPickups, StringComparison.OrdinalIgnoreCase)
            && string.Equals(routeMatch.MatchedTemplate, expectedTemplate, StringComparison.OrdinalIgnoreCase)
            && routeMatch.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Unexpected proof pickups route diagnostics for path '{path}'. {routeMatch.DiagnosticContext}");

        Assert.Contains($"template='{expectedTemplate}'", routeMatch.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"compatibility='{expectedCompatibilityAlias}'", routeMatch.DiagnosticContext, StringComparison.OrdinalIgnoreCase);

        var railState = StageModuleRail.ResolveStateForPath(path);
        AssertVisibleState(
            railState,
            path,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);
    }

    [Fact]
    public void ResolveStateForPath_ProofPickupsOwnership_RemainsProofModuleContract()
    {
        const string path = "/proof/pickups";

        var state = StageModuleRail.ResolveStateForPath(path);
        AssertVisibleState(
            state,
            path,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);

        var match = StageRouteCatalog.Resolve(path);
        Assert.True(match is not null, "Expected '/proof/pickups' to resolve after lifecycle/ledger UI expansion.");
        Assert.Equal(StageRouteCatalog.StageIds.Proof, match!.Stage.Id, ignoreCase: true);
        Assert.Equal(StageRouteCatalog.ModuleIds.ProofPickups, match.Module.Id, ignoreCase: true);
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

    [Theory]
    [InlineData("Chapter 09 - Round Trip")]
    [InlineData("Chapter 03 / Finale")]
    [InlineData("Chapter 02: Punctuation!?,;")]
    public void ResolveStateForPath_EditingPickupsEditingRoundTrip_PreservesChapterContinuity(string chapterName)
    {
        var editingEntryPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);
        var pickupsPath = StageRouteCatalog.GetProofPickupsHandoffPath();
        var editingReturnPath = ComposeProofEditingRoundTripPath(chapterName);

        var entryMatch = StageRouteCatalog.Resolve(editingEntryPath);
        Assert.True(
            entryMatch is not null
            && string.Equals(entryMatch.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(entryMatch.Module.Id, StageRouteCatalog.ModuleIds.ProofEditing, StringComparison.OrdinalIgnoreCase),
            $"Expected editing entry path '{editingEntryPath}' to resolve to proof-editing. Diagnostics: {entryMatch?.DiagnosticContext ?? "(none)"}.");

        AssertVisibleState(
            StageModuleRail.ResolveStateForPath(editingEntryPath),
            editingEntryPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);

        AssertVisibleState(
            StageModuleRail.ResolveStateForPath(pickupsPath),
            pickupsPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);

        AssertVisibleState(
            StageModuleRail.ResolveStateForPath(editingReturnPath),
            editingReturnPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);

        Assert.Equal(editingEntryPath, editingReturnPath);

        var returnMatch = StageRouteCatalog.Resolve(editingReturnPath);
        Assert.True(
            returnMatch is not null
            && string.Equals(returnMatch.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(returnMatch.Module.Id, StageRouteCatalog.ModuleIds.ProofEditing, StringComparison.OrdinalIgnoreCase),
            $"Expected editing return path '{editingReturnPath}' to resolve to proof-editing. Diagnostics: {returnMatch?.DiagnosticContext ?? "(none)"}.");
    }

    [Theory]
    [InlineData("overview", StageRouteCatalog.ModuleIds.ProofOverview)]
    [InlineData("patterns", StageRouteCatalog.ModuleIds.ProofPatterns)]
    [InlineData("pickups", StageRouteCatalog.ModuleIds.ProofPickups)]
    public void ResolveStateForPath_EditingRoundTripFallback_ReservedSlugsRouteToOwningModule(
        string chapterName,
        string expectedModuleId)
    {
        // The /proof/editing/{chapter} prefix that previously disambiguated
        // reserved chapter slugs has been retired. Round-tripping a reserved
        // slug now lands on the matching module (overview, patterns, pickups)
        // instead of falling back to the editing module.
        var editingReturnPath = ComposeProofEditingRoundTripPath(chapterName);
        var expectedCanonicalChapterPath = StageRouteCatalog.BuildProofChapterCanonicalPath(chapterName);

        Assert.Equal(expectedCanonicalChapterPath, editingReturnPath);

        var pickupsPath = StageRouteCatalog.GetProofPickupsHandoffPath();
        AssertVisibleState(
            StageModuleRail.ResolveStateForPath(pickupsPath),
            pickupsPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);

        AssertVisibleState(
            StageModuleRail.ResolveStateForPath(editingReturnPath),
            editingReturnPath,
            StageRouteCatalog.StageIds.Proof,
            expectedModuleId);

        var returnMatch = StageRouteCatalog.Resolve(editingReturnPath);
        Assert.True(
            returnMatch is not null
            && string.Equals(returnMatch.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(returnMatch.Module.Id, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Expected reserved slug return path '{editingReturnPath}' to resolve to module '{expectedModuleId}'. Diagnostics: {returnMatch?.DiagnosticContext ?? "(none)"}.");
    }

    private static string ComposeProofEditingRoundTripPath(string? activeChapterName)
    {
        var handoffPath = StageRouteCatalog.GetProofEditingHandoffPath(activeChapterName);

        if (string.IsNullOrWhiteSpace(activeChapterName))
        {
            return handoffPath;
        }

        var handoffMatch = StageRouteCatalog.Resolve(handoffPath);
        var resolvesToEditing = handoffMatch is not null
            && string.Equals(handoffMatch.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(handoffMatch.Module.Id, StageRouteCatalog.ModuleIds.ProofEditing, StringComparison.OrdinalIgnoreCase);

        return resolvesToEditing
            ? handoffPath
            : StageRouteCatalog.BuildProofChapterCanonicalPath(activeChapterName);
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
