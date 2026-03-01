---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
---
# ChapterManager::Deallocate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string chapterId)
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]
- [[ChapterManager.RemoveUsageNode]]

**Called-by <-**
- [[ChapterContextHandle.Dispose]]

