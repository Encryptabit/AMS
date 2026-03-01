---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::TryGetDeleteInsertPair
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Identifies adjacent delete/insert diff pairs and extracts their token counts for substitution-style comparison logic.**

`TryGetDeleteInsertPair` detects whether two adjacent `Diff` operations form a delete/insert substitution pair in either order (`DELETE`→`INSERT` or `INSERT`→`DELETE`). It initializes both out parameters to `0`, then assigns `deleteCount`/`insertCount` from the corresponding `text.Length` values when a valid pair is found. It returns `true` on a recognized pair and `false` otherwise.


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

