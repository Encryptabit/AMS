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
# BookCommand::CreateVerify
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Creates the CLI verify subcommand that resolves the target BookIndex file and executes asynchronous verification with centralized exception handling.**

`CreateVerify()` builds and returns the `System.CommandLine` `Command` for `book verify`, adding a nullable `--index` (`-i`) `Option<FileInfo?>` and binding an async handler with `SetHandler`. The handler resolves the input path through `CommandInputResolver.ResolveBookIndex(...)`, awaits `RunVerifyAsync(indexFile)`, and wraps execution in a broad `try/catch` that logs via `Log.Error(...)` and terminates with `Environment.Exit(1)` on failure.


#### [[BookCommand.CreateVerify]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateVerify()
```

**Calls ->**
- [[BookCommand.RunVerifyAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[Log.Error]]

**Called-by <-**
- [[BookCommand.Create]]

