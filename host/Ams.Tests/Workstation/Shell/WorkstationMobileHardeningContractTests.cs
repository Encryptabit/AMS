using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Ams.Tests.Workstation.Shell;

public sealed class WorkstationMobileHardeningContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string CrxModalRelativePath = "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor";
    private const string CrxServiceRelativePath = "host/Ams.Workstation.Server/Services/CrxService.cs";
    private const string ReviewedStatusServiceRelativePath = "host/Ams.Workstation.Server/Services/ReviewedStatusService.cs";

    [Fact]
    public void ChapterReview_GestureDiagnostics_RemainExplicitAndPersistenceNeutral()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var validateBody = ExtractMethodBody(source, "private bool TryValidateBatchGestureContext(");
        var selectionDiagnosticBody = ExtractMethodBody(source, "private void LogSelectionModeDiagnostic(");
        var batchIgnoreDiagnosticBody = ExtractMethodBody(source, "private void LogBatchIgnoreDiagnostic(");
        var crxExportDiagnosticBody = ExtractMethodBody(source, "private void LogCrxExportDiagnostic(");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private static bool IsFallbackActionSurface(string sourceSurface)",
            "fallback action surface helper declaration");

        AssertContains(
            validateBody,
            ChapterReviewRelativePath,
            "_lastSelectionGestureEvent = $\"{eventName}-ignored-surface-mismatch\";",
            "gesture validation fail-closes source-surface mismatches");

        AssertContains(
            validateBody,
            ChapterReviewRelativePath,
            "expectedSurface={CurrentMobileActionSurface};reason=surface-mismatch",
            "gesture validation diagnostics include expected surface and reason");

        AssertContains(
            selectionDiagnosticBody,
            ChapterReviewRelativePath,
            "[ProofSelectionMode]",
            "selection diagnostics marker remains present");

        AssertContains(
            batchIgnoreDiagnosticBody,
            ChapterReviewRelativePath,
            "[ProofBatchIgnore]",
            "batch-ignore diagnostics marker remains present");

        AssertContains(
            crxExportDiagnosticBody,
            ChapterReviewRelativePath,
            "[ProofCrxExport]",
            "CRX export diagnostics marker remains present");

        foreach (var forbiddenPattern in ForbiddenSecretAndPersistenceLeakPatterns)
        {
            AssertDoesNotMatch(
                selectionDiagnosticBody,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                ChapterReviewRelativePath,
                "forbidden secret/persistence leak in selection diagnostics");

            AssertDoesNotMatch(
                batchIgnoreDiagnosticBody,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                ChapterReviewRelativePath,
                "forbidden secret/persistence leak in batch-ignore diagnostics");

            AssertDoesNotMatch(
                crxExportDiagnosticBody,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                ChapterReviewRelativePath,
                "forbidden secret/persistence leak in CRX export diagnostics");
        }
    }

    [Fact]
    public void CrxModal_RangeValidationDiagnostics_RemainActionableAndPersistenceNeutral()
    {
        var source = ReadRepoFile(CrxModalRelativePath);

        AssertContains(
            source,
            CrxModalRelativePath,
            "data-ams-crx-range-validation=\"true\"",
            "range validation accessibility + contract marker");

        AssertContains(
            source,
            CrxModalRelativePath,
            "_rangeValidationMessage = \"Start time must use mm:ss.mmm format (example: 1:23.450).\";",
            "invalid start format validation message");

        AssertContains(
            source,
            CrxModalRelativePath,
            "_rangeValidationMessage = \"End time must use mm:ss.mmm format (example: 1:23.450).\";",
            "invalid end format validation message");

        AssertContains(
            source,
            CrxModalRelativePath,
            "_rangeValidationMessage = \"Start and End must be 0 or greater.\";",
            "negative range validation message");

        AssertContains(
            source,
            CrxModalRelativePath,
            "_rangeValidationMessage = \"Start must be before End. Adjust either bound to create a positive export range.\";",
            "start-before-end validation message");

        AssertContains(
            source,
            CrxModalRelativePath,
            "_rangeValidationMessage = \"Range is zero-duration. Adjust Start earlier or End later before submitting this CRX entry.\";",
            "zero-duration validation message");

        AssertContains(
            source,
            CrxModalRelativePath,
            "PaddingMs: 0,",
            "CRX submit request preserves zero-padding persistence contract");

        foreach (var forbiddenPattern in ForbiddenPersistenceMutationPatterns)
        {
            AssertDoesNotMatch(
                source,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                CrxModalRelativePath,
                "forbidden direct persistence mutation seam");
        }
    }

    [Fact]
    public void PersistenceServices_PreserveReviewedAndCrxArtifactPathAndSchemaSeams()
    {
        var reviewedStatusSource = ReadRepoFile(ReviewedStatusServiceRelativePath);
        var crxServiceSource = ReadRepoFile(CrxServiceRelativePath);

        AssertContains(
            reviewedStatusSource,
            ReviewedStatusServiceRelativePath,
            "private static readonly string BasePath = AmsAppDataPaths.Resolve(\"workstation\");",
            "reviewed-status base appdata root");

        AssertContains(
            reviewedStatusSource,
            ReviewedStatusServiceRelativePath,
            "private string GetFilePath() => Path.Combine(BasePath, \"reviewed-status.json\");",
            "reviewed-status artifact naming contract");

        AssertContains(
            reviewedStatusSource,
            ReviewedStatusServiceRelativePath,
            "JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ReviewedEntry>>>(json);",
            "reviewed-status schema deserialize seam");

        AssertContains(
            reviewedStatusSource,
            ReviewedStatusServiceRelativePath,
            "JsonSerializer.Serialize(allBooks, new JsonSerializerOptions { WriteIndented = true });",
            "reviewed-status schema serialize seam");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "var crxFolder = Path.Combine(_workspace.RootPath, \"CRX\");",
            "CRX persistence root remains workspace CRX folder");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "return Path.Combine(crxFolder, $\"{bookName}_CRX.xlsx\");",
            "CRX Excel artifact naming seam");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "return Path.Combine(crxFolder, $\"{bookName}_CRX.json\");",
            "CRX JSON artifact naming seam");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "ShouldBe: request.ShouldBe,",
            "CRX entry schema keeps ShouldBe field");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "ReadAs: request.ReadAs",
            "CRX entry schema keeps ReadAs field");

        AssertDoesNotMatch(
            reviewedStatusSource,
            "reviewed-status-v[0-9]+\\.json",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            ReviewedStatusServiceRelativePath,
            "unexpected reviewed-status artifact version seam");

        AssertDoesNotMatch(
            crxServiceSource,
            "_CRX[-_]?v[0-9]+\\.(json|xlsx)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            CrxServiceRelativePath,
            "unexpected CRX artifact version seam");
    }

    [Fact]
    public void ChapterReview_MobileBatchHandlers_DoNotBypassExistingPersistenceSeams()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var swipeRightBody = ExtractMethodBody(source, "private Task HandleSelectionSwipeRightAsync(");
        var batchIgnoreBody = ExtractMethodBody(source, "private async Task ExecuteSelectionBatchIgnoreAsync(");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "_crxModal.Open(",
            "batch export path stays routed through CRX modal seam");

        AssertDoesNotContain(
            swipeRightBody,
            ChapterReviewRelativePath,
            "CrxService.Submit(",
            "batch export gesture handler must not submit CRX persistence directly");

        AssertDoesNotContain(
            swipeRightBody,
            ChapterReviewRelativePath,
            "ReviewedStatusService.SetReviewed(",
            "batch export gesture handler must not mutate reviewed-status persistence");

        AssertDoesNotContain(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "CrxService.Submit(",
            "batch ignore handler must not submit CRX persistence");

        AssertDoesNotContain(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "ReviewedStatusService.SetReviewed(",
            "batch ignore handler must not mutate reviewed-status persistence");

        foreach (var forbiddenPattern in ForbiddenPersistenceMutationPatterns)
        {
            AssertDoesNotMatch(
                swipeRightBody,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                ChapterReviewRelativePath,
                "forbidden direct persistence mutation in batch export handler");

            AssertDoesNotMatch(
                batchIgnoreBody,
                forbiddenPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                ChapterReviewRelativePath,
                "forbidden direct persistence mutation in batch ignore handler");
        }
    }

    private static string[] ForbiddenSecretAndPersistenceLeakPatterns =>
    [
        "localStorage",
        "sessionStorage",
        "reviewed-status\\.json",
        "_CRX\\.json",
        "_CRX\\.xlsx",
        "AmsAppDataPaths",
        "RootPath",
        "password",
        "secret",
        "api[_-]?key",
        "token"
    ];

    private static string[] ForbiddenPersistenceMutationPatterns =>
    [
        "\\bFile\\.(WriteAllText|WriteAllTextAsync|AppendAllText|AppendAllTextAsync)\\b",
        "\\bDirectory\\.CreateDirectory\\b",
        "\\bJsonSerializer\\.(Serialize|Deserialize)\\b",
        "\\blocalStorage\\b",
        "\\bsessionStorage\\b",
        "reviewed-status\\.json",
        "_CRX\\.(json|xlsx)"
    ];

    private static string ExtractMethodBody(string source, string methodSignature)
    {
        var signatureIndex = source.IndexOf(methodSignature, StringComparison.Ordinal);
        Assert.True(
            signatureIndex >= 0,
            $"Missing method signature '{methodSignature}' while extracting contract body.");

        var bodyStart = source.IndexOf('{', signatureIndex);
        Assert.True(
            bodyStart >= 0,
            $"Could not locate opening brace for method signature '{methodSignature}'.");

        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            var current = source[index];
            if (current == '{')
            {
                depth++;
            }
            else if (current == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(bodyStart, index - bodyStart + 1);
                }
            }
        }

        throw new XunitException($"Unbalanced braces while extracting method body for signature '{methodSignature}'.");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing workstation mobile hardening anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale workstation mobile hardening anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
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
            throw new XunitException($"Required workstation mobile hardening source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read workstation mobile hardening source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
