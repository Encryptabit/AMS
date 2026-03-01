---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# EditablePause::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**It creates an `EditablePause` instance populated with pause timing and neighboring transcript context for validation logic.**

This constructor is a constant-time initializer for `EditablePause` that accepts a `PauseSpan`, adjacent text (`leftText`, `rightText`), and nullable neighboring paragraph IDs (`leftParagraphId`, `rightParagraphId`). With complexity 1, the implementation is a direct argument-to-member assignment pattern with no branching, async work, or external side effects.


#### [[EditablePause..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public EditablePause(PauseSpan span, string leftText, string rightText, int? leftParagraphId, int? rightParagraphId)
```

