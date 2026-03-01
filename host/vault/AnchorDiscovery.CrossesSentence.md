---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AnchorDiscovery::CrossesSentence
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorDiscovery.cs`

## Summary
**Checks whether an n-token span starts and ends in different sentences.**

CrossesSentence performs a boundary check by reading sentence ids at the n-gram endpoints (`sentIdx[i]` and `sentIdx[i + n - 1]`) and comparing them. It returns `true` when they differ, indicating the span crosses a sentence boundary, otherwise `false`.


#### [[AnchorDiscovery.CrossesSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool CrossesSentence(IReadOnlyList<int> sentIdx, int i, int n)
```

**Called-by <-**
- [[AnchorDiscovery.Collect]]

