---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# InteractiveState::AppendParagraphMarkup
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Render a paragraph into the output buffer as sentence-level markup with optional highlight/partner targeting and sentence fallback behavior.**

AppendParagraphMarkup builds paragraph-level manuscript markup into the provided `StringBuilder` by retrieving sentence IDs for `paragraphId` via `GetParagraphSentenceIds`, resolving each sentence text with `GetSentenceText`, and delegating per-sentence emission to `AppendSentenceFallback`. The nullable `highlightSentenceId` and `partnerSentenceId` parameters are used to conditionally annotate specific sentences during rendering, which explains the branching complexity. It is a shared internal renderer used by both chapter preview and full manuscript markup generation.


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

