---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/di
  - llm/error-handling
---
# IBookCache::GetAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Defines the cache API for asynchronously retrieving a valid cached `BookIndex` for a source file when available.**

`GetAsync` is an `IBookCache` interface contract method for asynchronous cache retrieval by source-file path, declared as `Task<BookIndex?> GetAsync(string sourceFile, CancellationToken cancellationToken = default)`. It specifies nullable-result semantics (cache miss/invalid/stale source) and cancellation support but contains no implementation logic. The XML docs define expected behavior and IO-failure surface (`IOException`) for concrete cache providers.


#### [[IBookCache.GetAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.GetAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Retrieves a cached book index for the specified source file.
    Returns null if no valid cache exists or if the source file has changed.
    </summary>
    <param name="sourceFile">Path to the original source file</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>Cached book index or null if not found/invalid</returns>
    <exception cref="T:System.IO.IOException">Cache file could not be read</exception>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookIndex> GetAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[DocumentProcessor.TryLoadCachedIndexAsync]]
- [[DocumentService.BuildIndexAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
- [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]

