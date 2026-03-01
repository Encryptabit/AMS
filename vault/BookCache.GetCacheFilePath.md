---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
---
# BookCache::GetCacheFilePath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.GetCacheFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetCacheFilePath(string sourceFile)
```

**Calls ->**
- [[BookCache.ComputeStringHash]]

**Called-by <-**
- [[BookCache.GetAsync]]
- [[BookCache.RemoveAsync]]
- [[BookCache.SetAsync]]

