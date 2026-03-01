---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
---
# ChapterContextHandle::Dispose
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`


#### [[ChapterContextHandle.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[ChapterManager.Deallocate]]

**Called-by <-**
- [[BlazorWorkspace.Clear]]
- [[BlazorWorkspace.Dispose]]
- [[BlazorWorkspace.SetWorkingDirectory]]

