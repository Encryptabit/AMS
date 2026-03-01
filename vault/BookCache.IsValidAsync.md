---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookCache::IsValidAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.IsValidAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> IsValidAsync(BookIndex bookIndex, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BookCache.ComputeFileHashAsync]]

**Called-by <-**
- [[BookCache.GetAsync]]

