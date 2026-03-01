---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineService::EnsureBookIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Conditionally builds the book index exactly when needed, under force/exists rules and semaphore-based concurrency control, and reports whether work was performed.**

`EnsureBookIndexAsync` performs a guarded, idempotent book-index build with concurrency coordination. It refreshes `options.BookIndexFile`, derives `requestRebuild` from `ForceIndex || Force`, and returns `false` early if a forced rebuild cannot obtain `TryClaimBookIndexForce` or if a non-forced run sees an existing index. It then awaits `WaitAsync` on `BookIndexSemaphore`, re-checks existence inside the critical section to avoid race conditions, calls `BuildBookIndexAsync`, returns `true` only when a build actually ran, and always invokes `Release` in a `finally` block.


#### [[PipelineService.EnsureBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<bool> EnsureBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineConcurrencyControl.TryClaimBookIndexForce]]
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.Release]]
- [[PipelineService.WaitAsync]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

