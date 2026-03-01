---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# ValidateTimingSession::TryLoadMfaSilences
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.TryLoadMfaSilences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<(double Start, double End)> TryLoadMfaSilences(TranscriptIndex transcript)
```

**Calls ->**
- [[ValidateTimingSession.BuildTextGridCandidates]]
- [[ValidateTimingSession.IsSilenceLabel]]
- [[TextGridParser.ParseWordIntervals]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]

