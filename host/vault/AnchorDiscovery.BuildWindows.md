---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::BuildWindows
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Builds clamped half-open book/ASR segment windows between anchors, including edge segments via sentinels.**

BuildWindows converts an ordered anchor list into alignment windows by inserting sentinel anchors at `(bookStart-1, asrStart-1)` and `(bookEnd+1, asrEnd+1)` to cover leading/trailing gaps. It iterates adjacent anchor pairs and computes clamped half-open ranges: `bLo = Max(bookStart, left.Bp+1)`, `bHi = Min(bookEnd+1, right.Bp)`, `aLo = Max(asrStart, left.Ap+1)`, `aHi = Min(asrEnd+1, right.Ap)`. Windows are emitted only when either side is non-empty (`bLo < bHi || aLo < aHi`), returning `List<(int bLo, int bHi, int aLo, int aHi)>`.


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

