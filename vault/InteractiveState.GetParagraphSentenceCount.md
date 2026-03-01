---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# InteractiveState::GetParagraphSentenceCount
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetParagraphSentenceCount]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int GetParagraphSentenceCount(int paragraphId)
```

**Calls ->**
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[InteractiveState.BuildParagraphLabel]]
- [[TimingRenderer.BuildTreeSummary]]

