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
# TextDiffAnalyzer::ResolveScoringTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.ResolveScoringTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> ResolveScoringTokens(string normalizedFallback, IReadOnlyList<string> providedTokens)
```

**Calls ->**
- [[TextDiffAnalyzer.Tokenize]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

