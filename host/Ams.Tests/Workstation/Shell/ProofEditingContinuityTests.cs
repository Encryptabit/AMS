using System.Text.RegularExpressions;
using Ams.Workstation.Server.Components.Navigation;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class ProofEditingContinuityTests
{
    private const string IndexRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor";
    private const string OverviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/Overview.razor";
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string KeyboardShortcutsRelativePath = "host/Ams.Workstation.Server/wwwroot/js/keyboard-shortcuts.js";

    [Fact]
    public void ProofPages_DeclareExpectedContinuityRouteTemplates()
    {
        AssertRouteTemplates(IndexRelativePath, "/proof", "/proof/editing");
        AssertRouteTemplates(OverviewRelativePath, "/proof/overview");
        AssertRouteTemplates(ChapterReviewRelativePath, "/proof/{ChapterName}", "/proof/editing/{ChapterName}");
    }

    [Fact]
    public void IndexPage_ContainsOverviewPatternsAndChapterEntryAnchors()
    {
        var source = ReadRepoFile(IndexRelativePath);

        AssertContainsAnchor(
            source,
            IndexRelativePath,
            "StageRouteCatalog.GetModuleCanonicalPath(",
            "index page uses module canonical-path lookup helper");

        AssertContainsAnchor(
            source,
            IndexRelativePath,
            "StageRouteCatalog.ModuleIds.ProofOverview",
            "Book Overview card resolves through StageRouteCatalog module ID");

        AssertContainsAnchor(
            source,
            IndexRelativePath,
            "StageRouteCatalog.ModuleIds.ProofPatterns",
            "Error Patterns card resolves through StageRouteCatalog module ID");

        AssertContainsAnchor(
            source,
            IndexRelativePath,
            "StageRouteCatalog.BuildProofChapterCompatibilityPath(",
            "chapter list link uses compatibility helper");

        AssertDoesNotContainAnchor(
            source,
            IndexRelativePath,
            "BitLink Href=\"/proof/overview\"",
            "legacy hardcoded Book Overview route literal");

        AssertDoesNotContainAnchor(
            source,
            IndexRelativePath,
            "BitLink Href=\"/proof/patterns\"",
            "legacy hardcoded Error Patterns route literal");

        AssertDoesNotContainAnchor(
            source,
            IndexRelativePath,
            "/proof/{Uri.EscapeDataString(chapter)}",
            "legacy inline chapter compatibility path template");
    }

    [Fact]
    public void OverviewPage_ContainsChapterJumpNavigationSeam()
    {
        var source = ReadRepoFile(OverviewRelativePath);

        AssertContainsAnchor(
            source,
            OverviewRelativePath,
            "OnClick=\"() => NavigateToChapter(chapter.Name)\"",
            "overview chapter card click binding");

        AssertContainsAnchor(
            source,
            OverviewRelativePath,
            "private void NavigateToChapter(string chapterName)",
            "overview chapter jump method declaration");

        AssertContainsAnchor(
            source,
            OverviewRelativePath,
            "Navigation.NavigateTo(StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName));",
            "overview chapter jump compatibility path generation");

        AssertDoesNotContainAnchor(
            source,
            OverviewRelativePath,
            "Navigation.NavigateTo($\"/proof/{Uri.EscapeDataString(chapterName)}\")",
            "legacy inline chapter jump path literal");
    }

    [Fact]
    public void ChapterReview_ContainsChapterNavigationAndKeyboardBridgeAnchors()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContainsAnchor(
            source,
            ChapterReviewRelativePath,
            "public void OnChapterNav(string direction)",
            "keyboard bridge method declaration: OnChapterNav");

        AssertContainsAnchor(
            source,
            ChapterReviewRelativePath,
            "public void OnSwitchView(string direction)",
            "keyboard bridge method declaration: OnSwitchView");

        AssertContainsAnchor(
            source,
            ChapterReviewRelativePath,
            "public async Task OnNavigateItem(string direction)",
            "keyboard bridge method declaration: OnNavigateItem");

        AssertContainsAnchor(
            source,
            ChapterReviewRelativePath,
            "public void OnCrossNav(string direction)",
            "keyboard bridge method declaration: OnCrossNav");

        AssertContainsAnchor(
            source,
            ChapterReviewRelativePath,
            "Navigation.NavigateTo(StageRouteCatalog.BuildProofChapterCompatibilityPath(chapters[next]));",
            "chapter keyboard navigation compatibility path generation");

        AssertDoesNotContainAnchor(
            source,
            ChapterReviewRelativePath,
            "Navigation.NavigateTo($\"/proof/{Uri.EscapeDataString(chapters[next])}\")",
            "legacy keyboard chapter navigation path literal");
    }

    [Fact]
    public void KeyboardShortcuts_DispatchesRequiredProofNavigationHooks()
    {
        var source = ReadRepoFile(KeyboardShortcutsRelativePath);
        var invokedMethods = ExtractInvokeMethodTargets(source, KeyboardShortcutsRelativePath);

        var requiredTargets = new[]
        {
            "OnChapterNav",
            "OnSwitchView",
            "OnNavigateItem",
            "OnCrossNav"
        };

        var missingTargets = requiredTargets
            .Where(target => !invokedMethods.Contains(target, StringComparer.Ordinal))
            .ToArray();

        Assert.True(
            missingTargets.Length == 0,
            $"Missing keyboard bridge dispatch anchor(s) in '{KeyboardShortcutsRelativePath}': {string.Join(", ", missingTargets)}. Declared invokeMethodAsync targets: {string.Join(", ", invokedMethods.OrderBy(name => name, StringComparer.Ordinal))}.");

        AssertContainsAnchor(
            source,
            KeyboardShortcutsRelativePath,
            "_dotNetRef.invokeMethodAsync('OnChapterNav', 'next');",
            "chapter next dispatch");

        AssertContainsAnchor(
            source,
            KeyboardShortcutsRelativePath,
            "_dotNetRef.invokeMethodAsync('OnChapterNav', 'prev');",
            "chapter previous dispatch");

        AssertContainsAnchor(
            source,
            KeyboardShortcutsRelativePath,
            "_dotNetRef.invokeMethodAsync('OnCrossNav', 'errors-to-playback');",
            "cross-nav dispatch from errors to playback");

        AssertContainsAnchor(
            source,
            KeyboardShortcutsRelativePath,
            "_dotNetRef.invokeMethodAsync('OnCrossNav', 'playback-to-errors');",
            "cross-nav dispatch from playback to errors");
    }

    private static void AssertRouteTemplates(string relativePath, params string[] expectedTemplates)
    {
        var declaredTemplates = ExtractAndValidateRouteTemplates(relativePath);

        var missingTemplates = expectedTemplates
            .Where(expected => !declaredTemplates.Contains(expected, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            missingTemplates.Length == 0,
            $"Missing route template anchor(s) in '{relativePath}': {string.Join(", ", missingTemplates)}. Declared @page templates: {string.Join(", ", declaredTemplates)}.");
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
            $"No @page route directives found in '{relativePath}'. Proof continuity contracts require explicit route templates.");

        var invalidTemplates = routeTemplates
            .Select(template =>
            {
                var isValid = StageRouteCatalog.IsValidTemplate(template, out var reason);
                return new { Template = template, IsValid = isValid, Reason = reason };
            })
            .Where(entry => !entry.IsValid)
            .Select(entry => $"template='{entry.Template}', reason='{entry.Reason}'")
            .ToArray();

        Assert.True(
            invalidTemplates.Length == 0,
            $"Malformed route template contract failure(s) in '{relativePath}': {string.Join(" | ", invalidTemplates)}.");

        return routeTemplates;
    }

    private static IReadOnlyList<string> ExtractInvokeMethodTargets(string source, string relativePath)
    {
        var invokeCalls = Regex.Matches(
            source,
            "invokeMethodAsync\\s*\\((?<args>[^\\)]*)\\)",
            RegexOptions.CultureInvariant);

        Assert.True(
            invokeCalls.Count > 0,
            $"No invokeMethodAsync calls found in '{relativePath}'. Keyboard bridge hooks must dispatch to ChapterReview JSInvokable methods.");

        var targets = new HashSet<string>(StringComparer.Ordinal);
        var malformedPayloads = new List<string>();

        foreach (Match invokeCall in invokeCalls)
        {
            var args = invokeCall.Groups["args"].Value;
            var targetMatch = Regex.Match(args, "^\\s*['\"](?<name>[^'\"]+)['\"]", RegexOptions.CultureInvariant);
            if (!targetMatch.Success)
            {
                malformedPayloads.Add(invokeCall.Value.Trim());
                continue;
            }

            targets.Add(targetMatch.Groups["name"].Value);
        }

        Assert.True(
            malformedPayloads.Count == 0,
            $"Malformed invokeMethodAsync payload(s) in '{relativePath}': {string.Join(" | ", malformedPayloads)}.");

        return targets.ToArray();
    }

    private static void AssertContainsAnchor(string source, string relativePath, string anchor, string anchorDescription)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof continuity anchor '{anchorDescription}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContainAnchor(string source, string relativePath, string anchor, string anchorDescription)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale proof route anchor '{anchorDescription}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required continuity source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read continuity source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
