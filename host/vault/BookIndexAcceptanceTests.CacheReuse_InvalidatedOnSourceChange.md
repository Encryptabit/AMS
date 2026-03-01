---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 5
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
---
# BookIndexAcceptanceTests::CacheReuse_InvalidatedOnSourceChange
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Verify that a cached book representation is invalidated and replaced when the underlying source content changes.**

This asynchronous acceptance test exercises cache invalidation on source mutation by orchestrating `BuildBookIndexAsync`, `CreateBookCache`, `GetAsync`, `ParseBookAsync`, and `SetAsync` in sequence. It simulates reading a cached book entry, reparsing after the source changes, and writing the refreshed value back so stale cache reuse is prevented. With complexity 3, the method is a compact arrange/act/assert flow with minimal branching around cache state.


#### [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task CacheReuse_InvalidatedOnSourceChange()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

