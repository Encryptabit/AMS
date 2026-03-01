---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# TextDiffAnalyzer::Tokenize
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.Tokenize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> Tokenize(string text)
```

**Calls ->**
- [[TextNormalizer.TokenizeWords]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]
- [[TextDiffAnalyzer.ResolveScoringTokens]]

