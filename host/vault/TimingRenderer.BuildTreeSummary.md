---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# TimingRenderer::BuildTreeSummary
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build a concise timing/statistics summary string for a scope entry so `BuildTree` can render informative tree nodes.**

`BuildTreeSummary(ValidateTimingSession.ScopeEntry entry)` in `TimingRenderer` derives node-level timing metrics by calling `CountParagraphPauses`, `CountSentencePauses`, and `GetParagraphSentenceCount`, then formats those aggregates into a single human-readable string. It is a private render helper used by `BuildTree`, so the summary is part of tree node presentation rather than core validation logic. Its stated complexity (5) suggests lightweight branching for formatting decisions (for example, conditional fragments or pluralization) while remaining side-effect free.


#### [[TimingRenderer.BuildTreeSummary]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string BuildTreeSummary(ValidateTimingSession.ScopeEntry entry)
```

**Calls ->**
- [[InteractiveState.CountParagraphPauses]]
- [[InteractiveState.CountSentencePauses]]
- [[InteractiveState.GetParagraphSentenceCount]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

