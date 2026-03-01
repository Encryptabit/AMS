---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 1
tags:
  - method
---
# BookCache::SetAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.SetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> SetAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]

