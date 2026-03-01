---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# CompactPipelineProgressReporter::Attach
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Stores the current live display context so the reporter can refresh the Spectre.Console progress view during pipeline execution.**

`Attach` captures the `LiveDisplayContext` provided in `RunAsync`’s `StartAsync` callback and assigns it to the reporter’s private `_liveContext` field. That stored reference is later consumed by `RefreshUnsafe()` through `_liveContext?.UpdateTarget(BuildView())` to push table redraws as chapter state changes. The method is intentionally minimal (single assignment) with no synchronization, validation, or exception path.


#### [[CompactPipelineProgressReporter.Attach]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Attach(LiveDisplayContext context)
```

**Called-by <-**
- [[CompactPipelineProgressReporter.RunAsync]]

