---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterManager::TryResolveSectionFromAliases
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.TryResolveSectionFromAliases]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SectionRange TryResolveSectionFromAliases(BookIndex bookIndex, IEnumerable<string> aliases)
```

**Calls ->**
- [[ChapterManager.TryResolveSection]]

**Called-by <-**
- [[ChapterManager.BuildAliasSet]]

