---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaPronunciationProvider::ExpandCombinations
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It generates up to `maxCount` combined pronunciation strings from prefix pronunciations and variant phoneme tokens.**

`ExpandCombinations` computes bounded Cartesian expansion between `basePronunciations` and `variants`, returning early with an empty list when `maxCount <= 0`. It pre-sizes the output list based on `basePronunciations.Count * max(variants.Count, 1)`, then for each pair builds a normalized phrase by appending `prefix.TrimEnd()`, a separating space when needed, and `variant.Trim()`. The method enforces an upper bound by returning immediately once `expanded.Count >= maxCount`.


#### [[MfaPronunciationProvider.ExpandCombinations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> ExpandCombinations(List<string> basePronunciations, List<string> variants, int maxCount)
```

**Called-by <-**
- [[MfaPronunciationProvider.ComposeLexemePronunciations]]

