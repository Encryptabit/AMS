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
# DocumentProcessor::WriteCacheAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs`

## Summary
**Writes a `BookIndex` into the configured document cache asynchronously.**

`WriteCacheAsync` is a pass-through cache write helper that instantiates a cache via `CreateBookCache(cacheDirectory)` and returns `cache.SetAsync(bookIndex, cancellationToken)`. It adds no extra validation or retry behavior around persistence.


#### [[DocumentProcessor.WriteCacheAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<bool> WriteCacheAsync(BookIndex bookIndex, string cacheDirectory = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.CreateBookCache]]
- [[IBookCache.SetAsync]]

