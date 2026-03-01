---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ReplState::ListChapters
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Render the chapter list from current REPL state so users can view available chapters and navigation context.**

`ListChapters()` on `Ams.Cli.Repl.ReplState` is a synchronous REPL helper reached via `TryHandleBuiltInAsync` that enumerates chapter state and emits a formatted chapter listing to the console. Given complexity 6, its implementation includes several branch paths (for example, empty-state handling, per-item formatting, and state-dependent markers such as current/selected chapter) rather than linear output logic.


#### [[ReplState.ListChapters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ListChapters()
```

**Called-by <-**
- [[Program.TryHandleBuiltInAsync]]

