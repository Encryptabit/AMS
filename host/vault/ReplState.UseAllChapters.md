---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# ReplState::UseAllChapters
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Set the REPL state to use every chapter and save that updated state.**

`UseAllChapters()` in `Ams.Cli.Repl.ReplState` appears to switch REPL chapter selection state to an “all chapters” mode and then persist that mutation via `PersistState()`. Its low complexity (2) and single outbound call suggest a small state-update path with minimal branching and an immediate side effect to durable state. Because it is called by `HandleUseCommand`, it functions as a command-level helper for the `use` flow rather than an application entry point.


#### [[ReplState.UseAllChapters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UseAllChapters()
```

**Calls ->**
- [[ReplState.PersistState]]

**Called-by <-**
- [[Program.HandleUseCommand]]

