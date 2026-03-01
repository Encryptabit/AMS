---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookCache::RemoveAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Removes a source file’s cached index artifact asynchronously and reports whether deletion occurred.**

`RemoveAsync` asynchronously evicts a cached entry for a source file and returns whether a file was actually deleted. It validates `sourceFile`, resolves the cache-path via `GetCacheFilePath`, checks existence, and when present performs deletion inside `Task.Run(() => File.Delete(...), cancellationToken)` before returning `true`; missing files return `false`. Non-cancellation/non-argument exceptions are wrapped in `BookCacheException` with source-file context.


#### [[BookCache.RemoveAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]

**Called-by <-**
- [[BookCache.GetAsync]]

