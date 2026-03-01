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
  - llm/async
  - llm/di
  - llm/validation
  - llm/error-handling
---
# AlignCommand::CreateHydrateTx
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs`

## Summary
**Creates and wires the command-line workflow that hydrates transcript tokens from BookIndex and ASR artifacts, then optionally copies the produced hydrate JSON to an output path.**

CreateHydrateTx constructs the `hydrate` CLI subcommand with required `--index/-i`, `--asr-json/-j`, and `--tx-json/-t` options plus optional `--out/-o`. Its async handler resolves input/output files through `CommandInputResolver`, opens a chapter using `ChapterOpenOptions` (`BookIndexFile`, `AsrFile`, `TranscriptFile`), executes `HydrateTranscriptCommand.ExecuteAsync(handle.Chapter, null, context.GetCancellationToken())` with `ConfigureAwait(false)`, and persists changes via `handle.Save()`. It then retrieves `GetHydratedTranscriptFile()` and calls `CopyIfRequested`; failures are caught, logged via `Log.Error`, and surfaced by setting `context.ExitCode = 1`.


#### [[AlignCommand.CreateHydrateTx]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateHydrateTx(HydrateTranscriptCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[HydrateTranscriptCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

