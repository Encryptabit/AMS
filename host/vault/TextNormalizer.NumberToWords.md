---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# TextNormalizer::NumberToWords
**Path**: `Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs`

## Summary
**Transforms a non-negative integer into its basic English word representation for text normalization.**

`NumberToWords` converts integers to English words using static lookup arrays for ones, teens, tens, and hundreds, returning `"zero"` for input `0`. It builds output with a `StringBuilder`, first emitting the hundreds component (`number / 100`), then handling the remainder with either tens-plus-ones (`>= 20`) or teen mapping (`10-19`). Spacing is conditionally inserted between segments and the final string is trimmed before return. The implementation is bounded to simple cardinal formatting logic (no thousands/negative handling in this method).


#### [[TextNormalizer.NumberToWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NumberToWords(int number)
```

**Called-by <-**
- [[TextNormalizer.Normalize]]

