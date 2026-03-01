---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
---
# TextDiffAnalyzer::TryGetDeleteInsertPair
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.TryGetDeleteInsertPair]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetDeleteInsertPair(Diff current, Diff next, out int deleteCount, out int insertCount)
```

**Called-by <-**
- [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]

