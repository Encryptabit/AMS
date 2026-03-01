---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# InteractiveState::AppendSentenceFallback
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Append a sentence-level fallback fragment so markup generation can continue when primary timing/session sentence data is incomplete.**

`AppendSentenceFallback` appends fallback sentence content into the provided `StringBuilder` for rendering flows that need resilient text output. It retrieves sentence text via `GetSentenceText`, and branches on `neighborSentenceId` (`int?`) to use adjacent-sentence context when needed. With cyclomatic complexity 4, the implementation is a small set of guard/fallback branches, and it is reused by both paragraph-markup and pause-sentence fallback paths.


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

