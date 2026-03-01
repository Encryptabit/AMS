---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 6
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PipelineService::BuildBookIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Builds (or reuses and enriches) a book index for the configured book file and persists it as JSON to the pipeline’s book-index output path.**

`BuildBookIndexAsync` validates `options.BookFile` with guard clauses (`InvalidOperationException` if missing, `FileNotFoundException` if absent on disk), logs the build target, and chooses between cached and rebuilt index paths based on `options.ForceIndex` and `DocumentProcessor.CreateBookCache()`. On cache hit it calls `EnsurePhonemesAsync` and conditionally backfills the cache with `SetAsync` when phoneme enrichment returns a different `BookIndex`; on miss or forced rebuild it calls `BuildBookIndexInternal(bookFile, options.AverageWordsPerMinute, cancellationToken)` and stores the result in cache. It then serializes the final index using indented camelCase JSON options and writes it to `options.BookIndexFile` with `File.WriteAllTextAsync`.


#### [[PipelineService.BuildBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task BuildBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.CreateBookCache]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]
- [[PipelineService.BuildBookIndexInternal]]
- [[PipelineService.EnsurePhonemesAsync]]

**Called-by <-**
- [[PipelineService.EnsureBookIndexAsync]]

