---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineService::CopyTreatedAudio
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

## Summary
**Conditionally copies a treated audio file to a destination, creating parent directories and respecting the caller’s overwrite policy.**

`CopyTreatedAudio` is a synchronous guard-and-copy helper invoked from `RunChapterAsync` to persist the treated audio output. It no-ops if `source.Exists` is false, and also returns when `destination.Exists` is true and `overwrite` is false. When copying proceeds, it ensures the target directory exists with `Directory.CreateDirectory(destination.Directory?.FullName ?? destination.DirectoryName ?? ".")`, then performs `File.Copy(..., overwrite: true)`, with overwrite behavior controlled by the earlier guard.


#### [[PipelineService.CopyTreatedAudio]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyTreatedAudio(FileInfo source, FileInfo destination, bool overwrite)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

