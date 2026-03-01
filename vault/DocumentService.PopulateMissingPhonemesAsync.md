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
# DocumentService::PopulateMissingPhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Documents/DocumentService.cs`


#### [[DocumentService.PopulateMissingPhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<BookIndex> PopulateMissingPhonemesAsync(BookIndex index, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

