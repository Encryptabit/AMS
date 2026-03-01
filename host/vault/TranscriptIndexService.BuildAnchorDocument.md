---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# TranscriptIndexService::BuildAnchorDocument
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Builds the persisted anchor document representation from pipeline outputs and the effective anchor computation settings.**

This private static mapper projects `AnchorPipelineResult` and `AnchorComputationOptions` into an `AnchorDocument` DTO graph. It materializes anchor entries by translating each pipeline anchor to `AnchorDocumentAnchor`, resolving `BookWordIndex` through `BookFilteredToOriginalWord` with bounds guarding (falling back to `-1`), and converts optional pipeline windows into `AnchorDocumentWindowSegment` records. It also embeds section metadata (when present), policy metadata derived from options, token/window statistics, and returns the assembled immutable document.


#### [[TranscriptIndexService.BuildAnchorDocument]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AnchorDocument BuildAnchorDocument(AnchorPipelineResult pipeline, AnchorComputationOptions options)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

