using System.Text.RegularExpressions;

namespace Ams.Tests.Documentation;

public sealed class EngineeringContractDocsTests
{
    [Fact]
    public void CodeStyle_Contains_AllRequiredClassificationBuckets()
    {
        var contract = ReadRepoFile("CODE-STYLE.md");

        Assert.Contains("## 1. Adopt Directly", contract);
        Assert.Contains("## 2. Adapt To The Current Stack", contract);
        Assert.Contains("## 3. Future Zig-Specific Rules", contract);
        Assert.Contains("## 4. Deferred", contract);
        Assert.Contains("## 5. Mechanical Enforcement Requirements", contract);
        Assert.Contains("## 7. AMS-Specific Product Rules", contract);
    }

    [Fact]
    public void CodeStyle_Anchors_Contract_To_CurrentAmsSeams()
    {
        var contract = ReadRepoFile("CODE-STYLE.md");

        foreach (var seam in new[]
                 {
                     "IWorkspace",
                     "CliWorkspace",
                     "BlazorWorkspace",
                     "PipelineService",
                     "ValidationService",
                     "host/Ams.Cli/Program.cs",
                     "host/Ams.Workstation.Server/Program.cs"
                 })
        {
            Assert.Contains(seam, contract);
        }

        Assert.Contains(".NET", contract);
        Assert.Contains("Blazor Server", contract);
        Assert.Contains("CLI host over shared Core/application services", contract);
        Assert.Contains("not a literal Zig transplant", contract);
    }

    [Fact]
    public void CodeStyle_Deferred_Section_Calls_Out_LaterEnforcement_Work()
    {
        var contract = ReadRepoFile("CODE-STYLE.md");
        var deferredSection = GetSection(contract, 4, 5);

        Assert.Contains("binding repo-wide today", deferredSection);
        Assert.Contains("Repo-wide mechanical style gates", deferredSection);
        Assert.Contains("Large-file and legacy host cleanup", deferredSection);
        Assert.Contains("evidence for later audit/refactor slices", deferredSection);
    }

    [Fact]
    public void Readme_Points_To_CodeStyle_And_Describes_CurrentHostDirection()
    {
        var readme = ReadRepoFile("README.md");
        var normalizedReadme = Normalize(readme);

        Assert.Contains("## start here", normalizedReadme);
        Assert.Contains("code-style.md", normalizedReadme);
        Assert.Contains("(code-style.md)", normalizedReadme);
        Assert.Contains("cli", normalizedReadme);
        Assert.Contains("blazor", normalizedReadme);
        Assert.Contains("workstation", normalizedReadme);
        Assert.Contains("shared core", normalizedReadme);
        Assert.Contains("future zig", normalizedReadme);
    }

    [Fact]
    public void Readme_DoesNot_Reintroduce_Stale_Avalonia_First_Messaging()
    {
        var normalizedReadme = Normalize(ReadRepoFile("README.md"));

        Assert.DoesNotContain("avalonia-first", normalizedReadme);
        Assert.DoesNotContain("avaloniaui", normalizedReadme);
        Assert.DoesNotContain("using zig + avalonia", normalizedReadme);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        return File.ReadAllText(Path.Combine(repoRoot, relativePath));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CODE-STYLE.md")) &&
                Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md.");
    }

    private static string GetSection(string markdown, int startSection, int nextSection)
    {
        var startHeading = $"## {startSection}.";
        var nextHeading = $"\n## {nextSection}.";

        var startIndex = markdown.IndexOf(startHeading, StringComparison.Ordinal);
        Assert.True(startIndex >= 0, $"Could not find section {startSection}.");

        var nextIndex = markdown.IndexOf(nextHeading, startIndex, StringComparison.Ordinal);
        return nextIndex >= 0
            ? markdown[startIndex..nextIndex]
            : markdown[startIndex..];
    }

    private static string Normalize(string markdown)
    {
        return Regex.Replace(markdown, @"\s+", " ").Trim().ToLowerInvariant();
    }
}
