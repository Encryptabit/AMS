---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/TreatCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# TreatCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/TreatCommand.cs`

## Summary
**Create and configure the Treat CLI command so Main can run chapter treatment with resolved workspace/index/artifact context and structured logging.**

`Create()` builds the `treat` CLI `Command` and wires its execution handler to resolve runtime inputs (`ResolveWorkspace`, `ResolveBookIndex`, `ResolveArtifactFile`) before chapter processing. The implementation emits diagnostics via `Debug`, `Info`, `Warn`, and `Error`, uses `OpenChapter` for chapter context, and calls `TreatChapterAsync` in two handler paths (specific chapter vs alternate flow), with early error logging on failed resolution.


#### [[TreatCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[AudioTreatmentService.TreatChapterAsync]]
- [[AudioTreatmentService.TreatChapterAsync_2]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]
- [[Log.Info]]
- [[Log.Warn]]
- [[ChapterContext.ResolveArtifactFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[Program.Main]]

