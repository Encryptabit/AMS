using Xunit.Sdk;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofEditingPlaybackSourceContractTests
{
    private const string ChapterReviewRelativePath = "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor";
    private const string AudioControllerRelativePath = "host/Ams.Workstation.Server/Controllers/AudioController.cs";

    [Fact]
    public void ChapterReview_PlaybackHelpers_TargetCorrectedAwareEndpoints()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected\";",
            "corrected waveform playback endpoint");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected/peaks?pxPerSec={PeakPxPerSec}\";",
            "corrected waveform peaks endpoint");

        AssertDoesNotContain(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}\";",
            "stale generic chapter playback endpoint literal");

        AssertDoesNotContain(
            source,
            ChapterReviewRelativePath,
            "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/peaks?pxPerSec={PeakPxPerSec}\";",
            "stale generic chapter peaks endpoint literal");
    }

    [Fact]
    public void ChapterReview_PlaybackHelpers_KeepEscapingAndBlankChapterGuards()
    {
        var source = ReadRepoFile(ChapterReviewRelativePath);

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (!string.IsNullOrWhiteSpace(ChapterName))",
            "audio URL blank-chapter guard");

        AssertContains(
            source,
            ChapterReviewRelativePath,
            "if (string.IsNullOrWhiteSpace(ChapterName))",
            "peaks URL blank-chapter guard");

        var escapedChapterUsageCount = CountOccurrences(source, "Uri.EscapeDataString(ChapterName)");
        Assert.True(
            escapedChapterUsageCount >= 2,
            $"Expected corrected playback helpers to URI-escape chapter names in '{ChapterReviewRelativePath}'. Found {escapedChapterUsageCount} occurrence(s) of Uri.EscapeDataString(ChapterName).");
    }

    [Fact]
    public void AudioController_CorrectedEndpoints_UseSharedResolutionPath()
    {
        var source = ReadRepoFile(AudioControllerRelativePath);

        const string resolverCall = "TryResolveCorrectedPlayback(chapterName, out var resolved, out var failureResult)";
        var resolverCallCount = CountOccurrences(source, resolverCall);

        Assert.True(
            resolverCallCount == 2,
            $"Expected corrected audio and peaks endpoints to share resolver call '{resolverCall}' exactly twice in '{AudioControllerRelativePath}', but found {resolverCallCount} occurrence(s).");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "private bool TryResolveCorrectedPlayback(",
            "shared corrected playback resolver declaration");
    }

    [Fact]
    public void AudioController_CorrectedResolver_DeclaresDeterministicFallbackAndFailClosedGuards()
    {
        var source = ReadRepoFile(AudioControllerRelativePath);

        AssertContains(
            source,
            AudioControllerRelativePath,
            "var requestedChapter = Uri.UnescapeDataString(chapterName ?? string.Empty).Trim();",
            "decoded chapter token normalization");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "if (string.IsNullOrWhiteSpace(requestedChapter))",
            "blank chapter fail-closed guard");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "if (!MatchesRequestedChapter(requestedChapter, activeChapter, descriptor))",
            "chapter mismatch fail-closed guard");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "new[] { (\"corrected\", audio.Corrected), (\"treated\", audio.Treated), (\"current\", currentContext) }",
            "deterministic corrected→treated→current fallback order");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "(checked corrected, treated, current)",
            "explicit deterministic fallback diagnostics");

        AssertContains(
            source,
            AudioControllerRelativePath,
            "private static bool MatchesRequestedChapter(",
            "chapter identity matcher for reserved slug/alias compatibility");
    }

    private static int CountOccurrences(string source, string token)
    {
        var count = 0;
        var index = 0;
        while (true)
        {
            index = source.IndexOf(token, index, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            index += token.Length;
        }
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing proof editing playback source contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale proof editing playback source anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required proof editing playback source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        try
        {
            return File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new XunitException($"Failed to read proof editing playback source file: relative='{relativePath}', full='{fullPath}', error='{ex.Message}'.");
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
