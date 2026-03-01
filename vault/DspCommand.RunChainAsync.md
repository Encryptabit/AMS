---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 7
tags:
  - method
  - danger/high-complexity
---
# DspCommand::RunChainAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[DspCommand.RunChainAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunChainAsync(TreatmentChain chain, string inputPath, string outputPath, string chainBaseDirectory, int? overrideSampleRate, int? overrideBlockSize, int? overrideOutChannels, int? overrideBitDepth, DirectoryInfo workDirOption, bool keepTemp, bool overwrite, CancellationToken cancellationToken)
```

**Calls ->**
- [[DspCommand.BuildProcessArguments]]
- [[DspCommand.ResolveInputs]]
- [[DspCommand.ResolveNodeOutput]]
- [[DspCommand.TryDeleteDirectory]]
- [[DspCommand.TryDeleteFile]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

