---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterFileComparer::GetStemSortKey
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Converts a chapter stem into category/number/name sort metadata used by numeric-aware comparisons.**

`GetStemSortKey` generates a normalized `SortKey` from a filename stem for numeric-aware chapter ordering. It matches the first digit sequence using `NumberRegex`; when parsing succeeds, it returns `new SortKey(0, primary, stem.ToLowerInvariant())` (numeric category). If no parseable number exists, it returns `new SortKey(1, int.MaxValue, stem.ToLowerInvariant())` so non-numeric stems sort after numeric ones. The method centralizes stem classification and primary-number extraction for both file and string comparers.


#### [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetStemSortKey]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDiscoveryService.ChapterFileComparer.SortKey GetStemSortKey(string stem)
```

**Called-by <-**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.CompareStemStrings]]
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetSortKey_2]]

