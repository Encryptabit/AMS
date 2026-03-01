---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 3
tags:
  - method
---
# ChapterManager::GetOrCreate
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterContext GetOrCreate(ChapterDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]
- [[ChapterManager.EnsureCapacity]]
- [[ChapterManager.TrackUsage]]

**Called-by <-**
- [[ChapterManager.Load]]
- [[ChapterManager.Load_2]]

