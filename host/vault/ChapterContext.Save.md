---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 8
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# ChapterContext::Save
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Summary
**Commits pending chapter document changes through the chapter document subsystem.**

`Save` is a thin façade on `ChapterContext` that delegates persistence to the chapter document manager. Its implementation is a single call to `Documents.SaveChanges()`, with no local branching, validation, or exception handling. All save semantics are owned by `ChapterDocuments`/`DocumentSlot` internals.


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

