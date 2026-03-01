---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 9
tags:
  - method
---
# DspCommand::ExecuteFilterChain
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ExecuteFilterChain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ExecuteFilterChain(FileInfo inputFile, IReadOnlyList<FilterConfig> filters, bool saveOutput, FileInfo explicitOutput, bool printAstats, bool printRaw)
```

**Calls ->**
- [[DspCommand.BuildFilterGraph]]
- [[DspCommand.PrintAstatsLogs]]
- [[DspCommand.PrintRawLogs]]
- [[DspCommand.ResolveFilteredOutput]]
- [[Log.Info]]
- [[AudioProcessor.Decode]]
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.StreamToWave]]

**Called-by <-**
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateTestAllCommand]]

