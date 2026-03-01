---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/validation
---
# DocumentService::BuildIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`

## Summary
**Produces a book index from a source document, optionally reusing/storing cache entries and filling missing phonemes.**

This async orchestration method builds a `BookIndex` with optional cache reuse and phoneme enrichment. It validates `sourceFile` (`ThrowIfNullOrWhiteSpace`), returns a cached index via `_cache.GetAsync` when `forceRefresh` is false and a cache exists, otherwise parses and indexes the source through `DocumentProcessor.ParseBookAsync` and `BuildBookIndexAsync`. If a pronunciation provider is configured it augments the result via `PopulateMissingPhonemesAsync`, then writes the final index back to cache with `_cache.SetAsync` when available.


#### [[DocumentService.BuildIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> BuildIndexAsync(string sourceFile, BookIndexOptions options = null, bool forceRefresh = false, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[DocumentService.ParseAndPopulatePhonemesAsync]]

