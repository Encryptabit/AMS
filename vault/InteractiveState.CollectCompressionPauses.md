---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 19
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
---
# InteractiveState::CollectCompressionPauses
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.


#### [[InteractiveState.CollectCompressionPauses]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ValidateTimingSession.EditablePause> CollectCompressionPauses(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[AddPause]]
- [[InteractiveState.GetParagraphRange]]
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

