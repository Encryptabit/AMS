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
  - llm/error-handling
---
# ReplState::UseChapterByName
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Select a chapter by name in REPL state and report success/failure to the command handler.**

In Ams.Cli.Repl.ReplState, UseChapterByName(string name) is a thin command-path method called by HandleUseCommand. It delegates name-based chapter selection to SelectChapterByNameInternal and returns a bool indicating whether selection succeeded. Its stated complexity (2) suggests only minimal control flow around that delegation.


#### [[ReplState.UseChapterByName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool UseChapterByName(string name)
```

**Calls ->**
- [[ReplState.SelectChapterByNameInternal]]

**Called-by <-**
- [[Program.HandleUseCommand]]

