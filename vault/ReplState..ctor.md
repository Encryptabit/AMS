---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 6
tags:
  - method
---
# ReplState::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ReplState()
```

**Calls ->**
- [[ReplState.InitializeFallbackSelection]]
- [[ReplState.LoadPersistedState]]
- [[ReplState.PersistState]]
- [[ReplState.RefreshChapters]]
- [[ReplState.ResolveStateFilePath]]
- [[ReplState.SelectChapterByNameInternal]]

