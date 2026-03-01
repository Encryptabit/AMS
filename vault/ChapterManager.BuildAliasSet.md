---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
---
# ChapterManager::BuildAliasSet
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.BuildAliasSet]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyCollection<string> BuildAliasSet(string chapterId, string chapterRoot, BookIndex bookIndex, out SectionRange matchedSection)
```

**Calls ->**
- [[ChapterManager.AddAlias]]
- [[ChapterManager.TryResolveSection]]
- [[ChapterManager.TryResolveSectionFromAliases]]

**Called-by <-**
- [[ChapterManager.CreateContext]]

