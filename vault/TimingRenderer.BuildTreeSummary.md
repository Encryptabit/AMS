---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# TimingRenderer::BuildTreeSummary
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingRenderer.BuildTreeSummary]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string BuildTreeSummary(ValidateTimingSession.ScopeEntry entry)
```

**Calls ->**
- [[InteractiveState.CountParagraphPauses]]
- [[InteractiveState.CountSentencePauses]]
- [[InteractiveState.GetParagraphSentenceCount]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

