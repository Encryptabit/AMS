---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 7
tags:
  - method
---
# ValidateTimingSession::RunHeadlessAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

