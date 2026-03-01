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
  - llm/validation
  - llm/error-handling
---
# DspCommand::GetFilterDefinition
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Resolve a provided filter name to a registered filter definition and reject unknown names with a descriptive exception.**

`GetFilterDefinition` performs a single `FilterDefinitionMap.TryGetValue(name, out var definition)` lookup and returns the matched `FilterDefinition` directly. Because `FilterDefinitionMap` is created with `StringComparer.OrdinalIgnoreCase`, filter-name resolution is case-insensitive. If no match exists, it throws an `InvalidOperationException` with a user-facing hint to run `dsp filters`, and this fail-fast path is used by both `ResolveFilterDefinitions` and `BuildFilterGraph`.


#### [[DspCommand.GetFilterDefinition]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static DspCommand.FilterDefinition GetFilterDefinition(string name)
```

**Called-by <-**
- [[DspCommand.BuildFilterGraph]]
- [[DspCommand.ResolveFilterDefinitions]]

