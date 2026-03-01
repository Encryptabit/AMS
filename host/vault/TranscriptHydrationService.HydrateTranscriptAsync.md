---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# TranscriptHydrationService::HydrateTranscriptAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Builds and stores a hydrated transcript for a chapter by delegating enrichment work and updating chapter documents.**

`HydrateTranscriptAsync` is the public orchestration entry that validates `context`, requires a loaded `TranscriptIndex` from `context.Documents.Transcript`, and delegates heavy enrichment work to `BuildHydratedTranscriptAsync(...)`. It persists the resulting `HydratedTranscript` back to `context.Documents.HydratedTranscript` before returning it. Missing transcript state is surfaced via `InvalidOperationException`.


#### [[TranscriptHydrationService.HydrateTranscriptAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<HydratedTranscript> HydrateTranscriptAsync(ChapterContext context, HydrationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

