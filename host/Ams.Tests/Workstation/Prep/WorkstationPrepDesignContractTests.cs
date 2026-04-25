using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepDesignContractTests
{
    private const string PrepIndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor";
    private const string PrepCssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor.css";

    private static readonly string[] ExpectedCustomUtilitySelectors =
    [
        "prep-control-section",
        "prep-control-sections",
        "prep-field-min",
        "prep-field-min--wide",
        "prep-grid",
        "prep-grid--compact",
        "prep-header-extra",
        "prep-inline-note",
        "prep-kv",
        "prep-mono",
        "prep-page",
        "prep-running-status",
        "prep-table",
        "prep-warning-list",
        "prep-wrap"
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
    public void PrepPage_ContainsBitFirstAnchors_AndRequiredDiagnosticsSurfaces()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        var requiredBitAnchors = new[]
        {
            "<BitStack Horizontal=\"true\"",
            "<BitCard",
            "<BitMessage",
            "<BitDropdown",
            "<BitCheckbox",
            "<BitButton Variant=\"BitVariant.Fill\"",
            "<BitProgress"
        };

        var requiredDiagnosticsAnchors = new[]
        {
            "Pipeline Dashboard",
            "Run batch prep",
            "Queue Builder + Pipeline Throughput",
            "Runtime readiness snapshot",
            "Last typed request snapshot",
            "Option normalization warnings",
            "Progress timeline",
            "Last artifact set",
            "Last book-index request"
        };

        foreach (var anchor in requiredBitAnchors)
        {
            AssertContainsAnchor(source, PrepIndexRelativePath, anchor, "Bit-first composition anchor");
        }

        foreach (var anchor in requiredDiagnosticsAnchors)
        {
            AssertContainsAnchor(source, PrepIndexRelativePath, anchor, "Prep diagnostics surface anchor");
        }

        var bitCardCount = Regex.Matches(source, "<BitCard\\b", RegexOptions.CultureInvariant).Count;
        Assert.True(
            bitCardCount >= 5,
            $"Expected at least five BitCard layout anchors in '{PrepIndexRelativePath}' to maintain Bit-first sectional composition, but found {bitCardCount}.");

        var diagnosticsTableCount = Regex.Matches(source, "<table class=\"prep-table\">", RegexOptions.CultureInvariant).Count;
        Assert.True(
            diagnosticsTableCount >= 2,
            $"Expected at least two diagnostics table anchors in '{PrepIndexRelativePath}' (active tasks and progress timeline), but found {diagnosticsTableCount}.");
    }

    [Fact]
    public void PrepPage_DoesNotReintroduceForbiddenCustomStylingPatterns()
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
            "inline style attribute");

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
    public void PrepCss_StaysWithinAllowlistedUtilitySelectors_AndTokenizedStyles()
    {
        var source = ReadRepoFile(PrepCssRelativePath);
        var selectors = ExtractPrepSelectors(source);

        Assert.Equal(ExpectedCustomUtilitySelectors, selectors);

        AssertDoesNotMatch(
            source,
            "#[0-9a-fA-F]{3,8}\\b",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "hardcoded hex color literal");

        AssertDoesNotMatch(
            source,
            "border-radius\\s*:",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            PrepCssRelativePath,
            "non-zero-radius custom chrome declaration");

        AssertDoesNotMatch(
            source,
            "box-shadow\\s*:",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            PrepCssRelativePath,
            "custom shadow chrome declaration");

        AssertDoesNotMatch(
            source,
            "!important",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            PrepCssRelativePath,
            "style override via !important");

        AssertContainsAnchor(source, PrepCssRelativePath, "var(--bit-clr-bg-sec)", "Bit tokenized tonal background");
        AssertContainsAnchor(source, PrepCssRelativePath, "var(--bit-clr-brd-sec)", "Bit tokenized subtle border");
        AssertContainsAnchor(source, PrepCssRelativePath, "var(--bit-clr-fg-secondary)", "Bit tokenized secondary foreground");
    }

    [Fact]
    public void PrepMobileLayout_DeclaresResponsiveDensityContracts()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);
        var css = ReadRepoFile(PrepCssRelativePath);

        var requiredMarkupAnchors = new[]
        {
            "data-ams-prep-mobile-contract=\"pipeline-action-bar\"",
            "pipeline-action-bar__stats",
            "pipeline-action-bar__controls",
            "pipeline-action-button",
            "prep-running-status",
            "pipeline-grid-item--top"
        };

        var requiredCssAnchors = new[]
        {
            "@media (max-width: 768px)",
            ".prep-page ::deep .pipeline-action-bar button",
            ".prep-page ::deep .pipeline-layout-grid > .pipeline-grid-item--top",
            "min-height: 44px;",
            "font-size: 16px;"
        };

        foreach (var anchor in requiredMarkupAnchors)
        {
            AssertContainsAnchor(source, PrepIndexRelativePath, anchor, "Prep mobile markup contract anchor");
        }

        foreach (var anchor in requiredCssAnchors)
        {
            AssertContainsAnchor(css, PrepCssRelativePath, anchor, "Prep mobile responsive style anchor");
        }
    }

    private static string[] ExtractPrepSelectors(string source)
    {
        var selectorMatches = Regex.Matches(
            source,
            "(?m)^\\.(?<selector>prep-[a-z0-9-]+)(?=\\s*(?:,|\\{))",
            RegexOptions.CultureInvariant);

        var selectors = selectorMatches
            .Select(match => match.Groups["selector"].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(selector => selector, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            selectors.Length > 0,
            $"Expected prep scoped CSS selectors in '{PrepCssRelativePath}', but none were found.");

        return selectors;
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

    private static void AssertContainsAnchor(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotMatch(string source, string pattern, RegexOptions options, string relativePath, string description)
    {
        Assert.False(
            Regex.IsMatch(source, pattern, options),
            $"Found forbidden {description} in '{relativePath}'. Pattern: {pattern}");
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
