---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 5
tags:
  - method
---
# BookIndexer::CreateIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`


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

