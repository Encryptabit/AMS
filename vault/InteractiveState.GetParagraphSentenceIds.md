---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 5
fan_out: 0
tags:
  - method
---
# InteractiveState::GetParagraphSentenceIds
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetParagraphSentenceIds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<int> GetParagraphSentenceIds(int paragraphId)
```

**Called-by <-**
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.CollectCompressionPauses]]
- [[InteractiveState.CollectPauses]]
- [[InteractiveState.CountParagraphPauses]]
- [[InteractiveState.GetParagraphSentenceCount]]

