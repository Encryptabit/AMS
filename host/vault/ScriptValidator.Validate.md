---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 9
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/utility
---
# ScriptValidator::Validate
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Validates ASR output against a reference script and returns a consolidated report of alignment diagnostics and error-rate metrics.**

`Validate(...)` is a synchronous orchestration method that turns `scriptText` and `AsrResponse` into normalized word streams (`ExtractWordsFromScript`, `ExtractWordsFromAsrResponse`), aligns them (`AlignWords`), and generates both detailed findings and segment-level stats (`GenerateFindings`, `GenerateSegmentStats`). It derives word-level confusion counts via `CalculateWordErrorStats`, computes WER/CER with `CalculateWordErrorRate` and `CalculateCharacterErrorRate(scriptText, GetTranscriptText(asrResponse))`, and packages all metrics into a new `ValidationReport`. The returned report includes input file paths, `DateTime.UtcNow`, total/correct/error counts, and materialized `Findings`/`SegmentStats` arrays.


#### [[ScriptValidator.Validate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidationReport Validate(string audioPath, string scriptPath, string asrJsonPath, string scriptText, AsrResponse asrResponse)
```

**Calls ->**
- [[ScriptValidator.AlignWords]]
- [[ScriptValidator.CalculateCharacterErrorRate]]
- [[ScriptValidator.CalculateWordErrorRate]]
- [[ScriptValidator.CalculateWordErrorStats]]
- [[ScriptValidator.ExtractWordsFromAsrResponse]]
- [[ScriptValidator.ExtractWordsFromScript]]
- [[ScriptValidator.GenerateFindings]]
- [[ScriptValidator.GenerateSegmentStats]]
- [[ScriptValidator.GetTranscriptText]]

**Called-by <-**
- [[ScriptValidator.ValidateAsync]]
- [[ScriptValidatorTests.Validate_ComplexScenario_ShouldCalculateCorrectMetrics]]
- [[ScriptValidatorTests.Validate_PerfectMatch_ShouldReturnZeroWER]]
- [[ScriptValidatorTests.Validate_WithContractions_ShouldNormalizeCorrectly]]
- [[ScriptValidatorTests.Validate_WithDeletion_ShouldCalculateCorrectWER]]
- [[ScriptValidatorTests.Validate_WithInsertion_ShouldCalculateCorrectWER]]
- [[ScriptValidatorTests.Validate_WithSubstitution_ShouldCalculateCorrectWER]]

