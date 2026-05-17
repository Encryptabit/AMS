namespace Ams.Core.Runtime.Book;

internal static class BookIndexCompatibility
{
    public static BookIndexCompatibilityResult ValidateForCache(BookIndex? index)
    {
        var issues = new List<string>();
        if (index is null)
        {
            return new BookIndexCompatibilityResult(false, ["Book index is missing."]);
        }

        if (index.SchemaVersion != BookIndex.CurrentSchemaVersion)
        {
            var actual = index.SchemaVersion?.ToString() ?? "<missing>";
            issues.Add(
                $"Book index schema version '{actual}' is not cache-compatible with version '{BookIndex.CurrentSchemaVersion}'.");
        }

        if (string.IsNullOrWhiteSpace(index.SourceFile))
        {
            issues.Add("Book index sourceFile is missing.");
        }

        if (string.IsNullOrWhiteSpace(index.SourceFileHash))
        {
            issues.Add("Book index sourceFileHash is missing.");
        }

        if (index.IndexedAt == default)
        {
            issues.Add("Book index indexedAt is missing.");
        }

        ValidateTotals(index, issues);
        ValidateWords(index, issues);
        ValidateSentenceRanges(index, issues);
        ValidateParagraphRanges(index, issues);
        ValidateSectionRanges(index, issues);

        return new BookIndexCompatibilityResult(issues.Count == 0, issues);
    }

    private static void ValidateTotals(BookIndex index, List<string> issues)
    {
        if (index.Totals is null)
        {
            issues.Add("Book index totals are missing.");
            return;
        }

        if (index.Words is not null && index.Totals.Words != index.Words.Length)
        {
            issues.Add($"Book index totals.words '{index.Totals.Words}' does not match words length '{index.Words.Length}'.");
        }

        if (index.Sentences is not null && index.Totals.Sentences != index.Sentences.Length)
        {
            issues.Add(
                $"Book index totals.sentences '{index.Totals.Sentences}' does not match sentences length '{index.Sentences.Length}'.");
        }

        if (index.Paragraphs is not null && index.Totals.Paragraphs != index.Paragraphs.Length)
        {
            issues.Add(
                $"Book index totals.paragraphs '{index.Totals.Paragraphs}' does not match paragraphs length '{index.Paragraphs.Length}'.");
        }

        if (index.Totals.Words < 0 || index.Totals.Sentences < 0 || index.Totals.Paragraphs < 0)
        {
            issues.Add("Book index totals cannot be negative.");
        }

        if (double.IsNaN(index.Totals.EstimatedDurationSec) || double.IsInfinity(index.Totals.EstimatedDurationSec))
        {
            issues.Add("Book index estimatedDurationSec must be finite.");
        }

        if (index.Totals.EstimatedDurationSec < 0d)
        {
            issues.Add("Book index estimatedDurationSec cannot be negative.");
        }
    }

