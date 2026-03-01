---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs"
access_modifier: "public"
complexity: 10
fan_in: 3
fan_out: 6
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorPipeline::ComputeAnchors
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs`

## Summary
**Computes anchor matches between book and ASR token streams with optional section restriction and window generation for downstream alignment.**

ComputeAnchors builds normalized token views via `AnchorPreprocessor.BuildBookView(book)` and `BuildAsrView(asr)`, then determines a filtered book window either from `overrideSection` or `SectionLocator.DetectSection` (guarded by `sectionOptions.Detect`) and `TryMapSectionWindow`. It runs `AnchorDiscovery.SelectAnchors` within that window, then optionally refines the window around the discovered anchor span using a bounded pad (`Max(64, Min(8192, Max(policy.NGram * 2, span / 5)))`) to shrink search scope without collapsing context. When `includeWindows` is true, it derives alignment windows with `BuildWindows` over the filtered book range and full ASR filtered range. It returns an `AnchorPipelineResult` containing detection metadata, anchors, counts (raw and filtered), optional windows, and the filtered-to-original book index map.


#### [[AnchorPipeline.ComputeAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AnchorPipelineResult ComputeAnchors(BookIndex book, AsrResponse asr, AnchorPolicy policy, SectionDetectOptions sectionOptions = null, bool includeWindows = false, SectionRange overrideSection = null)
```

**Calls ->**
- [[AnchorDiscovery.BuildWindows]]
- [[AnchorDiscovery.SelectAnchors_2]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[AnchorPreprocessor.BuildBookView]]
- [[AnchorPreprocessor.TryMapSectionWindow]]
- [[SectionLocator.DetectSection]]

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]
- [[SectionLocatorTests.Preprocessor_And_Pipeline_Map_And_Restrict]]

