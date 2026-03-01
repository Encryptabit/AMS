---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 7
tags:
  - method
---
# ValidateTimingSession::LoadSessionContextAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

