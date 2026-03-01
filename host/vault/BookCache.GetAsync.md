---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookCache::GetAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Asynchronously loads a cached book index, validates it against the source file, and invalidates stale cache entries.**

`GetAsync` retrieves a cached `BookIndex` for `sourceFile` with async file IO and cache-integrity checks. It validates `sourceFile`, resolves the cache file path (`GetCacheFilePath`), returns `null` if no cache file exists, otherwise reads and deserializes JSON into `BookIndex`. If deserialization succeeds, it verifies cache validity via `IsValidAsync`; valid entries are returned, while invalid entries are proactively evicted via `RemoveAsync` and treated as cache misses. Non-cancellation/non-argument failures are wrapped in `BookCacheException` with source-file context.


#### [[BookCache.GetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> GetAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]
- [[BookCache.IsValidAsync]]
- [[BookCache.RemoveAsync]]

