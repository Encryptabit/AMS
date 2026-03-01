---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ReplState::SelectChapterByNameInternal
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Select a chapter by its name while reusing the existing index-based selection path and optionally updating last-selected state.**

`SelectChapterByNameInternal` performs name-based chapter selection in `Ams.Cli.Repl.ReplState` by resolving `name` to a chapter index and delegating the state transition to `SelectChapterByIndexInternal`. The `updateLastSelected` optional parameter is propagated so callers can control whether last-selection tracking is mutated during selection. The `bool` return indicates success/failure of lookup or delegated selection, enabling callers like `.ctor`, `RefreshChapters`, and `UseChapterByName` to branch without exception-driven flow.


#### [[ReplState.SelectChapterByNameInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SelectChapterByNameInternal(string name, bool updateLastSelected = true)
```

**Calls ->**
- [[ReplState.SelectChapterByIndexInternal]]

**Called-by <-**
- [[ReplState..ctor]]
- [[ReplState.RefreshChapters]]
- [[ReplState.UseChapterByName]]

