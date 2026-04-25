using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Prep;

public sealed class WorkstationPrepDesignContractTests
{
    private const string PrepIndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor";
    private const string PrepCssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor.css";

    private static readonly string[] RequiredPrepUtilitySelectors =
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

        Assert.Contains("/prep", routeTemplates, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("/prep/pipeline", routeTemplates, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrepPage_DeclaresPipelineAndDiagnosticsContracts()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);

        AssertHasInjectDirective(source, "BlazorWorkspace", "Workspace", PrepIndexRelativePath);
        AssertHasInjectDirective(source, "PrepRunSession", "Session", PrepIndexRelativePath);

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
                     "pipeline-dashboard-table-wrap",
                     "pipeline-dashboard-table"
                 })
        {
            AssertHasClassToken(source, classToken, PrepIndexRelativePath);
        }

        foreach (var diagnosticsAnchor in new[]
                 {
                     "Pipeline Dashboard",
                     "Runtime readiness snapshot",
                     "Progress timeline",
                     "Last typed request snapshot",
                     "Last artifact set"
                 })
        {
            AssertContainsText(source, diagnosticsAnchor, PrepIndexRelativePath, "Prep diagnostics surface anchor");
        }
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

        foreach (var forbiddenAnchor in new[]
                 {
                     "prep-flow-card",
                     "prep-flow-header",
                     "prep-flow-actions"
                 })
        {
            Assert.DoesNotContain(forbiddenAnchor, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PrepCss_DeclaresTokenizedPrepUtilitiesWithoutForbiddenOverrides()
    {
        var source = ReadRepoFile(PrepCssRelativePath);
        var selectors = ExtractPrepSelectors(source);

        foreach (var selector in RequiredPrepUtilitySelectors)
        {
            Assert.Contains(selector, selectors, StringComparer.Ordinal);
        }

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

        AssertContainsText(source, "var(--bit-clr-bg-sec)", PrepCssRelativePath, "Bit tokenized tonal background");
        AssertContainsText(source, "var(--bit-clr-brd-sec)", PrepCssRelativePath, "Bit tokenized subtle border");
        AssertContainsText(source, "var(--bit-clr-fg-secondary)", PrepCssRelativePath, "Bit tokenized secondary foreground");
    }

    [Fact]
    public void PrepMobileLayout_DeclaresResponsiveDensityContracts()
    {
        var source = ReadRepoFile(PrepIndexRelativePath);
        var css = ReadRepoFile(PrepCssRelativePath);

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

        AssertHasClassToken(source, "pipeline-action-bar__stats", PrepIndexRelativePath);
        AssertHasClassToken(source, "pipeline-action-bar__controls", PrepIndexRelativePath);
        AssertHasClassToken(source, "pipeline-action-button", PrepIndexRelativePath);

        AssertMatches(
            source,
            "data-label\\s*=\\s*[\"']Action[\"']",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "table action column mobile label");

        AssertMatches(
            source,
            "data-label\\s*=\\s*[\"']Message[\"']",
            RegexOptions.CultureInvariant,
            PrepIndexRelativePath,
            "table message column mobile label");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile breakpoint declaration");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?min-height\\s*:\\s*44px",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "touch target minimum size rule in mobile breakpoint");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?font-size\\s*:\\s*16px",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile input font-size guard");

        AssertMatches(
            css,
            "touch-action\\s*:\\s*manipulation",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "global touch-action manipulation guard");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?padding-bottom\\s*:\\s*calc\\(1rem\\s*\\+\\s*env\\(safe-area-inset-bottom\\)\\)",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile safe-area bottom padding");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?pipeline-dashboard-table-wrap[\\s\\S]*?touch-action\\s*:\\s*pan-y",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "mobile table wrapper pan-y touch-action rule");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?pipeline-dashboard-table[\\s\\S]*?content\\s*:\\s*attr\\(data-label\\)",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "stacked table label rule in mobile breakpoint");

        AssertMatches(
            css,
            "@media\\s*\\(max-width:\\s*768px\\)[\\s\\S]*?inspector-surfaces[\\s\\S]*?max-height\\s*:\\s*10rem",
            RegexOptions.CultureInvariant,
            PrepCssRelativePath,
            "inspector mobile log clamp");
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

    private static void AssertContainsText(string source, string expectedText, string relativePath, string description)
    {
        Assert.True(
            source.Contains(expectedText, StringComparison.Ordinal),
            $"Missing {description} in '{relativePath}'. Expected text: {expectedText}");
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
                && Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md and host/.");
    }
}
