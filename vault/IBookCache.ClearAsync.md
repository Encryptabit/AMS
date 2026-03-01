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
# IBookCache::ClearAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`


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

