---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateTimingSession::BuildTextGridCandidates
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

