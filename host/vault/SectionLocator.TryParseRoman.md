---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::TryParseRoman
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Parses a Roman numeral token into its integer value with validation of allowed numeral characters.**

TryParseRoman converts a Roman-numeral token to an integer using a single left-to-right pass with a `RomanMap` lookup and subtractive-notation handling. It rejects null/empty input and any token containing non-Roman characters by returning `false` and `value = 0`. During accumulation, when the current symbol is greater than the previous one it applies `result += current - (2 * prev)`, otherwise it adds normally. It returns `true` only if the computed result is positive and assigns it to `value`.


#### [[SectionLocator.TryParseRoman]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseRoman(string token, out int value)
```

**Called-by <-**
- [[SectionLocator.TryParseCombinedNumber]]
- [[SectionLocator.TryParseFullNumber]]

