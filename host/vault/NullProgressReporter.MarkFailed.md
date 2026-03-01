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
# NullProgressReporter::MarkFailed
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Implements null-object failure reporting so pipeline code can call `MarkFailed` without side effects when progress output is disabled.**

In `PipelineCommand.CompactPipelineProgressReporter.NullProgressReporter`, `MarkFailed(string chapterId, string message)` is an intentional no-op with an empty method body. It satisfies the `IPipelineProgressReporter` interface while ignoring both parameters and performing no state mutation, logging, or exception handling. Runtime and memory cost are constant with no branching.


#### [[NullProgressReporter.MarkFailed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkFailed(string chapterId, string message)
```

