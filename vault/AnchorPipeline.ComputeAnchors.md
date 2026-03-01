---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs"
access_modifier: "public"
complexity: 10
fan_in: 3
fan_out: 6
tags:
  - method
---
# AnchorPipeline::ComputeAnchors
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPipeline.cs`


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

