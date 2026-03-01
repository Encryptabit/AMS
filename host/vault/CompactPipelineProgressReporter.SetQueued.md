---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# CompactPipelineProgressReporter::SetQueued
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Sets the target chapter’s pipeline progress state to queued.**

`SetQueued(string chapterId)` in `CompactPipelineProgressReporter` is a thin convenience method that delegates to `UpdateChapter` to apply the "queued" progress state for the given chapter. Its complexity of 1 reflects a straight-through implementation with no branching, input checks, or local error handling. The method centralizes this specific state transition behind a named API rather than duplicating update-call details at call sites.


#### [[CompactPipelineProgressReporter.SetQueued]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetQueued(string chapterId)
```

**Calls ->**
- [[CompactPipelineProgressReporter.UpdateChapter]]

