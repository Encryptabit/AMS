---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# TimingRenderer::EnumerateStats
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Transform pause statistics input into normalized `(PauseClass, PauseStats)` rows for rendering.**

The method is a private static projection that converts a `PauseStatsSet` into an `IEnumerable` of `(PauseClass Class, PauseStats Stats)` tuples. With cyclomatic complexity 1, the implementation is a simple linear enumeration with no meaningful branching, emitting class/stat pairs for consumption by `CreateStatsTable`.


#### [[TimingRenderer.EnumerateStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<(PauseClass Class, PauseStats Stats)> EnumerateStats(PauseStatsSet stats)
```

**Called-by <-**
- [[TimingRenderer.CreateStatsTable]]

