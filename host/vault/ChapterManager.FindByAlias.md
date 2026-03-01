---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterManager::FindByAlias
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Locates an existing chapter descriptor by matching a normalized alias against chapter IDs and alias lists.**

`FindByAlias` performs a linear search for a chapter descriptor whose normalized chapter ID or any normalized alias equals `normalizedAlias` (case-insensitive). It short-circuits `null` when `normalizedAlias` is empty, then checks each descriptor’s `ChapterId` and iterates its `Aliases`, comparing `NormalizeChapterId(...)` outputs. The first match is returned; if none match, it returns `null`.


#### [[ChapterManager.FindByAlias]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor FindByAlias(IReadOnlyList<ChapterDescriptor> descriptors, string normalizedAlias)
```

**Calls ->**
- [[ChapterManager.NormalizeChapterId]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

