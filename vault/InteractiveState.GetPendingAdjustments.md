---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 2
tags:
  - method
---
# InteractiveState::GetPendingAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetPendingAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.DiffRow> GetPendingAdjustments(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[InteractiveState.CollectPauses]]
- [[InteractiveState.TryCreateDiffRow]]

**Called-by <-**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

