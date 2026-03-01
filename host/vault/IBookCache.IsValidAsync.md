---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/di
  - llm/validation
---
# IBookCache::IsValidAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Defines the cache API for asynchronously determining whether a cached book index remains valid.**

`IsValidAsync` is an `IBookCache` interface contract for asynchronously validating cache freshness of a `BookIndex` against its source file state. The documented semantics include modification-time and hash-based checks, with cancellation support and boolean validity output. As an interface member, it defines behavior expectations but no implementation logic.


#### [[IBookCache.IsValidAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.IsValidAsync(Ams.Core.Runtime.Book.BookIndex,System.Threading.CancellationToken)">
    <summary>
    Validates that a cached book index is still current for its source file.
    Checks file modification time and content hash.
    </summary>
    <param name="bookIndex">Book index to validate</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>True if the cache entry is still valid</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

