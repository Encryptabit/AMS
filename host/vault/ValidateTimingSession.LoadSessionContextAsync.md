---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 7
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ValidateTimingSession::LoadSessionContextAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build and persist the complete validation session context needed to execute timing validation for a chapter.**

This private async helper is the context-loading orchestrator for `ValidateTimingSession`, and it is shared by both `RunAsync` and `RunHeadlessAsync`. It opens chapter data (`OpenChapter`), derives paragraph and sentence indexes (`BuildParagraphData`, `BuildSentenceLookup`), runs chapter analysis (`AnalyzeChapter`), and composes the resulting `SessionContext` via `Build`. It also attempts optional MFA silence hydration (`TryLoadMfaSilences`) and persists the session (`Save`) while honoring the provided `CancellationToken`.


#### [[ValidateTimingSession.LoadSessionContextAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<ValidateTimingSession.SessionContext> LoadSessionContextAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[ValidateTimingSession.BuildParagraphData]]
- [[ValidateTimingSession.BuildSentenceLookup]]
- [[ValidateTimingSession.TryLoadMfaSilences]]
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseMapBuilder.Build]]
- [[ChapterContextHandle.Save]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]
- [[ValidateTimingSession.RunHeadlessAsync]]

