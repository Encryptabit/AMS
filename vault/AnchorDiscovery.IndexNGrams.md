---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
---
# AnchorDiscovery::IndexNGrams
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.IndexNGrams]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Dictionary<string, List<int>> IndexNGrams(IReadOnlyList<string> toks, int n)
```

**Called-by <-**
- [[AnchorDiscovery.Collect]]

