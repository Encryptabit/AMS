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
# ChapterDocuments::GetTranscriptFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Returns the backing file handle for the transcript document slot.**

`GetTranscriptFile` is an internal accessor that delegates to `_transcript.GetBackingFile()` to expose the resolver-bound transcript artifact path. It performs no path computation or filesystem IO itself. In implementation the return type is nullable (`FileInfo?`), reflecting that a backing file may be unavailable.


#### [[ChapterDocuments.GetTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetTranscriptFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AlignCommand.CreateTranscriptIndex]]
- [[PipelineService.RunChapterAsync]]

