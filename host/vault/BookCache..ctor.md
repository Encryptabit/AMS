---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/di
  - llm/factory
  - llm/data-access
  - llm/utility
---
# BookCache::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`

## Summary
**Initializes file-backed cache storage settings and prepares the cache directory and JSON serialization options.**

The `BookCache` constructor selects the cache root by using the provided `cacheDirectory` or falling back to `GetDefaultCacheDirectory()`. It eagerly ensures that directory exists via `Directory.CreateDirectory(_cacheDirectory)`. It then initializes `_jsonOptions` for compact cache serialization (`WriteIndented = false`), camelCase naming, and null-value omission (`DefaultIgnoreCondition = WhenWritingNull`).


#### [[BookCache..ctor]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.BookCache.#ctor(System.String)">
    <summary>
    Initializes a new instance of the BookCache class.
    </summary>
    <param name="cacheDirectory">Directory to store cache files. If null, uses default cache directory.</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookCache(string cacheDirectory = null)
```

**Calls ->**
- [[BookCache.GetDefaultCacheDirectory]]

