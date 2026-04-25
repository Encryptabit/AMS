using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepMobileContractTests
{
    private const string PrepIndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor";
    private const string PrepCssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor.css";

    [Fact]
    public void PrepIndex_DeclaresMobileMarkupContracts()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        var requiredMarkupAnchors = new[]
        {
            "data-ams-prep-mobile-contract=\"pipeline-action-bar\"",
            "data-ams-prep-mobile-contract=\"active-tasks-table\"",
            "data-ams-prep-mobile-contract=\"history-table\"",
            "data-ams-prep-mobile-contract=\"inspector-surfaces\"",
            "Class=\"pipeline-action-bar\"",
            "Class=\"pipeline-action-bar__stats\"",
            "Class=\"pipeline-action-bar__controls\"",
            "Class=\"pipeline-action-button\"",
            "class=\"prep-table pipeline-dashboard-table\"",
            "data-label=\"Action\"",
            "data-label=\"Message\""
        };

        foreach (var anchor in requiredMarkupAnchors)
        {
            AssertContainsAnchor(source, PrepIndexRelativePath, anchor, "Prep mobile markup contract");
        }

        var mobileContractAnchorCount = Regex.Matches(
            source,
            "data-ams-prep-mobile-contract=\"",
            RegexOptions.CultureInvariant).Count;

        Assert.True(
            mobileContractAnchorCount >= 4,
            $"Expected at least four prep mobile contract anchors in '{PrepIndexRelativePath}', but found {mobileContractAnchorCount}.");
    }

    [Fact]
    public void PrepCss_DeclaresMobileBreakpointDensityRules()
    {
        var source = ReadRepoFile(PrepCssRelativePath);

        var requiredResponsiveRules = new[]
        {
            "@media (max-width: 768px)",
            ".prep-page ::deep .pipeline-action-bar button",
            "min-height: 44px;",
            ".prep-page ::deep input",
            "font-size: 16px;",
            ".prep-page ::deep .pipeline-layout-grid > .pipeline-grid-item--top",
            ".prep-page ::deep .pipeline-dashboard-table thead",
            ".prep-page ::deep .pipeline-dashboard-table td::before",
            "content: attr(data-label);",
            ".prep-page ::deep [data-ams-prep-mobile-contract=\"inspector-surfaces\"] .inspector-error-log",
            "max-height: 10rem;"
        };

        foreach (var anchor in requiredResponsiveRules)
        {
            AssertContainsAnchor(source, PrepCssRelativePath, anchor, "Prep mobile responsive rule");
        }

        Assert.True(
            Regex.IsMatch(
                source,
                "@media \\(max-width: 768px\\)[\\s\\S]*?min-height:\\s*44px;",
                RegexOptions.CultureInvariant),
            $"Expected the 768px breakpoint block in '{PrepCssRelativePath}' to include 44px touch-target rules.");

        Assert.True(
            Regex.IsMatch(
                source,
                "@media \\(max-width: 768px\\)[\\s\\S]*?font-size:\\s*16px;",
                RegexOptions.CultureInvariant),
            $"Expected the 768px breakpoint block in '{PrepCssRelativePath}' to include 16px input typography safeguards.");
    }

    [Fact]
    public void PrepIndex_DoesNotIntroduceNewPersistenceOrPipelineSeams()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        var requiredExistingSeams = new[]
        {
            "@inject BlazorWorkspace Workspace",
            "@inject PrepRunSession Session",
            "Session.BuildBookIndexAsync(_manuscriptPath)",
            "Session.RunChapterPrepAsync(queueItem.Chapter, request)",
            "Workspace.SelectChapter(queueItem.Chapter)",
            "_operatorControls.TryBuildRequest(out var request)"
        };

        foreach (var anchor in requiredExistingSeams)
        {
            AssertContainsAnchor(source, PrepIndexRelativePath, anchor, "existing prep orchestration seam");
        }

        var injectDirectiveCount = Regex.Matches(source, "(?m)^@inject\\s+", RegexOptions.CultureInvariant).Count;
        Assert.Equal(
            2,
            injectDirectiveCount);

        var forbiddenNewSeams = new[]
        {
            "@inject IJSRuntime",
            "@inject PipelineService",
            "@inject IPipelineService",
            "@inject ProtectedLocalStorage",
            "@inject ProtectedSessionStorage",
            "localStorage",
            "sessionStorage",
            "File.WriteAllText",
            "File.AppendAllText",
            "Directory.CreateDirectory",
            "JsonSerializer.Serialize",
            "JsonSerializer.Deserialize"
        };

        foreach (var forbiddenAnchor in forbiddenNewSeams)
        {
            Assert.DoesNotContain(
                forbiddenAnchor,
                source,
                StringComparison.Ordinal);
        }
    }

    private static void AssertContainsAnchor(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required prep mobile-contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read prep mobile-contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
