---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 5
tags:
  - method
---
# DocumentService::BuildIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`


#### [[DocumentService.BuildIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> BuildIndexAsync(string sourceFile, BookIndexOptions options = null, bool forceRefresh = false, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[DocumentService.ParseAndPopulatePhonemesAsync]]

