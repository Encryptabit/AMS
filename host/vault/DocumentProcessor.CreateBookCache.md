---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# DocumentProcessor::CreateBookCache
**Path**: `Projects/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs`

## Summary
**Creates an `IBookCache` implementation for book-index cache operations, optionally rooted at a caller-specified directory.**

`CreateBookCache` is an expression-bodied factory method that instantiates `new BookCache(cacheDirectory)` and returns it as `IBookCache`. It forwards the optional `cacheDirectory` argument unchanged and performs no additional validation, memoization, or configuration logic.


#### [[DocumentProcessor.CreateBookCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IBookCache CreateBookCache(string cacheDirectory = null)
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[DocumentProcessor.TryLoadCachedIndexAsync]]
- [[DocumentProcessor.WriteCacheAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
- [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]

