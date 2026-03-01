---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# ReplState::UseChapterByIndex
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Select a single chapter by zero-based index in REPL state and report whether the selection request was valid.**

`UseChapterByIndex(int index)` first validates bounds with `index < 0 || index >= Chapters.Count` and immediately returns `false` on invalid input. On success, it sets `RunAllChapters = false` and delegates chapter selection to `SelectChapterByIndexInternal(index)`. That internal call normalizes the selection (`Math.Clamp`), clears `_chapterOverride`, conditionally updates `_lastSelectedChapterName`, and persists state, after which `UseChapterByIndex` returns `true`.


#### [[ReplState.UseChapterByIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool UseChapterByIndex(int index)
```

**Calls ->**
- [[ReplState.SelectChapterByIndexInternal]]

**Called-by <-**
- [[Program.HandleUseCommand]]

