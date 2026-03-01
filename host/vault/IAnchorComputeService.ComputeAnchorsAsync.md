---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/IAnchorComputeService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/di
  - llm/utility
---
# IAnchorComputeService::ComputeAnchorsAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/IAnchorComputeService.cs`

## Summary
**Declares the anchor compute service API for asynchronously computing chapter anchor documents.**

`ComputeAnchorsAsync(...)` is the sole asynchronous contract method on `IAnchorComputeService` for producing anchor mappings from chapter book/ASR context. As an interface member it has no implementation body, but its signature defines optional computation options and cancellation support, returning `Task<AnchorDocument>`. The XML docs establish it as the service boundary for anchor computation.


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

