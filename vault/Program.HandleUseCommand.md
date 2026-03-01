---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 4
tags:
  - method
---
# Program::HandleUseCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.HandleUseCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void HandleUseCommand(IReadOnlyList<string> tokens, ReplState state)
```

**Calls ->**
- [[ReplState.PrintState]]
- [[ReplState.UseAllChapters]]
- [[ReplState.UseChapterByIndex]]
- [[ReplState.UseChapterByName]]

**Called-by <-**
- [[Program.HandleModeCommand]]
- [[Program.TryHandleBuiltInAsync]]

