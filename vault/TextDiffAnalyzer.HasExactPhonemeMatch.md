---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 1
tags:
  - method
---
# TextDiffAnalyzer::HasExactPhonemeMatch
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.HasExactPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasExactPhonemeMatch(string[] referenceVariants, string[] hypothesisVariants)
```

**Calls ->**
- [[TextDiffAnalyzer.NormalizePhonemeVariant]]

**Called-by <-**
- [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]

