---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
---
# AnchorDiscovery::SelectAnchors
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.SelectAnchors_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Anchor> SelectAnchors(IReadOnlyList<string> bookTokens, IReadOnlyList<int> bookSentenceIndex, IReadOnlyList<string> asrTokens, AnchorPolicy policy, int bookStart, int bookEnd)
```

**Calls ->**
- [[AnchorDiscovery.SelectAnchors]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[SectionLocatorTests.AnchorSelection_Respects_Book_Window]]

