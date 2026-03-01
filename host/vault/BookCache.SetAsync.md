---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookCache::SetAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Validates and asynchronously stores a book index in the cache for its source file.**

`SetAsync` asynchronously writes a `BookIndex` to the file-backed cache and reports success as `bool`. It validates inputs (`bookIndex` non-null and `bookIndex.SourceFile` non-empty), resolves the target cache path with `GetCacheFilePath`, ensures the parent directory exists, serializes with configured `_jsonOptions`, and persists via `File.WriteAllTextAsync`. Any non-cancellation/non-argument exception is wrapped in `BookCacheException` with source-file context.


#### [[BookCache.SetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]

