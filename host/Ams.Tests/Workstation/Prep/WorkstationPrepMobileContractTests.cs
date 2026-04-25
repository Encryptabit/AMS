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

        foreach (var contractValue in new[]
                 {
                     "pipeline-action-bar",
                     "active-tasks-table",
                     "history-table",
                     "inspector-surfaces"
                 })
        {
            AssertHasDataAttributeValue(source, "data-ams-prep-mobile-contract", contractValue, PrepIndexRelativePath);
        }

        foreach (var classToken in new[]
                 {
                     "pipeline-action-bar",
                     "pipeline-action-bar__stats",
                     "pipeline-action-bar__controls",
                     "pipeline-action-button",
                     "pipeline-dashboard-table"
                 })
        {
            AssertHasClassToken(source, classToken, PrepIndexRelativePath);
        }

        AssertMatches(
            source,
            "data-label\\s*=\\s*[\"']Action[\"']",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "mobile table action label");

        AssertMatches(
            source,
            "data-label\\s*=\\s*[\"']Message[\"']",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "mobile table message label");
    }

    [Fact]
    public void PrepCss_DeclaresMobileBreakpointDensityRules()
    {
        var source = ReadRepoFile(PrepCssRelativePath);

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile breakpoint declaration");

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?min-height\\s*:\\s*44px",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "touch target minimum size rule in mobile breakpoint");

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?font-size\\s*:\\s*16px",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile input font-size guard");

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?pipeline-dashboard-table[\\s\\S]*?display\\s*:\\s*block",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "stacked table block flow rule in mobile breakpoint");

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?content\\s*:\\s*attr\\(data-label\\)",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "stacked table label content rule in mobile breakpoint");

        AssertMatches(
            source,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?inspector-surfaces[\\s\\S]*?max-height\\s*:\\s*10rem",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "inspector mobile log clamp");
    }

    [Fact]
    public void PrepIndex_RetainsWorkspaceAndSessionSeamsWithoutDirectPersistenceWrites()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        AssertHasInjectDirective(source, "BlazorWorkspace", "Workspace", PrepIndexRelativePath);
        AssertHasInjectDirective(source, "PrepRunSession", "Session", PrepIndexRelativePath);

        AssertMatches(
            source,
            "\\bWorkspace\\.",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "workspace usage seam");

        AssertMatches(
            source,
            "\\bSession\\.",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "session usage seam");

        var forbiddenNewSeamPatterns = new[]
        {
            "(?m)^@inject\\s+IJSRuntime\\b",
            "(?m)^@inject\\s+PipelineService\\b",
            "(?m)^@inject\\s+IPipelineService\\b",
            "(?m)^@inject\\s+ProtectedLocalStorage\\b",
            "(?m)^@inject\\s+ProtectedSessionStorage\\b",
            "\\blocalStorage\\b",
            "\\bsessionStorage\\b",
            "\\bFile\\.WriteAllText\\b",
            "\\bFile\\.AppendAllText\\b",
            "\\bDirectory\\.CreateDirectory\\b",
            "\\bJsonSerializer\\.Serialize\\b",
            "\\bJsonSerializer\\.Deserialize\\b"
        };

        foreach (var pattern in forbiddenNewSeamPatterns)
        {
            AssertDoesNotMatch(
                source,
                pattern,
                RegexOptions.CultureInvariant,
                PrepIndexRelativePath,
                "forbidden persistence/pipeline seam");
        }
    }

    private static void AssertHasInjectDirective(string source, string serviceType, string alias, string relativePath)
    {
        var pattern = $"(?m)^@inject\\s+{Regex.Escape(serviceType)}\\s+{Regex.Escape(alias)}\\s*$";
        AssertMatches(source, pattern, RegexOptions.CultureInvariant, relativePath, $"inject directive for {serviceType} {alias}");
    }

    private static void AssertHasDataAttributeValue(string source, string attribute, string value, string relativePath)
    {
        var pattern = $"{Regex.Escape(attribute)}\\s*=\\s*[\"']{Regex.Escape(value)}[\"']";
        AssertMatches(source, pattern, RegexOptions.CultureInvariant, relativePath, $"{attribute} contract '{value}'");
    }

    private static void AssertHasClassToken(string source, string classToken, string relativePath)
    {
        var pattern = $"\\b(?:class|Class)\\s*=\\s*[\"'][^\"']*\\b{Regex.Escape(classToken)}\\b[^\"']*[\"']";
        AssertMatches(source, pattern, RegexOptions.CultureInvariant, relativePath, $"class token '{classToken}'");
    }

    private static void AssertMatches(string source, string pattern, RegexOptions options, string relativePath, string description)
    {
        Assert.True(
            Regex.IsMatch(source, pattern, options),
            $"Missing {description} in '{relativePath}'. Pattern: {pattern}");
    }

    private static void AssertDoesNotMatch(string source, string pattern, RegexOptions options, string relativePath, string description)
    {
        Assert.False(
            Regex.IsMatch(source, pattern, options),
            $"Found {description} in '{relativePath}'. Pattern: {pattern}");
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
                && Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md and host/.");
    }
}
