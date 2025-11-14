namespace Ams.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Hello, World!", "hello world")]
    [InlineData("Don't you think it's great?", "do not you think it is great")]
    [InlineData("He whispered, “Don’t move.”", "he whispered do not move")]
    [InlineData("I can't believe it's 123 degrees!", "i cannot believe it is one hundred twenty three degrees")]
    [InlineData("  Multiple    spaces   ", "multiple spaces")]
    [InlineData("", "")]
    public void Normalize_ShouldNormalizeTextCorrectly(string input, string expected)
    {
        var result = TextNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "hello world", 1.0)]
    [InlineData("hello", "helo", 0.8)]
    [InlineData("completely", "different", 0.0)]
    [InlineData("", "", 1.0)]
    [InlineData("", "something", 0.0)]
    public void CalculateSimilarity_ShouldReturnCorrectSimilarity(string text1, string text2, double expectedMin)
    {
        var similarity = TextNormalizer.CalculateSimilarity(text1, text2);
        Assert.True(similarity >= expectedMin - 0.1, $"Expected similarity >= {expectedMin}, got {similarity}");
    }

    [Theory]
    [InlineData("hello world test", new[] { "hello", "world", "test" })]
    [InlineData("", new string[0])]
    [InlineData("single", new[] { "single" })]
    public void TokenizeWords_ShouldTokenizeCorrectly(string input, string[] expected)
    {
        var result = TextNormalizer.TokenizeWords(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeTypography_ReplacesSmartQuotes()
    {
        var input = "“Don’t” ‘quote’";
        var normalized = TextNormalizer.NormalizeTypography(input);
        Assert.Equal("\"Don't\" 'quote'", normalized);
    }
}

public class ScriptValidatorTests
{
    [Fact]
    public void Validate_PerfectMatch_ShouldReturnZeroWER()
    {
        var validator = new ScriptValidator();
        var scriptText = "hello world test";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.5, Word: "hello"),
                new AsrToken(StartTime: 0.5, Duration: 0.5, Word: "world"),
                new AsrToken(StartTime: 1.0, Duration: 0.5, Word: "test")
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(0.0, report.WordErrorRate);
        Assert.Equal(3, report.TotalWords);
        Assert.Equal(3, report.CorrectWords);
        Assert.Equal(0, report.Substitutions);
        Assert.Equal(0, report.Insertions);
        Assert.Equal(0, report.Deletions);
    }

    [Fact]
    public void Validate_WithSubstitution_ShouldCalculateCorrectWER()
    {
        var validator = new ScriptValidator();
        var scriptText = "hello world test";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.5, Word: "hello"),
                new AsrToken(StartTime: 0.5, Duration: 0.5, Word: "word"), // "world" -> "word"
                new AsrToken(StartTime: 1.0, Duration: 0.5, Word: "test")
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(1.0/3.0, report.WordErrorRate, 2); // 1 error out of 3 words
        Assert.Equal(3, report.TotalWords);
        Assert.Equal(2, report.CorrectWords);
        Assert.Equal(1, report.Substitutions);
        Assert.Equal(0, report.Insertions);
        Assert.Equal(0, report.Deletions);
        
        var substitutionFindings = report.Findings.Where(f => f.Type == FindingType.Substitution).ToList();
        Assert.Single(substitutionFindings);
        Assert.Equal("world", substitutionFindings[0].Expected);
        Assert.Equal("word", substitutionFindings[0].Actual);
    }

    [Fact]
    public void Validate_WithInsertion_ShouldCalculateCorrectWER()
    {
        var validator = new ScriptValidator();
        var scriptText = "hello world";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.5, Word: "hello"),
                new AsrToken(StartTime: 0.5, Duration: 0.5, Word: "beautiful"), // extra "beautiful"
                new AsrToken(StartTime: 1.0, Duration: 0.5, Word: "world")
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(0.5, report.WordErrorRate); // 1 insertion out of 2 expected words
        Assert.Equal(2, report.TotalWords);
        Assert.Equal(2, report.CorrectWords);
        Assert.Equal(0, report.Substitutions);
        Assert.Equal(1, report.Insertions);
        Assert.Equal(0, report.Deletions);
        
        var insertionFindings = report.Findings.Where(f => f.Type == FindingType.Extra).ToList();
        Assert.Single(insertionFindings);
        Assert.Equal("beautiful", insertionFindings[0].Actual);
    }

    [Fact]
    public void Validate_WithDeletion_ShouldCalculateCorrectWER()
    {
        var validator = new ScriptValidator();
        var scriptText = "hello beautiful world";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.5, Word: "hello"),
                new AsrToken(StartTime: 1.0, Duration: 0.5, Word: "world") // missing "beautiful"
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(1.0/3.0, report.WordErrorRate, 2); // 1 deletion out of 3 expected words
        Assert.Equal(3, report.TotalWords);
        Assert.Equal(2, report.CorrectWords);
        Assert.Equal(0, report.Substitutions);
        Assert.Equal(0, report.Insertions);
        Assert.Equal(1, report.Deletions);
        
        var deletionFindings = report.Findings.Where(f => f.Type == FindingType.Missing).ToList();
        Assert.Single(deletionFindings);
        Assert.Equal("beautiful", deletionFindings[0].Expected);
    }

    [Fact]
    public void Validate_WithContractions_ShouldNormalizeCorrectly()
    {
        var validator = new ScriptValidator(new ValidationOptions { ExpandContractions = true });
        var scriptText = "I can't believe it's working";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.3, Word: "I"),
                new AsrToken(StartTime: 0.3, Duration: 0.5, Word: "cannot"),
                new AsrToken(StartTime: 0.8, Duration: 0.5, Word: "believe"),
                new AsrToken(StartTime: 1.3, Duration: 0.3, Word: "it"),
                new AsrToken(StartTime: 1.6, Duration: 0.3, Word: "is"),
                new AsrToken(StartTime: 1.9, Duration: 0.5, Word: "working")
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(0.0, report.WordErrorRate);
        Assert.Equal(6, report.CorrectWords); // Should match after normalization
    }

    [Fact]
    public void Validate_ComplexScenario_ShouldCalculateCorrectMetrics()
    {
        var validator = new ScriptValidator();
        var scriptText = "The quick brown fox jumps over the lazy dog";
        var asrResponse = new AsrResponse(
            "test@v1",
            new[]
            {
                new AsrToken(StartTime: 0.0, Duration: 0.3, Word: "The"),
                new AsrToken(StartTime: 0.3, Duration: 0.4, Word: "fast"), // quick->fast
                new AsrToken(StartTime: 0.7, Duration: 0.4, Word: "brown"),
                new AsrToken(StartTime: 1.1, Duration: 0.3, Word: "fox"),
                new AsrToken(StartTime: 1.4, Duration: 0.4, Word: "leaps"), // jumps->leaps
                new AsrToken(StartTime: 1.8, Duration: 0.3, Word: "over"),
                new AsrToken(StartTime: 2.1, Duration: 0.4, Word: "lazy"),
                new AsrToken(StartTime: 2.5, Duration: 0.3, Word: "dog") // missing "the"
            }
        );

        var report = validator.Validate("audio.wav", "script.txt", "asr.json", scriptText, asrResponse);

        Assert.Equal(9, report.TotalWords);
        Assert.Equal(6, report.CorrectWords); // the, brown, fox, over, lazy, dog
        Assert.Equal(2, report.Substitutions); // quick->fast, jumps->leaps
        Assert.Equal(0, report.Insertions);
        Assert.Equal(1, report.Deletions); // missing "the"
        
        var expectedWer = (2 + 0 + 1) / 9.0; // (S + I + D) / N
        Assert.Equal(expectedWer, report.WordErrorRate, 3);
    }
}
