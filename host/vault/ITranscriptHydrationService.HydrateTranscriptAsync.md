---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/ITranscriptHydrationService.cs"
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
# ITranscriptHydrationService::HydrateTranscriptAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/ITranscriptHydrationService.cs`

## Summary
**Declares the transcript hydration service API for asynchronously producing a hydrated transcript from chapter context data.**

`HydrateTranscriptAsync(...)` is the asynchronous contract method on `ITranscriptHydrationService` for transforming chapter transcript data into a hydrated/enriched model. As an interface member it defines API shape only (no implementation), including optional `HydrationOptions`, cancellation support, and `Task<HydratedTranscript>` return semantics. Its docs position it as the boundary for computing diffs, metrics, and enriched sentence/paragraph structures.


#### [[ITranscriptHydrationService.HydrateTranscriptAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.ITranscriptHydrationService.HydrateTranscriptAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.HydrationOptions,System.Threading.CancellationToken)">
    <summary>
    Hydrates a transcript index by computing diffs, metrics, and building enriched sentence/paragraph data.
    </summary>
    <param name="context">The chapter context containing transcript index and related documents.</param>
    <param name="options">Optional hydration options.</param>
    <param name="cancellationToken">Cancellation token.</param>
    <returns>A fully hydrated transcript with computed metrics and diffs.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<HydratedTranscript> HydrateTranscriptAsync(ChapterContext context, HydrationOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[AlignmentService.HydrateTranscriptAsync]]

