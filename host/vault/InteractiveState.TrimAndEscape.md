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
  - llm/error-handling
---
# InteractiveState::TrimAndEscape
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Prepare user/session text for diff display by trimming, bounding, and escaping it into a safe, deterministic string.**

`TrimAndEscape` is a small static sanitization helper used by `BuildDiffContext`, with low branch complexity (null/empty guard plus length-bound handling). It normalizes input text by trimming, applying a `maxLength` cap, and escaping characters that would otherwise break diff/context rendering, returning a safe single string for interactive output.


#### [[InteractiveState.TrimAndEscape]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TrimAndEscape(string text, int maxLength)
```

**Called-by <-**
- [[InteractiveState.BuildDiffContext]]

