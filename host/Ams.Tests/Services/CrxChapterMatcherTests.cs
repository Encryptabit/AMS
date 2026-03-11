using Ams.Workstation.Server.Models;

namespace Ams.Tests.Services;

public class CrxChapterMatcherTests
{
    [Fact]
    public void Matches_ChapterAndSameNumberChapter_ReturnsTrue()
    {
        Assert.True(CrxChapterMatcher.Matches(
            "Chapter 4: The Birth of an Empire",
            "Chapter 4: The Birth of an Empire"));
    }

    [Fact]
    public void Matches_EpilogueAndChapterWithSameNumber_ReturnsFalse()
    {
        Assert.False(CrxChapterMatcher.Matches(
            "Epilogue 4: Entity",
            "Chapter 4: The Birth of an Empire"));
    }

    [Fact]
    public void TryParseShouldBe_PrefersCorrectedText()
    {
        const string comments = "Should be: at the proclamation every [nearby] legionnaire stood at attention\r\nRead as: at the proclamation [nearly] every legionnaire stood at attention";

        Assert.Equal(
            "at the proclamation every nearby legionnaire stood at attention",
            CrxCommentParser.TryParseShouldBe(comments));
        Assert.Equal(
            "at the proclamation nearly every legionnaire stood at attention",
            CrxCommentParser.TryParseReadAs(comments));
    }
}
