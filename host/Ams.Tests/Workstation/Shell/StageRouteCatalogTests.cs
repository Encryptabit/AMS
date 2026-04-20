using Ams.Workstation.Server.Components.Navigation;

namespace Ams.Tests.Workstation.Shell;

public sealed class StageRouteCatalogTests
{
    [Fact]
    public void CatalogDeclaresExpectedStagesAndModuleDescriptors()
    {
        var stageIds = StageRouteCatalog.Stages
            .Select(stage => stage.Id)
            .ToArray();

        Assert.Equal(
            [
                StageRouteCatalog.StageIds.Prep,
                StageRouteCatalog.StageIds.Proof,
                StageRouteCatalog.StageIds.Polish
            ],
            stageIds);

        AssertModuleExists(StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline, expectedSupportsBatching: true);
        AssertModuleExists(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, expectedSupportsBatching: false);
        AssertModuleExists(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, expectedSupportsBatching: false);
        AssertModuleExists(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, expectedSupportsBatching: false);
        AssertModuleExists(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, expectedSupportsBatching: false);
        AssertModuleExists(StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, expectedSupportsBatching: false);
    }

    [Fact]
    public void ProofCompatibilityAliasesAreExplicitlyDeclared()
    {
        var requiredAliases = new[]
        {
            "/proof",
            "/proof/pickups",
            "/proof/overview",
            "/proof/patterns",
            StageRouteCatalog.ProofChapterCompatibilityTemplate
        };

        var declaredAliases = StageRouteCatalog.GetCompatibilityPaths(StageRouteCatalog.StageIds.Proof);
        var missingAliases = requiredAliases
            .Where(alias => !declaredAliases.Contains(alias, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            missingAliases.Length == 0,
            $"Missing required proof compatibility alias(es): {string.Join(", ", missingAliases)}. Declared aliases: {string.Join(", ", declaredAliases)}.");
    }

    [Fact]
    public void PolishCompatibilityAliasesResolveToScaffoldContract()
    {
        var requiredAliases = new[]
        {
            "/polish",
            "/polish/pickups",
            "/polish/batch"
        };

        var declaredAliases = StageRouteCatalog.GetCompatibilityPaths(StageRouteCatalog.StageIds.Polish);
        var missingAliases = requiredAliases
            .Where(alias => !declaredAliases.Contains(alias, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            missingAliases.Length == 0,
            $"Missing required polish compatibility alias(es): {string.Join(", ", missingAliases)}. Declared aliases: {string.Join(", ", declaredAliases)}.");

        var unexpectedAliases = declaredAliases
            .Where(alias => !requiredAliases.Contains(alias, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            unexpectedAliases.Length == 0,
            $"Unexpected polish compatibility alias(es): {string.Join(", ", unexpectedAliases)}. Required aliases: {string.Join(", ", requiredAliases)}.");
    }

    [Theory]
    [InlineData(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, "/proof/editing")]
    [InlineData(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, "/proof/pickups")]
    [InlineData(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, "/proof/overview")]
    [InlineData(StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, "/proof/patterns")]
    [InlineData(StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline, "/prep/pipeline")]
    [InlineData(StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, "/polish/scaffold")]
    public void TryGetModuleCanonicalPath_KnownStageModulePair_ReturnsCanonicalPath(
        string stageId,
        string moduleId,
        string expectedCanonicalPath)
    {
        var found = StageRouteCatalog.TryGetModuleCanonicalPath(stageId, moduleId, out var canonicalPath);

        Assert.True(
            found,
            $"Expected canonical path lookup for stage='{stageId}', module='{moduleId}' to succeed, but it failed.");

        Assert.True(
            string.Equals(canonicalPath, expectedCanonicalPath, StringComparison.OrdinalIgnoreCase),
            $"Canonical path lookup mismatch for stage='{stageId}', module='{moduleId}'. Expected='{expectedCanonicalPath}', actual='{canonicalPath}'.");
    }

    [Fact]
    public void TryGetModuleCanonicalPath_UnknownOrMalformedIdentifiers_ReturnsFalse()
    {
        var malformedInputs = new (string? StageId, string? ModuleId)[]
        {
            (null, StageRouteCatalog.ModuleIds.ProofOverview),
            (StageRouteCatalog.StageIds.Proof, null),
            ("", StageRouteCatalog.ModuleIds.ProofOverview),
            (StageRouteCatalog.StageIds.Proof, ""),
            ("   ", StageRouteCatalog.ModuleIds.ProofOverview),
            (StageRouteCatalog.StageIds.Proof, "   "),
            ("unknown-stage", StageRouteCatalog.ModuleIds.ProofOverview),
            (StageRouteCatalog.StageIds.Proof, "unknown-module")
        };

        foreach (var (stageId, moduleId) in malformedInputs)
        {
            var found = StageRouteCatalog.TryGetModuleCanonicalPath(stageId, moduleId, out var canonicalPath);

            Assert.True(
                !found,
                $"Expected canonical path lookup to fail for stage='{stageId ?? "(null)"}', module='{moduleId ?? "(null)"}', but it succeeded with path='{canonicalPath}'.");

            Assert.True(
                string.Equals(canonicalPath, StageRouteCatalog.RootPath, StringComparison.Ordinal),
                $"Expected failed canonical path lookups to return root fallback '{StageRouteCatalog.RootPath}', but got '{canonicalPath}' for stage='{stageId ?? "(null)"}', module='{moduleId ?? "(null)"}'.");
        }
    }

    [Theory]
    [InlineData(StageRouteCatalog.StageIds.Proof, "unknown-module", "/proof")]
    [InlineData(StageRouteCatalog.StageIds.Polish, "unknown-module", "/polish")]
    [InlineData("unknown-stage", StageRouteCatalog.ModuleIds.ProofOverview, StageRouteCatalog.RootPath)]
    [InlineData("   ", StageRouteCatalog.ModuleIds.ProofOverview, StageRouteCatalog.RootPath)]
    public void GetModuleCanonicalPath_WhenLookupFails_UsesDeterministicStageFallback(
        string stageId,
        string moduleId,
        string expectedPath)
    {
        var resolvedPath = StageRouteCatalog.GetModuleCanonicalPath(stageId, moduleId);

        Assert.True(
            string.Equals(resolvedPath, expectedPath, StringComparison.OrdinalIgnoreCase),
            $"Expected canonical path fallback for stage='{stageId}', module='{moduleId}' to resolve '{expectedPath}', but resolved '{resolvedPath}'.");
    }

    [Fact]
    public void GetProofPickupsHandoffPath_DelegatesToProofModuleCanonicalLookup()
    {
        var expectedPath = StageRouteCatalog.GetModuleCanonicalPath(
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);

        var handoffPath = StageRouteCatalog.GetProofPickupsHandoffPath();

        Assert.True(
            string.Equals(handoffPath, expectedPath, StringComparison.OrdinalIgnoreCase),
            $"Expected proof pickups handoff helper to resolve canonical module path '{expectedPath}', but got '{handoffPath}'.");

        AssertResolves(
            handoffPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups,
            expectedCompatibilityAlias: true);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetProofEditingHandoffPath_MalformedChapterInput_FallsBackToCanonicalEditingPath(string? chapterName)
    {
        var expectedCanonicalPath = StageRouteCatalog.GetModuleCanonicalPath(
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing);

        var handoffPath = StageRouteCatalog.GetProofEditingHandoffPath(chapterName);

        Assert.True(
            string.Equals(handoffPath, expectedCanonicalPath, StringComparison.OrdinalIgnoreCase),
            $"Expected malformed chapter input '{chapterName ?? "(null)"}' to fallback to canonical editing path '{expectedCanonicalPath}', but got '{handoffPath}'.");

        AssertResolves(
            handoffPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing,
            expectedCompatibilityAlias: false);
    }

    [Theory]
    [InlineData("Chapter 01 - Intro")]
    [InlineData("Chapter 02: Punctuation!?,;")]
    [InlineData("Chapter 03 / Finale")]
    [InlineData("MiXeD CaSe / EnCoDeD")]
    public void GetProofEditingHandoffPath_WithKnownChapter_UsesCompatibilityAlias(string chapterName)
    {
        var expectedPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);
        var handoffPath = StageRouteCatalog.GetProofEditingHandoffPath(chapterName);

        Assert.True(
            string.Equals(handoffPath, expectedPath, StringComparison.Ordinal),
            $"Expected chapter-aware editing handoff helper to return compatibility path '{expectedPath}' for chapter '{chapterName}', but got '{handoffPath}'.");

        AssertResolves(
            handoffPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing,
            expectedCompatibilityAlias: true);
    }

    [Fact]
    public void GetProofEditingHandoffPath_ReservedPickupsSlug_PreservesExactRoutePrecedence()
    {
        var handoffPath = StageRouteCatalog.GetProofEditingHandoffPath("pickups");

        Assert.True(
            string.Equals(handoffPath, "/proof/pickups", StringComparison.OrdinalIgnoreCase),
            $"Expected reserved chapter slug 'pickups' to produce '/proof/pickups', but got '{handoffPath}'.");

        var match = StageRouteCatalog.Resolve(handoffPath);

        Assert.True(
            match is not null
            && string.Equals(match.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, StageRouteCatalog.ModuleIds.ProofPickups, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.MatchedTemplate, "/proof/pickups", StringComparison.OrdinalIgnoreCase),
            $"Expected proof pickups exact route to win for reserved handoff path '{handoffPath}', but got {match?.DiagnosticContext ?? "(null)"}.");
    }

    [Fact]
    public void RouteTemplatesAreValidatedForLeadingSlashAndTokenShape()
    {
        var malformedTemplates = EnumerateTemplates()
            .Select(entry =>
            {
                var isValid = StageRouteCatalog.IsValidTemplate(entry.Template, out var reason);
                return new
                {
                    entry.StageId,
                    entry.ModuleId,
                    entry.Template,
                    IsValid = isValid,
                    Reason = reason
                };
            })
            .Where(entry => !entry.IsValid)
            .Select(entry =>
                $"stage='{entry.StageId}', module='{entry.ModuleId}', template='{entry.Template}', reason='{entry.Reason}'")
            .ToArray();

        Assert.True(
            malformedTemplates.Length == 0,
            $"Malformed route template contract failure(s): {string.Join(" | ", malformedTemplates)}.");
    }

    [Theory]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/pickups", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof/pickups/", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/PrOoF/PiCkUpS?sort=asc#top", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof/overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, true)]
    [InlineData("/proof/patterns", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, true)]
    [InlineData("/proof/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/Chapter%2F01", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/Chapter%2f01", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/PrOoF/Overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, true)]
    [InlineData("/prep", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline, true)]
    [InlineData("/prep/pipeline", StageRouteCatalog.StageIds.Prep, StageRouteCatalog.ModuleIds.PrepPipeline, false)]
    [InlineData("/polish", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, true)]
    [InlineData("/polish/scaffold", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, false)]
    [InlineData("/polish/pickups", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, true)]
    [InlineData("/polish/batch", StageRouteCatalog.StageIds.Polish, StageRouteCatalog.ModuleIds.PolishScaffold, true)]
    [InlineData("/proof/overview?sort=asc#top", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, true)]
    public void Resolve_KnownPaths_ReturnsExpectedStageAndModule(
        string path,
        string expectedStage,
        string expectedModule,
        bool expectedCompatibilityAlias)
    {
        AssertResolves(path, expectedStage, expectedModule, expectedCompatibilityAlias);
    }

    [Fact]
    public void Resolve_ProofPickupsPath_WinsOverProofChapterTemplateCollision()
    {
        var chapterSlugPath = StageRouteCatalog.BuildProofChapterCompatibilityPath("pickups");
        var match = StageRouteCatalog.Resolve(chapterSlugPath);

        Assert.True(
            match is not null,
            $"Expected chapter-compatible path '{chapterSlugPath}' to resolve, but no match returned.");

        Assert.True(
            string.Equals(match!.Stage.Id, StageRouteCatalog.StageIds.Proof, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, StageRouteCatalog.ModuleIds.ProofPickups, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.MatchedTemplate, "/proof/pickups", StringComparison.OrdinalIgnoreCase),
            $"Expected proof pickups exact route to win over chapter template for '{chapterSlugPath}', but got {match.DiagnosticContext}.");
    }

    [Theory]
    [InlineData("/polish/batch", "/polish/batch", true)]
    [InlineData("/polish/scaffold", "/polish/scaffold", false)]
    public void Resolve_PolishScaffoldPaths_EmitDeterministicDiagnosticContext(
        string path,
        string expectedTemplate,
        bool expectedCompatibilityAlias)
    {
        var match = StageRouteCatalog.Resolve(path);

        Assert.True(
            match is not null,
            $"Expected path '{path}' to resolve to polish scaffold contract, but no match was returned.");

        Assert.True(
            string.Equals(match!.Stage.Id, StageRouteCatalog.StageIds.Polish, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, StageRouteCatalog.ModuleIds.PolishScaffold, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.MatchedTemplate, expectedTemplate, StringComparison.OrdinalIgnoreCase)
            && match.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Unexpected polish scaffold diagnostics for path '{path}'. {match.DiagnosticContext}");

        Assert.Contains("stage='polish'", match.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("module='polish-scaffold'", match.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"template='{expectedTemplate}'", match.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"compatibility='{expectedCompatibilityAlias}'", match.DiagnosticContext, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/")]
    [InlineData("proof")]
    [InlineData("/unknown")]
    [InlineData("/proof/overview/extra")]
    [InlineData("/polish/unknown")]
    [InlineData("/polish/legacy/pickups")]
    [InlineData("/polish/legacy/batch")]
    public void Resolve_MalformedOrUnknownPaths_ReturnsNoMatch(string path)
    {
        var match = StageRouteCatalog.Resolve(path);

        Assert.True(
            match is null,
            $"Expected unresolved path '{path}', but resolver returned stage='{match?.Stage.Id}', module='{match?.Module.Id}', template='{match?.MatchedTemplate}'.");
    }

    private static void AssertResolves(
        string path,
        string expectedStage,
        string expectedModule,
        bool expectedCompatibilityAlias)
    {
        var match = StageRouteCatalog.Resolve(path);

        Assert.True(
            match is not null,
            $"Expected path '{path}' to resolve stage='{expectedStage}', module='{expectedModule}', compatibility='{expectedCompatibilityAlias}', but no match was returned.");

        Assert.True(
            string.Equals(match!.Stage.Id, expectedStage, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, expectedModule, StringComparison.OrdinalIgnoreCase)
            && match.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Path '{path}' resolved unexpectedly. Expected stage='{expectedStage}', module='{expectedModule}', compatibility='{expectedCompatibilityAlias}'. Actual: {match.DiagnosticContext}.");
    }

    private static void AssertModuleExists(string stageId, string moduleId, bool expectedSupportsBatching)
    {
        Assert.True(
            StageRouteCatalog.TryGetStage(stageId, out var stage),
            $"Stage '{stageId}' is missing from StageRouteCatalog.");

        var module = stage!.Modules.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, moduleId, StringComparison.OrdinalIgnoreCase));

        Assert.True(
            module is not null,
            $"Module '{moduleId}' is missing from stage '{stageId}'. Available modules: {string.Join(", ", stage.Modules.Select(candidate => candidate.Id))}.");

        Assert.True(
            module!.SupportsBatching == expectedSupportsBatching,
            $"Batching contract mismatch for stage='{stageId}', module='{moduleId}'. Expected supportsBatching='{expectedSupportsBatching}' but was '{module.SupportsBatching}'.");
    }

    private static IEnumerable<(string StageId, string ModuleId, string Template)> EnumerateTemplates()
    {
        foreach (var stage in StageRouteCatalog.Stages)
        {
            foreach (var module in stage.Modules)
            {
                yield return (stage.Id, module.Id, module.CanonicalPath);

                foreach (var compatibilityTemplate in module.CompatibilityPaths)
                {
                    yield return (stage.Id, module.Id, compatibilityTemplate);
                }
            }
        }
    }
}
