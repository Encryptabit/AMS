---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 3
tags:
  - method
---
# TextDiffAnalyzer::ApplyExactPhonemeEquivalence
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


#### [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HydratedDiffStats ApplyExactPhonemeEquivalence(HydratedDiffStats stats, IReadOnlyList<Diff> diffs, IReadOnlyList<string[]> referencePhonemes, IReadOnlyList<string[]> hypothesisPhonemes)
```

**Calls ->**
- [[TextDiffAnalyzer.GetPhonemes]]
- [[TextDiffAnalyzer.HasExactPhonemeMatch]]
- [[TextDiffAnalyzer.TryGetDeleteInsertPair]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

