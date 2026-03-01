---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/factory
---
# CommandInputResolver::ResolveDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Resolve the effective working directory for CLI commands using explicit input first, then REPL state, then the process current directory.**

`ResolveDirectory` is a precedence resolver: it returns the caller-supplied `DirectoryInfo` when present, otherwise falls back to `ReplContext.Current.WorkingDirectory`, and finally to `Directory.GetCurrentDirectory()`. The fallback paths are wrapped with `new DirectoryInfo(...)` each time, so it normalizes to a `DirectoryInfo` instance even when no argument is provided. The method has no I/O validation (for example, no existence check) and no throw path.


#### [[CommandInputResolver.ResolveDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static DirectoryInfo ResolveDirectory(DirectoryInfo provided)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.RunStats]]
- [[ValidateCommand.CreateServeCommand]]

