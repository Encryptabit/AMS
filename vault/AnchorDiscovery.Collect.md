---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
---
# AnchorDiscovery::Collect
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`


#### [[AnchorDiscovery.Collect]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<Anchor> Collect(IReadOnlyList<string> book, IReadOnlyList<int> bookSentIdx, IReadOnlyList<string> asr, int n, ISet<string> stop, Func<List<int>, bool> okBook, Func<List<int>, bool> okAsr, AnchorPolicy policy)
```

**Calls ->**
- [[AnchorDiscovery.CrossesSentence]]
- [[AnchorDiscovery.IndexNGrams]]
- [[AnchorDiscovery.PassContent]]

**Called-by <-**
- [[AnchorDiscovery.SelectAnchors]]

