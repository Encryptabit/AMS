---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 8
tags:
  - method
---
# AnchorComputeService::ComputeAnchorsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/AnchorComputeService.cs`


#### [[AnchorComputeService.ComputeAnchorsAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.AnchorComputeService.ComputeAnchorsAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.AnchorComputationOptions,System.Threading.CancellationToken)">
    <inheritdoc />
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AnchorDocument> ComputeAnchorsAsync(ChapterContext context, AnchorComputationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterContext.SetDetectedSection]]
- [[AnchorComputeService.BuildAnchorDocument]]
- [[AnchorComputeService.BuildPolicy]]
- [[AnchorComputeService.RequireBookAndAsr]]

**Called-by <-**
- [[AnchorComputeServiceTests.ComputeAnchorsAsync_NullContext_ThrowsArgumentNullException]]

