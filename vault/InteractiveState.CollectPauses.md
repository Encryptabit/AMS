---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 3
fan_out: 2
tags:
  - method
---
# InteractiveState::CollectPauses
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.CollectPauses]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.EditablePause> CollectPauses(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[Append]]
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[InteractiveState.CommitScope]]
- [[InteractiveState.GetPendingAdjustments]]
- [[InteractiveState.GetPendingPauseCount]]

