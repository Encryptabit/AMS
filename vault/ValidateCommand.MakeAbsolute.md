---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# ValidateCommand::MakeAbsolute
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.MakeAbsolute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string MakeAbsolute(string path, string baseDirectory)
```

**Called-by <-**
- [[ValidateCommand.ResolveAudioPath]]
- [[ValidateCommand.TryResolveBookIndex]]

