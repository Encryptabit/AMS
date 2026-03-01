---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# InteractiveState::AppendSentence
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AppendSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendSentence(List<ValidateTimingSession.ScopeEntry> entries, int paragraphId, SentencePauseMap sentence)
```

**Calls ->**
- [[InteractiveState.BuildPauseLabel]]
- [[InteractiveState.BuildSentenceLabel]]

**Called-by <-**
- [[InteractiveState.AppendParagraph]]

