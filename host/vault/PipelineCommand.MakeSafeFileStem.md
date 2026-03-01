---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::MakeSafeFileStem
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Normalizes a user- or file-derived chapter identifier into a filesystem-safe filename stem with a deterministic fallback.**

`MakeSafeFileStem` is a small sanitization helper used by `PipelineCommand.RunPipelineAsync` to derive `chapterStem` for directory and artifact filenames. It returns a constant fallback (`"chapter"`) for null/whitespace input, then scans characters with a pre-sized `StringBuilder`, replacing anything in `Path.GetInvalidFileNameChars()` with `_` via `Array.IndexOf`. The result is `Trim()`ed, and if trimming produces an empty string it again falls back to `"chapter"`.


#### [[PipelineCommand.MakeSafeFileStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string MakeSafeFileStem(string value)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineAsync]]

