---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 10
fan_in: 0
fan_out: 2
tags:
  - method
---
# ValidateTimingSession::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession(IWorkspace workspace, FileInfo transcriptFile, FileInfo bookIndexFile, FileInfo hydrateFile, bool runProsodyAnalysis, bool includeAllIntraSentenceGaps = false, bool interSentenceOnly = true)
```

**Calls ->**
- [[PausePolicyResolver.Resolve]]
- [[Log.Debug]]

