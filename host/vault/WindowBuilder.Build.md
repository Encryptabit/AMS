---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/WindowBuilder.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# WindowBuilder::Build
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/WindowBuilder.cs`

## Summary
**Builds clamped half-open book/ASR alignment windows from anchor points across a bounded region.**

`Build` converts ordered anchor pairs `(Bp, Ap)` into parallel half-open alignment windows by prepending/appending sentinel anchors `(bookStart-1, asrStart-1)` and `(bookEnd+1, asrEnd+1)`. It iterates adjacent anchor pairs, computes clamped bounds `bLo/bHi` and `aLo/aHi` via `Math.Max/Min` against the provided start/end limits (with upper limits shifted by `+1` for half-open ranges), and emits a window tuple for each gap. Windows are included when either side has positive span (`bLo < bHi || aLo < aHi`), allowing one side to be empty while preserving cross-stream segmentation.


#### [[WindowBuilder.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int bLo, int bHi, int aLo, int aHi)> Build(IReadOnlyList<(int Bp, int Ap)> anchors, int bookStart, int bookEnd, int asrStart, int asrEnd)
```

**Called-by <-**
- [[TxAlignTests.Align_SimpleNearMatch_YieldsSubNotDelIns]]
- [[TxAlignTests.WindowsFromAnchors_AreClampedAndHalfOpen]]

