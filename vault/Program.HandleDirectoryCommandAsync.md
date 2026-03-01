---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
---
# Program::HandleDirectoryCommandAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.HandleDirectoryCommandAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task HandleDirectoryCommandAsync(IReadOnlyList<string> tokens, ReplState state)
```

**Calls ->**
- [[ReplState.PrintState]]
- [[ReplState.SetWorkingDirectory]]

**Called-by <-**
- [[Program.TryHandleBuiltInAsync]]

