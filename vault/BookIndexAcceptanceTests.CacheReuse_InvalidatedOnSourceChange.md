---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "home/cari/repos/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 5
tags:
  - method
---
# BookIndexAcceptanceTests::CacheReuse_InvalidatedOnSourceChange
**Path**: `home/cari/repos/AMS/host/Ams.Tests/BookParsingTests.cs`


#### [[BookIndexAcceptanceTests.CacheReuse_InvalidatedOnSourceChange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task CacheReuse_InvalidatedOnSourceChange()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

