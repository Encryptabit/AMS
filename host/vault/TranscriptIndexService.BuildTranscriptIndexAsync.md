---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "public"
complexity: 12
fan_in: 0
fan_out: 16
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
---
# TranscriptIndexService::BuildTranscriptIndexAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Builds and stores a complete `TranscriptIndex` (and companion anchors) for a chapter by aligning book and ASR content with section-aware anchor processing.**

This async service method orchestrates transcript-index construction from chapter context by validating input (`ThrowIfNull`), requiring loaded Book/ASR documents, building anchor policy/views, and running `AnchorPipeline.ComputeAnchors` with optional section override or auto-detection. It ensures usable alignment windows (fallback via `BuildFallbackWindows`), then computes phoneme views (`BuildBookPhonemeView`/`BuildAsrPhonemeViewAsync`), derives word and anchor operations, rolls up sentence/paragraph structures, and stamps sentence timings via `ComputeTiming`. It resolves output paths from options or defaults, constructs a `TranscriptIndex` with metadata (`CreatedAtUtc`, normalization version), writes it back to `context.Documents.Transcript`, and also persists a derived anchor document.


#### [[TranscriptIndexService.BuildTranscriptIndexAsync]]
##### What it does:
<member name="M:Ams.Core.Services.Alignment.TranscriptIndexService.BuildTranscriptIndexAsync(Ams.Core.Runtime.Chapter.ChapterContext,Ams.Core.Services.Alignment.TranscriptBuildOptions,System.Threading.CancellationToken)">
    <inheritdoc />
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<TranscriptIndex> BuildTranscriptIndexAsync(ChapterContext context, TranscriptBuildOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]
- [[ChapterContext.GetOrResolveSection]]
- [[ChapterContext.SetDetectedSection]]
- [[TranscriptIndexService.BuildAnchorDocument]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
- [[TranscriptIndexService.BuildBookPhonemeView]]
- [[TranscriptIndexService.BuildFallbackWindows]]
- [[TranscriptIndexService.BuildPolicy]]
- [[TranscriptIndexService.BuildRollups]]
- [[TranscriptIndexService.BuildWordOperations]]
- [[TranscriptIndexService.ComputeTiming]]
- [[TranscriptIndexService.RequireBookAndAsr]]
- [[TranscriptIndexService.ResolveDefaultAudioPath]]
- [[TranscriptIndexService.ResolveDefaultBookIndexPath]]

