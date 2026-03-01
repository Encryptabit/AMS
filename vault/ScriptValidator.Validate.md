---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 9
tags:
  - method
---
# ScriptValidator::Validate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


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

