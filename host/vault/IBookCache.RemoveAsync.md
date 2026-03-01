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
# IBookCache::RemoveAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Defines the cache API for asynchronously removing a source file’s cached index entry.**

`RemoveAsync` is an `IBookCache` interface contract for asynchronously deleting a cache entry associated with a source file. It defines cancellation-aware removal semantics and a boolean result indicating whether an entry was actually removed, but provides no implementation details itself. Concrete cache backends determine path resolution, storage medium behavior, and exception policy.


#### [[IBookCache.RemoveAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.IBookCache.RemoveAsync(System.String,System.Threading.CancellationToken)">
    <summary>
    Removes a cached book index for the specified source file.
    </summary>
    <param name="sourceFile">Path to the original source file</param>
    <param name="cancellationToken">Cancellation token</param>
    <returns>True if cache entry was removed</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

