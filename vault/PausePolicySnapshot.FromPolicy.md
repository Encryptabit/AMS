---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# PausePolicySnapshot::FromPolicy
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAdjustmentsDocument.cs`


#### [[PausePolicySnapshot.FromPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PausePolicySnapshot FromPolicy(PausePolicy policy)
```

**Calls ->**
- [[PauseWindowSnapshot.FromWindow]]

**Called-by <-**
- [[PauseAdjustmentsDocument.Create]]
- [[PausePolicyStorage.Save]]

