---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IDocumentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# IDocumentService::BuildIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IDocumentService.cs`


#### [[IDocumentService.BuildIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<BookIndex> BuildIndexAsync(string sourceFile, BookIndexOptions options = null, bool forceRefresh = false, CancellationToken cancellationToken = default(CancellationToken))
```

