---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaPronunciationProvider::ComposeLexemePronunciations
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It composes final lexeme pronunciation candidates from component-word pronunciation sets with bounded combinatorial expansion.**

`ComposeLexemePronunciations` builds lexeme-level pronunciation variants by iterating each lexeme’s component words and combining per-word phoneme variants through `ExpandCombinations`, capped by `MaxPronunciationsPerLexeme`. If a component lacks generated pronunciations, it falls back to using the component token itself, ensuring composition can proceed. For each lexeme it post-processes combinations with trim, non-empty filter, case-insensitive `Distinct`, and `Take(MaxPronunciationsPerLexeme)`, returning an immutable-style dictionary of `string[]` variants.


#### [[MfaPronunciationProvider.ComposeLexemePronunciations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string[]> ComposeLexemePronunciations(IReadOnlyDictionary<string, IReadOnlyList<string>> lexemeComponents, IReadOnlyDictionary<string, List<string>> wordPronunciations)
```

**Calls ->**
- [[MfaPronunciationProvider.ExpandCombinations]]

**Called-by <-**
- [[MfaPronunciationProvider.GetPronunciationsAsync]]

