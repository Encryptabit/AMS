using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.UI;

/// <summary>
/// M007 S01 fail-closed guardrail contracts for the vanilla Blazor UI foundation.
/// Extending the allowlist: S02/S03/S04 append to <see cref="MigratedRazorFiles"/> and
/// <see cref="MigratedRazorDirectories"/> as they migrate additional surfaces off Bit.BlazorUI.
/// </summary>
public sealed class M007FoundationContractTests
{
    private const string WorkstationRelativeRoot = "host/Ams.Workstation.Server";
    private const string GeneratedAppCssRelativePath = "host/Ams.Workstation.Server/wwwroot/css/app.css";
    private const string AppRazorRelativePath = "host/Ams.Workstation.Server/Components/App.razor";

    // Allowlist of fully-migrated .razor files. S02/S03/S04 tasks append here as they migrate.
    private static readonly string[] MigratedRazorFiles =
    [
        "host/Ams.Workstation.Server/Components/App.razor",
        "host/Ams.Workstation.Server/Components/Pages/Home.razor",
        "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor",
    ];

    // Directories whose .razor files are entirely migrated. All *.razor under these must be Bit-free.
    private static readonly string[] MigratedRazorDirectories =
    [
        "host/Ams.Workstation.Server/Components/UI",
        "host/Ams.Workstation.Server/Components/Pages/Prep",
    ];

    private static readonly Regex BitTagPattern = new("<Bit[A-Z]", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    [Fact]
    public void MigratedRazorFiles_ContainNoBitBlazorUiTags()
    {
        var violations = new List<string>();

        foreach (var relativePath in EnumerateMigratedRazorFiles())
        {
            var source = ReadRepoFile(relativePath);
            var match = BitTagPattern.Match(source);
            if (!match.Success)
            {
                continue;
            }

            var lineNumber = GetLineNumber(source, match.Index);
            var offendingTag = ExtractOffendingTag(source, match.Index);
            violations.Add($"'{relativePath}':{lineNumber} contains forbidden Bit.BlazorUI tag '{offendingTag}'. Migrate it to a project-owned vanilla primitive or remove it before merging.");
        }

        Assert.True(
            violations.Count == 0,
            $"M007 anti-Bit guardrail failed. Migrated files must not reintroduce <Bit*> tags.\n{string.Join("\n", violations)}");
    }

    [Fact]
    public void EveryRazorScssHasSiblingRazorCss()
    {
        var repoRoot = FindRepoRoot();
        var workstationRoot = Path.Combine(repoRoot, WorkstationRelativeRoot);

        if (!Directory.Exists(workstationRoot))
        {
            throw new XunitException($"Workstation project root not found at '{workstationRoot}'. Cannot verify Sass↔CSS pairing.");
        }

        var scssFiles = Directory.EnumerateFiles(workstationRoot, "*.razor.scss", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            scssFiles.Length > 0,
            $"Expected at least one *.razor.scss under '{WorkstationRelativeRoot}', but found none. Sass toolchain may not be wired.");

        var missing = new List<string>();

        foreach (var scssPath in scssFiles)
        {
            var expectedCssPath = scssPath[..^".scss".Length] + ".css";
            if (!File.Exists(expectedCssPath))
            {
                var scssRelative = Path.GetRelativePath(repoRoot, scssPath).Replace('\\', '/');
                var cssRelative = Path.GetRelativePath(repoRoot, expectedCssPath).Replace('\\', '/');
                missing.Add($"'{scssRelative}' has no sibling '{cssRelative}' on disk. Either (a) the file is a partial and must be renamed with a leading underscore (e.g. '_{Path.GetFileName(scssPath)}'), or (b) the Sass compile failed and the CSS was not emitted — run 'dotnet build {WorkstationRelativeRoot}' and inspect build output.");
            }
        }

        Assert.True(
            missing.Count == 0,
            $"Sass↔CSS pairing contract failed.\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void GeneratedAppCss_DoesNotReferenceBitBlazorUiContentPaths()
    {
        var source = ReadRepoFile(GeneratedAppCssRelativePath);

        var match = Regex.Match(source, @"_content/Bit\.BlazorUI(?:\.Icons)?/", RegexOptions.CultureInvariant);
        Assert.False(
            match.Success,
            $"Generated '{GeneratedAppCssRelativePath}' references '{(match.Success ? match.Value : "_content/Bit.BlazorUI/")}' at offset {match.Index}. Project-owned global CSS must not import Bit.BlazorUI content assets. Bit asset <link> / <script> tags in App.razor are still permitted until S04, but the generated app.css is project-owned and must stay clean.");
    }

    [Fact]
    public void AppRazor_SetsDataAmsThemeAndDoesNotUseBitThemeDefault()
    {
        var source = ReadRepoFile(AppRazorRelativePath);

        Assert.True(
            Regex.IsMatch(source, "data-ams-theme\\s*=\\s*\"dark\"", RegexOptions.CultureInvariant),
            $"'{AppRazorRelativePath}' must declare data-ams-theme=\"dark\" on its root <html> element so the vanilla theme cascade has a dark initial state. See M007-UI-FOUNDATION §3.7 and MEM077.");

        Assert.False(
            Regex.IsMatch(source, "bit-theme-default\\s*=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase),
            $"'{AppRazorRelativePath}' still declares 'bit-theme-default='. The Bit.BlazorUI theme attribute has been replaced by data-ams-theme for M007. Remove the bit-theme-default attribute.");
    }

    private static IEnumerable<string> EnumerateMigratedRazorFiles()
    {
        foreach (var file in MigratedRazorFiles)
        {
            yield return file;
        }

        var repoRoot = FindRepoRoot();
        foreach (var dir in MigratedRazorDirectories)
        {
            var fullDir = Path.Combine(repoRoot, dir);
            if (!Directory.Exists(fullDir))
            {
                throw new XunitException($"Migrated razor directory '{dir}' does not exist at '{fullDir}'. Update MigratedRazorDirectories if the directory was renamed or removed.");
            }

            foreach (var razorPath in Directory.EnumerateFiles(fullDir, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                yield return Path.GetRelativePath(repoRoot, razorPath).Replace('\\', '/');
            }
        }
    }

    private static string ExtractOffendingTag(string source, int matchIndex)
    {
        var end = matchIndex;
        while (end < source.Length && source[end] != ' ' && source[end] != '\n' && source[end] != '\r' && source[end] != '\t' && source[end] != '>' && source[end] != '/')
        {
            end++;
        }
        return source.Substring(matchIndex, end - matchIndex);
    }

    private static int GetLineNumber(string source, int index)
    {
        var line = 1;
        for (var i = 0; i < index && i < source.Length; i++)
        {
            if (source[i] == '\n')
            {
                line++;
            }
        }
        return line;
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required M007 foundation contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read M007 foundation contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
