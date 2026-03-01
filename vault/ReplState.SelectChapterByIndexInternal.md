---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
complexity: 4
fan_in: 5
fan_out: 1
tags:
  - method
---
# ReplState::SelectChapterByIndexInternal
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState.SelectChapterByIndexInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void SelectChapterByIndexInternal(int index, bool updateLastSelected = true)
```

**Calls ->**
- [[ReplState.PersistState]]

**Called-by <-**
- [[ReplState.InitializeFallbackSelection]]
- [[ReplState.RefreshChapters]]
- [[ReplState.SelectChapterByNameInternal]]
- [[ReplState.SetWorkingDirectory]]
- [[ReplState.UseChapterByIndex]]

