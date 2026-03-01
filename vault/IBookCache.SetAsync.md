---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 0
tags:
  - method
---
# IBookCache::SetAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


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

