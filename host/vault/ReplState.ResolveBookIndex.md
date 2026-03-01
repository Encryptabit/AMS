---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ReplState::ResolveBookIndex
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Resolve the book index file location and optionally require that it already exists.**

ResolveBookIndex(bool mustExist) in ReplState appears to construct and return a FileInfo for the book index path, with a conditional branch controlled by mustExist to validate presence before returning. The reported cyclomatic complexity of 3 fits a small flow: resolve path, check existence when required, and handle missing-file behavior. It is used as the common resolution primitive by the ResolveBookIndex overload and ResolveDefaultBookIndex callers.


#### [[ReplState.ResolveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveBookIndex(bool mustExist)
```

**Called-by <-**
- [[CommandInputResolver.ResolveBookIndex]]
- [[CliWorkspace.ResolveDefaultBookIndex]]

