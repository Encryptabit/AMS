---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# CompactPipelineProgressReporter::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Initializes internal chapter ordering and per-chapter progress state containers from a list of chapter files.**

The constructor projects the incoming `chapters` into `_chapterOrder` by taking each file’s basename without extension (`Path.GetFileNameWithoutExtension(file.Name)`) and materializing it as a list. It then creates `_chapters` by deduplicating those IDs with `Distinct(StringComparer.OrdinalIgnoreCase)` and building a case-insensitive dictionary (`ToDictionary(..., StringComparer.OrdinalIgnoreCase)`) where each key maps to a new `ChapterStatus`. This keeps input order in `_chapterOrder` while ensuring status lookup uses normalized, case-insensitive chapter IDs.


#### [[CompactPipelineProgressReporter..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private CompactPipelineProgressReporter(IReadOnlyList<FileInfo> chapters)
```

