---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 5
tags:
  - method
---
# Program::StartRepl
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.StartRepl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task StartRepl(RootCommand rootCommand)
```

**Calls ->**
- [[Program.ExecuteWithScopeAsync]]
- [[Program.IsExit]]
- [[Program.ParseInput]]
- [[Program.Prompt]]
- [[Program.TryHandleBuiltInAsync]]

**Called-by <-**
- [[Program.Main]]

