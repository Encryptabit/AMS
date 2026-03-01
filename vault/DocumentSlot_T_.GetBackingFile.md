---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "public"
complexity: 3
fan_in: 9
fan_out: 1
tags:
  - method
---
# DocumentSlot<T>::GetBackingFile
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`


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

