using System.Reflection;
using System.Text.RegularExpressions;
using Ams.Workstation.Server.Components.Navigation;
using Microsoft.AspNetCore.Components;
using PolishScaffoldPage = Ams.Workstation.Server.Components.Pages.Polish.Index;
using ProofChapterReviewPage = Ams.Workstation.Server.Components.Pages.Proof.ChapterReview;
using ProofPatternsPage = Ams.Workstation.Server.Components.Pages.Proof.ErrorPatterns;
using ProofIndexPage = Ams.Workstation.Server.Components.Pages.Proof.Index;
using ProofOverviewPage = Ams.Workstation.Server.Components.Pages.Proof.Overview;
using ProofPickupsPage = Ams.Workstation.Server.Components.Pages.Proof.Pickups;

namespace Ams.Tests.Workstation.Shell;

public sealed class ProofRouteCompatibilityTests
{
    private const string PolishScaffoldPageRelativePath = "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor";
    private const string PolishScaffoldScssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor.scss";

    [Theory]
    [InlineData("/proof", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/pickups", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof/pickups/", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof/pickups?sort=asc#top", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof/overview", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofOverview, true)]
    [InlineData("/proof/patterns", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, true)]
    [InlineData("/proof/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/Chapter%2F01", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/Chapter%2f01", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/editing", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("/proof/editing/Chapter%201", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/PrOoF/PaTtErNs", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPatterns, true)]
    [InlineData("/PrOoF/PiCkUpS", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofPickups, true)]
    [InlineData("/proof//", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("/proof/%20", StageRouteCatalog.StageIds.Proof, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    public void Resolve_ProofCompatibilityAndCanonicalAliases_ReturnExpectedContract(
        string path,
        string expectedStageId,
        string expectedModuleId,
        bool expectedCompatibilityAlias)
    {
        _ = AssertResolves(path, expectedStageId, expectedModuleId, expectedCompatibilityAlias);
    }

    [Theory]
    [InlineData("Chapter 01 - Intro")]
    [InlineData("Chapter 02: Punctuation!?,;")]
    [InlineData("Chapter 03 / Finale")]
    public void CompatibilityAndCanonicalChapterAliases_ResolveToSameStageModuleContract(string chapterName)
    {
        var compatibilityPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);
        var canonicalAliasPath = StageRouteCatalog.BuildProofChapterCanonicalPath(chapterName);

        var compatibility = AssertResolves(
            compatibilityPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing,
            expectedCompatibilityAlias: true);

        var canonicalAlias = AssertResolves(
            canonicalAliasPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing,
            expectedCompatibilityAlias: true);

        Assert.True(
            string.Equals(compatibility.Stage.Id, canonicalAlias.Stage.Id, StringComparison.OrdinalIgnoreCase)
            && string.Equals(compatibility.Module.Id, canonicalAlias.Module.Id, StringComparison.OrdinalIgnoreCase),
            $"Compatibility/canonical transition mismatch for chapter '{chapterName}'. Compatibility path='{compatibilityPath}' => {compatibility.DiagnosticContext}. Canonical alias path='{canonicalAliasPath}' => {canonicalAlias.DiagnosticContext}.");
    }

    [Theory]
    [InlineData("overview", StageRouteCatalog.ModuleIds.ProofOverview, true, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("patterns", StageRouteCatalog.ModuleIds.ProofPatterns, true, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("pickups", StageRouteCatalog.ModuleIds.ProofPickups, true, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("editing", StageRouteCatalog.ModuleIds.ProofEditing, false, StageRouteCatalog.ModuleIds.ProofEditing, true)]
    public void ReservedChapterSlugs_ResolveToExpectedCompatibilityAndCanonicalTargets(
        string chapterName,
        string expectedCompatibilityModuleId,
        bool expectedCompatibilityAlias,
        string expectedCanonicalModuleId,
        bool expectedCanonicalAlias)
    {
        var compatibilityPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);
        var canonicalAliasPath = StageRouteCatalog.BuildProofChapterCanonicalPath(chapterName);

        _ = AssertResolves(
            compatibilityPath,
            StageRouteCatalog.StageIds.Proof,
            expectedCompatibilityModuleId,
            expectedCompatibilityAlias);

        _ = AssertResolves(
            canonicalAliasPath,
            StageRouteCatalog.StageIds.Proof,
            expectedCanonicalModuleId,
            expectedCanonicalAlias);
    }

    [Fact]
    public void ProofPickupsHandoffHelper_ComposesWithProofRouteContracts()
    {
        var expectedCanonicalPath = StageRouteCatalog.GetModuleCanonicalPath(
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups);

        var handoffPath = StageRouteCatalog.GetProofPickupsHandoffPath();

        Assert.True(
            string.Equals(handoffPath, expectedCanonicalPath, StringComparison.OrdinalIgnoreCase),
            $"Expected proof pickups handoff helper to return canonical module path '{expectedCanonicalPath}', but got '{handoffPath}'.");

        _ = AssertResolves(
            handoffPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups,
            expectedCompatibilityAlias: true);
    }

    [Theory]
    [InlineData(null, StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("", StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("   ", StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("Chapter 01 - Intro", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("Chapter 03 / Finale", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("pickups", StageRouteCatalog.ModuleIds.ProofPickups, true)]
    public void ProofEditingHandoffHelper_ComposesWithCompatibilityAndCanonicalAliasContracts(
        string? chapterName,
        string expectedCompatibilityModuleId,
        bool expectedCompatibilityAlias)
    {
        var handoffPath = StageRouteCatalog.GetProofEditingHandoffPath(chapterName);

        if (string.IsNullOrWhiteSpace(chapterName))
        {
            var expectedCanonicalPath = StageRouteCatalog.GetModuleCanonicalPath(
                StageRouteCatalog.StageIds.Proof,
                StageRouteCatalog.ModuleIds.ProofEditing);

            Assert.True(
                string.Equals(handoffPath, expectedCanonicalPath, StringComparison.OrdinalIgnoreCase),
                $"Expected null/whitespace editing handoff input '{chapterName ?? "(null)"}' to return canonical path '{expectedCanonicalPath}', but got '{handoffPath}'.");
        }
        else
        {
            var expectedCompatibilityPath = StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);
            var expectedCanonicalAliasPath = StageRouteCatalog.BuildProofChapterCanonicalPath(chapterName);

            Assert.True(
                string.Equals(handoffPath, expectedCompatibilityPath, StringComparison.Ordinal),
                $"Expected chapter-aware editing handoff helper to return compatibility path '{expectedCompatibilityPath}' for chapter '{chapterName}', but got '{handoffPath}'.");

            _ = AssertResolves(
                expectedCanonicalAliasPath,
                StageRouteCatalog.StageIds.Proof,
                StageRouteCatalog.ModuleIds.ProofEditing,
                expectedCompatibilityAlias: true);
        }

        _ = AssertResolves(
            handoffPath,
            StageRouteCatalog.StageIds.Proof,
            expectedCompatibilityModuleId,
            expectedCompatibilityAlias);
    }

    [Theory]
    [InlineData(null, "/proof/editing", StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("", "/proof/editing", StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("   ", "/proof/editing", StageRouteCatalog.ModuleIds.ProofEditing, false)]
    [InlineData("Chapter 01 - Intro", "/proof/chapter%2001%20-%20intro", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("Chapter 03 / Finale", "/proof/chapter%2003%20%2f%20finale", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("pickups", "/proof/editing/pickups", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("overview", "/proof/editing/overview", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    [InlineData("patterns", "/proof/editing/patterns", StageRouteCatalog.ModuleIds.ProofEditing, true)]
    public void ProofEditingRoundTripComposePath_PreventsRouteOwnershipDrift(
        string? activeChapterName,
        string expectedPath,
        string expectedModuleId,
        bool expectedCompatibilityAlias)
    {
        var returnPath = ComposeProofEditingRoundTripPath(activeChapterName);

        Assert.Equal(expectedPath, returnPath, ignoreCase: true);

        _ = AssertResolves(
            returnPath,
            StageRouteCatalog.StageIds.Proof,
            expectedModuleId,
            expectedCompatibilityAlias);

        var pickupsPath = StageRouteCatalog.GetProofPickupsHandoffPath();
        _ = AssertResolves(
            pickupsPath,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofPickups,
            expectedCompatibilityAlias: true);
    }

    [Fact]
    public void ProofAndPolishPages_DeclareExpectedCanonicalAndCompatibilityTemplates()
    {
        AssertRoutes(typeof(ProofIndexPage), "/proof", "/proof/editing");
        AssertRoutes(typeof(ProofPickupsPage), "/proof/pickups");
        AssertRoutes(typeof(ProofOverviewPage), "/proof/overview");
        AssertRoutes(typeof(ProofPatternsPage), "/proof/patterns");
        AssertRoutes(typeof(ProofChapterReviewPage), "/proof/{ChapterName}", "/proof/editing/{ChapterName}");
        AssertRoutes(typeof(PolishScaffoldPage), "/polish", "/polish/scaffold", "/polish/pickups", "/polish/batch");
    }

    [Fact]
    public void PolishScaffoldPage_PreservesProofHandoffAnchorWithNonBlankHref()
    {
        var source = ReadRepoFile(PolishScaffoldPageRelativePath);

        Assert.Contains("data-polish-scaffold-link=\"proof-pickups\"", source, StringComparison.Ordinal);

        var anchorMatch = Regex.Match(
            source,
            "<a\\s+[^>]*data-polish-scaffold-link=\"proof-pickups\"[^>]*>",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        Assert.True(
            anchorMatch.Success,
            $"Expected polish scaffold page to declare an <a> proof handoff anchor in '{PolishScaffoldPageRelativePath}', but none matched data-polish-scaffold-link=\"proof-pickups\".");

        var hrefMatch = Regex.Match(
            anchorMatch.Value,
            "href\\s*=\\s*\"(?<href>[^\"]*)\"",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        Assert.True(
            hrefMatch.Success,
            $"Expected polish scaffold proof handoff anchor to declare href in '{PolishScaffoldPageRelativePath}', but anchor was '{anchorMatch.Value}'.");

        var href = hrefMatch.Groups["href"].Value;
        Assert.True(
            !string.IsNullOrWhiteSpace(href),
            $"Expected polish scaffold proof handoff href to be non-empty in '{PolishScaffoldPageRelativePath}', but anchor was '{anchorMatch.Value}'.");

        Assert.Equal("@GetProofPickupsPath()", href);
    }

    [Fact]
    public void PolishScaffoldStyles_EncodeNarrowViewportOverflowGuardrails()
    {
        var source = ReadRepoFile(PolishScaffoldScssRelativePath);

        Assert.Contains("@media (max-width: 360px)", source, StringComparison.Ordinal);
        Assert.Matches(new Regex("overflow-x\\s*:\\s*(clip|hidden)\\s*;", RegexOptions.CultureInvariant), source);
        Assert.Contains(".polish-scaffold-card__action", source, StringComparison.Ordinal);
        Assert.Contains("width: 100%", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyPolishPickupRoutes_AreNotResolvedByStageRouteCatalog()
    {
        var legacyPaths = new[]
        {
            "/polish/legacy/pickups",
            "/polish/legacy/batch"
        };

        foreach (var legacyPath in legacyPaths)
        {
            var match = StageRouteCatalog.Resolve(legacyPath);
            Assert.True(match is null, $"Expected legacy path '{legacyPath}' to be removed, but resolver returned: {match?.DiagnosticContext}.");
        }
    }

    [Fact]
    public void ProofCompatibilityAliasCatalog_IncludesLegacyDeepLinksAndCanonicalChapterAlias()
    {
        var expectedAliases = new[]
        {
            "/proof",
            "/proof/pickups",
            "/proof/overview",
            "/proof/patterns",
            StageRouteCatalog.ProofChapterCompatibilityTemplate,
            StageRouteCatalog.ProofChapterCanonicalTemplate
        };

        var declaredAliases = StageRouteCatalog.GetCompatibilityPaths(StageRouteCatalog.StageIds.Proof);

        var missingAliases = expectedAliases
            .Where(expectedAlias => !declaredAliases.Contains(expectedAlias, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            missingAliases.Length == 0,
            $"Missing proof compatibility aliases: {string.Join(", ", missingAliases)}. Declared aliases: {string.Join(", ", declaredAliases)}.");
    }

    [Fact]
    public void ProofAndPolishPageRouteTemplates_AreValidWithComponentContext()
    {
        var malformedTemplates = EnumerateRouteTemplates(
                typeof(ProofIndexPage),
                typeof(ProofPickupsPage),
                typeof(ProofOverviewPage),
                typeof(ProofPatternsPage),
                typeof(ProofChapterReviewPage),
                typeof(PolishScaffoldPage))
            .Select(entry =>
            {
                var isValid = StageRouteCatalog.IsValidTemplate(entry.Template, out var reason);
                return new
                {
                    entry.ComponentType,
                    entry.Template,
                    IsValid = isValid,
                    Reason = reason
                };
            })
            .Where(entry => !entry.IsValid)
            .Select(entry => $"component='{entry.ComponentType.FullName}', template='{entry.Template}', reason='{entry.Reason}'")
            .ToArray();

        Assert.True(
            malformedTemplates.Length == 0,
            $"Malformed route template contract failure(s): {string.Join(" | ", malformedTemplates)}.");
    }

    [Theory]
    [InlineData("polish")]
    [InlineData("polish%2Fscaffold")]
    [InlineData("polish%2Flegacy%2Fbatch")]
    public void ProofCompatibilityPaths_WithPolishLikeChapterSlugs_RemainProofBound(string chapterSlug)
    {
        var path = $"/proof/{chapterSlug}";
        _ = AssertResolves(
            path,
            StageRouteCatalog.StageIds.Proof,
            StageRouteCatalog.ModuleIds.ProofEditing,
            expectedCompatibilityAlias: true);
    }

    [Fact]
    public void ProofCompatibilityAliases_DoNotCollideWithPolishScaffoldAliases()
    {
        var proofAliases = StageRouteCatalog.GetCompatibilityPaths(StageRouteCatalog.StageIds.Proof);
        var polishAliases = StageRouteCatalog.GetCompatibilityPaths(StageRouteCatalog.StageIds.Polish);

        var collisions = proofAliases
            .Intersect(polishAliases, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.True(
            collisions.Length == 0,
            $"Proof compatibility aliases collided with polish scaffold aliases: {string.Join(", ", collisions)}. Proof aliases: {string.Join(", ", proofAliases)}. Polish aliases: {string.Join(", ", polishAliases)}.");
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

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Required proof route contract source file is missing: relative='{relativePath}', full='{fullPath}'.", fullPath);
        }

        return File.ReadAllText(fullPath);
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

    private static StageRouteMatch AssertResolves(
        string path,
        string expectedStageId,
        string expectedModuleId,
        bool expectedCompatibilityAlias)
    {
        var match = StageRouteCatalog.Resolve(path);

        var resolvedStageId = match?.Stage.Id ?? "(none)";
        var resolvedModuleId = match?.Module.Id ?? "(none)";
        var resolvedTemplate = match?.MatchedTemplate ?? "(none)";
        var resolvedCompatibility = match?.IsCompatibilityAlias.ToString() ?? "(none)";

        Assert.True(
            match is not null
            && string.Equals(match.Stage.Id, expectedStageId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(match.Module.Id, expectedModuleId, StringComparison.OrdinalIgnoreCase)
            && match.IsCompatibilityAlias == expectedCompatibilityAlias,
            $"Proof route contract mismatch for path '{path}'. Expected stage='{expectedStageId}', module='{expectedModuleId}', compatibility='{expectedCompatibilityAlias}'. Resolved stage='{resolvedStageId}', module='{resolvedModuleId}', template='{resolvedTemplate}', compatibility='{resolvedCompatibility}'.");

        return match!;
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

    private static IEnumerable<(Type ComponentType, string Template)> EnumerateRouteTemplates(params Type[] componentTypes)
    {
        foreach (var componentType in componentTypes)
        {
            var templates = componentType
                .GetCustomAttributes<RouteAttribute>(inherit: true)
                .Select(route => route.Template);

            foreach (var template in templates)
            {
                yield return (componentType, template);
            }
        }
    }
}
