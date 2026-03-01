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
# PausePolicyStorage::Save
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PausePolicyStorage.cs`


#### [[PausePolicyStorage.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Save(string path, PausePolicy policy)
```

**Calls ->**
- [[PausePolicySnapshot.FromPolicy]]

**Called-by <-**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[FileArtifactResolver.SavePausePolicy]]

