---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# PausePolicyStorage::Load
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs`


#### [[PausePolicyStorage.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicy Load(string path)
```

**Calls ->**
- [[PausePolicySnapshot.ToPolicy]]

**Called-by <-**
- [[PausePolicyResolver.Resolve]]
- [[FileArtifactResolver.LoadPausePolicy]]

