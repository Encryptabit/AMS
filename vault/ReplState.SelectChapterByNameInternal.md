---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
---
# ReplState::SelectChapterByNameInternal
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


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

