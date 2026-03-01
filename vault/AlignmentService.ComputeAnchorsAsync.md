---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# AlignmentService::ComputeAnchorsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`


#### [[AlignmentService.ComputeAnchorsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AnchorDocument> ComputeAnchorsAsync(ChapterContext context, AnchorComputationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[IAnchorComputeService.ComputeAnchorsAsync]]

