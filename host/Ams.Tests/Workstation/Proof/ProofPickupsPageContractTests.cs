using Ams.Workstation.Server.Components.Navigation;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofPickupsPageContractTests
{
    private const string PickupsPageRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor";
    private const string ProofIndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor";

    [Fact]
    public void PickupsPage_DeclaresProofRouteAndQueueDiagnosticsAnchors()
    {
        var source = ReadRepoFile(PickupsPageRelativePath);

        AssertContains(source, PickupsPageRelativePath, "@page \"/proof/pickups\"", "proof pickups route");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-page=\"true\"", "page diagnostics marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-handoff=\"editing\"", "pickups-to-editing handoff marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-action=\"return-editing\"", "return-to-editing action marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"matched\"", "matched queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"unmatched\"", "unmatched queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"staged\"", "staged queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"applied\"", "applied queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"reverted\"", "reverted queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-queue=\"failed\"", "failed queue marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-diagnostics=\"true\"", "diagnostics panel marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-ledger=\"true\"", "artifact ledger panel marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-ledger-row=\"true\"", "artifact ledger row marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-action=\"commit\"", "commit action marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-action=\"revert\"", "revert action marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-phase=\"true\"", "phase marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pickups-last-op=\"true\"", "op marker");

        Assert.DoesNotContain("/polish/legacy/pickups", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-polish-pickups", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PickupsPage_ExposesBatchPickMapControlsAndDiagnostics()
    {
        var source = ReadRepoFile(PickupsPageRelativePath);

        AssertContains(source, PickupsPageRelativePath, "batch Pick", "batch Pick wording");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-map=\"true\"", "Pick map panel marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-chapter-group=\"true\"", "Pick chapter group marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-row=\"true\"", "Pick assignment row marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-status=\"true\"", "Pick assignment status marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-manual-target=\"true\"", "Pick manual target control marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-save=\"true\"", "Pick canonical save marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-revision=\"true\"", "Pick revision diagnostic marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-pick-read-error=\"true\"", "Pick read error marker");
        AssertContains(source, PickupsPageRelativePath, "Import Batch Pick", "batch import action copy");
        AssertContains(source, PickupsPageRelativePath, "Save Canonical Pick Map", "canonical save action copy");
        AssertContains(source, PickupsPageRelativePath, "SetPickAssignmentTargetAsync", "manual target session binding");
        AssertContains(source, PickupsPageRelativePath, "SetPickAssignmentDispositionAsync", "manual disposition session binding");
        AssertContains(source, PickupsPageRelativePath, "ConfirmPickMapAsync", "canonical save session binding");

        Assert.DoesNotContain("Import + Match", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/polish/legacy/", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PickupsPage_ExposesChapterFitControlsAndDiagnostics()
    {
        var source = ReadRepoFile(PickupsPageRelativePath);

        AssertContains(source, PickupsPageRelativePath, "Chapter Fit", "chapter Fit panel copy");
        AssertContains(source, PickupsPageRelativePath, "Load Chapter Fit", "chapter Fit load action copy");
        AssertContains(source, PickupsPageRelativePath, "Preview Fit", "chapter Fit preview action copy");
        AssertContains(source, PickupsPageRelativePath, "Accept Preview", "chapter Fit accept action copy");
        AssertContains(source, PickupsPageRelativePath, "Commit Fit", "chapter Fit commit action copy");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-plan=\"true\"", "Fit plan panel marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-row=\"true\"", "Fit row marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-status=\"true\"", "Fit status marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-outer-bounds=\"true\"", "Fit outer bounds marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-inner-bounds=\"true\"", "Fit inner bounds marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-placement=\"true\"", "Fit placement marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-roomtone-policy=\"true\"", "Fit roomtone policy marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-crossfade=\"true\"", "Fit crossfade marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-preview=\"true\"", "Fit preview marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-accept=\"true\"", "Fit accept marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-commit=\"true\"", "Fit commit marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-read-error=\"true\"", "Fit read error marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-validation-error=\"true\"", "Fit validation error marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-preview-version=\"true\"", "Fit preview version marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-commit-ready=\"true\"", "Fit commit-ready marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-commit-result=\"true\"", "Fit commit result marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-last-op=\"true\"", "Fit last operation marker");
        AssertContains(source, PickupsPageRelativePath, "data-proof-fit-status-counts=\"true\"", "Fit status count marker");
        AssertContains(source, PickupsPageRelativePath, "LoadOrCreateFitPlanAsync", "Fit load/create session binding");
        AssertContains(source, PickupsPageRelativePath, "SetFitOuterBoundaryAsync", "Fit outer boundary session binding");
        AssertContains(source, PickupsPageRelativePath, "SetFitInnerBoundaryAsync", "Fit inner boundary session binding");
        AssertContains(source, PickupsPageRelativePath, "SetFitPolicyAsync", "Fit policy session binding");
        AssertContains(source, PickupsPageRelativePath, "GenerateFitPreviewAsync", "Fit preview session binding");
        AssertContains(source, PickupsPageRelativePath, "AcceptFitPreviewAsync", "Fit accept session binding");
        AssertContains(source, PickupsPageRelativePath, "CommitFitAsync", "Fit commit session binding");
        AssertContains(source, PickupsPageRelativePath, "IsEnabled=\"@CanCommitFitItem(fitItem)\"", "Fit commit disabled until commit-ready state");
    }

    [Fact]
    public void PickupsPage_ManualTargetOptionsUseSessionTargetsAndLoadedMapFallback()
    {
        var source = ReadRepoFile(PickupsPageRelativePath);

        AssertContains(source, PickupsPageRelativePath, "private IReadOnlyList<PickupPickMapTargetReference> AvailablePickTargets", "manual target option source");
        AssertContains(source, PickupsPageRelativePath, "_snapshot.Targets", "session CRX target source for manual dropdown");
        AssertContains(source, PickupsPageRelativePath, "assignment.InferredTarget", "loaded Pick map inferred target fallback");
        AssertContains(source, PickupsPageRelativePath, "assignment.SelectedTarget", "loaded Pick map selected target fallback");
        AssertContains(source, PickupsPageRelativePath, ".Concat(mapTargets)", "manual target option merge for resumed maps");
        AssertContains(source, PickupsPageRelativePath, "BuildPickTargetOptionValue(target)", "stable manual target option value");
    }

    [Fact]
    public void PickupsPage_UsesEditingHandoffHelperWithDeterministicFallbackSeam()
    {
        var source = ReadRepoFile(PickupsPageRelativePath);

        AssertContains(source, PickupsPageRelativePath, "private void NavigateToEditing()", "editing handoff method declaration");
        AssertContains(source, PickupsPageRelativePath, "StageRouteCatalog.GetProofEditingHandoffPath(_snapshot.ActiveChapterName)", "editing handoff helper usage");
        AssertContains(source, PickupsPageRelativePath, "StageRouteCatalog.Resolve(handoffPath)", "handoff path route-resolution guard");
        AssertContains(source, PickupsPageRelativePath, "StageRouteCatalog.BuildProofChapterCanonicalPath(_snapshot.ActiveChapterName)", "reserved-slug canonical chapter fallback seam");
        AssertContains(source, PickupsPageRelativePath, "Navigation.NavigateTo(handoffPath);", "handoff navigation call");

        Assert.DoesNotContain("Navigation.NavigateTo(\"/proof/editing\")", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Navigation.NavigateTo(\"/proof/pickups\")", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProofIndex_ContainsCatalogBackedPickupsLink()
    {
        var source = ReadRepoFile(ProofIndexRelativePath);

        AssertContains(source, ProofIndexRelativePath, "GetProofPickupsPath", "proof pickups path helper");
        AssertContains(source, ProofIndexRelativePath, "StageRouteCatalog.ModuleIds.ProofPickups", "route catalog module id usage");
        AssertContains(source, ProofIndexRelativePath, "data-proof-index-link=\"pickups\"", "proof index pickups link marker");
    }

    [Fact]
    public void StageRouteCatalog_ResolvesProofPickupsModuleContract()
    {
        var match = StageRouteCatalog.Resolve("/proof/pickups");

        Assert.True(match is not null, "Expected '/proof/pickups' to resolve in StageRouteCatalog.");
        Assert.Equal(StageRouteCatalog.StageIds.Proof, match!.Stage.Id, ignoreCase: true);
        Assert.Equal(StageRouteCatalog.ModuleIds.ProofPickups, match.Module.Id, ignoreCase: true);

        Assert.True(
            StageRouteCatalog.TryGetModuleCanonicalPath(
                StageRouteCatalog.StageIds.Proof,
                StageRouteCatalog.ModuleIds.ProofPickups,
                out var canonicalPath),
            "Expected canonical path lookup for proof-pickups module to succeed.");

        Assert.Equal("/proof/pickups", canonicalPath, ignoreCase: true);

        Assert.True(
            StageRouteCatalog.TryGetModule(
                StageRouteCatalog.StageIds.Proof,
                StageRouteCatalog.ModuleIds.ProofPickups,
                out var module),
            "Expected module lookup for proof-pickups to succeed.");

        Assert.False(module.SupportsBatching);
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof pickups contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof pickups contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof pickups contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
