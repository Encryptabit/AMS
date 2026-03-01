---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# DspCommand::ResolveChainFile
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Determines the effective chain-file location by preferring an explicit file argument and falling back to the default chain filename in a base or current directory.**

`ResolveChainFile` returns a concrete `FileInfo` for the DSP chain file with a two-branch path strategy. If `provided` is non-null, it normalizes to an absolute path via `Path.GetFullPath(provided.FullName)` and wraps it in a new `FileInfo`. Otherwise it uses `baseDirectory ?? Directory.GetCurrentDirectory()` as the root and returns `Path.Combine(root, DefaultChainFileName)` (where `DefaultChainFileName` is `"dsp.chain.json"`). The method is intentionally path-only and does not validate file existence.


#### [[DspCommand.ResolveChainFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveChainFile(FileInfo provided, string baseDirectory = null)
```

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]
- [[DspCommand.CreateRunCommand]]

