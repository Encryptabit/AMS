---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::ExtractWordsFromScript
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Convert script text into a normalized, tokenized `List<string>` of expected words for validation/alignment.**

`ExtractWordsFromScript` normalizes raw script input via `TextNormalizer.Normalize(scriptText, _options.ExpandContractions, _options.RemoveNumbers)`, so token preparation is driven by validator configuration. It then tokenizes the normalized string with `TextNormalizer.TokenizeWords` and materializes the enumerable with `ToList()` for downstream alignment logic in `Validate`.


#### [[ScriptValidator.ExtractWordsFromScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<string> ExtractWordsFromScript(string scriptText)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[TextNormalizer.TokenizeWords]]

**Called-by <-**
- [[ScriptValidator.Validate]]

