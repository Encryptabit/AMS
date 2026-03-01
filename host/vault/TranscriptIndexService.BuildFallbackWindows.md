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
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::BuildFallbackWindows
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Computes a safe default alignment window from anchor coverage (or full ranges) for downstream transcript alignment.**

This private static fallback window builder produces a single `(bLo, bHi, aLo, aHi)` alignment segment when pipeline windows are missing. With no anchors, it returns the full filtered book window paired with full ASR token range; otherwise it derives min/max anchor spans, expands each side with bounded padding based on span and `policy.NGram`, and clamps to valid book/ASR limits. It also repairs degenerate ranges (`end <= start`) by forcing a minimal positive width before returning the window list.


#### [[TranscriptIndexService.BuildFallbackWindows]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> BuildFallbackWindows(AnchorPipelineResult pipeline, int asrTokenCount, AnchorPolicy policy)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

