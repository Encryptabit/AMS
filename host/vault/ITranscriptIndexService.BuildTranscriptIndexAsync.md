---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/ITranscriptIndexService.cs"
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
# ITranscriptIndexService::BuildTranscriptIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/ITranscriptIndexService.cs`

## Summary
**Declares the transcript index service API for asynchronously building a chapter transcript index.**

`BuildTranscriptIndexAsync(...)` is the asynchronous contract method on `ITranscriptIndexService` for producing transcript alignments from chapter data. As an interface member it has no implementation logic, but defines optional `TranscriptBuildOptions`, cancellation support, and a `Task<TranscriptIndex>` result. The XML docs specify its role as the boundary for book-to-ASR alignment and transcript rollup generation.


#### [[ITranscriptIndexService.BuildTranscriptIndexAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.ITranscriptIndexService.BuildTranscriptIndexAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.TranscriptBuildOptions,System.Threading.CancellationToken)">
    <summary>
    Builds a transcript index by aligning book text with ASR transcript.
    </summary>
    <param name="context">The chapter context containing book and ASR documents.</param>
    <param name="options">Optional build options.</param>
    <param name="cancellationToken">Cancellation token.</param>
    <returns>A transcript index containing word alignments and rollups.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[AlignmentService.BuildTranscriptIndexAsync]]

