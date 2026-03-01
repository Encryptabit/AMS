---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
---
# BookCache::GetAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.GetAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> GetAsync(string sourceFile, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.GetCacheFilePath]]
- [[BookCache.IsValidAsync]]
- [[BookCache.RemoveAsync]]

