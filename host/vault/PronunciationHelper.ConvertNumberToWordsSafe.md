---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PronunciationHelper::ConvertNumberToWordsSafe
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Safely converts numeric token text into spoken-word form, falling back to digit-by-digit spelling when full parsing is not possible.**

`ConvertNumberToWordsSafe` normalizes numeric text and converts it to spoken words with a fallback path. It strips `_` and `,` separators, attempts `long.TryParse` using invariant culture, and when parsing succeeds delegates to `NumberToWords(value)`. If parsing fails, it falls back to per-character digit spelling (`SpellOutDigits(digits)`) so mixed/oversized formats still produce deterministic output.


#### [[PronunciationHelper.ConvertNumberToWordsSafe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ConvertNumberToWordsSafe(string digits)
```

**Calls ->**
- [[PronunciationHelper.NumberToWords]]
- [[PronunciationHelper.SpellOutDigits]]

**Called-by <-**
- [[PronunciationHelper.ExtractPronunciationParts]]

