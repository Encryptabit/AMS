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
