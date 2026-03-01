---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 9
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/error-handling
---
# DspCommand::ExecuteFilterChain
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Execute a DSP filter chain for an input file, optionally write the filtered result, and optionally print captured analysis/raw logs.**

`ExecuteFilterChain` is the command-path orchestrator used by `CreateFilterChainRunCommand` and `CreateTestAllCommand`: it builds the runtime filter graph with `BuildFilterGraph`, executes decode/filter processing, and captures execution logs through `CaptureLogs`. When `saveOutput` is enabled, it computes the destination with `ResolveFilteredOutput` and writes filtered PCM/WAV output via `StreamToWave`; otherwise it runs the chain with `RunDiscardingOutput` to execute without persisting data. It conditionally emits diagnostics using `PrintAstatsLogs` and `PrintRawLogs`, with `Info` used for user-facing status/output reporting.


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

