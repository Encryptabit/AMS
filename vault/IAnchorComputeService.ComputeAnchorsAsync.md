---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/IAnchorComputeService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# IAnchorComputeService::ComputeAnchorsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/IAnchorComputeService.cs`


#### [[IAnchorComputeService.ComputeAnchorsAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.IAnchorComputeService.ComputeAnchorsAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.AnchorComputationOptions,System.Threading.CancellationToken)">
    <summary>
    Computes anchor points between book text and ASR transcript.
    </summary>
    <param name="context">The chapter context containing book and ASR documents.</param>
    <param name="options">Optional computation options.</param>
    <param name="cancellationToken">Cancellation token.</param>
    <returns>An anchor document containing computed anchors.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<AnchorDocument> ComputeAnchorsAsync(ChapterContext context, AnchorComputationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[AlignmentService.ComputeAnchorsAsync]]

