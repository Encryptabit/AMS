---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# BookCommand::CreateVerify
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BookCommand.cs`


#### [[BookCommand.CreateVerify]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateVerify()
```

**Calls ->**
- [[BookCommand.RunVerifyAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[Log.Error]]

**Called-by <-**
- [[BookCommand.Create]]

