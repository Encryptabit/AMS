---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# InteractiveState::AppendSentenceFallback
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AppendSentenceFallback]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendSentenceFallback(StringBuilder sb, int sentenceId, int? neighborSentenceId)
```

**Calls ->**
- [[InteractiveState.GetSentenceText]]

**Called-by <-**
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.AppendPauseSentencesFallback]]

