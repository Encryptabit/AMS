---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterManager::TryMatchByRootPath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.TryMatchByRootPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor TryMatchByRootPath(IReadOnlyList<ChapterDescriptor> descriptors, string chapterRoot)
```

**Calls ->**
- [[ChapterManager.NormalizePath]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

