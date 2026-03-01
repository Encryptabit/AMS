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
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# ScriptValidator::CalculateCharacterErrorRate
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Compute normalized character error rate between expected script text and ASR transcript text for validation scoring.**

`CalculateCharacterErrorRate` normalizes both inputs via `TextNormalizer.Normalize(expected|actual, _options.ExpandContractions, _options.RemoveNumbers)` before comparison. It short-circuits when normalized expected text is empty, returning `0.0` if both are empty or `1.0` if only actual has content. Otherwise it computes Levenshtein distance by converting each normalized string into per-character string tokens and calling `CalculateEditDistance(...)`, then returns `distance / normalizedExpected.Length` as the CER.


#### [[ScriptValidator.CalculateCharacterErrorRate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private double CalculateCharacterErrorRate(string expected, string actual)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[ScriptValidator.CalculateEditDistance]]

**Called-by <-**
- [[ScriptValidator.Validate]]

