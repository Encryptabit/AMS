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
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# InteractiveState::GetParagraphInfo
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Fetch the `ParagraphInfo` associated with a specific paragraph ID so other interactive-session operations can read or update that paragraph consistently.**

In `ValidateTimingSession.InteractiveState`, `GetParagraphInfo(int paragraphId)` is a low-complexity (2) state-access helper that resolves the `ParagraphInfo` for a paragraph key used by both `AppendParagraph` and `BuildParagraphDetail`. The method structure is consistent with a single conditional lookup flow: return an existing `ParagraphInfo` from interactive state when present, otherwise follow a fallback/missing-path branch. This keeps paragraph mutation and paragraph-detail rendering aligned on the same per-paragraph state object.


#### [[InteractiveState.GetParagraphInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.ParagraphInfo GetParagraphInfo(int paragraphId)
```

**Called-by <-**
- [[InteractiveState.AppendParagraph]]
- [[TimingRenderer.BuildParagraphDetail]]

