---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
---
# ReplState::SetWorkingDirectory
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState.SetWorkingDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetWorkingDirectory(string path)
```

**Calls ->**
- [[ReplState.PersistState]]
- [[ReplState.RefreshChapters]]
- [[ReplState.SelectChapterByIndexInternal]]

**Called-by <-**
- [[Program.HandleDirectoryCommandAsync]]

