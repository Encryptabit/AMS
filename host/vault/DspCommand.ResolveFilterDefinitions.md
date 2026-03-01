---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ResolveFilterDefinitions
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Map optional user-provided filter names to concrete built-in `FilterDefinition` instances, with support for selecting all filters and failing fast on invalid names.**

`ResolveFilterDefinitions` normalizes filter selection for CLI flow by returning the full static `FilterDefinitions` array when `requested` is `null`/empty or when any token equals `"all"` (case-insensitive). Otherwise it allocates a `List<FilterDefinition>` sized to `requested.Length`, resolves each name via `GetFilterDefinition`, and returns that list. Resolution is backed by a case-insensitive dictionary (`FilterDefinitionMap`), so unknown names surface as `InvalidOperationException` from `GetFilterDefinition`, and requested order (including duplicates) is preserved.


#### [[DspCommand.ResolveFilterDefinitions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<DspCommand.FilterDefinition> ResolveFilterDefinitions(string[] requested)
```

**Calls ->**
- [[DspCommand.GetFilterDefinition]]

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]

