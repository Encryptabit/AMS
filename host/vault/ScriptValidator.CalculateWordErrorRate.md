---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# ScriptValidator::CalculateWordErrorRate
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Compute word error rate for `Validate` from substitution, insertion, deletion, and expected-word totals with empty-reference handling.**

`CalculateWordErrorRate` is a small helper that computes WER using edit-operation counts rather than match counts. It has an explicit zero-denominator guard: when `totalExpected == 0`, it returns `1.0` if there were insertions, otherwise `0.0`. For normal cases, it returns `(double)(substitutions + insertions + deletions) / totalExpected`. The `correct` parameter is accepted but not used in the calculation.


#### [[ScriptValidator.CalculateWordErrorRate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private double CalculateWordErrorRate(int correct, int substitutions, int insertions, int deletions, int totalExpected)
```

**Called-by <-**
- [[ScriptValidator.Validate]]

