---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::CountParagraphPauses
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.CountParagraphPauses]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int CountParagraphPauses(int paragraphId)
```

**Calls ->**
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[TimingRenderer.BuildTreeSummary]]

