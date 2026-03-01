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
# ChapterDocuments::GetHydratedTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Exposes the backing file handle for the hydrated transcript document slot.**

`GetHydratedTranscriptFile` is a thin internal accessor that returns the hydrated-transcript slot’s backing file via `_hydratedTranscript.GetBackingFile()`. It performs no filesystem operations or path computation in this method body. The implementation return is nullable (`FileInfo?`), so callers must handle missing backing-file cases.


#### [[ChapterDocuments.GetHydratedTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetHydratedTranscriptFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AlignCommand.CreateHydrateTx]]
- [[RunMfaCommand.ExecuteAsync]]
- [[PipelineService.RunChapterAsync]]

