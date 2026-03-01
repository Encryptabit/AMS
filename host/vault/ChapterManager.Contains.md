---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::Contains
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Checks whether the manager currently has a descriptor for a given chapter ID.**

`Contains` performs a case-insensitive membership check for a chapter ID within `_descriptors`. It validates `chapterId` via `ArgumentException.ThrowIfNullOrEmpty`, then linearly scans descriptors comparing `ChapterId` with `StringComparison.OrdinalIgnoreCase`. It returns `true` on first match and `false` when no descriptor matches.


#### [[ChapterManager.Contains]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool Contains(string chapterId)
```

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

