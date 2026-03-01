---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PronunciationHelper::SpellOutDigits
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Transforms a digit-containing string into a space-delimited spoken-digit representation.**

`SpellOutDigits` converts a character sequence into spoken digit tokens using a per-char `switch` mapping (`'0'`→`"zero"` … `'9'`→`"nine"`). It projects `raw.Select(...)`, drops non-digit outputs by mapping unknown chars to `string.Empty` and filtering (`Where(s => s.Length > 0)`), then joins remaining words with single spaces via `string.Join(" ", ...)`. The method is deterministic and ignores unsupported characters rather than failing.


#### [[PronunciationHelper.SpellOutDigits]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SpellOutDigits(string raw)
```

**Called-by <-**
- [[PronunciationHelper.ConvertNumberToWordsSafe]]
- [[PronunciationHelper.NumberToWords]]

