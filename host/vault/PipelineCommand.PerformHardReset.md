---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::PerformHardReset
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**It performs an aggressive workspace reset that removes most directories and files while preserving CRX content and document source files.**

`PerformHardReset` runs a two-phase filesystem cleanup on `root`: it first recursively deletes every immediate subdirectory except the one whose name matches `CrxDirectoryName`, then deletes immediate files except `.docx` and `.pdf` (case-insensitive). Before processing each directory/file, it calls `cancellationToken.ThrowIfCancellationRequested()` so cancellation can stop the reset mid-pass. Each delete is wrapped in `try/catch`; failures are swallowed and emitted with `Log.Debug`, and a final debug line marks hard-reset completion for the root.


#### [[PipelineCommand.PerformHardReset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PerformHardReset(DirectoryInfo root, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepResetCommand]]

