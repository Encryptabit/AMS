---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 23
fan_in: 1
fan_out: 14
tags:
  - method
  - danger/high-complexity
---
# BookIndexer::Process
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.


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

