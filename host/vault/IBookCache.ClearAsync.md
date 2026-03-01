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
---
# IBookCache::ClearAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Defines the cache API for asynchronously clearing all cached book-index entries.**

`ClearAsync` is an `IBookCache` interface contract for asynchronous global cache eviction. It declares cancellation support and no return payload, implying side-effect-only behavior across all cached entries. As an interface member, it defines capability and intent without prescribing implementation strategy.


#### [[IBookCache.ClearAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.ClearAsync(System.Threading.CancellationToken)">
    <summary>
    Clears all cached book indexes.
    </summary>
    <param name="cancellationToken">Cancellation token</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task ClearAsync(CancellationToken cancellationToken = default(CancellationToken))
```

