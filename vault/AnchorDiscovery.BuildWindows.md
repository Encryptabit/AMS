---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# AnchorDiscovery::BuildWindows
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.BuildWindows]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int bLo, int bHi, int aLo, int aHi)> BuildWindows(IReadOnlyList<Anchor> anchors, int bookStart, int bookEnd, int asrStart, int asrEnd)
```

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorDiscoveryTests.WindowsAreClampedWithSentinels]]