    private static void ValidateWords(BookIndex index, List<string> issues)
    {
        if (index.Words is null)
        {
            issues.Add("Book index words are missing.");
            return;
        }

        for (var i = 0; i < index.Words.Length; i++)
        {
            var word = index.Words[i];
            if (word is null)
            {
                issues.Add($"Book index word at position '{i}' is missing.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(word.Text))
            {
                issues.Add($"Book index word '{i}' has empty text.");
            }

            if (word.WordIndex != i)
            {
                issues.Add($"Book index word '{i}' has non-contiguous wordIndex '{word.WordIndex}'.");
            }

            if (index.Sentences is not null && (word.SentenceIndex < 0 || word.SentenceIndex >= index.Sentences.Length))
            {
                issues.Add($"Book index word '{i}' references missing sentenceIndex '{word.SentenceIndex}'.");
            }

            if (index.Paragraphs is not null
                && (word.ParagraphIndex < 0 || word.ParagraphIndex >= index.Paragraphs.Length))
            {
                issues.Add($"Book index word '{i}' references missing paragraphIndex '{word.ParagraphIndex}'.");
            }

            if (word.SectionIndex >= 0 && index.Sections is not null
                                      && !index.Sections.Any(section => section.Id == word.SectionIndex))
            {
                issues.Add($"Book index word '{i}' references missing sectionIndex '{word.SectionIndex}'.");
            }
        }
    }

    private static void ValidateSentenceRanges(BookIndex index, List<string> issues)
    {
        if (index.Sentences is null)
        {
            issues.Add("Book index sentences are missing.");
            return;
        }

        ValidateClosedRanges(index.Sentences, index.Words?.Length ?? 0, "sentence", issues);
    }

    private static void ValidateParagraphRanges(BookIndex index, List<string> issues)
    {
        if (index.Paragraphs is null)
        {
            issues.Add("Book index paragraphs are missing.");
            return;
        }

        var wordCount = index.Words?.Length ?? 0;
        foreach (var paragraph in index.Paragraphs)
        {
            if (paragraph is null)
            {
                issues.Add("Book index paragraph range is missing.");
                continue;
            }

            if (paragraph.Index < 0)
            {
                issues.Add($"Book index paragraph has negative index '{paragraph.Index}'.");
            }

            if (paragraph.Start < 0)
            {
                issues.Add($"Book index paragraph '{paragraph.Index}' has negative start '{paragraph.Start}'.");
            }

            var isEmptyRange = paragraph.End == paragraph.Start - 1;
            if (!isEmptyRange && paragraph.End < paragraph.Start)
            {
                issues.Add($"Book index paragraph '{paragraph.Index}' has end before start.");
            }

            if (!isEmptyRange && wordCount == 0)
            {
                issues.Add($"Book index paragraph '{paragraph.Index}' references words in an empty word list.");
            }

            if (!isEmptyRange && wordCount > 0 && paragraph.End >= wordCount)
            {
                issues.Add($"Book index paragraph '{paragraph.Index}' end '{paragraph.End}' exceeds word count '{wordCount}'.");
            }
        }
    }

    private static void ValidateSectionRanges(BookIndex index, List<string> issues)
    {
        if (index.Sections is null)
        {
            issues.Add("Book index sections are missing.");
            return;
        }

        var ids = new HashSet<int>();
        var wordCount = index.Words?.Length ?? 0;
        foreach (var section in index.Sections)
        {
            if (section is null)
            {
                issues.Add("Book index section range is missing.");
                continue;
            }

            if (!ids.Add(section.Id))
            {
                issues.Add($"Book index section id '{section.Id}' is duplicated.");
            }

            if (section.Id < 0)
            {
                issues.Add($"Book index section has negative id '{section.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(section.Title))
            {
                issues.Add($"Book index section '{section.Id}' has empty title.");
            }

            if (section.Level <= 0)
            {
                issues.Add($"Book index section '{section.Id}' has non-positive level '{section.Level}'.");
            }

            if (section.StartWord < 0 || section.EndWord < section.StartWord)
            {
                issues.Add($"Book index section '{section.Id}' has invalid word range.");
            }

            if (wordCount == 0 && section.EndWord >= 0)
            {
                issues.Add($"Book index section '{section.Id}' references words in an empty word list.");
            }

            if (wordCount > 0 && section.EndWord >= wordCount)
            {
                issues.Add($"Book index section '{section.Id}' endWord '{section.EndWord}' exceeds word count '{wordCount}'.");
            }
        }
    }

    private static void ValidateClosedRanges(
        IReadOnlyList<SentenceRange> ranges,
        int wordCount,
        string label,
        List<string> issues)
    {
        for (var i = 0; i < ranges.Count; i++)
        {
            var range = ranges[i];
            if (range is null)
            {
                issues.Add($"Book index {label} range at position '{i}' is missing.");
                continue;
            }

            if (range.Index != i)
            {
                issues.Add($"Book index {label} range '{i}' has non-contiguous index '{range.Index}'.");
            }

            if (range.Start < 0)
            {
                issues.Add($"Book index {label} range '{i}' has negative start '{range.Start}'.");
            }

            if (range.End < range.Start)
            {
                issues.Add($"Book index {label} range '{i}' has end before start.");
            }

            if (wordCount == 0)
            {
                issues.Add($"Book index {label} range '{i}' references words in an empty word list.");
            }

            if (wordCount > 0 && range.End >= wordCount)
            {
                issues.Add($"Book index {label} range '{i}' end '{range.End}' exceeds word count '{wordCount}'.");
            }
        }
    }
}

internal sealed record BookIndexCompatibilityResult(bool IsCompatible, IReadOnlyList<string> Issues);
