---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
---
# DspCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[DspCommand.CreateChainCommand]]
- [[DspCommand.CreateFilterChainCommand]]
- [[DspCommand.CreateFiltersCommand]]
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.CreateListPluginsCommand]]
- [[DspCommand.CreateOutputModeCommand]]
- [[DspCommand.CreateOverwriteCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateSetDirCommand]]
- [[DspCommand.CreateTestAllCommand]]

**Called-by <-**
- [[Program.Main]]

