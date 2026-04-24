using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepDesignContractTests
{
    private const string PrepIndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor";
    private const string PrepScssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor.scss";

    private static readonly string[] AllowedTopLevelSelectorPrefixes =
    [
        ".prep-",
        ".pipeline-",
        ".inspector-",
        ".history-",
        ".queue-builder-",
        ".throughput-"
    ];

    [Fact]
    public void PrepPage_DeclaresCanonicalAndCompatibilityRoutes()
    {
        var routeTemplates = ExtractAndValidateRouteTemplates(PrepIndexRelativePath);

        Assert.True(
            routeTemplates.Contains("/prep", StringComparer.OrdinalIgnoreCase),
            $"Missing canonical prep compatibility alias '/prep' in '{PrepIndexRelativePath}'. Declared templates: {string.Join(", ", routeTemplates)}.");

        Assert.True(
            routeTemplates.Contains("/prep/pipeline", StringComparer.OrdinalIgnoreCase),
            $"Missing canonical prep module route '/prep/pipeline' in '{PrepIndexRelativePath}'. Declared templates: {string.Join(", ", routeTemplates)}.");
    }

    [Fact]
    public void PrepPage_IsBitFree_AndDeclaresVanillaDiagnosticSurfaces()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        var bitMatches = Regex.Matches(source, "<Bit[A-Z][A-Za-z0-9]*", RegexOptions.CultureInvariant);
        if (bitMatches.Count > 0)
        {
            var offenders = bitMatches
                .Select(m => $"{PrepIndexRelativePath}:{GetLineNumber(source, m.Index)}:{m.Value}")
                .ToArray();
            Assert.Fail(
                $"Found {bitMatches.Count} Bit.BlazorUI tag occurrence(s) in '{PrepIndexRelativePath}'. Prep must be fully vanilla after M007/S02. Offenders:\n{string.Join("\n", offenders)}");
        }

        var requiredLiteralAnchors = new[]
        {
            "<AmsButton",
            "<AmsTag",
            "<AmsProgress",
            "<AmsTabs",
            "<AmsTabItem",
            "<AmsDialog",
            "<AmsRangeSlider",
            "<input type=\"checkbox\"",
            "data-ams-prep=\"queue-builder\"",
            "data-ams-prep=\"throughput\"",
            "data-ams-prep=\"pipeline-tabs\"",
            "data-ams-prep=\"pipeline-settings-button\""
        };

        foreach (var anchor in requiredLiteralAnchors)
        {
            if (!source.Contains(anchor, StringComparison.Ordinal))
            {
                Assert.Fail(
                    $"Missing required vanilla/native anchor '{anchor}' in '{PrepIndexRelativePath}'. Migrated Prep page must expose this surface so UAT and contract tests have a stable selector.");
            }
        }

        // <select> uses .ams-select as one class in a possibly-multi-class attribute
        // (e.g. class="ams-select prep-field-min"), so match the class token within the attribute.
        if (!Regex.IsMatch(source, "<select\\b[^>]*\\bclass\\s*=\\s*\"[^\"]*\\bams-select\\b", RegexOptions.CultureInvariant))
        {
            Assert.Fail(
                $"Missing required native '<select class=\"... ams-select ...\">' anchor in '{PrepIndexRelativePath}'. Operator controls must use the project-owned ams-select class so post-Bit UAT can target the dropdown.");
        }

        if (!Regex.IsMatch(source, "data-ams-inspector-section=\"", RegexOptions.CultureInvariant))
        {
            Assert.Fail(
                $"Missing required anchor 'data-ams-inspector-section=\"...\"' in '{PrepIndexRelativePath}'. Inspector panel must expose at least one tagged section for UAT parity.");
        }

        var requiredSectionHeaders = new[]
        {
            "Pipeline Dashboard",
            "Queue Builder",
            "Pipeline Throughput",
            "Runtime",
            "Pipeline settings"
        };

        foreach (var header in requiredSectionHeaders)
        {
            if (!source.Contains(header, StringComparison.Ordinal))
            {
                Assert.Fail(
                    $"Missing required diagnostics section header '{header}' in '{PrepIndexRelativePath}'. Operator-facing section label is part of the Prep UI contract.");
            }
        }
    }

    [Fact]
    public void PrepPage_DoesNotReintroduceForbiddenInlinePatterns()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        AssertDoesNotMatch(
            source,
            "<style\\b",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            PrepIndexRelativePath,
            "embedded <style> block");

        AssertDoesNotMatch(
            source,
            "\\sstyle\\s*=",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            PrepIndexRelativePath,
            "inline style= attribute (progress fills are owned by <AmsProgress>)");

        AssertDoesNotMatch(
            source,
            "#[0-9a-fA-F]{3,8}\\b",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "hardcoded hex color literal");

        var forbiddenLegacyAnchors = new[]
        {
            "prep-flow-card",
            "prep-flow-header",
            "prep-flow-actions"
        };

        foreach (var forbiddenAnchor in forbiddenLegacyAnchors)
        {
            Assert.DoesNotContain(
                forbiddenAnchor,
                source,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PrepScss_UsesAmsTokensAndApprovedSelectors()
    {
        var source = ReadRepoFile(PrepScssRelativePath);

        var bitTokenMatches = Regex.Matches(source, "var\\(--bit-clr-[A-Za-z0-9-]+\\)", RegexOptions.CultureInvariant);
        if (bitTokenMatches.Count > 0)
        {
            var offenders = bitTokenMatches
                .Select(m => $"{PrepScssRelativePath}:{GetLineNumber(source, m.Index)}:{m.Value}")
                .ToArray();
            Assert.Fail(
                $"Found {bitTokenMatches.Count} Bit custom-property reference(s) in '{PrepScssRelativePath}'. Prep SCSS must consume only --ams-* tokens. Offenders:\n{string.Join("\n", offenders)}");
        }

        Assert.True(
            Regex.IsMatch(source, "var\\(--ams-color-[A-Za-z0-9-]+\\)", RegexOptions.CultureInvariant),
            $"Expected at least one 'var(--ams-color-*)' reference in '{PrepScssRelativePath}' to prove token consumption.");

        Assert.True(
            Regex.IsMatch(source, "(?m)^\\.prep-grid-12\\s*\\{", RegexOptions.CultureInvariant),
            $"Expected top-level '.prep-grid-12' selector in '{PrepScssRelativePath}' — the 12-column pipeline grid block is part of the Prep layout contract.");

        var disallowedSelectors = ExtractDisallowedTopLevelSelectors(source);
        if (disallowedSelectors.Count > 0)
        {
            Assert.Fail(
                $"Found {disallowedSelectors.Count} disallowed top-level selector(s)/at-rule(s) in '{PrepScssRelativePath}'. Every top-level block must be a '@use' / '@forward' directive, a '@media' / '@for' at-rule, or a class selector starting with one of: {string.Join(", ", AllowedTopLevelSelectorPrefixes)}.\n{string.Join("\n", disallowedSelectors)}");
        }
    }

    private static IReadOnlyList<string> ExtractDisallowedTopLevelSelectors(string source)
    {
        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n');
        var disallowed = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (raw.Length == 0)
            {
                continue;
            }

            // Top-level = starts at column 0, not whitespace, not a comment.
            var first = raw[0];
            if (first == ' ' || first == '\t' || first == '}' || first == '/')
            {
                continue;
            }

            var trimmed = raw.TrimEnd();

            // Only consider block-opening lines (end with '{') or directive lines (@use/@forward ending ';').
            if (trimmed.EndsWith(";", StringComparison.Ordinal))
            {
                if (trimmed.StartsWith("@use", StringComparison.Ordinal)
                    || trimmed.StartsWith("@forward", StringComparison.Ordinal))
                {
                    continue;
                }

                disallowed.Add($"{PrepScssRelativePath}:{i + 1}:{trimmed}");
                continue;
            }

            if (!trimmed.EndsWith("{", StringComparison.Ordinal))
            {
                continue;
            }

            if (trimmed.StartsWith("@media", StringComparison.Ordinal)
                || trimmed.StartsWith("@for", StringComparison.Ordinal)
                || trimmed.StartsWith("@supports", StringComparison.Ordinal)
                || trimmed.StartsWith("@keyframes", StringComparison.Ordinal))
            {
                continue;
            }

            var selectorBody = trimmed[..^1].TrimEnd();
            var isAllowedClassSelector = AllowedTopLevelSelectorPrefixes.Any(prefix =>
                selectorBody.StartsWith(prefix, StringComparison.Ordinal));

            if (!isAllowedClassSelector)
            {
                disallowed.Add($"{PrepScssRelativePath}:{i + 1}:{trimmed}");
            }
        }

        return disallowed;
    }

    private static IReadOnlyList<string> ExtractAndValidateRouteTemplates(string relativePath)
    {
        var source = ReadRepoFile(relativePath);
        var lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var routeTemplates = new List<string>();
        var malformedDirectives = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("@page", StringComparison.Ordinal))
            {
                continue;
            }

            var match = Regex.Match(trimmed, "^@page\\s+\"(?<template>[^\"]+)\"$", RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                malformedDirectives.Add(trimmed);
                continue;
            }

            routeTemplates.Add(match.Groups["template"].Value);
        }

        Assert.True(
            malformedDirectives.Count == 0,
            $"Malformed @page directive(s) in '{relativePath}': {string.Join(" | ", malformedDirectives)}.");

        Assert.True(
            routeTemplates.Count > 0,
            $"No @page route directives found in '{relativePath}'. Prep route continuity requires explicit route templates.");

        return routeTemplates;
    }

    private static void AssertDoesNotMatch(string source, string pattern, RegexOptions options, string relativePath, string description)
    {
        var match = Regex.Match(source, pattern, options);
        if (!match.Success)
        {
            return;
        }

        var lineNumber = GetLineNumber(source, match.Index);
        Assert.Fail($"Found forbidden {description} in '{relativePath}':{lineNumber}. Pattern: {pattern}. Matched: '{match.Value}'.");
    }

    private static int GetLineNumber(string source, int index)
    {
        var line = 1;
        var upper = Math.Min(index, source.Length);
        for (var i = 0; i < upper; i++)
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
            throw new XunitException($"Required prep design-contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read prep design-contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
