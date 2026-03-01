---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ScriptValidator::CalculateSegmentWER
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Compute segment-level word error rate between expected script text and recognized text for downstream segment statistics generation.**

In `ScriptValidator.CalculateSegmentWER`, both `expected` and `actual` are split with `TextNormalizer.TokenizeWords`, and token-level Levenshtein distance is computed via `CalculateEditDistance(string[], string[])`. It includes an explicit empty-reference guard (`expectedWords.Length == 0`) that returns `1.0` if any actual words exist, otherwise `0.0`, preventing divide-by-zero semantics issues. For normal cases, it returns normalized WER as `(double)distance / expectedWords.Length`.


#### [[ScriptValidator.CalculateSegmentWER]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private double CalculateSegmentWER(string expected, string actual)
```

**Calls ->**
- [[TextNormalizer.TokenizeWords]]
- [[ScriptValidator.CalculateEditDistance]]

**Called-by <-**
- [[ScriptValidator.GenerateSegmentStats]]

