---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 1
tags:
  - method
---
# DspCommand::DeserializeParameters
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.DeserializeParameters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static object DeserializeParameters(DspCommand.FilterDefinition definition, JsonElement element)
```

**Calls ->**
- [[DspCommand.CreateDefaultParameterInstance]]

**Called-by <-**
- [[DspCommand.BuildFilterGraph]]

