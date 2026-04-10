using Ams.Workstation.Server.Components.Layout;
using Ams.Workstation.Server.Components.Navigation;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellBatchingContractTests
{
    [Fact]
    public void Catalog_BatchingSupportIsDisabledForAllShellVisibleModules()
    {
        var batchEnabledModules = StageRouteCatalog.Stages
            .SelectMany(stage => stage.Modules.Select(module => (StageId: stage.Id, Module: module)))
            .Where(entry => entry.Module.SupportsBatching)
            .Select(entry => $"{entry.StageId}/{entry.Module.Id}")
            .ToArray();

        Assert.True(
            batchEnabledModules.Length == 0,
            $"Batching contract drift detected. Expected no shell-visible batching modules, but found: {string.Join(", ", batchEnabledModules)}.");
    }

    [Theory]
    [InlineData("/prep/pipeline", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline, false)]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("/proof/overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, false)]
    [InlineData("/proof/patterns", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, false)]
    [InlineData("/polish", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, false)]
    [InlineData("/polish/scaffold", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, false)]
    [InlineData("/polish/pickups", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, false)]
    [InlineData("/polish/batch", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, false)]
    public void ActiveModuleBatchingSupport_IsModuleSpecific(
        string path,
        string expectedStageId,
        string expectedModuleId,
        bool expectedSupportsBatching)
    {
        AssertActiveModuleBatching(path, expectedStageId, expectedModuleId, expectedSupportsBatching);
    }

    [Fact]
    public void ShellStateDescriptors_DoNotExposeShellLevelBatchingFlag()
    {
        var shellPropertyNames = typeof(StageModuleRail.StageModuleRailState)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        var modulePropertyNames = typeof(StageModuleRail.StageModuleRailItem)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.True(
            !shellPropertyNames.Contains(nameof(StageModuleRail.StageModuleRailItem.SupportsBatching), StringComparer.OrdinalIgnoreCase),
            $"Shell-level batching leakage detected. StageModuleRailState should not expose '{nameof(StageModuleRail.StageModuleRailItem.SupportsBatching)}', but has: {string.Join(", ", shellPropertyNames)}.");

        Assert.True(
            modulePropertyNames.Contains(nameof(StageModuleRail.StageModuleRailItem.SupportsBatching), StringComparer.OrdinalIgnoreCase),
            $"Module-level batching metadata missing. StageModuleRailItem properties: {string.Join(", ", modulePropertyNames)}.");
    }

    [Fact]
    public void BatchingContractHelper_RejectsUnknownModuleIdWithActionableMessage()
    {
        var exception = Assert.ThrowsAny<XunitException>(() =>
            AssertModuleSupportsBatching(
                stageId: StageRouteCatalog.StageIds.Polish,
                moduleId: "polish-unknown",
                expectedSupportsBatching: false,
                pathContext: "/polish/unknown"));

        Assert.Contains("/polish/unknown", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("polish-unknown", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("known modules", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(StageRouteCatalog.StageIds.Polish, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertActiveModuleBatching(
        string path,
        string expectedStageId,
        string expectedModuleId,
        bool expectedSupportsBatching)
    {
        var state = StageModuleRail.ResolveStateForPath(path);

        Assert.True(
            state.IsVisible,
            $"Expected visible stage shell state for path '{path}', but got hidden state with warning '{state.WarningMessage}' and marker '{state.DiagnosticMarker}'.");

        Assert.True(
            string.Equals(state.ActiveStageId, expectedStageId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(state.ActiveModuleId, expectedModuleId, StringComparison.OrdinalIgnoreCase),
            $"Unexpected stage/module for path '{path}'. Expected stage='{expectedStageId}', module='{expectedModuleId}'. Resolved stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', marker='{state.DiagnosticMarker}'.");

        var activeModule = state.Modules.FirstOrDefault(module => module.IsActive);
        Assert.True(
            activeModule is not null,
            $"No active module found for path '{path}'. Resolved stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', modules='{string.Join(", ", state.Modules.Select(module => module.Id))}'.");

        Assert.True(
            activeModule!.SupportsBatching == expectedSupportsBatching,
            $"Batching mismatch for path '{path}'. Expected supportsBatching='{expectedSupportsBatching}' for stage='{expectedStageId}', module='{expectedModuleId}'. Resolved stage='{state.ActiveStageId}', module='{state.ActiveModuleId}', activeSupportsBatching='{activeModule.SupportsBatching}', marker='{state.DiagnosticMarker}'.");
    }

    private static void AssertModuleSupportsBatching(
        string stageId,
        string moduleId,
        bool expectedSupportsBatching,
        string pathContext)
    {
        Assert.True(
            StageRouteCatalog.TryGetStage(stageId, out var stage),
            $"Unknown stage '{stageId}' while validating batching contract for path '{pathContext}'. Known stages: {string.Join(", ", StageRouteCatalog.Stages.Select(candidate => candidate.Id))}.");

        var module = stage!.Modules.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, moduleId, StringComparison.OrdinalIgnoreCase));

        Assert.True(
            module is not null,
            $"Unknown module '{moduleId}' in stage '{stageId}' while validating batching contract for path '{pathContext}'. Known modules: {string.Join(", ", stage.Modules.Select(candidate => candidate.Id))}.");

        Assert.True(
            module!.SupportsBatching == expectedSupportsBatching,
            $"Batching contract mismatch for path '{pathContext}'. Stage='{stageId}', module='{moduleId}', expectedSupportsBatching='{expectedSupportsBatching}', actualSupportsBatching='{module.SupportsBatching}'.");
    }
}
