---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# TextDiffAnalyzer::BuildTokenDiff
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.BuildTokenDiff]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TextDiffAnalyzer.TokenDiffResult BuildTokenDiff(IReadOnlyList<string> referenceTokens, IReadOnlyList<string> hypothesisTokens)
```

**Calls ->**
- [[Encode]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

