---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/di
  - llm/utility
---
# AlignmentService::ComputeAnchorsAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`

## Summary
**Forwards anchor computation requests to the underlying anchor compute service.**

`ComputeAnchorsAsync` is a facade pass-through that delegates anchor computation directly to `_anchorService.ComputeAnchorsAsync(context, options, cancellationToken)`. It adds no local validation, transformation, or error handling. The method exists to preserve `IAlignmentService` surface while relying on the injected/default anchor service implementation.


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

