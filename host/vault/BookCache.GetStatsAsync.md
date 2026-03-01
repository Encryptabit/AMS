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
---
# BookCache::GetStatsAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Returns asynchronous aggregate statistics about cache contents, size, and entry validity.**

`GetStatsAsync` asynchronously computes cache metrics for `_cacheDirectory`: total JSON file count, valid-entry count, and total byte size. It short-circuits to `new BookCacheStats(0, 0, 0)` when the directory is missing; otherwise it runs a `Task.Run` scan over recursive `*.json` files, summing `FileInfo.Length` and incrementing valid count when a deserialized `BookIndex` exists and its `SourceFile` still exists. Per-file parse/read failures are swallowed to ignore corrupt cache entries, cancellation is honored during iteration, and outer non-cancellation failures are wrapped in `BookCacheException`.


#### [[BookCache.GetStatsAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.BookCache.GetStatsAsync(System.Threading.CancellationToken)">
    <summary>
    Gets cache statistics including total cache size and number of cached items.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookCacheStats> GetStatsAsync(CancellationToken cancellationToken = default(CancellationToken))
```

