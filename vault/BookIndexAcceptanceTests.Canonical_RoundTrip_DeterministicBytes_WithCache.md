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
# BookIndexAcceptanceTests::Canonical_RoundTrip_DeterministicBytes_WithCache
**Path**: `home/cari/repos/AMS/host/Ams.Tests/BookParsingTests.cs`


#### [[BookIndexAcceptanceTests.Canonical_RoundTrip_DeterministicBytes_WithCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task Canonical_RoundTrip_DeterministicBytes_WithCache()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

