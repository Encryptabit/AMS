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
# ChapterManager::MergeAliases
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.MergeAliases]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyCollection<string> MergeAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
```

**Calls ->**
- [[ChapterManager.AddAlias]]

**Called-by <-**
- [[ChapterManager.CloneWithAliases]]

