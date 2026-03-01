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
# ChapterDocuments::GetTextGridFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Returns the backing file handle for the text-grid document slot.**

`GetTextGridFile` is an internal one-line accessor that delegates to `_textGrid.GetBackingFile()`. It performs no local path derivation, validation, or IO side effects. The implementation return type is nullable (`FileInfo?`), indicating the text-grid backing file may be unavailable.


#### [[ChapterDocuments.GetTextGridFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetTextGridFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[MergeTimingsCommand.ExecuteAsync]]
- [[RunMfaCommand.ExecuteAsync]]
- [[PipelineService.RunChapterAsync]]

