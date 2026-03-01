---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PronunciationHelper::NumberToWords
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Transforms an integer value into its spoken English-word representation with sign and magnitude handling.**

`NumberToWords` converts a signed `long` into English words using chunked base-1000 decomposition. It handles `0` and negative values as special cases (`"zero"`, `"minus ..."`), then iterates 3-digit chunks, converting each non-zero chunk with `ChunkToWords` and appending magnitude units (`thousand`, `million`, ...), inserting results at the front to preserve order. A defensive overflow branch (`if (number > 0)` after unit exhaustion) falls back to `SpellOutDigits(...)`. The final phrase is returned as space-joined parts.


#### [[PronunciationHelper.NumberToWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NumberToWords(long number)
```

**Calls ->**
- [[PronunciationHelper.ChunkToWords]]
- [[PronunciationHelper.NumberToWords]]
- [[PronunciationHelper.SpellOutDigits]]

**Called-by <-**
- [[PronunciationHelper.ConvertNumberToWordsSafe]]
- [[PronunciationHelper.NumberToWords]]

