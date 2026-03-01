---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
  - llm/factory
  - llm/di
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# AlignCommand::CreateAnchors
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs`

## Summary
**Creates the CLI command that computes chapter anchor data from BookIndex and ASR inputs and persists/exports the resulting anchors artifact.**

`CreateAnchors` is a command factory that builds the `align anchors` `System.CommandLine` subcommand, wires required `--index`/`--asr-json` inputs, optional output and anchor-tuning options, and attaches an async handler. The handler resolves files via `ResolveBookIndex`/`ResolveChapterArtifact`, maps parsed values into `AnchorComputationOptions`, opens a chapter through `ResolveWorkspace(...).OpenChapter(...)`, executes `command.ExecuteAsync(...)` with cancellation, then `Save`s and retrieves the generated artifact via `GetAnchorsFile`. It throws if the anchors artifact is missing, conditionally copies output with `CopyIfRequested`, and converts any failure into logged `Error` plus `context.ExitCode = 1`.


#### [[AlignCommand.CreateAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateAnchors(ComputeAnchorsCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[ComputeAnchorsCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetAnchorsFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

