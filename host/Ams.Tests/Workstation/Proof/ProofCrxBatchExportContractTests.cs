using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofCrxBatchExportContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string CrxModalRelativePath = "host/Ams.Workstation.Server/Components/Shared/CrxModal.razor";
    private const string CrxServiceRelativePath = "host/Ams.Workstation.Server/Services/CrxService.cs";
    private const string ReviewedStatusServiceRelativePath = "host/Ams.Workstation.Server/Services/ReviewedStatusService.cs";

    [Fact]
    public void ChapterReview_BatchExport_UsesSelectedSetAnchorsAndSharedComposition()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var swipeRightBody = ExtractMethodBody(source, "private Task HandleSelectionSwipeRightAsync(");
        var exportDiagnosticBody = ExtractMethodBody(source, "private void LogCrxExportDiagnostic(");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "const string eventName = \"swipe-right-export\";",
            "batch export swipe-right contract event anchor");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "selectedSentenceIds.Contains(sentence.Id)",
            "batch export resolves export seeds from selected sentence ids");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "var composition = ComposeCrxExportComposition(exportSeeds);",
            "batch export uses shared composition helper");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "composition.RequiresRangeConfirmation);",
            "batch export modal handoff carries fallback-range confirmation contract");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "LogCrxExportDiagnostic(trigger, sourceSurface, sentenceId, composition);",
            "batch export handler routes observability through shared export diagnostic seam");

        AssertContains(
            exportDiagnosticBody,
            ChapterReviewRelativePath,
            "[ProofCrxExport]",
            "batch export observability diagnostics anchor");
    }

    [Fact]
    public void ChapterReview_ComposeCrxExportComposition_UsesMinMaxDefaultsAndFallbackSeed()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);
        var composeBody = ExtractMethodBody(source, "private static CrxExportComposition ComposeCrxExportComposition(");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "var batchStart = startCandidates.Count > 0 ? startCandidates.Min() : double.NaN;",
            "selected-set merged start range uses min start time");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "var batchEnd = endCandidates.Count > 0 ? endCandidates.Max() : double.NaN;",
            "selected-set merged end range uses max end time");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "var requiresRangeConfirmation = !IsValidCrxRange(batchStart, batchEnd);",
            "invalid merged ranges trigger explicit confirmation mode");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "batchStart = Math.Max(0, fallbackAnchor - CrxFallbackLeadInSec);",
            "fallback start range seeds deterministic lead-in window");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "batchEnd = batchStart + CrxFallbackExportDurationSec;",
            "fallback end range seeds deterministic fixed duration window");

        AssertContains(
            composeBody,
            ChapterReviewRelativePath,
            "RangeMode: \"fallback-empty\"",
            "empty seed set fail-closes to fallback-empty range mode");
    }

    [Fact]
    public void ChapterReview_BatchIgnore_UsesSharedExecutorAcrossSwipeButtonAndShortcut()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "private async Task ExecuteSelectionBatchIgnoreAsync(",
            "shared batch-ignore executor declaration");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return ExecuteSelectionBatchIgnoreAsync(",
            "batch ignore wrappers delegate through shared executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"swipe-left\"",
            "swipe-left dispatch uses shared batch-ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"ignore-button\"",
            "errors-card ignore dispatch uses shared batch-ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "trigger: \"ignore-shortcut\"",
            "keyboard ignore dispatch uses shared batch-ignore executor");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "eventName: \"explicit-ignore\"",
            "explicit ignore paths share same event contract");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "const string selectionPolicy = \"retain\";",
            "batch-ignore keeps deterministic selection retention policy");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "[ProofBatchIgnore]",
            "batch-ignore observability diagnostics anchor");
    }

    [Fact]
    public void ChapterReview_BatchGestureHandlers_PreservePersistenceSeams()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        var swipeRightBody = ExtractMethodBody(source, "private Task HandleSelectionSwipeRightAsync(");
        var batchIgnoreBody = ExtractMethodBody(source, "private async Task ExecuteSelectionBatchIgnoreAsync(");

        AssertContains(
            swipeRightBody,
            ChapterReviewRelativePath,
            "_crxModal.Open(",
            "batch export routes through modal seam before persistence submit");

        AssertDoesNotContain(
            swipeRightBody,
            ChapterReviewRelativePath,
            "CrxService.Submit(",
            "batch export gesture handler should not write CRX artifacts directly");

        AssertDoesNotContain(
            swipeRightBody,
            ChapterReviewRelativePath,
            "ReviewedStatusService.SetReviewed(",
            "batch export gesture handler should not mutate reviewed-status persistence");

        AssertContains(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "IgnorePatternsForSentence(selectedSentence)",
            "batch ignore persists only through existing ignored-patterns seam");

        AssertDoesNotContain(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "_crxModal.Open(",
            "batch ignore path should not invoke CRX export flow");

        AssertDoesNotContain(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "CrxService.Submit(",
            "batch ignore path should not write CRX artifacts");

        AssertDoesNotContain(
            batchIgnoreBody,
            ChapterReviewRelativePath,
            "ReviewedStatusService.SetReviewed(",
            "batch ignore path should not mutate reviewed-status persistence");
    }

    [Fact]
    public void CrxModalAndServices_PreserveExistingPersistenceContracts()
    {
        var crxModalSource = ReadRepoFile(CrxModalRelativePath);
        var crxServiceSource = ReadRepoFile(CrxServiceRelativePath);
        var reviewedStatusServiceSource = ReadRepoFile(ReviewedStatusServiceRelativePath);

        AssertContains(
            crxModalSource,
            CrxModalRelativePath,
            "private CrxSubmitRequest BuildSubmitRequest()",
            "CRX modal submit request builder declaration");

        AssertContains(
            crxModalSource,
            CrxModalRelativePath,
            "Start: _startTime,",
            "CRX submit request keeps absolute start bound contract");

        AssertContains(
            crxModalSource,
            CrxModalRelativePath,
            "End: _endTime,",
            "CRX submit request keeps absolute end bound contract");

        AssertContains(
            crxModalSource,
            CrxModalRelativePath,
            "PaddingMs: 0,",
            "CRX submit request keeps zero-padding persistence contract");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "var crxFolder = Path.Combine(_workspace.RootPath, \"CRX\");",
            "CRX persistence folder remains rooted under workspace CRX directory");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "return Path.Combine(crxFolder, $\"{bookName}_CRX.xlsx\");",
            "CRX Excel artifact naming contract");

        AssertContains(
            crxServiceSource,
            CrxServiceRelativePath,
            "return Path.Combine(crxFolder, $\"{bookName}_CRX.json\");",
            "CRX JSON artifact naming contract");

        AssertContains(
            reviewedStatusServiceSource,
            ReviewedStatusServiceRelativePath,
            "private string GetFilePath() => Path.Combine(BasePath, \"reviewed-status.json\");",
            "reviewed-status persistence path contract");
    }

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
            $"Missing proof CRX batch contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale proof CRX batch contract anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof CRX batch contract file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof CRX batch contract file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
