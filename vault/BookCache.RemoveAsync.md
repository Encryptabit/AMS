---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookCache::RemoveAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.RemoveAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> RemoveAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]

**Called-by <-**
- [[BookCache.GetAsync]]

