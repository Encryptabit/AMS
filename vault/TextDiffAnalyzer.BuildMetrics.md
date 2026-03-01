---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# TextDiffAnalyzer::BuildMetrics
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.BuildMetrics]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SentenceMetrics BuildMetrics(string reference, string hypothesis, HydratedDiffStats stats)
```

**Calls ->**
- [[TextDiffAnalyzer.ComputeCer]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

