---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterManager::FindByAlias
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


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

