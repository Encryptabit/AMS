---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::MatchesScope
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Check whether a candidate scope is equivalent to the compression state's current scope so existing compression state can be safely reused.**

`MatchesScope` is a pure predicate that determines if an incoming `ScopeEntry` targets the same logical scope as `CompressionState.Scope`. It fails fast when `scope.Kind` differs, then uses a `switch` expression: `Chapter` always matches, `Paragraph` matches on `ParagraphId`, `Sentence` matches on both `ParagraphId` and `SentenceId`, and `Pause` matches only when both pauses are non-null and `ReferenceEquals(scope.Pause, Scope.Pause)` is true. Unknown kinds return `false`, and the method performs no state mutation.


#### [[CompressionState.MatchesScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool MatchesScope(ValidateTimingSession.ScopeEntry scope)
```

**Called-by <-**
- [[InteractiveState.CommitScope]]
- [[CompressionState.HandleCommit]]
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

