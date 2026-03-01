---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/di
---
# PipelineService::BuildBookIndexInternal
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Parse a book file, build its index with the provided reading-speed option and pronunciation provider, then ensure phoneme completeness before returning.**

BuildBookIndexInternal is an async orchestration helper in `PipelineService` that parses the source file via `DocumentProcessor.ParseBookAsync(bookFile.FullName, cancellationToken)`, then builds a `BookIndex` using `DocumentProcessor.BuildBookIndexAsync` with a new `BookIndexOptions { AverageWpm = averageWpm }` and the injected `_pronunciationProvider`. It uses `ConfigureAwait(false)` on each await and returns `EnsurePhonemesAsync(built, cancellationToken)`, so the caller gets a phoneme-normalized index.


#### [[PipelineService.BuildBookIndexInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookIndex> BuildBookIndexInternal(FileInfo bookFile, double averageWpm, CancellationToken cancellationToken)
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[PipelineService.EnsurePhonemesAsync]]

**Called-by <-**
- [[PipelineService.BuildBookIndexAsync]]

