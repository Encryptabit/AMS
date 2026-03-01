---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/TextCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/utility
---
# TextCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/TextCommand.cs`

## Summary
**Create and return the text command definition with debug logging and normalization support.**

`TextCommand.Create()` is a static factory used by `Main` to build and return the CLI `Command` for text handling. Its implementation is deliberately flat (complexity 1), invoking `Debug(...)` for diagnostics and delegating text canonicalization to `Normalize(...)` instead of doing inline processing. The method primarily wires command creation while offloading behavior to helper calls.


#### [[TextCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[Log.Debug]]
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[Program.Main]]

