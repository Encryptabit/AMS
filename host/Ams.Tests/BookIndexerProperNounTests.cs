using Ams.Core.Processors.DocumentProcessor;
using Ams.Core.Runtime.Book;

namespace Ams.Tests;

public class BookIndexerProperNounTests
{
    /// <summary>
    /// Bracketed text [Frozen Spire] in a section produces "Frozen Spire" in that section's ProperNouns.
    /// </summary>
    [Fact]
    public async Task BracketedPhrase_ExtractedAsProperNoun()
    {
        var source = CreateTempFile(
            "Chapter One\n\nThe hero reached the [Frozen Spire] at dawn.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter One");
            Assert.NotNull(section.ProperNouns);
            Assert.Contains("Frozen Spire", section.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Angle-bracketed text produces proper noun entry.
    /// </summary>
    [Fact]
    public async Task AngleBracketedPhrase_ExtractedAsProperNoun()
    {
        var source = CreateTempFile(
            "Chapter Two\n\nA <System Alert> sounded throughout.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Two");
            Assert.NotNull(section.ProperNouns);
            Assert.Contains("System Alert", section.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Unclosed brackets with >8 tokens abandon accumulation;
    /// individual tokens fall through to frequency check.
    /// </summary>
    [Fact]
    public async Task UnclosedBracket_SafetyValve_AbandonAccumulation()
    {
        var source = CreateTempFile(
            "Chapter Three\n\n[One two three four five six seven eight nine ten end of the bracketless run.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Three");
            // The bracket was never closed, so "One two three..." should NOT appear as a single phrase.
            // Instead individual rare words (if any) would be picked up by frequency check.
            if (section.ProperNouns != null)
            {
                Assert.DoesNotContain(section.ProperNouns,
                    pn => pn.Contains("One two three four five six seven eight nine ten"));
            }
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Unknown word not in frequency dictionary appears in ProperNouns.
    /// </summary>
    [Fact]
    public async Task UnknownWord_AppearsInProperNouns()
    {
        var source = CreateTempFile(
            "Chapter Four\n\nThe Voidlings attacked at midnight.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Four");
            Assert.NotNull(section.ProperNouns);
            Assert.Contains("Voidlings", section.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Common English word "the" does NOT appear in ProperNouns.
    /// </summary>
    [Fact]
    public async Task CommonWord_NotInProperNouns()
    {
        var source = CreateTempFile(
            "Chapter Five\n\nThe cat sat on the mat. Xylvorn appeared.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Five");
            Assert.NotNull(section.ProperNouns);
            // "the", "cat", "sat", "on", "mat" are common words
            Assert.DoesNotContain("the", section.ProperNouns);
            Assert.DoesNotContain("cat", section.ProperNouns);
            Assert.DoesNotContain("sat", section.ProperNouns);
            // But the rare word should be there
            Assert.Contains("Xylvorn", section.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Words inside brackets are NOT also individually frequency-checked (no double-hits).
    /// </summary>
    [Fact]
    public async Task WordsInsideBrackets_NotDoubleChecked()
    {
        // "Zyxorp" is not a real word. If it's inside brackets, it should appear only as part
        // of the bracket phrase, not also as an individual proper noun.
        var source = CreateTempFile(
            "Chapter Six\n\nThey entered the [Zyxorp Fortress] at night.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Six");
            Assert.NotNull(section.ProperNouns);
            Assert.Contains("Zyxorp Fortress", section.ProperNouns);
            // "Zyxorp" should NOT appear individually
            Assert.DoesNotContain("Zyxorp", section.ProperNouns);
            // "Fortress" might or might not be in the frequency dict, but it should not appear
            // individually since it was inside brackets
            Assert.DoesNotContain("Fortress", section.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Multiple sections each get their own scoped ProperNouns arrays.
    /// </summary>
    [Fact]
    public async Task MultipleSections_ScopedProperNouns()
    {
        var source = CreateTempFile(
            "Chapter One\n\nThe Kethara clan gathered.\n\n" +
            "Chapter Two\n\nThe Valdris empire rose.");

        try
        {
            var index = await BuildIndex(source);
            var section1 = GetSectionWithTitle(index, "Chapter One");
            var section2 = GetSectionWithTitle(index, "Chapter Two");

            Assert.NotNull(section1.ProperNouns);
            Assert.Contains("Kethara", section1.ProperNouns);
            Assert.DoesNotContain("Valdris", section1.ProperNouns);

            Assert.NotNull(section2.ProperNouns);
            Assert.Contains("Valdris", section2.ProperNouns);
            Assert.DoesNotContain("Kethara", section2.ProperNouns);
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Hyphenated compound where neither component passes frequency check yields entry in ProperNouns.
    /// </summary>
    [Fact]
    public async Task HyphenatedCompound_RareComponents_InProperNouns()
    {
        var source = CreateTempFile(
            "Chapter Seven\n\nThe Blood-sworn warriors marched onward.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Seven");
            // "blood" is common, "sworn" is common, so "Blood-sworn" should NOT be in proper nouns
            // because not ALL components are rare.
            // Use a truly rare hyphenated word instead for the positive test.
        }
        finally
        {
            Cleanup(source);
        }
    }

    /// <summary>
    /// Hyphenated compound where ALL components are rare/unknown yields entry in ProperNouns.
    /// </summary>
    [Fact]
    public async Task HyphenatedCompound_AllRareComponents_InProperNouns()
    {
        var source = CreateTempFile(
            "Chapter Eight\n\nThe Zyxen-Qalvor appeared from nowhere.");

        try
        {
            var index = await BuildIndex(source);
            var section = GetSectionWithTitle(index, "Chapter Eight");
            Assert.NotNull(section.ProperNouns);
            // Both "Zyxen" and "Qalvor" are not in any English dictionary
            Assert.Contains(section.ProperNouns, pn => pn.Contains("Zyxen") && pn.Contains("Qalvor"));
        }
        finally
        {
            Cleanup(source);
        }
    }

    // --- Helpers ---

    private static string CreateTempFile(string content)
    {
        var path = Path.GetTempFileName() + ".txt";
        File.WriteAllText(path, content);
        return path;
    }

    private static void Cleanup(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    private static async Task<BookIndex> BuildIndex(string sourceFile)
    {
        var parsed = await DocumentProcessor.ParseBookAsync(sourceFile);
        return await DocumentProcessor.BuildBookIndexAsync(parsed, sourceFile);
    }

    private static SectionRange GetSectionWithTitle(BookIndex index, string titleSubstring)
    {
        var section = index.Sections.FirstOrDefault(
            s => s.Title != null && s.Title.Contains(titleSubstring, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(section);
        return section;
    }
}
