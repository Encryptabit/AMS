---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PipelineService::Release
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Safely release an optional `SemaphoreSlim` guarding pipeline concurrency without requiring callers to null-check.**

`Release` is a private static helper in `PipelineService` that performs a null-safe semaphore release using `semaphore?.Release()`. It is used from `finally` blocks after `WaitAsync` acquisitions in stage/concurrency paths (ASR, MFA, and book-index), so lock release logic is centralized and safe when concurrency semaphores are not configured.


#### [[PipelineService.Release]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void Release(SemaphoreSlim semaphore)
```

**Called-by <-**
- [[PipelineService.EnsureBookIndexAsync]]
- [[PipelineService.RunChapterAsync]]

