---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 3
fan_in: 4
fan_out: 1
tags:
  - method
---
# AnchorPreprocessor::BuildBookView
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`


#### [[AnchorPreprocessor.BuildBookView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static BookAnchorView BuildBookView(BookIndex book)
```

**Calls ->**
- [[AnchorTokenizer.Normalize]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]
- [[TranscriptIndexService.BuildWordOperations]]

