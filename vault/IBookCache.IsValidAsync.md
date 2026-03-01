---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IBookCache::IsValidAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


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

