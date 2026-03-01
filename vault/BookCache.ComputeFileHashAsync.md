---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# BookCache::ComputeFileHashAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookCache.cs`


#### [[BookCache.ComputeFileHashAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
```

**Called-by <-**
- [[BookCache.IsValidAsync]]

