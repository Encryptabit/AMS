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
# AlignmentService::BuildTranscriptIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`

## Summary
**Delegates transcript index construction to the underlying transcript index service.**

`BuildTranscriptIndexAsync` is a thin facade method that directly forwards to `_transcriptService.BuildTranscriptIndexAsync(context, options, cancellationToken)`. It performs no local validation, mapping, or exception handling, relying on the delegated service for behavior. This maintains backward-compatible `IAlignmentService` entry points while using focused service implementations.


#### [[AlignmentService.BuildTranscriptIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ITranscriptIndexService.BuildTranscriptIndexAsync]]

