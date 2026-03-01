---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# NullProgressReporter::MarkComplete
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Implements a null-object completion callback that intentionally ignores chapter completion notifications.**

In `PipelineCommand.CompactPipelineProgressReporter.NullProgressReporter`, `MarkComplete(string chapterId)` is a no-op interface implementation with an empty body. It accepts `chapterId` only to satisfy `IPipelineProgressReporter` and performs no validation, logging, state update, or side effect.


#### [[NullProgressReporter.MarkComplete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkComplete(string chapterId)
```

