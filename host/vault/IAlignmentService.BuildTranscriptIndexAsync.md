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
# IAlignmentService::BuildTranscriptIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IAlignmentService.cs`

## Summary
**Provide a cancellable asynchronous API to build and return a chapter transcript index using caller-supplied or default build options.**

`IAlignmentService.BuildTranscriptIndexAsync` defines the async service contract for producing a `TranscriptIndex` from a `ChapterContext`, optional `TranscriptBuildOptions`, and a `CancellationToken`. In the concrete `AlignmentService` facade, this method is complexity-1 pass-through logic that directly delegates to injected `_transcriptService.BuildTranscriptIndexAsync(context, options, cancellationToken)`. It is invoked by `BuildTranscriptIndexCommand.ExecuteAsync`, which prepares paths/options before awaiting this call and persisting the resulting transcript onto chapter documents.


#### [[IAlignmentService.BuildTranscriptIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]

