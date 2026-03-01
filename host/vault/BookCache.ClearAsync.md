---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/error-handling
  - llm/validation
---
# BookCache::ClearAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Clears the on-disk book cache by deleting all cached JSON entries with cancellation support.**

`ClearAsync` asynchronously purges cached JSON files under `_cacheDirectory` when the directory exists. It runs deletion work inside `Task.Run`, enumerates `*.json` recursively, checks cancellation per file (`cancellationToken.ThrowIfCancellationRequested()`), and deletes each file via `File.Delete`. `OperationCanceledException` is allowed to propagate, while other exceptions are wrapped as `BookCacheException` with cache-directory context.


#### [[BookCache.ClearAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ClearAsync(CancellationToken cancellationToken = default(CancellationToken))
```

