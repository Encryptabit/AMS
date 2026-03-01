---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 4
fan_out: 0
tags:
  - method
---
# ChapterManager::NormalizeChapterId
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.NormalizeChapterId]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeChapterId(string value)
```

**Called-by <-**
- [[ChapterManager.AddAlias]]
- [[ChapterManager.EnsureChapterDescriptor]]
- [[ChapterManager.FindByAlias]]
- [[ChapterManager.TryMatchByRootSlug]]

