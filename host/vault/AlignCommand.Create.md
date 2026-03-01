---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/di
  - llm/validation
---
# AlignCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/AlignCommand.cs`

## Summary
**Build and return the top-level `align` command with its three operational subcommands from injected command handlers.**

`AlignCommand.Create(...)` acts as the CLI command builder for the `align` verb in `System.CommandLine`. It validates all three injected dependencies using `ArgumentNullException.ThrowIfNull`, creates `new Command("align", "Alignment utilities")`, then attaches subcommands from `CreateAnchors(anchorsCommand)`, `CreateTranscriptIndex(transcriptCommand)`, and `CreateHydrateTx(hydrateCommand)` before returning the composed command object.


#### [[AlignCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(ComputeAnchorsCommand anchorsCommand, BuildTranscriptIndexCommand transcriptCommand, HydrateTranscriptCommand hydrateCommand)
```

**Calls ->**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]

**Called-by <-**
- [[Program.Main]]

