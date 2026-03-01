---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
---
# HydrateTranscriptCommand::ExecuteAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs`

## Summary
**It runs transcript hydration for a chapter through the alignment service and saves the updated chapter.**

`ExecuteAsync` is the command entrypoint that validates `chapter` via `ArgumentNullException.ThrowIfNull(chapter)`, then delegates transcript hydration to `_alignmentService.HydrateTranscriptAsync(chapter, options, cancellationToken)` with `ConfigureAwait(false)`. After the async hydration call completes, it persists chapter state by invoking `chapter.Save()`. The method contains no branching and acts as orchestration around service invocation plus persistence.


#### [[HydrateTranscriptCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, HydrationOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ChapterContext.Save]]
- [[IAlignmentService.HydrateTranscriptAsync]]

**Called-by <-**
- [[AlignCommand.CreateHydrateTx]]
- [[PipelineService.RunChapterAsync]]

