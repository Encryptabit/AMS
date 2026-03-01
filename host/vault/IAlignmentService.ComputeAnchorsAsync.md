---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/di
---
# IAlignmentService::ComputeAnchorsAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs`

## Summary
**Delegate chapter anchor computation to the configured anchor compute service and return the resulting `AnchorDocument` task.**

In `AlignmentService`, `ComputeAnchorsAsync` is a thin facade: it directly returns `_anchorService.ComputeAnchorsAsync(context, options, cancellationToken)` with no local `await`, validation, or error translation. The `_anchorService` dependency is constructor-injected and defaults to `new AnchorComputeService()` when not supplied, so behavior is fully delegated to that service while preserving caller-provided options and cancellation.


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

