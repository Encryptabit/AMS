---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/utility
---
# DspCommand::BuildSingleNodeChain
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Build a single-node `TreatmentChain` from CLI plugin inputs by resolving plugin location/name and packaging node parameters.**

`BuildSingleNodeChain` resolves the plugin path relative to `baseDirectory` using `ResolvePluginPath` when `config` exists, otherwise `ResolvePath`. It then tries to map the resolved path to a friendly node name via `TryGetFriendlyName(config, pluginPath)` and falls back to `Path.GetFileNameWithoutExtension(pluginPath)`. The method creates a `TreatmentNode` populated with the resolved plugin path, CLI `parameters`, `preset`, and `paramFile?.FullName`, with all processing/output-specific fields left null. It returns a `TreatmentChain` whose chain-level metadata fields are null and whose `Nodes` contains exactly that single node.


#### [[DspCommand.BuildSingleNodeChain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TreatmentChain BuildSingleNodeChain(string plugin, IReadOnlyList<string> parameters, string preset, FileInfo paramFile, string baseDirectory, DspConfig config = null)
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[DspCommand.ResolvePluginPath]]
- [[DspCommand.TryGetFriendlyName]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

