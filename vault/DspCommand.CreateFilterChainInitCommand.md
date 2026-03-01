---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 7
tags:
  - method
---
# DspCommand::CreateFilterChainInitCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateFilterChainInitCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateFilterChainInitCommand()
```

**Calls ->**
- [[DspCommand.CreateDefaultParameterInstance]]
- [[DspCommand.ResolveFilterConfigFile]]
- [[DspCommand.ResolveFilterDefinitions]]
- [[DspCommand.SerializeParameters]]
- [[FilterChainConfig.SaveAsync]]
- [[Log.Error]]
- [[Log.Info]]

**Called-by <-**
- [[DspCommand.CreateFilterChainCommand]]

