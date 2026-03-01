---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TimingRenderer::CreateStatsTable
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Create a renderable statistics table from `PauseStatsSet` for timing detail panels.**

`CreateStatsTable` builds a Spectre.Console `Table` with `TableBorder.Rounded`, `Expand = true`, and fixed columns for class/count/min/median/max/mean pause durations. It enumerates `(PauseClass, PauseStats)` pairs via `EnumerateStats(stats)`, skips any entry with `Count == 0`, and formats numeric values with `"0.000"` before adding each row. A `hasRows` guard adds a fallback row (`[grey]–[/]`, `0`, `-`, `-`, `-`, `-`) when no pause class has data.


#### [[TimingRenderer.CreateStatsTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table CreateStatsTable(PauseStatsSet stats)
```

**Calls ->**
- [[TimingRenderer.EnumerateStats]]

**Called-by <-**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

