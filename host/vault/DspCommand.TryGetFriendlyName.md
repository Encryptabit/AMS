---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# DspCommand::TryGetFriendlyName
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Resolve a plugin’s friendly display name from cached DSP configuration metadata using a normalized plugin path.**

`TryGetFriendlyName` canonicalizes `pluginPath` with `Path.GetFullPath` and then does a dictionary lookup against `config.Plugins` keyed by that absolute path. On hit, it returns `metadata.PluginName`; on miss, it returns `null` (the current implementation is nullable `string?`). This provides a single lightweight lookup path for callers that later fall back to filename-based naming.


#### [[DspCommand.TryGetFriendlyName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryGetFriendlyName(DspConfig config, string pluginPath)
```

**Called-by <-**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateNodeFromOptions]]

