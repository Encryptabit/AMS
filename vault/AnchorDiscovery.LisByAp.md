---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
---
# AnchorDiscovery::LisByAp
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.LisByAp]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int bp, int ap)> LisByAp(IReadOnlyList<(int bp, int ap)> pairs)
```

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors]]
- [[AnchorDiscoveryTests.LisEnforcesMonotonicity]]

