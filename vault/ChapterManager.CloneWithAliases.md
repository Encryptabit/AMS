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
# ChapterManager::CloneWithAliases
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.CloneWithAliases]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ChapterDescriptor CloneWithAliases(ChapterDescriptor existing, ChapterDescriptor incoming)
```

**Calls ->**
- [[ChapterManager.MergeAliases]]

**Called-by <-**
- [[ChapterManager.EnsureChapterDescriptor]]

