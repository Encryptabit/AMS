using Ams.Core.Runtime.Book;

namespace Ams.Tests;

public class ProperNounPromptFilterTests
{
    [Fact]
    public void Filter_RemovesPromptNoiseAndKeepsNameLikeTerms()
    {
        var input = new[]
        {
            "2,000 Prosperity has been consumed",
            "Consume",
            "Oh",
            "Reeze"
        };

        var filtered = ProperNounPromptFilter.Filter(input);

        Assert.DoesNotContain("2,000 Prosperity has been consumed", filtered);
        Assert.DoesNotContain("Consume", filtered);
        Assert.DoesNotContain("Oh", filtered);
        Assert.Contains("Reeze", filtered);
    }

    [Fact]
    public void Filter_NormalizesFormattingCharacters()
    {
        var input = new[]
        {
            "you\u2060",
            "  Frozen\u200B Spire  "
        };

        var filtered = ProperNounPromptFilter.Filter(input);

        Assert.DoesNotContain("you\u2060", filtered);
        Assert.Contains("Frozen Spire", filtered);
    }
}
