---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# ChapterDocuments::GetAsrTranscriptTextFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Exposes the backing file handle for the ASR transcript text document slot.**

`GetAsrTranscriptTextFile` is a minimal internal accessor that returns `_asrTranscriptText.GetBackingFile()`. It adds no local computation, IO, or validation logic. The implementation return is nullable (`FileInfo?`), matching optional backing-file availability.


#### [[ChapterDocuments.GetAsrTranscriptTextFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetAsrTranscriptTextFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

