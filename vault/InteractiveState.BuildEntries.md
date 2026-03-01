---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
---
# InteractiveState::BuildEntries
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.BuildEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ValidateTimingSession.ScopeEntry> BuildEntries()
```

**Calls ->**
- [[InteractiveState.AppendChapterPause]]
- [[InteractiveState.AppendParagraph]]

**Called-by <-**
- [[InteractiveState..ctor]]

