---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# ValidateCommand::TryResolveBookIndex
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.TryResolveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo TryResolveBookIndex(string bookIndexPath, string fallbackDirectory)
```

**Calls ->**
- [[ValidateCommand.MakeAbsolute]]

