---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PronunciationHelper::ChunkToWords
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/PronunciationHelper.cs`

## Summary
**Translates a three-digit numeric chunk into its English word form for larger number conversion.**

`ChunkToWords` converts a 0–999 numeric chunk into English words using local lookup tables for ones, teens, and tens. It emits hundreds (`"x hundred"`), then handles tens/teens branches, and finally appends remaining ones, mutating `number` as each magnitude is consumed. Empty placeholders are filtered out and the final tokens are space-joined. The method assumes chunk-sized input from `NumberToWords` and performs no range checks.


#### [[PronunciationHelper.ChunkToWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ChunkToWords(int number)
```

**Called-by <-**
- [[PronunciationHelper.NumberToWords]]

