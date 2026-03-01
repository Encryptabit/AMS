---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# BuildIndexCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`


#### [[BuildIndexCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[BuildIndexCommand.BuildBookIndexAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveBookSource]]
- [[Log.Error]]

**Called-by <-**
- [[Program.Main]]

