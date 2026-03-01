---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 7
tags:
  - method
---
# Program::TryHandleBuiltInAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.TryHandleBuiltInAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<bool> TryHandleBuiltInAsync(string input, ReplState state, RootCommand rootCommand)
```

**Calls ->**
- [[Program.ExecuteWithScopeAsync]]
- [[Program.HandleDirectoryCommandAsync]]
- [[Program.HandleModeCommand]]
- [[Program.HandleUseCommand]]
- [[Program.ParseInput]]
- [[ReplState.ListChapters]]
- [[ReplState.PrintState]]

**Called-by <-**
- [[Program.StartRepl]]

