---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookCache::IsValidAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Determines whether a cached book index is still valid relative to the current source file state.**

`IsValidAsync` verifies cache integrity for a `BookIndex` against its source file using asynchronous checks. It validates `bookIndex`, ensures `bookIndex.SourceFile` exists, computes the current file hash via `ComputeFileHashAsync`, and compares it to `SourceFileHash` (ordinal-ignore-case). It also invalidates cache entries when the source file was modified after `IndexedAt` (`LastWriteTimeUtc > IndexedAt`). Non-cancellation/non-argument failures are wrapped in `BookCacheException` with source-file context.


#### [[BookCache.IsValidAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.ComputeFileHashAsync]]

**Called-by <-**
- [[BookCache.GetAsync]]

