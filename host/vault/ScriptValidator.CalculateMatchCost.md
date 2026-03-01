---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# ScriptValidator::CalculateMatchCost
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**It produces the per-word match/substitution cost used by alignment logic when comparing expected and actual tokens.**

`CalculateMatchCost` computes substitution penalty for token alignment using a fast exact-match path and a similarity-based fallback. It returns `0.0` immediately when `expected == actual`, then for non-equal strings calls `TextNormalizer.CalculateSimilarity(expected, actual)`. The returned cost is `_options.SubstitutionCost * (1.0 - similarity)`, so cost increases linearly as similarity decreases.


#### [[ScriptValidator.CalculateMatchCost]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private double CalculateMatchCost(string expected, string actual)
```

**Calls ->**
- [[TextNormalizer.CalculateSimilarity]]

**Called-by <-**
- [[ScriptValidator.AlignWords]]

