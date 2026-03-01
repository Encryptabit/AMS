---
namespace: "Ams.Core.Services.Documents"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# DocumentService::ParseAndPopulatePhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`


#### [[DocumentService.ParseAndPopulatePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> ParseAndPopulatePhonemesAsync(string sourceFile, BookIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentService.BuildIndexAsync]]

