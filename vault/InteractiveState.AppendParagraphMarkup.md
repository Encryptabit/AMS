---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 3
tags:
  - method
---
# InteractiveState::AppendParagraphMarkup
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AppendParagraphMarkup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendParagraphMarkup(StringBuilder sb, int paragraphId, int? highlightSentenceId, int? partnerSentenceId)
```

**Calls ->**
- [[InteractiveState.AppendSentenceFallback]]
- [[InteractiveState.GetParagraphSentenceIds]]
- [[InteractiveState.GetSentenceText]]

**Called-by <-**
- [[InteractiveState.AppendChapterPreview]]
- [[InteractiveState.BuildManuscriptMarkup]]

