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
# AlignmentService::HydrateTranscriptAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`

## Summary
**Delegates transcript hydration to the configured transcript hydration service.**

`HydrateTranscriptAsync` is a one-line facade method that delegates hydration work to `_hydrationService.HydrateTranscriptAsync(context, options, cancellationToken)`. It performs no local argument checks, mapping, or exception handling. This keeps `AlignmentService` as a compatibility wrapper over focused underlying services.


#### [[AlignmentService.HydrateTranscriptAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<HydratedTranscript> HydrateTranscriptAsync(ChapterContext context, HydrationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ITranscriptHydrationService.HydrateTranscriptAsync]]

