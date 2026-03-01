---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/WindowBuilder.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# WindowBuilder::Build
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/WindowBuilder.cs`


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

