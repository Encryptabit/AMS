---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# ScriptValidator::GenerateFindings
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Converts word-alignment operations into structured validation findings for the final validation report.**

`GenerateFindings` performs a one-pass projection of `AlignmentResult` items into `ValidationFinding` objects, emitting records only for `Delete`, `Insert`, and `Substitute`. `Delete` maps to `FindingType.Missing` with `Expected` and `Cost`; `Insert` maps to `Extra` with `Actual`, `StartTime`, `EndTime`, and `Cost`; `Substitute` maps to `Substitution` with both expected/actual values, timing metadata, and cost. `Match` is intentionally ignored, and the default branch throws `ArgumentOutOfRangeException` (after explicitly resetting `HelpLink`, `HResult`, and `Source`); `asrResponse` is currently unused.


#### [[ScriptValidator.GenerateFindings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ValidationFinding> GenerateFindings(List<ScriptValidator.AlignmentResult> alignment, AsrResponse asrResponse)
```

**Called-by <-**
- [[ScriptValidator.Validate]]

