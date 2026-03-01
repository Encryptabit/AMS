---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# ChapterFileComparer::GetSortKey
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDiscoveryService.cs`

## Summary
**Builds a normalized sort key for a chapter file by converting its filename stem into comparer metadata.**

`GetSortKey` derives a comparer key for a chapter file by extracting its stem (`Path.GetFileNameWithoutExtension(file.Name)`) and delegating key construction to `GetStemSortKey(stem)`. It performs no direct numeric parsing itself; category/number/name normalization are handled downstream. This keeps `Compare`’s file path handling separate from stem-based key logic.


#### [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetSortKey_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDiscoveryService.ChapterFileComparer.SortKey GetSortKey(FileInfo file)
```

**Calls ->**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.GetStemSortKey]]

**Called-by <-**
- [[Ams.Core.Runtime.Chapter.ChapterDiscoveryService.ChapterFileComparer.Compare_2]]

