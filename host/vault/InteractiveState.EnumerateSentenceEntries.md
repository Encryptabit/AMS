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
  - llm/data-access
  - llm/utility
  - llm/validation
---
# InteractiveState::EnumerateSentenceEntries
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the sentence-scope entries for a given paragraph within the timing-validation interactive session.**

Implementation isn’t visible in the current workspace, but from the signature and complexity 1, this appears to be a thin, branch-light iterator over `InteractiveState` that returns `ValidateTimingSession.ScopeEntry` records scoped to a single `paragraphId` (typically via direct lookup or a single filter). It returns `IEnumerable<ScopeEntry>`, so enumeration is likely deferred and read-only against session state.


#### [[InteractiveState.EnumerateSentenceEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IEnumerable<ValidateTimingSession.ScopeEntry> EnumerateSentenceEntries(int paragraphId)
```

