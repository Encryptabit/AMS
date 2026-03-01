---
namespace: "Ams.Core.Processors.DocumentProcessor"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
---
# DocumentProcessor::CreateBookCache
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/DocumentProcessor/DocumentProcessor.Cache.cs`


#### [[DocumentProcessor.CreateBookCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IBookCache CreateBookCache(string cacheDirectory = null)
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[DocumentProcessor.TryLoadCachedIndexAsync]]
- [[DocumentProcessor.WriteCacheAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
- [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]

