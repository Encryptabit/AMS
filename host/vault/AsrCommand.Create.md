---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/AsrCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 10
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
---
# AsrCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AsrCommand.cs`

## Summary
**Constructs and returns the command-line `asr run` command that drives transcript generation for chapter audio and optional artifact export.**

Create validates `transcriptCommand` and builds the `asr` root command with a `run` subcommand, wiring ASR CLI options like `--audio`, `--engine`, model/GPU/timestamp flags, `--book-index`, `--chapter-id`, and `--parallel`. Its async handler resolves and validates inputs (`RequireAudio`, `ResolveBookIndex`, `ResolveWorkspace`), opens chapter context via `workspace.OpenChapter`, maps parsed values into `GenerateTranscriptOptions` (including `AsrEngineConfig.Resolve`), executes `transcriptCommand.ExecuteAsync(...)`, and persists with `handle.Save()`. If `--out` is set, it retrieves the artifact with `GetAsrFile`, creates the output directory, verifies artifact existence, and copies it; exceptions are centrally caught, logged with `Log.Error`, and converted to `context.ExitCode = 1` (with `Log.Debug` when `--parallel` is ignored outside ALL mode).


#### [[AsrCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(GenerateTranscriptCommand transcriptCommand)
```

**Calls ->**
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[GenerateTranscriptCommand.ExecuteAsync]]
- [[AsrEngineConfig.Resolve]]
- [[Log.Debug]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetAsrFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[Program.Main]]

