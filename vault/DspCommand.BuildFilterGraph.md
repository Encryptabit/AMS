---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
---
# DspCommand::BuildFilterGraph
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.BuildFilterGraph]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FfFilterGraph BuildFilterGraph(AudioBuffer buffer, IReadOnlyList<FilterConfig> filters)
```

**Calls ->**
- [[DspCommand.DeserializeParameters]]
- [[DspCommand.GetFilterDefinition]]
- [[FfFilterGraph.FromBuffer]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

