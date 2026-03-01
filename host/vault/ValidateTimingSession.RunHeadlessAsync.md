---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 7
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ValidateTimingSession::RunHeadlessAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Execute a non-interactive timing validation/compression pass, persist any resulting pause adjustments, and return machine-readable outcome metrics.**

`RunHeadlessAsync` awaits `LoadSessionContextAsync`, optionally copies `context.Analysis` into `_prosodyAnalysis`, then constructs `InteractiveState` from the loaded pause/analysis/sentence-paragraph context. It toggles options focus to initialize compression state, runs `ApplyCompressionPreview()`, and commits the current scope with `CommitScope(state.Current, compressionSummary.HasChanges ? compressionSummary : null)`, relying on internal state mutation rather than the returned commit payload. It then calls `PersistPauseAdjustments`, derives `hasAdjustments` from both `adjustments.Count` and `_pauseAdjustmentsFile.Exists`, logs either the saved relative path plus compression stats or a no-op message, and returns `HeadlessResult` with adjustment/file/compression counts.


#### [[ValidateTimingSession.RunHeadlessAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<ValidateTimingSession.HeadlessResult> RunHeadlessAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[ValidateTimingSession.GetRelativePathSafe]]
- [[InteractiveState.ApplyCompressionPreview]]
- [[InteractiveState.CommitScope]]
- [[InteractiveState.ToggleOptionsFocus]]
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[ValidateTimingSession.PersistPauseAdjustments]]
- [[Log.Debug]]

