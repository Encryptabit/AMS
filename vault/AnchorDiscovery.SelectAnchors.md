---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 4
tags:
  - method
---
# AnchorDiscovery::SelectAnchors
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.SelectAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Anchor> SelectAnchors(IReadOnlyList<string> bookTokens, IReadOnlyList<int> bookSentenceIndex, IReadOnlyList<string> asrTokens, AnchorPolicy policy)
```

**Calls ->**
- [[AnchorDiscovery.Collect]]
- [[FarApart]]
- [[AnchorDiscovery.LisByAp]]
- [[AnchorDiscovery.SelectAnchors]]

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors_2]]
- [[AnchorDiscovery.SelectAnchors]]
- [[AnchorDiscoveryTests.UniqueTrigrams_ProduceAnchors]]

