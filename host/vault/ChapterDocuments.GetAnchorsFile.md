---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# ChapterDocuments::GetAnchorsFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Returns the backing file handle for the anchors document slot.**

`GetAnchorsFile` is an internal pass-through accessor that returns `_anchors.GetBackingFile()`. It performs no local computation, validation, or filesystem IO. The underlying return is nullable (`FileInfo?` in implementation), indicating the anchors backing file may be absent.


#### [[ChapterDocuments.GetAnchorsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetAnchorsFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[PipelineService.RunChapterAsync]]

