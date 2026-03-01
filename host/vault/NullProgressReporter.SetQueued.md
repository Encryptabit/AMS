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
# NullProgressReporter::SetQueued
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Implements the queue notification hook without side effects when using the null progress reporter.**

`SetQueued` in `NullProgressReporter` is an intentional no-op: its body is empty, so it ignores `chapterId`, performs no state updates, and emits no progress output. It exists to satisfy the `IPipelineProgressReporter` interface in the null-object reporter path where progress reporting is disabled.


#### [[NullProgressReporter.SetQueued]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetQueued(string chapterId)
```

