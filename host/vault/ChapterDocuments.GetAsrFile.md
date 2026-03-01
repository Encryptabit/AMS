---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# ChapterDocuments::GetAsrFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Returns the backing file handle for the ASR document slot.**

`GetAsrFile` is a one-line internal accessor that delegates to `_asr.GetBackingFile()` to retrieve the ASR slot backing file reference. It performs no path derivation, IO, or validation in this method. The concrete return is nullable (`FileInfo?`), so absence of a backing file is representable.


#### [[ChapterDocuments.GetAsrFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetAsrFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AsrCommand.Create]]
- [[BuildTranscriptIndexCommand.ExecuteAsync]]
- [[PipelineService.RunChapterAsync]]

