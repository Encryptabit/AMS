---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateTimingSession::BuildTextGridCandidates
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Generate the ordered TextGrid filename/path candidates for a transcript so silence-loading can attempt resolution against known roots.**

`BuildTextGridCandidates` assembles a read-only list of TextGrid path candidates from a `TranscriptIndex`, delegating root normalization/expansion to `AddRootFrom(string?)` instead of duplicating path logic inline. With cyclomatic complexity 2, the method is intentionally branch-light (likely a simple null/availability guard plus candidate accumulation). It serves as the candidate-generation stage used by `TryLoadMfaSilences`, keeping lookup ordering and root handling centralized.


#### [[ValidateTimingSession.BuildTextGridCandidates]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<string> BuildTextGridCandidates(TranscriptIndex transcript)
```

**Calls ->**
- [[AddRootFrom]]

**Called-by <-**
- [[ValidateTimingSession.TryLoadMfaSilences]]

