---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# Program::HandleModeCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.HandleModeCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void HandleModeCommand(IReadOnlyList<string> tokens, ReplState state)
```

**Calls ->**
- [[Program.HandleUseCommand]]

**Called-by <-**
- [[Program.TryHandleBuiltInAsync]]

