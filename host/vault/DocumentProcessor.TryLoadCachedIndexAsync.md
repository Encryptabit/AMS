---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/factory
  - llm/utility
---
# DocumentProcessor::TryLoadCachedIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs`

## Summary
**Attempts to load a `BookIndex` for a source file from the configured document cache.**

`TryLoadCachedIndexAsync` is a thin async-forwarding helper: it creates an `IBookCache` via `CreateBookCache(cacheDirectory)` and immediately returns `cache.GetAsync(sourceFile, cancellationToken)`. It performs no extra validation, transformation, or fallback logic around cache retrieval.


#### [[DocumentProcessor.TryLoadCachedIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> TryLoadCachedIndexAsync(string sourceFile, string cacheDirectory = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.CreateBookCache]]
- [[IBookCache.GetAsync]]

