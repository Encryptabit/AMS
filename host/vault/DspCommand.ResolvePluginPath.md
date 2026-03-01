---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ResolvePluginPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Resolve a user-supplied plugin token into a concrete plugin file path using path resolution first and cached plugin metadata as a secondary lookup.**

`ResolvePluginPath` first validates `token` and immediately throws `ArgumentException` when it is null/whitespace. It then attempts `ResolvePath(token, baseDirectory)` and returns early if the resolved file exists; any exception during resolution is intentionally swallowed to allow fallback matching. Fallback searches `config.Plugins.Values` for case-insensitive matches against `PluginName`, `Path.GetFileNameWithoutExtension(meta.Path)`, or full `meta.Path`, deduplicates by path, and returns the single match. It throws `InvalidOperationException` for ambiguous matches and `FileNotFoundException` when no match is found.


#### [[DspCommand.ResolvePluginPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePluginPath(string token, string baseDirectory, DspConfig config)
```

**Calls ->**
- [[DspCommand.ResolvePath]]

**Called-by <-**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateNodeFromOptions]]

