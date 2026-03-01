---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# BookIndexer::CreateIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Asynchronously builds a `BookIndex` by preparing text structure, resolving pronunciations, and running indexed document processing with contextual exception wrapping.**

`CreateIndexAsync` orchestrates end-to-end index construction with async pronunciation enrichment and guarded error handling. It validates `parseResult`/`sourceFile`, normalizes `options` (`options ??= new BookIndexOptions()`), builds/folds paragraph text, collects lexical tokens, then asynchronously fetches pronunciations via `_pronunciationProvider.GetPronunciationsAsync(...)`. It wraps pronunciation failures (except cancellation) in `BookIndexException`, then executes heavy indexing work in `Task.Run(() => Process(...), cancellationToken)`, wrapping non-cancellation/non-argument failures similarly. The method returns the fully built `BookIndex` produced by `Process`.


#### [[BookIndexer.CreateIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> CreateIndexAsync(BookParseResult parseResult, string sourceFile, BookIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[IPronunciationProvider.GetPronunciationsAsync]]
- [[BookIndexer.BuildParagraphTexts]]
- [[BookIndexer.CollectLexicalTokens]]
- [[BookIndexer.FoldAdjacentHeadings]]
- [[BookIndexer.Process]]

**Called-by <-**
- [[DocumentProcessor.BuildBookIndexAsync]]

