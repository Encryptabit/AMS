---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
---
# ValidateTimingSession::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task RunAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[ValidateTimingSession.OnCommit]]
- [[ValidateTimingSession.RenderIntro]]
- [[TimingController.Run]]

**Called-by <-**
- [[ValidateCommand.CreateTimingCommand]]

