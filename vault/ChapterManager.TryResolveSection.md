---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# ChapterManager::TryResolveSection
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.TryResolveSection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SectionRange TryResolveSection(BookIndex bookIndex, string label)
```

**Calls ->**
- [[SectionLocator.ResolveSectionByTitle]]

**Called-by <-**
- [[ChapterManager.BuildAliasSet]]
- [[ChapterManager.TryResolveSectionFromAliases]]

