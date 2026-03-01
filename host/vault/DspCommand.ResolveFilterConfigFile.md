---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# DspCommand::ResolveFilterConfigFile
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Resolve the effective filter-chain configuration file by using the explicit file when passed or defaulting to `filter-chain.json` in the current working directory.**

`ResolveFilterConfigFile` centralizes filter-chain config path resolution for DSP filter commands. Its implementation checks whether `provided` is non-null, and if so returns a new `FileInfo` built from `Path.GetFullPath(provided.FullName)` to normalize to an absolute path. When no file is provided, it uses `Directory.GetCurrentDirectory()` plus `DefaultFilterChainFileName` (`"filter-chain.json"`) to construct the fallback `FileInfo`.


#### [[DspCommand.ResolveFilterConfigFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveFilterConfigFile(FileInfo provided)
```

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]

