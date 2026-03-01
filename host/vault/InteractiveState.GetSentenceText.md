---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# InteractiveState::GetSentenceText
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the text for a given sentence ID from the interactive timing-validation state in a caller-safe way.**

`GetSentenceText(int sentenceId)` in `ValidateTimingSession.InteractiveState` is a small synchronous accessor that resolves sentence text from interactive/session state using `sentenceId`, with a single branch consistent with cyclomatic complexity 2 (lookup plus missing/invalid handling). It is reused by both `AppendParagraphMarkup` and `AppendSentenceFallback`, indicating it centralizes sentence lookup behavior and the fallback/normalization path for missing sentence content.


#### [[InteractiveState.GetSentenceText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string GetSentenceText(int sentenceId)
```

**Called-by <-**
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.AppendSentenceFallback]]

