---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 3
fan_in: 9
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/di
---
# DocumentSlot<T>::GetBackingFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Summary
**Returns the slot’s backing file reference from configured accessor sources.**

`GetBackingFile` resolves the slot’s backing file via configured accessors with fallback semantics. It first invokes `_backingFileAccessor` when present; if absent, it delegates to `_adapter?.GetBackingFile()`. This method does not perform filesystem IO itself and may return `null` when no accessor/adapter backing file is available.


#### [[DocumentSlot_T_.GetBackingFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetBackingFile()
```

**Calls ->**
- [[IDocumentSlotAdapter_T_.GetBackingFile]]

**Called-by <-**
- [[BookDocuments.GetBookIndexFile]]
- [[ChapterDocuments.GetAnchorsFile]]
- [[ChapterDocuments.GetAsrFile]]
- [[ChapterDocuments.GetAsrTranscriptTextFile]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[ChapterDocuments.GetPauseAdjustmentsFile]]
- [[ChapterDocuments.GetPausePolicyFile]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.GetTranscriptFile]]

