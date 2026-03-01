---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 11
fan_in: 1
fan_out: 0
tags:
  - method
---
# AnchorPreprocessor::TryMapSectionWindow
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`


#### [[AnchorPreprocessor.TryMapSectionWindow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool TryMapSectionWindow(BookAnchorView view, (int startWord, int endWord) section, out (int startFiltered, int endFiltered) window)
```

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]

