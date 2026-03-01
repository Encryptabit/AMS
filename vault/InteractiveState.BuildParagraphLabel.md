---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::BuildParagraphLabel
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.BuildParagraphLabel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string BuildParagraphLabel(ParagraphPauseMap paragraph, ValidateTimingSession.ParagraphInfo info)
```

**Calls ->**
- [[InteractiveState.GetParagraphSentenceCount]]

**Called-by <-**
- [[InteractiveState.AppendParagraph]]

