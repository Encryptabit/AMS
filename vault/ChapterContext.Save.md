---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 8
fan_out: 1
tags:
  - method
---
# ChapterContext::Save
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`


#### [[ChapterContext.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save()
```

**Calls ->**
- [[ChapterDocuments.SaveChanges]]

**Called-by <-**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]
- [[ComputeAnchorsCommand.ExecuteAsync]]
- [[GenerateTranscriptCommand.ExecuteAsync]]
- [[HydrateTranscriptCommand.ExecuteAsync]]
- [[ChapterContextHandle.Save]]
- [[ChapterManager.Deallocate]]
- [[ChapterManager.DeallocateAll]]
- [[ChapterManager.EnsureCapacity]]

