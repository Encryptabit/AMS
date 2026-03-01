---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 5
fan_in: 8
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::ResolveWorkspace
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Resolve and return the effective `IWorkspace` for CLI command execution, optionally anchored by a provided book index file.**

`ResolveWorkspace` is a static resolver in `Ams.Cli.Utilities.CommandInputResolver` that synchronously returns an `IWorkspace` from an optional `FileInfo bookIndexFile` input. With cyclomatic complexity 5 and broad reuse by `Create*` command builders plus `RunPipelineAsync`, the implementation is a centralized branching path that validates/normalizes file context and applies fallback workspace resolution when no file is provided. Its call-site spread indicates it enforces consistent workspace selection before downstream command and pipeline logic runs.


#### [[CommandInputResolver.ResolveWorkspace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IWorkspace ResolveWorkspace(FileInfo bookIndexFile = null)
```

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[PipelineCommand.RunPipelineAsync]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]

