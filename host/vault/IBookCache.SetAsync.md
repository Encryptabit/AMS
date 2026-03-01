---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/di
  - llm/validation
  - llm/error-handling
---
# IBookCache::SetAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Defines the cache API for asynchronously storing a validated book index and reporting write success.**

`SetAsync` is the asynchronous `IBookCache` contract for persisting a `BookIndex` into cache storage, returning a success boolean. As an interface member it contains no implementation, but the API and docs specify cancellation support, integrity-oriented caching semantics, and expected failure categories (`ArgumentException` for invalid index, `IOException` for write failures). It defines the write-side counterpart to cache retrieval operations.


#### [[IBookCache.SetAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.SetAsync(Ams.Core.Runtime.Book.BookIndex,System.Threading.CancellationToken)">
    <summary>
    Stores a book index in the cache with file integrity validation.
    The cache entry will be associated with the SHA256 hash of the source file.
    </summary>
    <param name="bookIndex">Book index to cache</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>True if successfully cached</returns>
    <exception cref="T:System.ArgumentException">Invalid book index</exception>
    <exception cref="T:System.IO.IOException">Cache file could not be written</exception>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[BuildIndexCommand.ProcessBookFromScratch]]
- [[DocumentProcessor.WriteCacheAsync]]
- [[DocumentService.BuildIndexAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
- [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]

