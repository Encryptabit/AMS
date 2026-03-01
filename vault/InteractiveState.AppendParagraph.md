---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
---
# InteractiveState::AppendParagraph
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AppendParagraph]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendParagraph(List<ValidateTimingSession.ScopeEntry> entries, ParagraphPauseMap paragraph)
```

**Calls ->**
- [[InteractiveState.AppendSentence]]
- [[InteractiveState.BuildParagraphLabel]]
- [[InteractiveState.GetParagraphInfo]]

**Called-by <-**
- [[InteractiveState.BuildEntries]]

