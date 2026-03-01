---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
---
# ChapterManager::DeallocateAll
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`


#### [[ChapterManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void DeallocateAll()
```

**Calls ->**
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[BookManager.Deallocate]]
- [[BookManager.DeallocateAll]]

