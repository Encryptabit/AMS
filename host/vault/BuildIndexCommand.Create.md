---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/di
  - llm/error-handling
---
# BuildIndexCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Creates the build-index CLI command that resolves dependencies and runs the asynchronous index-building workflow.**

`Create()` is a low-complexity command factory that constructs the CLI `Command` invoked from `Main` and wires a single execution path. Its handler resolves collaborators via `ResolveBookSource` and `ResolveBookIndex`, executes `BuildBookIndexAsync`, and routes failures through `Error` for centralized CLI error handling.


#### [[BuildIndexCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveBookSource]]
- [[Log.Error]]

**Called-by <-**
- [[Program.Main]]

