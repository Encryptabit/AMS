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
  - llm/async
  - llm/di
  - llm/utility
---
# IAlignmentService::HydrateTranscriptAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs`

## Summary
**Asynchronously delegates transcript hydration for a chapter to the injected hydration service and returns the resulting `HydratedTranscript`.**

`IAlignmentService.HydrateTranscriptAsync` is implemented in `AlignmentService` as a thin facade call that forwards `context`, `options`, and `cancellationToken` directly to `_hydrationService.HydrateTranscriptAsync(...)`, which matches the reported complexity of 1. The method itself adds no validation or branching, and simply returns the downstream task. `HydrateTranscriptCommand.ExecuteAsync` is a direct caller, awaiting this method before saving the chapter.


#### [[IAlignmentService.HydrateTranscriptAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<HydratedTranscript> HydrateTranscriptAsync(ChapterContext context, HydrationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[HydrateTranscriptCommand.ExecuteAsync]]

