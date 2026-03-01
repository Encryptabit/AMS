---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
---
# DspCommand::ResolveFilterDefinitions
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveFilterDefinitions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<DspCommand.FilterDefinition> ResolveFilterDefinitions(string[] requested)
```

**Calls ->**
- [[DspCommand.GetFilterDefinition]]

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]

