---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
---
# DocumentProcessor::TryLoadCachedIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs`


#### [[DocumentProcessor.TryLoadCachedIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> TryLoadCachedIndexAsync(string sourceFile, string cacheDirectory = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.CreateBookCache]]
- [[IBookCache.GetAsync]]

