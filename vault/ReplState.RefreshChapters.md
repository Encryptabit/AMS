---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 13
fan_in: 3
fan_out: 3
tags:
  - method
---
# ReplState::RefreshChapters
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState.RefreshChapters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RefreshChapters()
```

**Calls ->**
- [[ReplState.PersistState]]
- [[ReplState.SelectChapterByIndexInternal]]
- [[ReplState.SelectChapterByNameInternal]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[ReplState..ctor]]
- [[ReplState.SetWorkingDirectory]]

