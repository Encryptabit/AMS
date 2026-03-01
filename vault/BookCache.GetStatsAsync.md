---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
---
# BookCache::GetStatsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.GetStatsAsync]]
##### What it does:
<member name="M:Ams.Core.Runtime.Book.BookCache.GetStatsAsync(System.Threading.CancellationToken)">
    <summary>
    Gets cache statistics including total cache size and number of cached items.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookCacheStats> GetStatsAsync(CancellationToken cancellationToken = default(CancellationToken))
```

