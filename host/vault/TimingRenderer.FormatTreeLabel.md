---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# TimingRenderer::FormatTreeLabel
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Formats a scope entry into an indented, conditionally highlighted label for the tree rendered by `BuildTree`.**

`FormatTreeLabel` creates a tree row label by allocating indentation with `new string(' ', entry.Depth * 2)` and appending `entry.Label`. It then compares the node to `_state.Current` via `entry.Equals(_state.Current)` and, if matched, wraps the label in Spectre.Console markup (`[bold dodgerblue1]...[/]`) to visually highlight the active scope. Otherwise it returns the unstyled label string.


#### [[TimingRenderer.FormatTreeLabel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string FormatTreeLabel(ValidateTimingSession.ScopeEntry entry)
```

**Called-by <-**
- [[TimingRenderer.BuildTree]]

