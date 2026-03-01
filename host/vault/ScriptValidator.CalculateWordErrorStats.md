---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::CalculateWordErrorStats
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Aggregate alignment operations into correct/substitution/insertion/deletion counts for downstream validation metrics.**

`CalculateWordErrorStats` walks the `List<AlignmentResult>` produced by word alignment and classifies each item by `AlignmentResult.Operation`. A `switch` increments four local counters for `Match`, `Substitute`, `Insert`, and `Delete`, then returns them as a named tuple `(correct, substitutions, insertions, deletions)`. The method is pure (no side effects) and its output is consumed by `Validate` to compute WER and populate `ValidationReport` fields.


#### [[ScriptValidator.CalculateWordErrorStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private (int correct, int substitutions, int insertions, int deletions) CalculateWordErrorStats(List<ScriptValidator.AlignmentResult> alignment)
```

**Called-by <-**
- [[ScriptValidator.Validate]]

