---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::GetParagraphRange
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Generate the paragraph ID range starting from a specified paragraph so compression-pause collection can process the relevant segment.**

This private helper on `ValidateTimingSession.InteractiveState` returns an `IEnumerable<int>` of paragraph IDs derived from `startParagraphId` for `CollectCompressionPauses` to consume. With reported cyclomatic complexity 3, the implementation is a small control-flow routine (typically guard/branch plus simple iteration) rather than heavy domain logic. Its private scope and single known caller indicate localized range-selection behavior inside the timing-session validation flow.


#### [[InteractiveState.GetParagraphRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IEnumerable<int> GetParagraphRange(int startParagraphId)
```

**Called-by <-**
- [[InteractiveState.CollectCompressionPauses]]

