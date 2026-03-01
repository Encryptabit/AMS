---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 4
tags:
  - method
---
# PausePolicyResolver::Resolve
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs`


#### [[PausePolicyResolver.Resolve]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static (PausePolicy Policy, string SourcePath) Resolve(FileInfo transcriptFile = null)
```

**Calls ->**
- [[PausePolicyResolver.EnumerateCandidates]]
- [[Log.Debug]]
- [[PausePolicyPresets.House]]
- [[PausePolicyStorage.Load]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]
- [[ValidateTimingSession..ctor]]

