---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# InteractiveState::IsParagraphZero
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Determine whether a given sentence ID belongs to paragraph index 0 in the interactive timing-session state.**

`IsParagraphZero(int sentenceId)` is a small predicate on `InteractiveState` used by `FilterParagraphZeroAdjustments` to gate records by paragraph-0 membership. With cyclomatic complexity 2, the implementation is effectively a single state lookup plus one branch/comparison, returning a boolean without mutating session state. This centralizes the paragraph-zero rule so callers can filter adjustments consistently.


#### [[InteractiveState.IsParagraphZero]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool IsParagraphZero(int sentenceId)
```

**Called-by <-**
- [[InteractiveState.FilterParagraphZeroAdjustments]]

