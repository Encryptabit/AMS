---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 7
tags:
  - method
---
# ChapterManager::EnsureChapterDescriptor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.EnsureChapterDescriptor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterDescriptor EnsureChapterDescriptor(ChapterDescriptor template)
```

**Calls ->**
- [[ChapterManager.CloneWithAliases]]
- [[ChapterManager.Contains]]
- [[ChapterManager.FindByAlias]]
- [[ChapterManager.NormalizeChapterId]]
- [[ChapterManager.TryMatchByRootPath]]
- [[ChapterManager.TryMatchByRootSlug]]
- [[ChapterManager.UpsertDescriptor]]

**Called-by <-**
- [[ChapterManager.CreateContext]]

