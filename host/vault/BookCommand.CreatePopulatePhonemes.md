---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# BookCommand::CreatePopulatePhonemes
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Creates the CLI subcommand that enriches a book index with missing phonemes and routes parsed arguments to the async population workflow.**

`CreatePopulatePhonemes()` constructs and returns a `System.CommandLine.Command` named `populate-phonemes`, wires `--index/-i`, `--out/-o`, and `--g2p-model` options, and registers an async handler via `SetHandler`. The handler resolves the index path with `CommandInputResolver.ResolveBookIndex(...)`, reads optional output/model values from `ParseResult`, then awaits `PopulatePhonemesAsync(indexFile, outputFile, g2pModel, context.GetCancellationToken())`. It wraps execution in a broad `try/catch`, logging failures with `Log.Error(...)` and terminating the process with `Environment.Exit(1)`.


#### [[BookCommand.CreatePopulatePhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePopulatePhonemes()
```

**Calls ->**
- [[BookCommand.PopulatePhonemesAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[Log.Error]]

**Called-by <-**
- [[BookCommand.Create]]

