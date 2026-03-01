---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::EnsureDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Ensure an output directory exists when a usable directory path is supplied.**

`EnsureDirectory` is a small guard helper used across prep/verify/pipeline flows before file output. The implementation checks `!string.IsNullOrWhiteSpace(dir)` and only then invokes `Directory.CreateDirectory(dir)`, making directory creation conditional and effectively idempotent for existing paths. It performs no local catch/logging, so filesystem exceptions propagate to callers.


#### [[PipelineCommand.EnsureDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureDirectory(string dir)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.RunPipelineAsync]]
- [[PipelineCommand.RunVerify]]

