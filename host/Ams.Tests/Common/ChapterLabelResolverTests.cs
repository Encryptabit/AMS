using Ams.Core.Common;

namespace Ams.Tests.Common;

/// <summary>
/// Tests for <see cref="ChapterLabelResolver"/> chapter number extraction and label enumeration.
/// </summary>
public sealed class ChapterLabelResolverTests
{
    [Theory]
    [InlineData("05-12 Something", 12)]  // dash separator, space after number
    [InlineData("01_1", 1)]               // underscore separator, end of string
    [InlineData("10-99", 99)]             // dash separator, end of string
    [InlineData("03-2 Title", 2)]         // dash separator, space after number
    [InlineData("07_3 Chapter Name", 3)]  // underscore separator, space after number
    public void TryExtractChapterNumber_ValidPatterns_ReturnsTrue(string label, int expected)
    {
        var result = ChapterLabelResolver.TryExtractChapterNumber(label, out var number);

        Assert.True(result);
        Assert.Equal(expected, number);
    }

    [Theory]
    [InlineData("03_2_Title")]           // underscore after second number (no word boundary)
    [InlineData("  07_3_Chapter Name  ")] // underscore after second number (no word boundary)
    public void TryExtractChapterNumber_NoWordBoundary_ReturnsFalse(string label)
    {
        // Regex requires word boundary after second number, underscore doesn't create boundary
        var result = ChapterLabelResolver.TryExtractChapterNumber(label, out var number);

        Assert.False(result);
        Assert.Equal(0, number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Introduction")]
    [InlineData("Chapter One")]
    [InlineData("NoNumbers")]
    [InlineData("123")] // Single number, no separator
    public void TryExtractChapterNumber_InvalidPatterns_ReturnsFalse(string? label)
    {
        var result = ChapterLabelResolver.TryExtractChapterNumber(label!, out var number);

        Assert.False(result);
        Assert.Equal(0, number);
    }

    [Fact]
    public void EnumerateLabelCandidates_BothProvided_YieldsChapterIdFirst()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates(
            "Chapter 01",
            @"C:\Books\01_Chapter").ToList();

        Assert.Equal(2, candidates.Count);
        Assert.Equal("Chapter 01", candidates[0]);
        Assert.Equal("01_Chapter", candidates[1]);
    }

    [Fact]
    public void EnumerateLabelCandidates_OnlyChapterId_YieldsSingle()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates(
            "Chapter 01",
            null).ToList();

        Assert.Single(candidates);
        Assert.Equal("Chapter 01", candidates[0]);
    }

    [Fact]
    public void EnumerateLabelCandidates_OnlyRootPath_YieldsDirectoryName()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates(
            null,
            @"C:\Books\05-12 The Chapter").ToList();

        Assert.Single(candidates);
        Assert.Equal("05-12 The Chapter", candidates[0]);
    }

    [Fact]
    public void EnumerateLabelCandidates_TrailingSeparator_StripsCorrectly()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates(
            null,
            @"C:\Books\MyChapter\").ToList();

        Assert.Single(candidates);
        Assert.Equal("MyChapter", candidates[0]);
    }

    [Fact]
    public void EnumerateLabelCandidates_BothNull_YieldsEmpty()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates(null, null).ToList();

        Assert.Empty(candidates);
    }

    [Fact]
    public void EnumerateLabelCandidates_WhitespaceOnly_YieldsEmpty()
    {
        var candidates = ChapterLabelResolver.EnumerateLabelCandidates("   ", "   ").ToList();

        Assert.Empty(candidates);
    }
}
