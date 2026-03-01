---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 14
fan_in: 3
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
  - llm/utility
  - llm/data-access
---
# DspCommand::CreateNodeFromOptions
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Build a validated `TreatmentNode` from command-line node options, including plugin/path resolution and normalized optional fields.**

`CreateNodeFromOptions` reads all node CLI inputs from `InvocationContext.ParseResult`, requires a non-empty plugin token, resolves it with `ResolvePluginPath`, and throws if the resolved plugin file is missing. It computes the node display name with precedence `--name` then cached friendly name (`TryGetFriendlyName`) then plugin filename without extension. It constructs and returns a `TreatmentNode`, passing typed overrides directly while normalizing empty strings/arrays to `null`, resolving `--preset` through `ResolvePath`, and storing `--param-file` as `paramFile?.FullName`.


#### [[DspCommand.CreateNodeFromOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TreatmentNode CreateNodeFromOptions(InvocationContext context, DspCommand.NodeOptionBundle options, string baseDirectory, DspConfig config)
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[DspCommand.ResolvePluginPath]]
- [[DspCommand.TryGetFriendlyName]]

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainPrependCommand]]

