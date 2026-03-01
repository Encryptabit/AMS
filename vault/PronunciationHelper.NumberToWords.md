---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
---
# PronunciationHelper::NumberToWords
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`


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

