using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class WorkstationProofDesignContractTests
{
    private const string PickupsScssRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor.scss";
    private const string WaveformPlayerRelativePath = "host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor";
    private const string ToastContainerRelativePath = "host/Ams.Workstation.Server/Components/Shared/ToastContainer.razor";

    private static readonly string[] ProofPages =
    [
        "host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor",
        "host/Ams.Workstation.Server/Components/Pages/Proof/Overview.razor",
        "host/Ams.Workstation.Server/Components/Pages/Proof/ErrorPatterns.razor",
        "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor",
        "host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor"
    ];

    private static readonly string[] SharedComponents =
    [
        "host/Ams.Workstation.Server/Components/Shared/ErrorsView.razor",
        "host/Ams.Workstation.Server/Components/Shared/SentenceList.razor",
        "host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor",
        "host/Ams.Workstation.Server/Components/Shared/SentenceErrorCard.razor",
        "host/Ams.Workstation.Server/Components/Shared/ChapterCard.razor",
        "host/Ams.Workstation.Server/Components/Shared/PatternCard.razor",
        "host/Ams.Workstation.Server/Components/Shared/StatCard.razor",
        "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor",
        "host/Ams.Workstation.Server/Components/Shared/IgnoreErrorModal.razor",
        "host/Ams.Workstation.Server/Components/Shared/DiffView.razor",
        "host/Ams.Workstation.Server/Components/Shared/ToastContainer.razor"
    ];

    private static readonly string[] ProofScssFiles =
    [
        "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor.scss",
        "host/Ams.Workstation.Server/Components/Pages/Proof/ErrorPatterns.razor.scss",
        "host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor.scss",
        "host/Ams.Workstation.Server/Components/Pages/Proof/Overview.razor.scss",
        "host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/ChapterCard.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/DiffView.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/ErrorsView.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/IgnoreErrorModal.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/PatternCard.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/SentenceErrorCard.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/SentenceList.razor.scss",
        "host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor.scss"
    ];

    private static readonly string[] AllowedTopLevelSelectorPrefixes =
    [
        ".proof-",
        ".overview-",
        ".chapters",
        ".chapter-",
        ".playback-",
        ".pattern-",
        ".sentence-",
        ".diff-",
        ".error-",
        ".errors-",
        ".waveform-",
        ".crx-",
        ".ignore-",
        ".stat-",
        ".metric",
        ".label",
        ".value",
        ".padding-"
    ];

    private static readonly IReadOnlyDictionary<string, HashSet<string>> AllowedHexColorsByFile = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
    {
        [WaveformPlayerRelativePath] = new(StringComparer.OrdinalIgnoreCase)
        {
            "#4a9eff",
            "#1177bb",
            "#ffffff"
        },
        [ToastContainerRelativePath] = new(StringComparer.OrdinalIgnoreCase)
        {
            "#1e3a5f",
            "#b8d4f0",
            "#4a9eff",
            "#1a3a2a",
            "#a8d8b8",
            "#4caf50",
            "#3a3520",
            "#e0d0a0",
            "#ff9800",
            "#3a1a1a",
            "#e0a0a0",
            "#f44336"
        }
    };

    [Fact]
    public void ProofPages_PreserveCanonicalRouteDirectives()
    {
        var expectedRoutesByPage = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor"] = ["/proof", "/proof/editing"],
            ["host/Ams.Workstation.Server/Components/Pages/Proof/Overview.razor"] = ["/proof/overview"],
            ["host/Ams.Workstation.Server/Components/Pages/Proof/ErrorPatterns.razor"] = ["/proof/patterns"],
            ["host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor"] = ["/proof/pickups"],
            ["host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor"] = ["/proof/{ChapterName}", "/proof/editing/{ChapterName}"]
        };

        foreach (var pagePath in ProofPages)
        {
            var actualRoutes = ExtractAndValidateRouteTemplates(pagePath);
            var expectedRoutes = expectedRoutesByPage[pagePath];

            var missing = expectedRoutes
                .Except(actualRoutes, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            var unexpected = actualRoutes
                .Except(expectedRoutes, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            Assert.True(
                missing.Length == 0 && unexpected.Length == 0,
                $"Route contract mismatch for '{pagePath}'. Missing: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}. Unexpected: {(unexpected.Length == 0 ? "none" : string.Join(", ", unexpected))}. Declared: {string.Join(", ", actualRoutes)}.");
        }
    }

    [Fact]
    public void ProofRazors_AreBitFree_AndDeclareVanillaAnchors()
    {
        var allRazorFiles = ProofPages.Concat(SharedComponents).ToArray();

        var bitOffenders = new List<string>();

        foreach (var relativePath in allRazorFiles)
        {
            var source = ReadRepoFile(relativePath);

            var bitTagMatches = Regex.Matches(source, "<Bit[A-Z][A-Za-z0-9]*", RegexOptions.CultureInvariant);
            foreach (Match match in bitTagMatches)
            {
                bitOffenders.Add($"{relativePath}:{GetLineNumber(source, match.Index)}:{match.Value}");
            }

            var bitEnumMatches = Regex.Matches(
                source,
                "\\b(BitColor|BitSize|BitTypography|BitIconName|BitVariant|BitDropdownItem)\\b",
                RegexOptions.CultureInvariant);

            foreach (Match match in bitEnumMatches)
            {
                bitOffenders.Add($"{relativePath}:{GetLineNumber(source, match.Index)}:{match.Value}");
            }
        }

        if (bitOffenders.Count > 0)
        {
            Assert.Fail(
                $"Proof/Shared Razor contract is not Bit-free. Found {bitOffenders.Count} forbidden Bit tag/enum occurrence(s). Offenders (file:line:tag):\n{string.Join("\n", bitOffenders.OrderBy(x => x, StringComparer.Ordinal))}");
        }

        AssertContainsLiteralAcrossFiles(allRazorFiles, "<AmsButton", "AmsButton primitive");
        AssertContainsLiteralAcrossFiles(allRazorFiles, "<AmsTag", "AmsTag primitive");
        AssertContainsLiteralAcrossFiles(allRazorFiles, "<AmsProgress", "AmsProgress primitive");
        AssertContainsLiteralAcrossFiles(allRazorFiles, "<AmsDialog", "AmsDialog primitive");
        AssertContainsLiteralAcrossFiles(allRazorFiles, "<AmsSpinner", "AmsSpinner primitive");

        AssertContainsRegexAcrossFiles(
            allRazorFiles,
            "<select\\b[^>]*\\bclass\\s*=\\s*\"[^\"]*\\bams-select\\b",
            "native select with ams-select class");

        AssertContainsRegexAcrossFiles(
            allRazorFiles,
            "<input\\b[^>]*\\btype\\s*=\\s*\"checkbox\"",
            "native checkbox input");

        AssertContainsRegexAcrossFiles(
            allRazorFiles,
            "<input\\b[^>]*(\\btype\\s*=\\s*\"text\")?[^>]*\\bclass\\s*=\\s*\"[^\"]*\\bams-input\\b",
            "native text input with ams-input class");

        var requiredProofAnchors = new[]
        {
            "no-workspace-message",
            "stats-grid",
            "chapter-grid",
            "patterns-filter",
            "patterns-grid",
            "view-switcher",
            "alert-sound-card"
        };

        foreach (var anchor in requiredProofAnchors)
        {
            AssertContainsLiteralAcrossFiles(allRazorFiles, $"data-ams-proof=\"{anchor}\"", $"proof anchor '{anchor}'");
        }

        var indexSource = ReadRepoFile("host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor");
        Assert.Contains("data-proof-index-link=\"pickups\"", indexSource, StringComparison.Ordinal);

        var chapterReviewSource = ReadRepoFile("host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor");
        Assert.Contains("data-proof-editing-handoff=\"pickups\"", chapterReviewSource, StringComparison.Ordinal);

        var pickupsSource = ReadRepoFile("host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor");
        var pickupsAnchorCount = Regex.Matches(pickupsSource, "data-proof-pickups-", RegexOptions.CultureInvariant).Count;

        Assert.True(
            pickupsAnchorCount >= 17,
            $"Pickups page must preserve >=17 'data-proof-pickups-' anchors for downstream contract/UAT seams. Found {pickupsAnchorCount} in 'host/Ams.Workstation.Server/Components/Pages/Proof/Pickups.razor'.");
    }

    [Fact]
    public void ProofRazors_DoNotReintroduceForbiddenInlinePatterns()
    {
        var allRazorFiles = ProofPages.Concat(SharedComponents).ToArray();

        var unexpectedHexOffenders = new List<string>();

        foreach (var relativePath in allRazorFiles)
        {
            var source = ReadRepoFile(relativePath);

            // ToastContainer still carries legacy inline toast skin styles until a later dedicated migration.
            if (!string.Equals(relativePath, ToastContainerRelativePath, StringComparison.Ordinal))
            {
                AssertDoesNotMatch(
                    source,
                    "<style\\b",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                    relativePath,
                    "embedded <style> block");
            }

            var sourceForStyleCheck = source;

            // WaveformPlayer requires one dynamic-height CSS variable seam for wavesurfer host sizing.
            if (string.Equals(relativePath, WaveformPlayerRelativePath, StringComparison.Ordinal))
            {
                sourceForStyleCheck = sourceForStyleCheck.Replace(
                    "style=\"--wfp-height: @(Height)px;\"",
                    string.Empty,
                    StringComparison.Ordinal);
            }

            AssertDoesNotMatch(
                sourceForStyleCheck,
                "\\sstyle\\s*=",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                relativePath,
                "inline style= attribute");

            foreach (Match hexMatch in Regex.Matches(source, "#[0-9a-fA-F]{3,8}\\b", RegexOptions.CultureInvariant))
            {
                var allowedSet = AllowedHexColorsByFile.TryGetValue(relativePath, out var allow)
                    ? allow
                    : null;

                if (allowedSet is not null && allowedSet.Contains(hexMatch.Value))
                {
                    continue;
                }

                unexpectedHexOffenders.Add($"{relativePath}:{GetLineNumber(source, hexMatch.Index)}:{hexMatch.Value}");
            }

            AssertDoesNotMatch(
                source,
                "var\\(--bit-clr-",
                RegexOptions.CultureInvariant,
                relativePath,
                "legacy Bit CSS variable reference");
        }

        if (unexpectedHexOffenders.Count > 0)
        {
            Assert.Fail(
                $"Found forbidden hex color literal(s) in Proof/Shared Razor files. Offenders (file:line:tag):\n{string.Join("\n", unexpectedHexOffenders.OrderBy(x => x, StringComparer.Ordinal))}");
        }
    }

    [Fact]
    public void ProofScss_UsesAmsTokensAndApprovedSelectors()
    {
        foreach (var relativePath in ProofScssFiles)
        {
            var source = ReadRepoFile(relativePath);

            var bitTokenMatches = Regex.Matches(source, "var\\(--bit-clr-[A-Za-z0-9-]+\\)", RegexOptions.CultureInvariant);
            if (bitTokenMatches.Count > 0)
            {
                var offenders = bitTokenMatches
                    .Select(match => $"{relativePath}:{GetLineNumber(source, match.Index)}:{match.Value}")
                    .ToArray();

                Assert.Fail(
                    $"Found {bitTokenMatches.Count} forbidden Bit token reference(s) in '{relativePath}'. Offenders (file:line:tag):\n{string.Join("\n", offenders)}");
            }

            Assert.True(
                Regex.IsMatch(source, "var\\(--ams-color-[A-Za-z0-9-]+\\)", RegexOptions.CultureInvariant),
                $"Missing required ams color token usage in '{relativePath}'. Expected at least one 'var(--ams-color-*)' reference.");

            var disallowedSelectors = ExtractDisallowedTopLevelSelectors(relativePath, source);
            if (disallowedSelectors.Count > 0)
            {
                Assert.Fail(
                    $"Found {disallowedSelectors.Count} disallowed top-level selector(s)/at-rule(s) in '{relativePath}'. Allowed prefixes: {string.Join(", ", AllowedTopLevelSelectorPrefixes)}. Allowed directives: @use, @forward, @media, @for, @supports, @keyframes. Offenders (file:line:tag):\n{string.Join("\n", disallowedSelectors.OrderBy(x => x, StringComparer.Ordinal))}");
            }
        }

        var pickupsScssSource = ReadRepoFile(PickupsScssRelativePath);
        Assert.True(
            Regex.IsMatch(pickupsScssSource, "(?m)^\\.proof-pickups-page\\s*\\{", RegexOptions.CultureInvariant),
            $"Missing top-level '.proof-pickups-page' selector in '{PickupsScssRelativePath}'.");
    }

    private static void AssertContainsLiteralAcrossFiles(IEnumerable<string> relativePaths, string token, string description)
    {
        foreach (var relativePath in relativePaths)
        {
            var source = ReadRepoFile(relativePath);
            if (source.Contains(token, StringComparison.Ordinal))
            {
                return;
            }
        }

        Assert.Fail(
            $"Missing required Proof contract anchor '{description}' token '{token}'. Checked files: {string.Join(", ", relativePaths)}.");
    }

    private static void AssertContainsRegexAcrossFiles(IEnumerable<string> relativePaths, string pattern, string description)
    {
        var regex = new Regex(pattern, RegexOptions.CultureInvariant);

        foreach (var relativePath in relativePaths)
        {
            var source = ReadRepoFile(relativePath);
            if (regex.IsMatch(source))
            {
                return;
            }
        }

        Assert.Fail(
            $"Missing required Proof contract anchor '{description}'. Pattern: {pattern}. Checked files: {string.Join(", ", relativePaths)}.");
    }

    private static IReadOnlyList<string> ExtractDisallowedTopLevelSelectors(string relativePath, string source)
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

            var first = raw[0];
            if (first == ' ' || first == '\t' || first == '}' || first == '/')
            {
                continue;
            }

            var trimmed = raw.TrimEnd();

            if (trimmed.EndsWith(";", StringComparison.Ordinal))
            {
                if (trimmed.StartsWith("@use", StringComparison.Ordinal)
                    || trimmed.StartsWith("@forward", StringComparison.Ordinal))
                {
                    continue;
                }

                disallowed.Add($"{relativePath}:{i + 1}:{trimmed}");
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
                disallowed.Add($"{relativePath}:{i + 1}:{trimmed}");
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
            $"No @page route directives found in '{relativePath}'. Proof route continuity requires explicit templates.");

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
            throw new XunitException($"Required proof design-contract source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof design-contract source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
