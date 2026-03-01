---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# DspCommand::ExpandInputToken
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ExpandInputToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExpandInputToken(string token, string baseDirectory, string initialInput, string previousOutput)
```

**Calls ->**
- [[DspCommand.ResolvePath]]

**Called-by <-**
- [[DspCommand.ResolveInputs]]

