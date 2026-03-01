---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
---
# ChapterManager::EnsureCapacity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.EnsureCapacity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureCapacity()
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[ChapterManager.GetOrCreate]]

