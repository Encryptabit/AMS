---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 10
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/di
  - llm/validation
  - llm/error-handling
---
# AlignCommand::CreateTranscriptIndex
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs`

## Summary
**Creates and configures the `tx` subcommand that builds and writes a transcript index from book index, ASR, and audio chapter inputs.**

`CreateTranscriptIndex` is a command-factory method that builds the `align tx` `System.CommandLine.Command`, wiring required inputs (`--index`, `--asr-json`, `--audio`) plus optional output and anchor-tuning flags. Its async handler resolves/normalizes CLI inputs through `CommandInputResolver`, composes `AnchorComputationOptions` and `BuildTranscriptIndexOptions`, opens a chapter via `ResolveWorkspace(...).OpenChapter(...)`, executes `BuildTranscriptIndexCommand.ExecuteAsync(...)`, then persists with `handle.Save()` and optionally copies the generated transcript artifact via `CopyIfRequested`. The handler wraps execution in a broad `try/catch`, logs failures with `Log.Error`, and sets `context.ExitCode = 1` for CLI error signaling.


#### [[AlignCommand.CreateTranscriptIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTranscriptIndex(BuildTranscriptIndexCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[BuildTranscriptIndexCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetTranscriptFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

