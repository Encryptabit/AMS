---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::GetTreeViewportEntries
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetTreeViewportEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.ScopeEntry> GetTreeViewportEntries(out bool hasPrevious, out bool hasNext)
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

