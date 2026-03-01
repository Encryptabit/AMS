---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# ChapterManager::AddAlias
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.AddAlias]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AddAlias(ISet<string> aliases, string value)
```

**Calls ->**
- [[ChapterManager.NormalizeChapterId]]

**Called-by <-**
- [[ChapterManager.BuildAliasSet]]
- [[ChapterManager.MergeAliases]]

