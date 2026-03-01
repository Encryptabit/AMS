---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 2
tags:
  - method
---
# AnchorPreprocessor::BuildAsrView
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`


#### [[AnchorPreprocessor.BuildAsrView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AsrAnchorView BuildAsrView(AsrResponse asr)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[AnchorTokenizer.Normalize]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

