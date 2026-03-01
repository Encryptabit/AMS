---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# IAlignmentService::ComputeAnchorsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs`


#### [[IAlignmentService.ComputeAnchorsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<AnchorDocument> ComputeAnchorsAsync(ChapterContext context, AnchorComputationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[ComputeAnchorsCommand.ExecuteAsync]]

