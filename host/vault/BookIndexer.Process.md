---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 23
fan_in: 1
fan_out: 14
tags:
  - method
  - danger/high-complexity
  - llm/factory
  - llm/utility
---
# BookIndexer::Process
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.

## Summary
**Builds a canonical `BookIndex` from parsed paragraphs and pronunciation data by deriving word, sentence, paragraph, and section ranges.**

`Process` performs the core synchronous index build: it computes `SourceFileHash`, then iterates `paragraphTexts` to emit `BookWord`, `SentenceRange`, `ParagraphRange`, and `SectionRange` collections with global word/sentence counters. It classifies/opens/closes sections using heading heuristics (`ShouldStartSection`, `GetHeadingLevel`, `ClassifySectionKind`), tokenizes paragraph text, normalizes tokens, attaches pronunciation phonemes from the provided lookup, and closes sentences on terminal punctuation or paragraph-end fallback. It marks non-lexical paragraphs as `"Meta"`/`"Pause"`, computes `BookTotals` (including estimated duration from `options.AverageWpm`), closes any remaining open section, applies duplicate chapter suffix normalization, and materializes the final `BookIndex` with array-backed collections.


#### [[BookIndexer.Process]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private BookIndex Process(BookParseResult parseResult, string sourceFile, BookIndexOptions options, List<(string Text, string Style, string Kind)> paragraphTexts, IReadOnlyDictionary<string, string[]> pronunciations, CancellationToken cancellationToken)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]
- [[BookIndexer.ApplyChapterDuplicateSuffixes]]
- [[BookIndexer.ClassifySectionKind]]
- [[BookIndexer.ComputeFileHash]]
- [[BookIndexer.ContainsLexicalContent]]
- [[BookIndexer.GetHeadingLevel]]
- [[BookIndexer.IsSentenceTerminal]]
- [[BookIndexer.LooksLikeHeadingStyle]]
- [[BookIndexer.LooksLikeSectionHeading]]
- [[BookIndexer.NormalizeHeadingArtifacts]]
- [[BookIndexer.NormalizeTokenSurface]]
- [[BookIndexer.ShouldSkipParagraphFromIndex]]
- [[BookIndexer.ShouldStartSection]]
- [[BookIndexer.TokenizeByWhitespace]]

**Called-by <-**
- [[BookIndexer.CreateIndexAsync]]

