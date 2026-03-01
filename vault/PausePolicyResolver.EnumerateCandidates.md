---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# PausePolicyResolver::EnumerateCandidates
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/PausePolicyResolver.cs`


#### [[PausePolicyResolver.EnumerateCandidates]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> EnumerateCandidates(FileInfo transcriptFile)
```

**Calls ->**
- [[PausePolicyResolver.AddCandidate]]

**Called-by <-**
- [[PausePolicyResolver.Resolve]]

