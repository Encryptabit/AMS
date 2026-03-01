---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/utility
---
# DspCommand::CreateFilterConfig
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates a normalized FilterConfig object for a filter definition, including enabled state and serialized default parameters.**

CreateFilterConfig is a static expression-bodied factory that converts a DspCommand.FilterDefinition into a FilterConfig instance. It sets Name from definition.Name, applies the enabled argument (default true), and initializes Parameters by calling SerializeParameters on definition.DefaultParameters when present, otherwise on a fallback from CreateDefaultParameterInstance(definition). This guarantees the resulting config carries a serialized JsonElement parameter payload even when explicit defaults are missing.


#### [[DspCommand.CreateFilterConfig]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FilterConfig CreateFilterConfig(DspCommand.FilterDefinition definition, bool enabled = true)
```

**Calls ->**
- [[DspCommand.CreateDefaultParameterInstance]]
- [[DspCommand.SerializeParameters]]

**Called-by <-**
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateTestAllCommand]]

